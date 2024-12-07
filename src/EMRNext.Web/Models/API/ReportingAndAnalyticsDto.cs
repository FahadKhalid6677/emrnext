using System;
using System.Collections.Generic;

namespace EMRNext.Web.Models.API
{
    public class ClinicalReportDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string ReportType { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Dictionary<string, object> ReportData { get; set; }
        public List<ReportSectionDto> Sections { get; set; }
    }

    public class ReportSectionDto
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public Dictionary<string, object> Metrics { get; set; }
        public List<string> Highlights { get; set; }
    }

    public class PopulationHealthMetricsDto
    {
        public Guid Id { get; set; }
        public DateTime CalculatedAt { get; set; }
        public int TotalPatients { get; set; }
        public Dictionary<string, int> PatientDemographics { get; set; }
        public Dictionary<string, double> ChronicConditionPrevalence { get; set; }
        public Dictionary<string, double> RiskStratification { get; set; }
    }

    public class PerformanceMetricDto
    {
        public Guid Id { get; set; }
        public string MetricName { get; set; }
        public string Category { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public DateTime CalculatedAt { get; set; }
        public List<PerformanceTrendDto> Trends { get; set; }
    }

    public class PerformanceTrendDto
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }

    public class AuditTrailDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; }
        public string Entity { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; }
    }

    public class ReportRequestDto
    {
        public Guid? PatientId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportCategory { get; set; }
        public string AnalyticsType { get; set; }
    }
}
