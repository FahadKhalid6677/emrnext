using System;

namespace EMRNext.Core.Domain.Entities
{
    public class Allergy
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Type { get; set; } // Medication, Food, Environmental
        public string Allergen { get; set; }
        public string Reaction { get; set; }
        public string Severity { get; set; } // Mild, Moderate, Severe
        public DateTime OnsetDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } // Active, Inactive, Resolved
        public string Source { get; set; } // Patient, Provider, External
        public string Notes { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }

        // SNOMED CT Coding
        public string AllergenCode { get; set; }
        public string ReactionCode { get; set; }
        public string SeverityCode { get; set; }

        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Patient Patient { get; set; }
    }
}
