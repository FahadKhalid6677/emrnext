using System;
using FluentValidation;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Validation
{
    public class ClaimValidator : AbstractValidator<Claim>
    {
        public ClaimValidator()
        {
            RuleFor(c => c.PatientId)
                .NotEmpty().WithMessage("Patient ID is required");

            RuleFor(c => c.ProviderId)
                .NotEmpty().WithMessage("Provider ID is required");

            RuleFor(c => c.InsuranceId)
                .NotEmpty().WithMessage("Insurance ID is required");

            RuleFor(c => c.ServiceDate)
                .NotEmpty().WithMessage("Service date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Service date cannot be in the future");

            RuleFor(c => c.DiagnosisCodes)
                .NotEmpty().WithMessage("At least one diagnosis code is required");

            RuleFor(c => c.ProcedureCodes)
                .NotEmpty().WithMessage("At least one procedure code is required");

            RuleFor(c => c.ClaimAmount)
                .GreaterThan(0).WithMessage("Claim amount must be greater than 0");

            RuleFor(c => c.Status)
                .NotEmpty().WithMessage("Claim status is required")
                .IsInEnum().WithMessage("Invalid claim status");
        }
    }

    public class PaymentValidator : AbstractValidator<Payment>
    {
        public PaymentValidator()
        {
            RuleFor(p => p.AccountId)
                .NotEmpty().WithMessage("Account ID is required");

            RuleFor(p => p.PaymentAmount)
                .GreaterThan(0).WithMessage("Payment amount must be greater than 0");

            RuleFor(p => p.PaymentDate)
                .NotEmpty().WithMessage("Payment date is required");

            RuleFor(p => p.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required")
                .IsInEnum().WithMessage("Invalid payment method");

            RuleFor(p => p.PaymentStatus)
                .NotEmpty().WithMessage("Payment status is required")
                .IsInEnum().WithMessage("Invalid payment status");

            RuleFor(p => p.TransactionId)
                .NotEmpty().WithMessage("Transaction ID is required")
                .When(p => p.PaymentMethod != PaymentMethod.Cash);
        }
    }

    public class InsuranceVerificationValidator : AbstractValidator<InsuranceVerification>
    {
        public InsuranceVerificationValidator()
        {
            RuleFor(i => i.PatientId)
                .NotEmpty().WithMessage("Patient ID is required");

            RuleFor(i => i.InsuranceId)
                .NotEmpty().WithMessage("Insurance ID is required");

            RuleFor(i => i.VerificationDate)
                .NotEmpty().WithMessage("Verification date is required");

            RuleFor(i => i.PolicyNumber)
                .NotEmpty().WithMessage("Policy number is required")
                .MaximumLength(50).WithMessage("Policy number cannot exceed 50 characters");

            RuleFor(i => i.GroupNumber)
                .MaximumLength(50).WithMessage("Group number cannot exceed 50 characters");

            RuleFor(i => i.CoverageStatus)
                .NotEmpty().WithMessage("Coverage status is required")
                .IsInEnum().WithMessage("Invalid coverage status");
        }
    }

    public class StatementValidator : AbstractValidator<Statement>
    {
        public StatementValidator()
        {
            RuleFor(s => s.AccountId)
                .NotEmpty().WithMessage("Account ID is required");

            RuleFor(s => s.StatementDate)
                .NotEmpty().WithMessage("Statement date is required");

            RuleFor(s => s.DueDate)
                .NotEmpty().WithMessage("Due date is required")
                .GreaterThan(s => s.StatementDate).WithMessage("Due date must be after statement date");

            RuleFor(s => s.TotalAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Total amount cannot be negative");

            RuleFor(s => s.Status)
                .NotEmpty().WithMessage("Statement status is required")
                .IsInEnum().WithMessage("Invalid statement status");
        }
    }

    public class AccountValidator : AbstractValidator<Account>
    {
        public AccountValidator()
        {
            RuleFor(a => a.PatientId)
                .NotEmpty().WithMessage("Patient ID is required");

            RuleFor(a => a.AccountType)
                .NotEmpty().WithMessage("Account type is required")
                .IsInEnum().WithMessage("Invalid account type");

            RuleFor(a => a.Status)
                .NotEmpty().WithMessage("Account status is required")
                .IsInEnum().WithMessage("Invalid account status");

            RuleFor(a => a.BillingAddress)
                .NotEmpty().WithMessage("Billing address is required");

            RuleFor(a => a.PaymentTerms)
                .NotEmpty().WithMessage("Payment terms are required");
        }
    }

    public class PaymentPlanValidator : AbstractValidator<PaymentPlan>
    {
        public PaymentPlanValidator()
        {
            RuleFor(p => p.AccountId)
                .NotEmpty().WithMessage("Account ID is required");

            RuleFor(p => p.TotalAmount)
                .GreaterThan(0).WithMessage("Total amount must be greater than 0");

            RuleFor(p => p.MonthlyPayment)
                .GreaterThan(0).WithMessage("Monthly payment must be greater than 0");

            RuleFor(p => p.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .GreaterThanOrEqualTo(DateTime.UtcNow).WithMessage("Start date must be in the future");

            RuleFor(p => p.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThan(p => p.StartDate).WithMessage("End date must be after start date");

            RuleFor(p => p.Status)
                .NotEmpty().WithMessage("Payment plan status is required")
                .IsInEnum().WithMessage("Invalid payment plan status");
        }
    }

    public class AdjustmentValidator : AbstractValidator<Adjustment>
    {
        public AdjustmentValidator()
        {
            RuleFor(a => a.AccountId)
                .NotEmpty().WithMessage("Account ID is required");

            RuleFor(a => a.Amount)
                .NotEqual(0).WithMessage("Adjustment amount cannot be zero");

            RuleFor(a => a.AdjustmentType)
                .NotEmpty().WithMessage("Adjustment type is required")
                .IsInEnum().WithMessage("Invalid adjustment type");

            RuleFor(a => a.Reason)
                .NotEmpty().WithMessage("Adjustment reason is required")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");

            RuleFor(a => a.ApprovedBy)
                .NotEmpty().WithMessage("Approver ID is required");
        }
    }
}
