using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int EncounterId { get; set; }
        public int? FacilityId { get; set; }

        // Order Details
        public string OrderNumber { get; set; }
        public string OrderType { get; set; } // Lab, Radiology, Referral, etc.
        public string Category { get; set; }
        public string Status { get; set; } // Pending, In Progress, Completed, etc.
        public DateTime OrderDate { get; set; }
        public DateTime? CollectionDate { get; set; }
        public DateTime? ExpectedReportDate { get; set; }
        public string Priority { get; set; } // Routine, STAT, Urgent
        public bool IsRecurring { get; set; }
        public string Frequency { get; set; }
        public int? RecurrenceCount { get; set; }

        // Clinical Details
        public string ClinicalHistory { get; set; }
        public string ClinicalIndication { get; set; }
        public string ExamRequested { get; set; }
        public string SpecialInstructions { get; set; }
        public string PatientInstructions { get; set; }
        public string TransportMode { get; set; }
        public string SpecimenType { get; set; }
        public string SpecimenSource { get; set; }
        public string AnatomicalSite { get; set; }
        public string Laterality { get; set; }

        // Authorization
        public string AuthorizationNumber { get; set; }
        public DateTime? AuthorizationDate { get; set; }
        public string AuthorizationStatus { get; set; }
        public DateTime? AuthorizationExpirationDate { get; set; }

        // Results
        public bool ResultsReceived { get; set; }
        public DateTime? ResultsReceivedDate { get; set; }
        public string ResultStatus { get; set; }
        public string ResultNotes { get; set; }
        public bool IsAbnormal { get; set; }
        public string AbnormalityFlag { get; set; }
        public bool ReviewRequired { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public int? ReviewedByProviderId { get; set; }
        public string ReviewNotes { get; set; }

        // Billing
        public bool IsBillable { get; set; }
        public decimal? ExpectedCost { get; set; }
        public string BillingNotes { get; set; }

        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Patient Patient { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual Provider ReviewedByProvider { get; set; }
        public virtual Encounter Encounter { get; set; }
        public virtual Facility Facility { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<OrderResult> OrderResults { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
    }

    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Code { get; set; }
        public string CodeSystem { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public decimal? Quantity { get; set; }
        public string QuantityUnit { get; set; }
        public string Instructions { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string CompletionNotes { get; set; }

        public virtual Order Order { get; set; }
    }

    public class OrderResult
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Code { get; set; }
        public string CodeSystem { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public string ReferenceRange { get; set; }
        public string AbnormalFlag { get; set; }
        public string Status { get; set; }
        public DateTime ResultDate { get; set; }
        public string Notes { get; set; }
        public string PerformingLab { get; set; }
        public string Technician { get; set; }

        public virtual Order Order { get; set; }
    }
}
