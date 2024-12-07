using System;
using System.Collections.Generic;

namespace EMRNext.Core.Models
{
    public class ClinicalGuideline
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public List<ClinicalRecommendation> Recommendations { get; set; }
        public List<string> ApplicableConditions { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ClinicalRecommendation
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public RecommendationLevel Level { get; set; }
        public List<string> EvidenceSources { get; set; }
    }

    public class PatientRiskAssessment
    {
        public Guid PatientId { get; set; }
        public List<RiskFactor> RiskFactors { get; set; }
        public RiskLevel OverallRiskLevel { get; set; }
        public List<string> RecommendedInterventions { get; set; }
        public DateTime AssessmentDate { get; set; }
    }

    public class RiskFactor
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public string Description { get; set; }
    }

    public enum RecommendationLevel
    {
        Strong,
        Moderate,
        Weak,
        Conditional
    }

    public enum RiskLevel
    {
        Low,
        Medium,
        High,
        VeryHigh
    }

    public class AlertNotification
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public AlertSeverity Severity { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum AlertSeverity
    {
        Information,
        Warning,
        Critical
    }
}
