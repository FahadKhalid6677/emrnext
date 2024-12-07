using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities
{
    public class CarePlan
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int? EncounterId { get; set; }
        
        // Plan Details
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string Intent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Clinical Information
        public string PrimaryDiagnosis { get; set; }
        public string DiagnosisCode { get; set; }
        public string SecondaryDiagnoses { get; set; }
        public string ClinicalStatus { get; set; }
        public string Severity { get; set; }
        public string Stage { get; set; }
        
        // Goals
        public string Goals { get; set; }
        public string Outcomes { get; set; }
        public string ProgressNotes { get; set; }
        public string Barriers { get; set; }
        
        // Interventions
        public string Interventions { get; set; }
        public string TreatmentPlan { get; set; }
        public string Medications { get; set; }
        public string Procedures { get; set; }
        public string Activities { get; set; }
        
        // Care Team
        public string CareTeamMembers { get; set; }
        public string PrimaryCareProvider { get; set; }
        public string Specialists { get; set; }
        public string CareCoordinator { get; set; }
        
        // Patient Engagement
        public string PatientPreferences { get; set; }
        public string PatientGoals { get; set; }
        public string SupportSystem { get; set; }
        public string Barriers { get; set; }
        public string EducationNeeds { get; set; }
        
        // Monitoring
        public string MonitoringParameters { get; set; }
        public string AssessmentSchedule { get; set; }
        public string FollowUpSchedule { get; set; }
        public DateTime? NextAssessmentDate { get; set; }
        public string AlertTriggers { get; set; }
        
        // Resources
        public string EducationMaterials { get; set; }
        public string SupportServices { get; set; }
        public string CommunityResources { get; set; }
        public string ReferralNeeds { get; set; }
        
        // Progress Tracking
        public string ProgressStatus { get; set; }
        public string Achievements { get; set; }
        public string Challenges { get; set; }
        public string ModificationHistory { get; set; }
        
        // Review Information
        public DateTime? LastReviewDate { get; set; }
        public string ReviewedBy { get; set; }
        public string ReviewNotes { get; set; }
        public DateTime? NextReviewDate { get; set; }
        
        // Documentation
        public string DocumentationPath { get; set; }
        public string AttachmentPaths { get; set; }
        public string ConsentPath { get; set; }
        
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
        public virtual ICollection<CarePlanActivity> Activities { get; set; }
    }
}
