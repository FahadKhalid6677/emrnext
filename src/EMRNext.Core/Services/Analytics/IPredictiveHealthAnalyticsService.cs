using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities.Analytics;

namespace EMRNext.Core.Services.Analytics
{
    /// <summary>
    /// Service for advanced predictive health analytics
    /// </summary>
    public interface IPredictiveHealthAnalyticsService
    {
        /// <summary>
        /// Generate a comprehensive health profile with advanced risk prediction
        /// </summary>
        Task<PatientHealthProfile> GenerateComprehensiveHealthProfile(Patient patient);

        /// <summary>
        /// Predict potential health trajectories with advanced machine learning
        /// </summary>
        Task<HealthTrajectory> PredictHealthTrajectoryAsync(Guid patientId);

        /// <summary>
        /// Perform advanced risk factor analysis
        /// </summary>
        Task<IEnumerable<HealthRiskFactor>> AnalyzeRiskFactorsAsync(Guid patientId);

        /// <summary>
        /// Generate personalized health recommendations based on risk assessment
        /// </summary>
        Task<IEnumerable<HealthRecommendation>> GeneratePersonalizedRecommendationsAsync(Guid patientId);
    }
}
