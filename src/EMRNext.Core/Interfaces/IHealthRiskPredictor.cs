using EMRNext.Core.Models;

namespace EMRNext.Core.Interfaces
{
    public interface IHealthRiskPredictor
    {
        HealthRiskAssessment PredictRisk(HealthData healthData);
    }
}
