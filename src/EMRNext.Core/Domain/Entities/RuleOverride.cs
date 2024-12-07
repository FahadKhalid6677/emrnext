using System;

namespace EMRNext.Core.Domain.Entities
{
    public class RuleOverride
    {
        public int Id { get; set; }
        public int ClinicalRuleId { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int? EncounterId { get; set; }
        
        // Override Details
        public DateTime OverrideTime { get; set; }
        public string OverrideReason { get; set; }
        public string AdditionalNotes { get; set; }
        public string Justification { get; set; }
        public bool RequiresReview { get; set; }
        public DateTime? ReviewDue { get; set; }
        
        // Review Process
        public bool IsReviewed { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ReviewedBy { get; set; }
        public string ReviewNotes { get; set; }
        public string ReviewOutcome { get; set; }
        
        // Duration
        public DateTime? ExpirationDate { get; set; }
        public bool IsExpired { get; set; }
        public string ExpirationReason { get; set; }
        
        // Audit
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        
        // Navigation Properties
        public virtual ClinicalRule ClinicalRule { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual Encounter Encounter { get; set; }
    }
}
