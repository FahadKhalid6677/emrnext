using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Models;
using EMRNext.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace EMRNext.Core.Services
{
    public class VitalSignsService
    {
        private readonly IRepository<VitalSignsRecord> _vitalSignsRepository;
        private readonly IRepository<Patient> _patientRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<VitalSignsService> _logger;

        public VitalSignsService(
            IRepository<VitalSignsRecord> vitalSignsRepository,
            IRepository<Patient> patientRepository,
            INotificationService notificationService,
            ILogger<VitalSignsService> logger)
        {
            _vitalSignsRepository = vitalSignsRepository;
            _patientRepository = patientRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<VitalSignsRecord> RecordVitalSignsAsync(VitalSignsRecord vitalSigns)
        {
            try
            {
                // Validate patient
                var patient = await _patientRepository.GetByIdAsync(vitalSigns.PatientId);
                if (patient == null)
                {
                    throw new ArgumentException("Invalid patient ID");
                }

                // Calculate BMI if height and weight are provided
                if (vitalSigns.Height.HasValue && vitalSigns.Weight.HasValue)
                {
                    vitalSigns.BMI = CalculateBMI(vitalSigns.Height.Value, vitalSigns.Weight.Value);
                }

                // Perform comprehensive vital signs assessment
                vitalSigns.Assessment = AssessVitalSigns(vitalSigns);

                // Generate alerts for abnormal readings
                vitalSigns.Alerts = GenerateVitalSignAlerts(vitalSigns);

                // Save vital signs record
                await _vitalSignsRepository.AddAsync(vitalSigns);

                // Send notifications for critical alerts
                await NotifyCriticalAlerts(vitalSigns);

                _logger.LogInformation("Vital signs recorded for patient {PatientId}", vitalSigns.PatientId);

                return vitalSigns;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording vital signs for patient {PatientId}", vitalSigns.PatientId);
                throw;
            }
        }

        private decimal CalculateBMI(decimal height, decimal weight)
        {
            // Assumes height in meters and weight in kilograms
            return Math.Round(weight / (height * height), 2);
        }

        private VitalSignsAssessment AssessVitalSigns(VitalSignsRecord vitalSigns)
        {
            var assessment = new VitalSignsAssessment
            {
                RecommendedActions = new List<string>()
            };

            // Temperature Assessment
            if (vitalSigns.Temperature != null)
            {
                assessment.RecommendedActions.AddRange(AssessTemperature(vitalSigns.Temperature));
            }

            // Blood Pressure Assessment
            if (vitalSigns.BloodPressure != null)
            {
                assessment.RecommendedActions.AddRange(AssessBloodPressure(vitalSigns.BloodPressure));
            }

            // Heart Rate Assessment
            if (vitalSigns.HeartRate != null)
            {
                assessment.RecommendedActions.AddRange(AssessHeartRate(vitalSigns.HeartRate));
            }

            // Respiratory Rate Assessment
            if (vitalSigns.RespiratoryRate != null)
            {
                assessment.RecommendedActions.AddRange(AssessRespiratoryRate(vitalSigns.RespiratoryRate));
            }

            // Oxygen Saturation Assessment
            if (vitalSigns.OxygenSaturation != null)
            {
                assessment.RecommendedActions.AddRange(AssessOxygenSaturation(vitalSigns.OxygenSaturation));
            }

            // Determine overall risk level
            assessment.RiskLevel = DetermineOverallRiskLevel(assessment.RecommendedActions);
            assessment.ClinicalInterpretation = GenerateClinicalInterpretation(assessment);

            return assessment;
        }

        private List<string> AssessTemperature(TemperatureReading temperature)
        {
            var actions = new List<string>();

            switch (temperature.Status)
            {
                case TemperatureStatus.Hypothermia:
                    actions.Add("Initiate warming protocols");
                    actions.Add("Monitor for signs of hypothermia-related complications");
                    break;
                case TemperatureStatus.Fever:
                    actions.Add("Administer fever-reducing medication");
                    actions.Add("Ensure hydration");
                    break;
                case TemperatureStatus.HighFever:
                    actions.Add("Immediate medical evaluation required");
                    actions.Add("Consider antipyretic treatment");
                    actions.Add("Monitor for signs of infection");
                    break;
            }

            return actions;
        }

        private List<string> AssessBloodPressure(BloodPressureReading bloodPressure)
        {
            var actions = new List<string>();

            switch (bloodPressure.Category)
            {
                case BloodPressureCategory.Hypotensive:
                    actions.Add("Monitor for signs of shock");
                    actions.Add("Assess fluid status");
                    break;
                case BloodPressureCategory.Elevated:
                    actions.Add("Lifestyle modification counseling");
                    actions.Add("Consider blood pressure monitoring");
                    break;
                case BloodPressureCategory.HypertensionStage1:
                case BloodPressureCategory.HypertensionStage2:
                    actions.Add("Medication review");
                    actions.Add("Lifestyle modification counseling");
                    actions.Add("Close blood pressure monitoring");
                    break;
                case BloodPressureCategory.HypertensiveCrisis:
                    actions.Add("Immediate medical intervention required");
                    actions.Add("Prepare for potential hospitalization");
                    break;
            }

            return actions;
        }

        private List<string> AssessHeartRate(HeartRateReading heartRate)
        {
            var actions = new List<string>();

            switch (heartRate.Category)
            {
                case HeartRateCategory.Bradycardia:
                    actions.Add("Cardiac rhythm evaluation");
                    actions.Add("Monitor for underlying causes");
                    break;
                case HeartRateCategory.Tachycardia:
                    actions.Add("Investigate potential causes");
                    actions.Add("Cardiac monitoring");
                    break;
            }

            return actions;
        }

        private List<string> AssessRespiratoryRate(RespiratoryRateReading respiratoryRate)
        {
            var actions = new List<string>();

            switch (respiratoryRate.Category)
            {
                case RespiratoryRateCategory.Bradypnea:
                    actions.Add("Assess respiratory function");
                    actions.Add("Monitor for respiratory depression");
                    break;
                case RespiratoryRateCategory.Tachypnea:
                    actions.Add("Investigate potential respiratory issues");
                    actions.Add("Oxygen saturation monitoring");
                    break;
            }

            return actions;
        }

        private List<string> AssessOxygenSaturation(OxygenSaturationReading oxygenSaturation)
        {
            var actions = new List<string>();

            switch (oxygenSaturation.Status)
            {
                case OxygenSaturationStatus.Hypoxemia:
                    actions.Add("Supplemental oxygen consideration");
                    actions.Add("Respiratory function assessment");
                    break;
                case OxygenSaturationStatus.SevereHypoxemia:
                    actions.Add("Immediate oxygen therapy");
                    actions.Add("Urgent medical evaluation");
                    actions.Add("Potential ventilatory support");
                    break;
            }

            return actions;
        }

        private VitalSignsRiskLevel DetermineOverallRiskLevel(List<string> recommendedActions)
        {
            if (recommendedActions.Any(a => a.Contains("Immediate") || a.Contains("Urgent")))
                return VitalSignsRiskLevel.Critical;

            if (recommendedActions.Any(a => a.Contains("medical") || a.Contains("intervention")))
                return VitalSignsRiskLevel.High;

            if (recommendedActions.Any(a => a.Contains("monitoring") || a.Contains("evaluation")))
                return VitalSignsRiskLevel.Moderate;

            return VitalSignsRiskLevel.Low;
        }

        private string GenerateClinicalInterpretation(VitalSignsAssessment assessment)
        {
            return $"Risk Level: {assessment.RiskLevel}. " +
                   $"Recommended actions include: {string.Join(", ", assessment.RecommendedActions)}";
        }

        private List<VitalSignAlert> GenerateVitalSignAlerts(VitalSignsRecord vitalSigns)
        {
            var alerts = new List<VitalSignAlert>();

            if (vitalSigns.Temperature?.Status != TemperatureStatus.Normal)
            {
                alerts.Add(new VitalSignAlert
                {
                    Id = Guid.NewGuid(),
                    Type = VitalSignAlertType.TemperatureAbnormality,
                    Description = $"Abnormal temperature: {vitalSigns.Temperature.Value}°",
                    Severity = MapStatusToSeverity(vitalSigns.Temperature.Status),
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Similar alert generation for other vital signs...

            return alerts;
        }

        private AlertSeverity MapStatusToSeverity(TemperatureStatus status)
        {
            return status switch
            {
                TemperatureStatus.Normal => AlertSeverity.Information,
                TemperatureStatus.Hypothermia => AlertSeverity.Urgent,
                TemperatureStatus.Fever => AlertSeverity.Warning,
                TemperatureStatus.HighFever => AlertSeverity.Emergency,
                _ => AlertSeverity.Information
            };
        }

        private async Task NotifyCriticalAlerts(VitalSignsRecord vitalSigns)
        {
            var criticalAlerts = vitalSigns.Alerts?
                .Where(a => a.Severity == AlertSeverity.Emergency || a.Severity == AlertSeverity.Urgent)
                .ToList();

            if (criticalAlerts?.Any() == true)
            {
                await _notificationService.SendCriticalVitalSignAlertsAsync(
                    vitalSigns.PatientId, 
                    criticalAlerts
                );
            }
        }

        public async Task<List<VitalSignsRecord>> GetPatientVitalSignHistoryAsync(
            string patientId, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                var query = await _vitalSignsRepository.FindAsync(v => v.PatientId == patientId);

                if (startDate.HasValue)
                    query = query.Where(v => v.RecordedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(v => v.RecordedAt <= endDate.Value);

                return query
                    .OrderByDescending(v => v.RecordedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vital signs history for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<VitalSignsTrend> AnalyzeVitalSignTrendsAsync(string patientId)
        {
            try
            {
                var history = await GetPatientVitalSignHistoryAsync(
                    patientId, 
                    startDate: DateTime.UtcNow.AddMonths(-6)
                );

                return new VitalSignsTrend
                {
                    PatientId = patientId,
                    TemperatureTrend = CalculateTemperatureTrend(history),
                    BloodPressureTrend = CalculateBloodPressureTrend(history),
                    HeartRateTrend = CalculateHeartRateTrend(history),
                    RespiratoryRateTrend = CalculateRespiratoryRateTrend(history),
                    OxygenSaturationTrend = CalculateOxygenSaturationTrend(history)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing vital signs trends for patient {PatientId}", patientId);
                throw;
            }
        }

        private TrendAnalysis CalculateTemperatureTrend(List<VitalSignsRecord> history)
        {
            var temperatures = history
                .Select(h => h.Temperature?.Value ?? 0)
                .Where(t => t > 0)
                .ToList();

            return new TrendAnalysis
            {
                Average = temperatures.Any() ? temperatures.Average() : 0,
                Trend = CalculateTrendDirection(temperatures)
            };
        }

        // Similar trend calculation methods for other vital signs...

        private TrendDirection CalculateTrendDirection(List<decimal> values)
        {
            if (values.Count < 2) return TrendDirection.Stable;

            var trend = values.Skip(1).Select((value, index) => 
                value.CompareTo(values[index]) > 0
            );

            var increasingCount = trend.Count(t => t);
            var decreasingCount = trend.Count(t => !t);

            if (increasingCount > decreasingCount)
                return TrendDirection.Increasing;
            else if (decreasingCount > increasingCount)
                return TrendDirection.Decreasing;
            
            return TrendDirection.Stable;
        }
    }

    public class VitalSignsTrend
    {
        public string PatientId { get; set; }
        public TrendAnalysis TemperatureTrend { get; set; }
        public TrendAnalysis BloodPressureTrend { get; set; }
        public TrendAnalysis HeartRateTrend { get; set; }
        public TrendAnalysis RespiratoryRateTrend { get; set; }
        public TrendAnalysis OxygenSaturationTrend { get; set; }
    }

    public class TrendAnalysis
    {
        public decimal Average { get; set; }
        public TrendDirection Trend { get; set; }
    }

    public enum TrendDirection
    {
        Increasing,
        Decreasing,
        Stable
    }
}
