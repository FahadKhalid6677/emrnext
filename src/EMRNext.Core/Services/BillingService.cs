using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Validation;
using EMRNext.Core.Exceptions;

namespace EMRNext.Core.Services
{
    public class BillingService : IBillingService
    {
        private readonly IBillingRepository _billingRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ILoggingService _loggingService;
        private readonly ClaimValidator _claimValidator;
        private readonly PaymentValidator _paymentValidator;
        private readonly InsuranceVerificationValidator _insuranceValidator;
        private readonly StatementValidator _statementValidator;
        private readonly AccountValidator _accountValidator;
        private readonly PaymentPlanValidator _paymentPlanValidator;

        public BillingService(
            IBillingRepository billingRepository,
            IPatientRepository patientRepository,
            ILoggingService loggingService)
        {
            _billingRepository = billingRepository;
            _patientRepository = patientRepository;
            _loggingService = loggingService;
            _claimValidator = new ClaimValidator();
            _paymentValidator = new PaymentValidator();
            _insuranceValidator = new InsuranceVerificationValidator();
            _statementValidator = new StatementValidator();
            _accountValidator = new AccountValidator();
            _paymentPlanValidator = new PaymentPlanValidator();
        }

        public async Task<Claim> CreateClaimAsync(Claim claim)
        {
            try
            {
                var validationResult = await _claimValidator.ValidateAsync(claim);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

                // Verify insurance eligibility
                var eligibility = await VerifyInsuranceAsync(claim.PatientId, claim.InsuranceId);
                if (!eligibility.IsEligible)
                    throw new BusinessRuleException($"Patient is not eligible for insurance: {eligibility.Message}");

                // Check for pre-authorization requirements
                if (RequiresPreAuthorization(claim))
                {
                    var authorization = await _billingRepository.GetPreAuthorizationAsync(claim.AuthorizationNumber);
                    if (authorization == null || !authorization.IsValid)
                        throw new AuthorizationException("Valid pre-authorization is required");
                }

                // Apply billing rules
                await ApplyBillingRulesAsync(claim);

                claim.Status = ClaimStatus.Pending;
                claim.CreatedDate = DateTime.UtcNow;
                claim.LastModified = DateTime.UtcNow;

                var result = await _billingRepository.CreateClaimAsync(claim);

                await _loggingService.LogAuditAsync(
                    "CreateClaim",
                    "Claim",
                    result.Id.ToString(),
                    $"Created claim for patient {claim.PatientId}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error creating claim for patient {PatientId}", claim.PatientId);
                throw;
            }
        }

        public async Task<Payment> ProcessPaymentAsync(Payment payment)
        {
            try
            {
                var validationResult = await _paymentValidator.ValidateAsync(payment);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

                // Verify account exists and is active
                var account = await _billingRepository.GetAccountAsync(payment.AccountId);
                if (account == null)
                    throw new NotFoundException($"Account with ID {payment.AccountId} not found");
                if (account.Status != AccountStatus.Active)
                    throw new BusinessRuleException("Account is not active");

                // Process payment based on method
                switch (payment.PaymentMethod)
                {
                    case PaymentMethod.CreditCard:
                        await ProcessCreditCardPaymentAsync(payment);
                        break;
                    case PaymentMethod.Check:
                        await ProcessCheckPaymentAsync(payment);
                        break;
                    case PaymentMethod.Cash:
                        await ProcessCashPaymentAsync(payment);
                        break;
                    default:
                        throw new BusinessRuleException($"Unsupported payment method: {payment.PaymentMethod}");
                }

                payment.Status = PaymentStatus.Processed;
                payment.ProcessedDate = DateTime.UtcNow;
                payment.LastModified = DateTime.UtcNow;

                var result = await _billingRepository.RecordPaymentAsync(payment);

                // Update account balance
                await UpdateAccountBalanceAsync(payment.AccountId);

                await _loggingService.LogAuditAsync(
                    "ProcessPayment",
                    "Payment",
                    result.Id.ToString(),
                    $"Processed payment for account {payment.AccountId}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error processing payment for account {AccountId}", payment.AccountId);
                throw;
            }
        }

        public async Task<Statement> GenerateStatementAsync(int patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                    throw new NotFoundException($"Patient with ID {patientId} not found");

                // Get account information
                var account = await _billingRepository.GetPatientAccountAsync(patientId);
                if (account == null)
                    throw new NotFoundException($"Account not found for patient {patientId}");

                // Get unbilled charges
                var charges = await _billingRepository.GetUnbilledChargesAsync(patientId);
                if (!charges.Any())
                    throw new BusinessRuleException("No unbilled charges found");

                // Calculate totals
                var statement = new Statement
                {
                    AccountId = account.Id,
                    StatementDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    BillingPeriodStart = charges.Min(c => c.ServiceDate),
                    BillingPeriodEnd = charges.Max(c => c.ServiceDate),
                    Charges = charges.ToList(),
                    TotalAmount = charges.Sum(c => c.Amount),
                    Status = StatementStatus.Generated
                };

                var validationResult = await _statementValidator.ValidateAsync(statement);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

                var result = await _billingRepository.CreateStatementAsync(statement);

                // Mark charges as billed
                await _billingRepository.MarkChargesAsBilledAsync(charges.Select(c => c.Id));

                await _loggingService.LogAuditAsync(
                    "GenerateStatement",
                    "Statement",
                    result.Id.ToString(),
                    $"Generated statement for patient {patientId}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error generating statement for patient {PatientId}", patientId);
                throw;
            }
        }

        // Private helper methods
        private bool RequiresPreAuthorization(Claim claim)
        {
            // Implementation of pre-authorization rules
            return claim.TotalAmount > 1000 || 
                   claim.ProcedureCodes.Any(p => PreAuthorizationRequired(p));
        }

        private bool PreAuthorizationRequired(string procedureCode)
        {
            // Implementation of procedure-specific pre-authorization rules
            return false; // Placeholder
        }

        private async Task ApplyBillingRulesAsync(Claim claim)
        {
            // Apply fee schedules
            await ApplyFeeSchedulesAsync(claim);

            // Apply contractual adjustments
            await ApplyContractualAdjustmentsAsync(claim);

            // Apply bundling rules
            await ApplyBundlingRulesAsync(claim);
        }

        private async Task ProcessCreditCardPaymentAsync(Payment payment)
        {
            // Implementation of credit card payment processing
        }

        private async Task ProcessCheckPaymentAsync(Payment payment)
        {
            // Implementation of check payment processing
        }

        private async Task ProcessCashPaymentAsync(Payment payment)
        {
            // Implementation of cash payment processing
        }

        private async Task UpdateAccountBalanceAsync(int accountId)
        {
            var account = await _billingRepository.GetAccountAsync(accountId);
            var balance = await _billingRepository.CalculateAccountBalanceAsync(accountId);
            
            account.CurrentBalance = balance;
            account.LastModified = DateTime.UtcNow;
            
            await _billingRepository.UpdateAccountAsync(accountId, account);
        }

        private async Task ApplyFeeSchedulesAsync(Claim claim)
        {
            // Implementation of fee schedule application
        }

        private async Task ApplyContractualAdjustmentsAsync(Claim claim)
        {
            // Implementation of contractual adjustment application
        }

        private async Task ApplyBundlingRulesAsync(Claim claim)
        {
            // Implementation of procedure code bundling rules
        }
    }
}
