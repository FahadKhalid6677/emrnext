using EMRNext.Core.Interfaces;
using EMRNext.Core.Models;

namespace EMRNext.Infrastructure.Services
{
    public class BasicHealthRiskPredictor : IHealthRiskPredictor
    {
        public HealthRiskAssessment PredictRisk(HealthData healthData)
        {
            if (healthData == null)
            {
                throw new ArgumentNullException(nameof(healthData));
            }

            // Implement basic risk assessment logic
            // This is a simplified example - in a real application, this would involve
            // more sophisticated analysis of the health data
            var riskLevel = CalculateRiskLevel(healthData);
            
            return new HealthRiskAssessment
            {
                PatientId = healthData.PatientId,
                RiskLevel = riskLevel,
                AssessmentDate = DateTime.UtcNow,
                Recommendations = GenerateRecommendations(riskLevel)
            };
        }

        private RiskLevel CalculateRiskLevel(HealthData healthData)
        {
            // Simple risk calculation based on vital signs
            // This is a basic example - real implementation would be more comprehensive
            int riskFactors = 0;

            if (healthData.BloodPressureSystolic > 140 || healthData.BloodPressureDiastolic > 90)
                riskFactors++;

            if (healthData.HeartRate > 100 || healthData.HeartRate < 60)
                riskFactors++;

            if (healthData.Temperature > 38.0m || healthData.Temperature < 36.0m)
                riskFactors++;

            return riskFactors switch
            {
                0 => RiskLevel.Low,
                1 => RiskLevel.Medium,
                _ => RiskLevel.High
            };
        }

        private string GenerateRecommendations(RiskLevel riskLevel)
        {
            return riskLevel switch
            {
                RiskLevel.Low => "Continue regular health monitoring. No immediate action required.",
                RiskLevel.Medium => "Schedule a follow-up appointment. Monitor vital signs more frequently.",
                RiskLevel.High => "Immediate medical attention recommended. Contact healthcare provider.",
                _ => throw new ArgumentException("Invalid risk level", nameof(riskLevel))
            };
        }
    }
}
