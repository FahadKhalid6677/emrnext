using System;

namespace EMRNext.Core.Domain.Entities
{
    public class FamilyHistory
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Relationship { get; set; }
        public string RelativeFirstName { get; set; }
        public string RelativeLastName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public string CauseOfDeath { get; set; }
        
        // Condition Information
        public string Condition { get; set; }
        public string ICD10Code { get; set; }
        public string SNOMEDCode { get; set; }
        public string Status { get; set; }
        public string AgeAtOnset { get; set; }
        public string AgeAtDeath { get; set; }
        public string Severity { get; set; }
        public bool IsGeneticRisk { get; set; }
        
        // Additional Information
        public string Notes { get; set; }
        public string Source { get; set; }
        public bool IsVerified { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }

        // Genetic Information
        public bool GeneticTestingDone { get; set; }
        public string GeneticTestResults { get; set; }
        public string GeneticMarkers { get; set; }
        public DateTime? TestDate { get; set; }
        public string TestLaboratory { get; set; }

        // Risk Assessment
        public string RiskLevel { get; set; }
        public string RiskFactors { get; set; }
        public string PreventiveRecommendations { get; set; }
        public string ScreeningGuidelines { get; set; }

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
