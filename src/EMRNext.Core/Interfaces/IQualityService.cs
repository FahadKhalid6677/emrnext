using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Clinical;
using EMRNext.Core.Models.Laboratory;

namespace EMRNext.Core.Interfaces
{
    public interface IQualityService
    {
        Task<bool> LogQualityIssueAsync(QualityIssue issue);
        Task<bool> UpdateIssueStatusAsync(int issueId, string status, string resolution);
        Task<IEnumerable<QualityIssue>> GetOpenIssuesAsync();
        Task<QualityMetrics> CalculateMetricsAsync(DateTime startDate, DateTime endDate);
        Task<bool> ValidateResultAsync(LabResultEntity result);
        Task<bool> ValidateSampleAsync(LabOrderEntity order);
        Task<bool> TrackTurnaroundTimeAsync(LabOrderEntity order);
        Task<bool> MonitorCriticalValuesAsync(LabResultEntity result);
        Task<byte[]> GenerateQualityReportAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, decimal>> GetPerformanceMetricsAsync();
        Task<bool> PerformQualityAuditAsync(int orderId);
        Task<bool> LogQualityControlResultAsync(QualityControlResult result);
        Task<bool> ValidateInstrumentCalibrationAsync(string instrumentId);
    }

    public class QualityControlResult
    {
        public string InstrumentId { get; set; }
        public string ControlLotNumber { get; set; }
        public DateTime TestDate { get; set; }
        public string TestType { get; set; }
        public decimal ExpectedValue { get; set; }
        public decimal ActualValue { get; set; }
        public bool IsWithinRange { get; set; }
        public string PerformedBy { get; set; }
        public string Comments { get; set; }
    }
}
