using System;

namespace EMRNext.Core.Domain.Entities
{
    public class ClinicalNote
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int? EncounterId { get; set; }
        
        // Note Details
        public string Type { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime NoteDate { get; set; }
        public string Version { get; set; }
        
        // Clinical Context
        public string VisitType { get; set; }
        public string Department { get; set; }
        public string Specialty { get; set; }
        public string ServiceLocation { get; set; }
        
        // Note Structure
        public string ChiefComplaint { get; set; }
        public string SubjectiveNotes { get; set; }
        public string ObjectiveNotes { get; set; }
        public string Assessment { get; set; }
        public string Plan { get; set; }
        public string Instructions { get; set; }
        
        // Clinical Data
        public string Diagnoses { get; set; }
        public string Procedures { get; set; }
        public string Medications { get; set; }
        public string Allergies { get; set; }
        public string VitalSigns { get; set; }
        public string LabResults { get; set; }
        
        // Documentation
        public string TemplateUsed { get; set; }
        public bool IsSigned { get; set; }
        public DateTime? SignedDate { get; set; }
        public string SignedBy { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockedDate { get; set; }
        
        // Cosign Information
        public bool RequiresCosign { get; set; }
        public string CosignProvider { get; set; }
        public DateTime? CosignDate { get; set; }
        public string CosignNotes { get; set; }
        
        // Amendments
        public bool IsAmended { get; set; }
        public DateTime? AmendedDate { get; set; }
        public string AmendedBy { get; set; }
        public string AmendmentReason { get; set; }
        public string AmendmentNotes { get; set; }
        
        // References
        public string References { get; set; }
        public string AttachmentPaths { get; set; }
        public string RelatedNotes { get; set; }
        
        // Billing/Coding
        public string BillingCodes { get; set; }
        public string CPTCodes { get; set; }
        public string ICDCodes { get; set; }
        public string CodingNotes { get; set; }
        
        // Quality Metrics
        public bool MeetsQualityMetrics { get; set; }
        public string QualityMetricsNotes { get; set; }
        public string ComplianceStatus { get; set; }
        public string ComplianceNotes { get; set; }
        
        // Access Control
        public string AccessLevel { get; set; }
        public string RestrictedTo { get; set; }
        public bool IsConfidential { get; set; }
        public string ConfidentialityNotes { get; set; }
        
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
    }
}
