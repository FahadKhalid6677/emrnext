using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMRNext.Core.Domain.Entities.Analytics
{
    /// <summary>
    /// Comprehensive patient health profile for predictive analytics
    /// </summary>
    public class PatientHealthProfile
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        /// <summary>
        /// Comprehensive health risk score
        /// </summary>
        public double OverallHealthRiskScore { get; set; }

        /// <summary>
        /// Predicted health trajectory
        /// </summary>
        public HealthTrajectory PredictedTrajectory { get; set; }

        /// <summary>
        /// Chronic condition risk factors
        /// </summary>
        public List<ChronicConditionRisk> ChronicConditionRisks { get; set; }

        /// <summary>
        /// Lifestyle and environmental risk factors
        /// </summary>
        public List<RiskFactor> RiskFactors { get; set; }

        /// <summary>
        /// Predictive model metadata
        /// </summary>
        public PredictiveModelMetadata ModelMetadata { get; set; }

        /// <summary>
        /// Historical health trend data
        /// </summary>
        public List<HealthTrendDataPoint> HealthTrends { get; set; }
    }

    /// <summary>
    /// Represents a predicted health trajectory
    /// </summary>
    public class HealthTrajectory
    {
        public Guid Id { get; set; }

        [Required]
        public Guid PatientHealthProfileId { get; set; }

        public HealthTrajectoryType TrajectoryType { get; set; }

        public double ProgressionProbability { get; set; }

        public DateTime PredictionDate { get; set; }

        public List<TrajectoryDataPoint> TrajectoryPoints { get; set; }
    }

    /// <summary>
    /// Represents a risk factor for chronic conditions
    /// </summary>
    public class ChronicConditionRisk
    {
        public Guid Id { get; set; }

        [Required]
        public Guid PatientHealthProfileId { get; set; }

        [StringLength(100)]
        public string ConditionName { get; set; }

        public double RiskScore { get; set; }

        public RiskLevel RiskLevel { get; set; }

        public List<RiskIndicator> Indicators { get; set; }
    }

    /// <summary>
    /// Represents a general risk factor
    /// </summary>
    public class RiskFactor
    {
        public Guid Id { get; set; }

        [Required]
        public Guid PatientHealthProfileId { get; set; }

        [StringLength(100)]
        public string FactorName { get; set; }

        public double Weight { get; set; }

        public RiskLevel Impact { get; set; }
    }

    /// <summary>
    /// Metadata for predictive models
    /// </summary>
    public class PredictiveModelMetadata
    {
        public Guid Id { get; set; }

        [Required]
        public Guid PatientHealthProfileId { get; set; }

        [StringLength(100)]
        public string ModelVersion { get; set; }

        public DateTime TrainingDate { get; set; }

        public double ModelAccuracy { get; set; }

        public string ModelType { get; set; }
    }

    /// <summary>
    /// Represents a single health trend data point
    /// </summary>
    public class HealthTrendDataPoint
    {
        public Guid Id { get; set; }

        [Required]
        public Guid PatientHealthProfileId { get; set; }

        public DateTime Timestamp { get; set; }

        [StringLength(100)]
        public string MetricName { get; set; }

        public double Value { get; set; }
    }

    /// <summary>
    /// Represents a point in a health trajectory
    /// </summary>
    public class TrajectoryDataPoint
    {
        public Guid Id { get; set; }

        [Required]
        public Guid HealthTrajectoryId { get; set; }

        public DateTime Timestamp { get; set; }

        [StringLength(100)]
        public string MetricName { get; set; }

        public double PredictedValue { get; set; }

        public double Confidence { get; set; }
    }

    /// <summary>
    /// Represents the type of health trajectory
    /// </summary>
    public enum HealthTrajectoryType
    {
        Improving = 1,
        Stable = 2,
        Declining = 3,
        Critical = 4
    }

    /// <summary>
    /// Represents the level of risk
    /// </summary>
    public enum RiskLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// Represents a specific risk indicator
    /// </summary>
    public class RiskIndicator
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ChronicConditionRiskId { get; set; }

        [StringLength(100)]
        public string IndicatorName { get; set; }

        public double Value { get; set; }

        public bool IsSignificant { get; set; }
    }
}
