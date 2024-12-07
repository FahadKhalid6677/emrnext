using System;

namespace EMRNext.Core.Domain.Entities
{
    public class LabResult
    {
        public int Id { get; set; }
        public int LabOrderId { get; set; }
        public int PatientId { get; set; }
        
        // Result Details
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string LOINC { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        
        // Result Values
        public string Value { get; set; }
        public string Units { get; set; }
        public string ReferenceRange { get; set; }
        public string Interpretation { get; set; }
        public bool IsAbnormal { get; set; }
        public string AbnormalFlag { get; set; }
        public string Status { get; set; }
        
        // Result Metadata
        public DateTime ResultDate { get; set; }
        public string PerformingLab { get; set; }
        public string Methodology { get; set; }
        public string SpecimenType { get; set; }
        public DateTime? SpecimenCollectionDate { get; set; }
        public string SpecimenCondition { get; set; }
        
        // Critical Values
        public bool IsCritical { get; set; }
        public DateTime? CriticalNotificationDate { get; set; }
        public string NotifiedProvider { get; set; }
        public string CriticalNotificationNotes { get; set; }
        
        // Quality Control
        public string QualityControl { get; set; }
        public string AnalyticalTime { get; set; }
        public string InstrumentId { get; set; }
        public string Technologist { get; set; }
        public bool SupervisorReviewed { get; set; }
        public string SupervisorNotes { get; set; }
        
        // Result Notes
        public string Comments { get; set; }
        public string TechnicalNotes { get; set; }
        public string ClinicalNotes { get; set; }
        public string Disclaimer { get; set; }
        
        // Tracking
        public string LabAccessionNumber { get; set; }
        public DateTime? ReceivedDateTime { get; set; }
        public DateTime? ProcessedDateTime { get; set; }
        public DateTime? ReportedDateTime { get; set; }
        public DateTime? LastModifiedDateTime { get; set; }
        
        // Document References
        public string ReportPath { get; set; }
        public string ImagePaths { get; set; }
        public string AttachmentPaths { get; set; }
        
        // Delta Checking
        public string PreviousValue { get; set; }
        public DateTime? PreviousResultDate { get; set; }
        public string DeltaChange { get; set; }
        public string DeltaAlert { get; set; }
        
        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation Properties
        public virtual LabOrder LabOrder { get; set; }
        public virtual Patient Patient { get; set; }
    }
}
