using System;
using System.Collections.Generic;

namespace EMRNext.Web.Models.API
{
    public class PatientRiskAssessmentDto
    {
        public Guid PatientId { get; set; }
        public List<RiskFactorDto> RiskFactors { get; set; }
        public string OverallRiskLevel { get; set; }
        public List<string> RecommendedInterventions { get; set; }
        public DateTime AssessmentDate { get; set; }
    }

    public class RiskFactorDto
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string RiskLevel { get; set; }
        public string Description { get; set; }
    }

    public class AlertNotificationDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
