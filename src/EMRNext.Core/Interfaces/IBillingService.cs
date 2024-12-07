using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Interfaces
{
    public interface IBillingService
    {
        // Claims Management
        Task<Claim> CreateClaimAsync(Claim claim);
        Task<Claim> UpdateClaimAsync(int claimId, Claim claim);
        Task<Claim> GetClaimAsync(int claimId);
        Task<IEnumerable<Claim>> GetPatientClaimsAsync(int patientId);
        Task<ClaimStatus> SubmitClaimAsync(int claimId);
        Task<ClaimStatus> CheckClaimStatusAsync(int claimId);
        
        // Payment Processing
        Task<Payment> ProcessPaymentAsync(Payment payment);
        Task<Payment> RecordPaymentAsync(Payment payment);
        Task<Payment> VoidPaymentAsync(int paymentId, string reason);
        Task<IEnumerable<Payment>> GetPatientPaymentsAsync(int patientId);
        
        // Insurance Management
        Task<InsuranceVerification> VerifyInsuranceAsync(int patientId, int insuranceId);
        Task<IEnumerable<InsuranceCoverage>> GetCoverageDetailsAsync(int patientId);
        Task<PreAuthorizationResult> RequestPreAuthorizationAsync(PreAuthRequest request);
        Task<IEnumerable<InsuranceEligibility>> CheckEligibilityAsync(int patientId);
        
        // Statement Generation
        Task<Statement> GenerateStatementAsync(int patientId);
        Task<Statement> GetStatementAsync(int statementId);
        Task<IEnumerable<Statement>> GetPatientStatementsAsync(int patientId);
        Task<bool> SendStatementAsync(int statementId);
        
        // Account Management
        Task<Account> CreateAccountAsync(Account account);
        Task<Account> UpdateAccountAsync(int accountId, Account account);
        Task<Account> GetAccountAsync(int accountId);
        Task<AccountBalance> GetAccountBalanceAsync(int accountId);
        
        // Fee Schedule Management
        Task<FeeSchedule> GetFeeScheduleAsync(string procedureCode);
        Task<IEnumerable<FeeSchedule>> GetProviderFeeScheduleAsync(int providerId);
        Task<bool> UpdateFeeScheduleAsync(FeeSchedule feeSchedule);
        
        // Financial Reporting
        Task<FinancialReport> GenerateFinancialReportAsync(DateTime startDate, DateTime endDate);
        Task<AgingReport> GenerateAgingReportAsync();
        Task<CollectionReport> GenerateCollectionReportAsync(DateTime startDate, DateTime endDate);
        
        // Contract Management
        Task<Contract> GetContractTermsAsync(int insurerId);
        Task<IEnumerable<ContractRate>> GetContractRatesAsync(int contractId);
        Task<bool> ValidateContractTermsAsync(int contractId, string procedureCode);
        
        // Adjustment Management
        Task<Adjustment> CreateAdjustmentAsync(Adjustment adjustment);
        Task<bool> ApplyAdjustmentAsync(int adjustmentId);
        Task<IEnumerable<Adjustment>> GetAccountAdjustmentsAsync(int accountId);
        
        // Credit Management
        Task<CreditCheck> PerformCreditCheckAsync(int patientId);
        Task<PaymentPlan> CreatePaymentPlanAsync(PaymentPlan plan);
        Task<IEnumerable<PaymentPlan>> GetActivePaymentPlansAsync(int patientId);
    }
}
