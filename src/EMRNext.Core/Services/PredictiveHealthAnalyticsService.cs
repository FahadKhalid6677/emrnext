using EMRNext.Core.Interfaces;
using EMRNext.Core.Models;

namespace EMRNext.Core.Services
{
    public class PredictiveHealthAnalyticsService : IPredictiveHealthAnalyticsService
    {
        private readonly IHealthRiskPredictor _healthRiskPredictor;

        public PredictiveHealthAnalyticsService(IHealthRiskPredictor healthRiskPredictor)
        {
            _healthRiskPredictor = healthRiskPredictor ?? throw new ArgumentNullException(nameof(healthRiskPredictor));
        }

        public async Task<HealthRiskAssessment> PredictHealthRisk(HealthData healthData)
        {
            if (healthData == null)
            {
                throw new ArgumentNullException(nameof(healthData));
            }

            // Perform any async validations or data enrichment here
            await Task.CompletedTask;

            // Use the predictor to get the risk assessment
            return _healthRiskPredictor.PredictRisk(healthData);
        }
    }
}
