using System;

namespace EMRNext.Core.Domain.Entities
{
    public class RuleEvaluation
    {
        public int Id { get; set; }
        public int ClinicalRuleId { get; set; }
        public int PatientId { get; set; }
        public int? EncounterId { get; set; }
        public int? ProviderId { get; set; }
        
        // Evaluation Details
        public DateTime EvaluationTime { get; set; }
        public string TriggerContext { get; set; }
        public string EvaluationResult { get; set; } // Triggered, Not Triggered
        public string ContextData { get; set; } // JSON data used in evaluation
        public string AlertMessage { get; set; }
        public string ActionTaken { get; set; } // Accepted, Rejected, Override
        public DateTime? ActionTime { get; set; }
        public string ActionBy { get; set; }
        public string OverrideReason { get; set; }
        
        // Notification Management
        public bool NotificationSent { get; set; }
        public DateTime? NotificationTime { get; set; }
        public string NotificationChannel { get; set; }
        public string NotificationStatus { get; set; }
        
        // Audit
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        
        // Navigation Properties
        public virtual ClinicalRule ClinicalRule { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual Encounter Encounter { get; set; }
        public virtual Provider Provider { get; set; }
    }
}
