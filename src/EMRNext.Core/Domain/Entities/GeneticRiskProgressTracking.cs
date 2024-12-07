using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMRNext.Core.Domain.Entities
{
    /// <summary>
    /// Tracks the progress and evolution of a patient's genetic risk assessment
    /// </summary>
    public class GeneticRiskProgressTracking : BaseIntEntity
    {
        /// <summary>
        /// Patient ID this tracking record belongs to
        /// </summary>
        [Required]
        public Guid PatientId { get; set; }

        /// <summary>
        /// Reference to the Patient entity
        /// </summary>
        public Patient Patient { get; set; }

        /// <summary>
        /// Timestamp of the risk assessment
        /// </summary>
        [Required]
        public DateTime AssessmentDate { get; set; }

        /// <summary>
        /// Overall risk severity at the time of assessment
        /// </summary>
        public RiskSeverity RiskSeverity { get; set; }

        /// <summary>
        /// Detailed risk factors at the time of assessment
        /// </summary>
        public Patient.MedicalRiskProfile RiskProfile { get; set; }

        /// <summary>
        /// Recommendations provided during this assessment
        /// </summary>
        public List<string> Recommendations { get; set; } = new List<string>();

        /// <summary>
        /// Tracking of lifestyle and medical interventions
        /// </summary>
        public List<RiskInterventionProgress> Interventions { get; set; } = new List<RiskInterventionProgress>();

        /// <summary>
        /// Changes in risk factors compared to previous assessment
        /// </summary>
        public RiskFactorChanges RiskFactorTrends { get; set; }
    }

    /// <summary>
    /// Tracks individual intervention progress for risk mitigation
    /// </summary>
    public class RiskInterventionProgress
    {
        /// <summary>
        /// Description of the intervention
        /// </summary>
        public string InterventionDescription { get; set; }

        /// <summary>
        /// Date the intervention was recommended
        /// </summary>
        public DateTime RecommendedDate { get; set; }

        /// <summary>
        /// Date the intervention was initiated
        /// </summary>
        public DateTime? InitiatedDate { get; set; }

        /// <summary>
        /// Current status of the intervention
        /// </summary>
        public InterventionStatus Status { get; set; }

        /// <summary>
        /// Notes or comments about the intervention progress
        /// </summary>
        public string ProgressNotes { get; set; }
    }

    /// <summary>
    /// Represents changes in risk factors over time
    /// </summary>
    public class RiskFactorChanges
    {
        /// <summary>
        /// Change in Diabetes Risk
        /// </summary>
        public double DiabetesRiskChange { get; set; }

        /// <summary>
        /// Change in Cardiovascular Risk
        /// </summary>
        public double CardiovascularRiskChange { get; set; }

        /// <summary>
        /// Change in Cancer Risk
        /// </summary>
        public double CancerRiskChange { get; set; }

        /// <summary>
        /// Determines the overall trend of risk changes
        /// </summary>
        public RiskTrend GetOverallTrend()
        {
            var averageChange = (DiabetesRiskChange + CardiovascularRiskChange + CancerRiskChange) / 3;

            return averageChange switch
            {
                double change when change < -0.1 => RiskTrend.Improving,
                double change when change > 0.1 => RiskTrend.Worsening,
                _ => RiskTrend.Stable
            };
        }
    }

    /// <summary>
    /// Status of a specific intervention
    /// </summary>
    public enum InterventionStatus
    {
        Recommended,
        Initiated,
        InProgress,
        Completed,
        Discontinued
    }

    /// <summary>
    /// Trend of risk factors over time
    /// </summary>
    public enum RiskTrend
    {
        Improving,
        Stable,
        Worsening
    }
}
