using System;
using System.Collections.Generic;

namespace EMRNext.Core.Models
{
    public class VitalSignsRecord
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; }
        public DateTime RecordedAt { get; set; }
        public string RecordedBy { get; set; }
        
        // Core Vital Signs
        public TemperatureReading Temperature { get; set; }
        public BloodPressureReading BloodPressure { get; set; }
        public HeartRateReading HeartRate { get; set; }
        public RespiratoryRateReading RespiratoryRate { get; set; }
        public OxygenSaturationReading OxygenSaturation { get; set; }
        
        // Additional Measurements
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? BMI { get; set; }
        
        // Clinical Assessments
        public VitalSignsAssessment Assessment { get; set; }
        public List<VitalSignAlert> Alerts { get; set; }
    }

    public class TemperatureReading
    {
        public decimal Value { get; set; }
        public TemperatureUnit Unit { get; set; }
        public TemperatureStatus Status { get; set; }
    }

    public class BloodPressureReading
    {
        public int SystolicPressure { get; set; }
        public int DiastolicPressure { get; set; }
        public BloodPressureCategory Category { get; set; }
    }

    public class HeartRateReading
    {
        public int BeatsPerMinute { get; set; }
        public HeartRateCategory Category { get; set; }
    }

    public class RespiratoryRateReading
    {
        public int BreathsPerMinute { get; set; }
        public RespiratoryRateCategory Category { get; set; }
    }

    public class OxygenSaturationReading
    {
        public decimal Percentage { get; set; }
        public OxygenSaturationStatus Status { get; set; }
    }

    public class VitalSignsAssessment
    {
        public VitalSignsRiskLevel RiskLevel { get; set; }
        public string ClinicalInterpretation { get; set; }
        public List<string> RecommendedActions { get; set; }
    }

    public class VitalSignAlert
    {
        public Guid Id { get; set; }
        public VitalSignAlertType Type { get; set; }
        public string Description { get; set; }
        public AlertSeverity Severity { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum TemperatureUnit
    {
        Celsius,
        Fahrenheit
    }

    public enum TemperatureStatus
    {
        Normal,
        Hypothermia,
        Fever,
        HighFever
    }

    public enum BloodPressureCategory
    {
        Hypotensive,
        Normal,
        Elevated,
        HypertensionStage1,
        HypertensionStage2,
        HypertensiveCrisis
    }

    public enum HeartRateCategory
    {
        Bradycardia,
        Normal,
        Tachycardia
    }

    public enum RespiratoryRateCategory
    {
        Bradypnea,
        Normal,
        Tachypnea
    }

    public enum OxygenSaturationStatus
    {
        Normal,
        Hypoxemia,
        SevereHypoxemia
    }

    public enum VitalSignsRiskLevel
    {
        Low,
        Moderate,
        High,
        Critical
    }

    public enum VitalSignAlertType
    {
        TemperatureAbnormality,
        BloodPressureAbnormality,
        HeartRateAbnormality,
        RespiratoryRateAbnormality,
        OxygenSaturationAbnormality
    }

    public enum AlertSeverity
    {
        Information,
        Warning,
        Urgent,
        Emergency
    }
}
