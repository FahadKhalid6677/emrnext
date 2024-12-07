using System;
using System.Threading.Tasks;

namespace EMRNext.Infrastructure.Services.External
{
    public interface IInsuranceVerificationService
    {
        Task<CoverageVerificationResult> VerifyCoverageAsync(CoverageVerificationRequest request);
        Task<EligibilityResponse> CheckEligibilityAsync(EligibilityRequest request);
        Task<AuthorizationResponse> RequestAuthorizationAsync(AuthorizationRequest request);
        Task<ClaimStatus> CheckClaimStatusAsync(string claimId);
    }

    public class CoverageVerificationRequest
    {
        public string MemberId { get; set; }
        public string GroupNumber { get; set; }
        public string PayerId { get; set; }
        public string SubscriberFirstName { get; set; }
        public string SubscriberLastName { get; set; }
        public DateTime? SubscriberDOB { get; set; }
        public string RelationshipToSubscriber { get; set; }
        public string ServiceType { get; set; }
        public DateTime ServiceDate { get; set; }
    }

    public class CoverageVerificationResult
    {
        public bool IsActive { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string PlanName { get; set; }
        public string PlanType { get; set; }
        public string NetworkStatus { get; set; }
        public CoverageBenefits Benefits { get; set; }
        public string[] Alerts { get; set; }
        public string VerificationId { get; set; }
    }

    public class CoverageBenefits
    {
        public decimal Deductible { get; set; }
        public decimal DeductibleMet { get; set; }
        public decimal OutOfPocketMax { get; set; }
        public decimal OutOfPocketMet { get; set; }
        public decimal Copay { get; set; }
        public decimal Coinsurance { get; set; }
        public string[] ExcludedServices { get; set; }
        public string[] RequiresPreauth { get; set; }
    }

    public class EligibilityRequest
    {
        public string MemberId { get; set; }
        public string GroupNumber { get; set; }
        public string PayerId { get; set; }
        public string ServiceType { get; set; }
        public DateTime ServiceDate { get; set; }
        public decimal EstimatedAmount { get; set; }
        public string[] DiagnosisCodes { get; set; }
        public string[] ProcedureCodes { get; set; }
    }

    public class EligibilityResponse
    {
        public bool IsEligible { get; set; }
        public string Status { get; set; }
        public decimal EstimatedPatientResponsibility { get; set; }
        public decimal EstimatedPayerAmount { get; set; }
        public string[] Warnings { get; set; }
        public string[] Notes { get; set; }
        public string EligibilityId { get; set; }
    }

    public class AuthorizationRequest
    {
        public string MemberId { get; set; }
        public string GroupNumber { get; set; }
        public string PayerId { get; set; }
        public string ServiceType { get; set; }
        public DateTime ServiceDate { get; set; }
        public string[] DiagnosisCodes { get; set; }
        public string[] ProcedureCodes { get; set; }
        public string ProviderId { get; set; }
        public string FacilityId { get; set; }
        public string ClinicalNotes { get; set; }
        public string[] SupportingDocuments { get; set; }
    }

    public class AuthorizationResponse
    {
        public string AuthorizationNumber { get; set; }
        public string Status { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int AuthorizedUnits { get; set; }
        public string[] ApprovedServices { get; set; }
        public string[] DeniedServices { get; set; }
        public string[] RequiredDocuments { get; set; }
        public string Notes { get; set; }
    }

    public class ClaimStatus
    {
        public string ClaimId { get; set; }
        public string Status { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public decimal BilledAmount { get; set; }
        public decimal AllowedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PatientResponsibility { get; set; }
        public string[] AdjustmentCodes { get; set; }
        public string[] RemarkCodes { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string CheckNumber { get; set; }
    }
}
