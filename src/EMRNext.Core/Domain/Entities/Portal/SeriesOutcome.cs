using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Core.Domain.Entities.Portal
{
    public class SeriesOutcome : AuditableEntity
    {
        public SeriesOutcome()
        {
            Details = new HashSet<OutcomeDetail>();
        }

        public Guid Id { get; set; }

        [Required]
        public Guid GroupSeriesId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        [StringLength(50)]
        public string OutcomeType { get; set; }  // e.g., Clinical, Educational, Behavioral

        [Required]
        [StringLength(100)]
        public string MeasurementTool { get; set; }

        [Required]
        public DateTime MeasurementDate { get; set; }

        [Required]
        [StringLength(100)]
        public string MeasuredBy { get; set; }

        [Required]
        public decimal Score { get; set; }

        [StringLength(500)]
        public string ScoreInterpretation { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }

        public bool IsBaselineMeasurement { get; set; }

        public bool IsFinalMeasurement { get; set; }

        [StringLength(100)]
        public string ComparisonToBaseline { get; set; }

        public decimal? PercentageImprovement { get; set; }

        [StringLength(500)]
        public string ClinicalSignificance { get; set; }

        [StringLength(1000)]
        public string RecommendedActions { get; set; }

        public virtual GroupSeries GroupSeries { get; set; }
        public virtual ICollection<OutcomeDetail> Details { get; set; }
    }

    public class OutcomeDetail : AuditableEntity
    {
        public Guid Id { get; set; }

        [Required]
        public Guid SeriesOutcomeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [Required]
        [StringLength(100)]
        public string Metric { get; set; }

        [Required]
        public decimal Value { get; set; }

        [Required]
        [StringLength(20)]
        public string Unit { get; set; }

        [StringLength(500)]
        public string Interpretation { get; set; }

        public bool IsAbnormal { get; set; }

        [StringLength(100)]
        public string ReferenceRange { get; set; }

        public virtual SeriesOutcome SeriesOutcome { get; set; }
    }
}
