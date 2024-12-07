using System;
using System.Collections.Generic;

namespace EMRNext.Core.Models.Growth
{
    public enum MeasurementType
    {
        Weight,
        Height,
        HeadCircumference,
        BMI,
        WeightForHeight
    }

    public enum GrowthStandardType
    {
        WHO,
        CDC
    }

    public class GrowthStandard
    {
        public int Id { get; set; }
        public GrowthStandardType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<PercentileDefinition> Percentiles { get; set; }
        public List<ChartDefinition> Charts { get; set; }
    }

    public class PercentileDefinition
    {
        public int Id { get; set; }
        public decimal Value { get; set; }  // e.g., 3, 5, 10, 25, 50, 75, 90, 95, 97
        public string Label { get; set; }
        public string Color { get; set; }
        public bool IsMainLine { get; set; }
        public string LineStyle { get; set; }
    }

    public class ChartDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public MeasurementType MeasurementType { get; set; }
        public string Gender { get; set; }
        public int MinAgeMonths { get; set; }
        public int MaxAgeMonths { get; set; }
        public string XAxisLabel { get; set; }
        public string YAxisLabel { get; set; }
        public string Unit { get; set; }
        public List<PercentileCurve> PercentileCurves { get; set; }
    }

    public class PercentileCurve
    {
        public int Id { get; set; }
        public decimal Percentile { get; set; }
        public List<DataPoint> Points { get; set; }
    }

    public class DataPoint
    {
        public int AgeMonths { get; set; }
        public decimal Value { get; set; }
        public decimal L { get; set; }  // Box-Cox power
        public decimal M { get; set; }  // Median
        public decimal S { get; set; }  // Coefficient of variation
    }

    public class GrowthMeasurement
    {
        public int PatientId { get; set; }
        public DateTime MeasurementDate { get; set; }
        public MeasurementType Type { get; set; }
        public decimal Value { get; set; }
        public string Unit { get; set; }
        public decimal Percentile { get; set; }
        public decimal ZScore { get; set; }
        public string Interpretation { get; set; }
        public string MeasuredBy { get; set; }
        public string Notes { get; set; }
    }

    public class GrowthChartData
    {
        public ChartDefinition ChartDefinition { get; set; }
        public List<GrowthMeasurement> PatientMeasurements { get; set; }
        public Dictionary<decimal, List<DataPoint>> PercentileLines { get; set; }
        public List<GrowthAlert> Alerts { get; set; }
    }

    public class GrowthAlert
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }  // e.g., "CrossingPercentiles", "RapidChange", "OutOfRange"
        public string Severity { get; set; }
        public string Message { get; set; }
        public string Recommendation { get; set; }
    }

    public class GrowthVelocity
    {
        public MeasurementType Type { get; set; }
        public decimal ChangeValue { get; set; }
        public string ChangeUnit { get; set; }
        public decimal TimeSpan { get; set; }
        public string TimeUnit { get; set; }
        public decimal VelocityValue { get; set; }
        public string VelocityUnit { get; set; }
        public decimal Percentile { get; set; }
        public string Interpretation { get; set; }
    }
}
