using System;

namespace EMRNext.Core.Domain.Entities
{
    public class Problem
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Description { get; set; }
        public DateTime OnsetDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string Status { get; set; } // Active, Inactive, Resolved
        public string Severity { get; set; }
        public string Course { get; set; } // Improving, Worsening, Stable
        public string Type { get; set; } // Condition, Symptom, Finding, Complaint
        public bool IsPrimary { get; set; }
        public bool IsChronicCondition { get; set; }
        public string Notes { get; set; }
        
        // ICD-10 and SNOMED CT Coding
        public string ICD10Code { get; set; }
        public string ICD10Description { get; set; }
        public string SNOMEDCode { get; set; }
        public string SNOMEDDescription { get; set; }

        // Clinical Information
        public string ClinicalStatus { get; set; }
        public string VerificationStatus { get; set; }
        public string Category { get; set; }
        public string Stage { get; set; }
        public string Evidence { get; set; }

        // Related Information
        public int? RelatedEncounterId { get; set; }
        public string TreatingProvider { get; set; }
        public string CarePlan { get; set; }
        public string Goals { get; set; }

        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Patient Patient { get; set; }
        public virtual Encounter RelatedEncounter { get; set; }
    }
}
