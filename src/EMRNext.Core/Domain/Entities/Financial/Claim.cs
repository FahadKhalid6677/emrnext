using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Core.Domain.Entities.Financial
{
    public class Claim : AuditableEntity
    {
        public int Id { get; set; }
        public string ClaimNumber { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int? EncounterId { get; set; }
        public int? PrescriptionId { get; set; }
        public int? LabOrderId { get; set; }
        public DateTime ServiceDate { get; set; }
        public DateTime FilingDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AllowedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PatientResponsibility { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public string BillingProvider { get; set; }
        public string RenderingProvider { get; set; }
        public string FacilityCode { get; set; }
        public string PlaceOfService { get; set; }
        public bool IsElectronic { get; set; }
        public bool RequiresAuthorization { get; set; }
        public string AuthorizationNumber { get; set; }
        public DateTime? AuthorizationDate { get; set; }
        public string RejectionReason { get; set; }
        public string Notes { get; set; }

        // Navigation properties
        public Patient Patient { get; set; }
        public Provider Provider { get; set; }
        public Encounter Encounter { get; set; }
        public Insurance Insurance { get; set; }
        public ICollection<ClaimItem> Items { get; set; } = new List<ClaimItem>();
        public ICollection<ClaimAdjustment> Adjustments { get; set; } = new List<ClaimAdjustment>();
        public ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();
        public ICollection<ClaimHistory> History { get; set; } = new List<ClaimHistory>();
    }

    public class ClaimItem : AuditableEntity
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }
        public string ServiceCode { get; set; }
        public string Modifier { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int Units { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AllowedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; }
        public string DiagnosisPointers { get; set; }
        public DateTime ServiceDate { get; set; }
        public string RevenueCode { get; set; }
        public string NDCCode { get; set; }
        public string RejectionReason { get; set; }

        public Claim Claim { get; set; }
    }

    public class ClaimAdjustment : AuditableEntity
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }
        public string Type { get; set; }
        public string Reason { get; set; }
        public decimal Amount { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public string ProcessedBy { get; set; }
        public string Notes { get; set; }

        public Claim Claim { get; set; }
    }

    public class ClaimDocument : AuditableEntity
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentPath { get; set; }
        public string Description { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; }

        public Claim Claim { get; set; }
    }

    public class ClaimHistory : AuditableEntity
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime ActionDate { get; set; }
        public string ActionBy { get; set; }
        public string Notes { get; set; }

        public Claim Claim { get; set; }
    }
}
