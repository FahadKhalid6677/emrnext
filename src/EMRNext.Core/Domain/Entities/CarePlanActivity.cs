using System;

namespace EMRNext.Core.Domain.Entities
{
    public class CarePlanActivity
    {
        public int Id { get; set; }
        public int CarePlanId { get; set; }
        
        // Activity Details
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        
        // Timing
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Frequency { get; set; }
        public string Duration { get; set; }
        public string Schedule { get; set; }
        
        // Goals and Outcomes
        public string Goals { get; set; }
        public string ExpectedOutcomes { get; set; }
        public string ActualOutcomes { get; set; }
        public string ProgressNotes { get; set; }
        
        // Responsibility
        public string AssignedTo { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
        public string Instructions { get; set; }
        
        // Tracking
        public string CompletionStatus { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string CompletedBy { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerificationDate { get; set; }
        
        // Documentation
        public string Notes { get; set; }
        public string Barriers { get; set; }
        public string Modifications { get; set; }
        public string AttachmentPaths { get; set; }
        
        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation Properties
        public virtual CarePlan CarePlan { get; set; }
    }
}
