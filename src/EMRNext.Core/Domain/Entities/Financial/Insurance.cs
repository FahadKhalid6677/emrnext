using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Core.Domain.Entities.Financial
{
    public class Insurance : AuditableEntity
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PayerId { get; set; }
        public string PayerName { get; set; }
        public string PlanName { get; set; }
        public string PlanType { get; set; }
        public string PolicyNumber { get; set; }
        public string GroupNumber { get; set; }
        public string SubscriberId { get; set; }
        public string SubscriberName { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? LastVerificationDate { get; set; }
        public string VerificationMethod { get; set; }
        public string CoverageLevel { get; set; }
        public decimal Copay { get; set; }
        public decimal Deductible { get; set; }
        public decimal DeductibleMet { get; set; }
        public decimal OutOfPocketMax { get; set; }
        public decimal OutOfPocketMet { get; set; }
        public string AuthorizationPhone { get; set; }
        public string ClaimsPhone { get; set; }
        public string ClaimsAddress { get; set; }
        public string ElectronicPayerId { get; set; }
        public string Notes { get; set; }

        // Navigation properties
        public Patient Patient { get; set; }
        public ICollection<InsuranceVerification> Verifications { get; set; } = new List<InsuranceVerification>();
        public ICollection<InsuranceAuthorization> Authorizations { get; set; } = new List<InsuranceAuthorization>();
        public ICollection<InsuranceDocument> Documents { get; set; } = new List<InsuranceDocument>();
    }

    public class InsuranceVerification : AuditableEntity
    {
        public int Id { get; set; }
        public int InsuranceId { get; set; }
        public DateTime VerificationDate { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public string ResponseCode { get; set; }
        public string VerifiedBy { get; set; }
        public string Coverage { get; set; }
        public decimal? CopayAmount { get; set; }
        public decimal? DeductibleAmount { get; set; }
        public decimal? DeductibleMet { get; set; }
        public decimal? OutOfPocketMax { get; set; }
        public decimal? OutOfPocketMet { get; set; }
        public string Notes { get; set; }

        public Insurance Insurance { get; set; }
    }

    public class InsuranceAuthorization : AuditableEntity
    {
        public int Id { get; set; }
        public int InsuranceId { get; set; }
        public string AuthorizationNumber { get; set; }
        public string Type { get; set; }
        public string ServiceCode { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UnitsApproved { get; set; }
        public int UnitsUsed { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

        public Insurance Insurance { get; set; }
    }

    public class InsuranceDocument : AuditableEntity
    {
        public int Id { get; set; }
        public int InsuranceId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentPath { get; set; }
        public string Description { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; }

        public Insurance Insurance { get; set; }
    }
}
