using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities
{
    public class LabOrder
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int? EncounterId { get; set; }
        
        // Order Details
        public string OrderNumber { get; set; }
        public string OrderType { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? CollectionDate { get; set; }
        public DateTime? ResultDate { get; set; }
        
        // Clinical Information
        public string Diagnosis { get; set; }
        public string DiagnosisCode { get; set; }
        public string ClinicalNotes { get; set; }
        public string SpecialInstructions { get; set; }
        
        // Specimen Information
        public string SpecimenType { get; set; }
        public string SpecimenSource { get; set; }
        public string CollectionMethod { get; set; }
        public string CollectionSite { get; set; }
        public string SpecimenCondition { get; set; }
        public string CollectorName { get; set; }
        
        // Laboratory Information
        public string LabFacility { get; set; }
        public string LabAddress { get; set; }
        public string LabContactInfo { get; set; }
        public string PerformingLab { get; set; }
        public string LabAccessionNumber { get; set; }
        
        // Test Information
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string TestDescription { get; set; }
        public string LOINC { get; set; }
        public string CPTCode { get; set; }
        public string TestMethodology { get; set; }
        
        // Result Information
        public string ResultStatus { get; set; }
        public string ResultValue { get; set; }
        public string Units { get; set; }
        public string ReferenceRange { get; set; }
        public string Interpretation { get; set; }
        public bool IsAbnormal { get; set; }
        public string AbnormalFlag { get; set; }
        public string ResultNotes { get; set; }
        
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
        public string SupervisorReview { get; set; }
        
        // Insurance Information
        public string InsuranceStatus { get; set; }
        public string PriorAuthNumber { get; set; }
        public string BillingCodes { get; set; }
        public string CoverageNotes { get; set; }
        
        // Tracking
        public string OrderingFacility { get; set; }
        public DateTime? ReceivedDateTime { get; set; }
        public DateTime? ProcessedDateTime { get; set; }
        public DateTime? ReportedDateTime { get; set; }
        public string TrackingNumber { get; set; }
        public string TransportationConditions { get; set; }
        
        // Document References
        public string RequisitionPath { get; set; }
        public string ReportPath { get; set; }
        public string AttachmentPaths { get; set; }
        
        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation Properties
        public virtual Patient Patient { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual Encounter Encounter { get; set; }
        public virtual ICollection<LabResult> Results { get; set; }
    }
}
