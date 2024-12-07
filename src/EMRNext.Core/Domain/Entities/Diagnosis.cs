using System;

namespace EMRNext.Core.Domain.Entities
{
    public class Diagnosis
    {
        public int Id { get; set; }
        public int EncounterId { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }

        // Diagnosis Details
        public string Code { get; set; } // ICD-10 code
        public string CodeSystem { get; set; } // e.g., "ICD10"
        public string Description { get; set; }
        public DateTime DateDiagnosed { get; set; }
        public DateTime? DateResolved { get; set; }
        public string Type { get; set; } // Medical, Dental, etc.
        public string Category { get; set; } // Primary, Secondary, etc.
        public string Status { get; set; } // Active, Resolved, Ruled Out, etc.
        public string Severity { get; set; } // Mild, Moderate, Severe
        public string Stage { get; set; }
        public string Verification { get; set; } // Confirmed, Provisional, etc.
        public bool IsChronic { get; set; }
        public bool IsDeferred { get; set; }
        
        // Clinical Notes
        public string ClinicalNotes { get; set; }
        public string TreatmentNotes { get; set; }
        public string Complications { get; set; }
        public string Prognosis { get; set; }

        // Problem List Management
        public bool OnProblemList { get; set; }
        public DateTime? ProblemListDate { get; set; }
        public string ProblemListStatus { get; set; }
        
        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Encounter Encounter { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual Provider Provider { get; set; }
    }
}
