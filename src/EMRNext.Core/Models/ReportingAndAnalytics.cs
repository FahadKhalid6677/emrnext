using System;
using System.Collections.Generic;

namespace EMRNext.Core.Models
{
    public class ClinicalReport
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string ReportType { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Dictionary<string, object> ReportData { get; set; }
        public List<ReportSection> Sections { get; set; }
    }

    public class ReportSection
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public Dictionary<string, object> Metrics { get; set; }
        public List<string> Highlights { get; set; }
    }

    public class PopulationHealthMetrics
    {
        public Guid Id { get; set; }
        public DateTime CalculatedAt { get; set; }
        public int TotalPatients { get; set; }
        public Dictionary<string, int> PatientDemographics { get; set; }
        public Dictionary<string, double> ChronicConditionPrevalence { get; set; }
        public Dictionary<string, double> RiskStratification { get; set; }
    }

    public class PerformanceMetric
    {
        public Guid Id { get; set; }
        public string MetricName { get; set; }
        public string Category { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public DateTime CalculatedAt { get; set; }
        public List<PerformanceTrend> Trends { get; set; }
    }

    public class PerformanceTrend
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }

    public class AuditTrail
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; }
        public string Entity { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; }
    }

    public enum ReportCategory
    {
        Clinical,
        Financial,
        Operational,
        Quality
    }

    public enum AnalyticsType
    {
        Descriptive,
        Diagnostic,
        Predictive,
        Prescriptive
    }
}
