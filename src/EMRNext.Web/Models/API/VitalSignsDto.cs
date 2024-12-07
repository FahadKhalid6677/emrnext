using System;
using System.Collections.Generic;
using EMRNext.Core.Models;

namespace EMRNext.Web.Models.API
{
    public class VitalSignsRecordDto
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; }
        public DateTime RecordedAt { get; set; }
        public string RecordedBy { get; set; }
        
        public TemperatureReadingDto Temperature { get; set; }
        public BloodPressureReadingDto BloodPressure { get; set; }
        public HeartRateReadingDto HeartRate { get; set; }
        public RespiratoryRateReadingDto RespiratoryRate { get; set; }
        public OxygenSaturationReadingDto OxygenSaturation { get; set; }
        
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? BMI { get; set; }
        
        public VitalSignsAssessmentDto Assessment { get; set; }
        public List<VitalSignAlertDto> Alerts { get; set; }
    }

    public class TemperatureReadingDto
    {
        public decimal Value { get; set; }
        public TemperatureUnit Unit { get; set; }
        public TemperatureStatus Status { get; set; }
    }

    public class BloodPressureReadingDto
    {
        public int SystolicPressure { get; set; }
        public int DiastolicPressure { get; set; }
        public BloodPressureCategory Category { get; set; }
    }

    public class HeartRateReadingDto
    {
        public int BeatsPerMinute { get; set; }
        public HeartRateCategory Category { get; set; }
    }

    public class RespiratoryRateReadingDto
    {
        public int BreathsPerMinute { get; set; }
        public RespiratoryRateCategory Category { get; set; }
    }

    public class OxygenSaturationReadingDto
    {
        public decimal Percentage { get; set; }
        public OxygenSaturationStatus Status { get; set; }
    }

    public class VitalSignsAssessmentDto
    {
        public VitalSignsRiskLevel RiskLevel { get; set; }
        public string ClinicalInterpretation { get; set; }
        public List<string> RecommendedActions { get; set; }
    }

    public class VitalSignAlertDto
    {
        public Guid Id { get; set; }
        public VitalSignAlertType Type { get; set; }
        public string Description { get; set; }
        public AlertSeverity Severity { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class VitalSignsTrendDto
    {
        public string PatientId { get; set; }
        public TrendAnalysisDto TemperatureTrend { get; set; }
        public TrendAnalysisDto BloodPressureTrend { get; set; }
        public TrendAnalysisDto HeartRateTrend { get; set; }
        public TrendAnalysisDto RespiratoryRateTrend { get; set; }
        public TrendAnalysisDto OxygenSaturationTrend { get; set; }
    }

    public class TrendAnalysisDto
    {
        public decimal Average { get; set; }
        public TrendDirection Trend { get; set; }
    }
}
