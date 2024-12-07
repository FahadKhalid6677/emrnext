using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using EMRNext.Core.Domain.Entities.Analytics;
using EMRNext.Core.Domain.Entities.Patient;
using EMRNext.Core.Services.Analytics;

namespace EMRNext.Core.Analytics.MachineLearning
{
    /// <summary>
    /// Advanced machine learning predictor for comprehensive health risk assessment
    /// </summary>
    public class HealthRiskPredictor
    {
        private readonly MLContext _mlContext;
        private ITransformer _riskModel;
        private readonly ILogger<HealthRiskPredictor> _logger;

        public HealthRiskPredictor(ILogger<HealthRiskPredictor> logger)
        {
            _logger = logger;
            _mlContext = new MLContext(seed: 0);
            _logger.LogInformation("HealthRiskPredictor initialized with ML.NET version {Version}", typeof(MLContext).Assembly.GetName().Version);
        }

        /// <summary>
        /// Train a predictive model for health risk assessment
        /// </summary>
        public void TrainModel(IEnumerable<PatientHealthProfile> historicalProfiles)
        {
            try 
            {
                if (historicalProfiles == null || !historicalProfiles.Any())
                {
                    _logger.LogWarning("No historical profiles provided for model training");
                    throw new ArgumentException("Historical profiles cannot be null or empty", nameof(historicalProfiles));
                }

                _logger.LogInformation("Starting model training with {ProfileCount} historical profiles", historicalProfiles.Count());

                var trainingData = _mlContext.Data.LoadFromEnumerable(historicalProfiles);

                var pipeline = _mlContext.Transforms.Concatenate(
                    "Features", 
                    nameof(PatientHealthProfile.Age),
                    nameof(PatientHealthProfile.BMI),
                    nameof(PatientHealthProfile.ChronicConditionScore),
                    nameof(PatientHealthProfile.LifestyleRiskScore)
                )
                .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: nameof(PatientHealthProfile.PredictedHealthRisk)));

                _riskModel = pipeline.Fit(trainingData);
                _logger.LogInformation("Model training completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during model training");
                throw;
            }
        }

        /// <summary>
        /// Predict health risk for a given patient profile
        /// </summary>
        public double PredictHealthRisk(PatientHealthProfile patientProfile)
        {
            try 
            {
                if (_riskModel == null)
                    throw new InvalidOperationException("Model must be trained before prediction");

                if (patientProfile == null)
                    throw new ArgumentNullException(nameof(patientProfile), "Patient profile cannot be null");

                var predictionEngine = _mlContext.Model.CreatePredictionEngine<PatientHealthProfile, HealthRiskPrediction>(_riskModel);
                var prediction = predictionEngine.Predict(patientProfile);

                _logger.LogInformation("Health risk prediction completed. Predicted Risk: {PredictedRisk}", prediction.PredictedHealthRisk);

                return prediction.PredictedHealthRisk;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during health risk prediction");
                throw;
            }
        }

        /// <summary>
        /// Perform advanced risk factor analysis
        /// </summary>
        public Dictionary<string, double> AnalyzeRiskFactors(PatientHealthProfile patientProfile)
        {
            return new Dictionary<string, double>
            {
                { "Cardiovascular Risk", CalculateCardiovascularRisk(patientProfile) },
                { "Metabolic Syndrome Risk", CalculateMetabolicRisk(patientProfile) },
                { "Lifestyle Impact", CalculateLifestyleRisk(patientProfile) }
            };
        }

        private double CalculateCardiovascularRisk(PatientHealthProfile profile)
        {
            // Complex cardiovascular risk calculation
            return (profile.BMI * 0.3) + 
                   (profile.Age * 0.2) + 
                   (profile.ChronicConditionScore * 0.5);
        }

        private double CalculateMetabolicRisk(PatientHealthProfile profile)
        {
            // Metabolic syndrome risk assessment
            return (profile.BMI * 0.4) + 
                   (profile.LifestyleRiskScore * 0.6);
        }

        private double CalculateLifestyleRisk(PatientHealthProfile profile)
        {
            // Lifestyle impact on health risk
            return profile.LifestyleRiskScore;
        }
    }

    /// <summary>
    /// Prediction output class for health risk
    /// </summary>
    public class HealthRiskPrediction
    {
        [ColumnName("Score")]
        public double PredictedHealthRisk { get; set; }
    }
}
