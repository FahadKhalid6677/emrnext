using System;

namespace EMRNext.Core.Models
{
    public class BmiResult
    {
        public decimal Bmi { get; set; }
        public string Category { get; set; }
        public string RecommendedRange { get; set; }
    }

    public class BmiPercentileResult
    {
        public decimal Bmi { get; set; }
        public decimal Percentile { get; set; }
        public string Category { get; set; }
    }

    public class VitalTrend
    {
        public string TrendDirection { get; set; } // Increasing, Decreasing, Stable
        public decimal ChangeRate { get; set; }
        public string Interpretation { get; set; }
        public DateTime TrendStartDate { get; set; }
        public DateTime TrendEndDate { get; set; }
    }

    public class VitalRange
    {
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public string Unit { get; set; }
        public string AgeGroup { get; set; }
        public string Gender { get; set; }
    }
}
