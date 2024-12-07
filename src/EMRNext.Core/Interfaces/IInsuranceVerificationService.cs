using System;
using System.Threading.Tasks;

namespace EMRNext.Core.Interfaces
{
    public interface IInsuranceVerificationService
    {
        Task<InsuranceVerificationResult> VerifyInsuranceAsync(string memberId, string insuranceProvider);
        Task<InsuranceVerificationResult> VerifyInsuranceForServiceAsync(string memberId, string insuranceProvider, string serviceCode);
        Task<InsuranceVerificationResult> VerifyInsuranceForAppointmentAsync(int appointmentId);
        Task<CoverageDetails> GetCoverageDetailsAsync(string memberId, string insuranceProvider);
        Task<AuthorizationResult> RequestAuthorizationAsync(string memberId, string serviceCode, int units);
        Task<AuthorizationResult> CheckAuthorizationStatusAsync(string authorizationNumber);
        Task<EligibilityResult> CheckEligibilityAsync(string memberId, string insuranceProvider);
        Task<BenefitDetails> GetBenefitDetailsAsync(string memberId, string insuranceProvider, string benefitType);
    }

    public class InsuranceVerificationResult
    {
        public bool IsValid { get; set; }
        public string Status { get; set; }
        public DateTime VerificationDate { get; set; }
        public string ErrorMessage { get; set; }
        public CoverageDetails Coverage { get; set; }
    }

    public class CoverageDetails
    {
        public string PlanName { get; set; }
        public string PlanType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public decimal Deductible { get; set; }
        public decimal DeductibleMet { get; set; }
        public decimal OutOfPocketMax { get; set; }
        public decimal OutOfPocketMet { get; set; }
        public string CoverageLevel { get; set; }
        public string NetworkStatus { get; set; }
    }

    public class AuthorizationResult
    {
        public bool IsAuthorized { get; set; }
        public string AuthorizationNumber { get; set; }
        public DateTime? AuthorizationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? AuthorizedUnits { get; set; }
        public int? UsedUnits { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class EligibilityResult
    {
        public bool IsEligible { get; set; }
        public string Status { get; set; }
        public DateTime? EligibilityStartDate { get; set; }
        public DateTime? EligibilityEndDate { get; set; }
        public string Message { get; set; }
    }

    public class BenefitDetails
    {
        public string BenefitType { get; set; }
        public string Coverage { get; set; }
        public decimal? CopayAmount { get; set; }
        public decimal? CoinsurancePercentage { get; set; }
        public int? VisitsAllowed { get; set; }
        public int? VisitsUsed { get; set; }
        public string Limitations { get; set; }
        public string Notes { get; set; }
    }
}
