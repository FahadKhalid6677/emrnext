using EMRNext.Core.Models;

namespace EMRNext.Core.Interfaces
{
    public interface IPredictiveHealthAnalyticsService
    {
        Task<HealthRiskAssessment> PredictHealthRisk(HealthData healthData);
    }
}
