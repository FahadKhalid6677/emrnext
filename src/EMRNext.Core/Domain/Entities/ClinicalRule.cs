using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities
{
    public class ClinicalRule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // Preventive, Diagnostic, Treatment, etc.
        public string SpecialtyType { get; set; }
        public string TriggerEvent { get; set; } // When to evaluate: Encounter, OrderEntry, ResultReceived, etc.
        public string Condition { get; set; } // JSON-structured condition logic
        public string Recommendation { get; set; }
        public string Evidence { get; set; }
        public string EvidenceLevel { get; set; } // A, B, C, D
        public string Severity { get; set; } // Info, Warning, Critical
        public int Priority { get; set; }
        public bool RequiresAcknowledgment { get; set; }
        public bool AllowOverride { get; set; }
        public string OverrideReason { get; set; }
        
        // Version Control
        public int Version { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsActive { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        
        // Navigation Properties
        public virtual ICollection<RuleEvaluation> Evaluations { get; set; }
        public virtual ICollection<RuleOverride> Overrides { get; set; }
    }
}
