using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Models;
using EMRNext.Core.Repositories;
using EMRNext.Core.Services;

namespace EMRNext.Core.Services
{
    public class ReportingAndAnalyticsService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IVitalSignRepository _vitalSignRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IClinicalDecisionSupportService _cdsService;
        private readonly IAuditRepository _auditRepository;

        public ReportingAndAnalyticsService(
            IPatientRepository patientRepository,
            IVitalSignRepository vitalSignRepository,
            IPrescriptionRepository prescriptionRepository,
            IClinicalDecisionSupportService cdsService,
            IAuditRepository auditRepository)
        {
            _patientRepository = patientRepository;
            _vitalSignRepository = vitalSignRepository;
            _prescriptionRepository = prescriptionRepository;
            _cdsService = cdsService;
            _auditRepository = auditRepository;
        }

        public async Task<ClinicalReport> GeneratePatientClinicalReportAsync(Guid patientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var patient = await _patientRepository.GetPatientByIdAsync(patientId);
            if (patient == null)
            {
                throw new ArgumentException("Patient not found");
            }

            startDate ??= DateTime.UtcNow.AddYears(-1);
            endDate ??= DateTime.UtcNow;

            var vitalSigns = await _vitalSignRepository.GetVitalSignsInRangeAsync(patientId, startDate.Value, endDate.Value);
            var prescriptions = await _prescriptionRepository.GetPatientPrescriptionsAsync(patientId);
            var riskAssessment = await _cdsService.AssessPatientRiskAsync(patientId);

            var report = new ClinicalReport
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                ReportType = "Comprehensive Clinical Report",
                GeneratedAt = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                ReportData = new Dictionary<string, object>
                {
                    { "PatientName", patient.Name },
                    { "Age", CalculateAge(patient.DateOfBirth) },
                    { "Gender", patient.Gender }
                },
                Sections = new List<ReportSection>
                {
                    CreateVitalSignsSection(vitalSigns),
                    CreatePrescriptionSection(prescriptions),
                    CreateRiskAssessmentSection(riskAssessment)
                }
            };

            return report;
        }

        public async Task<PopulationHealthMetrics> CalculatePopulationHealthMetricsAsync()
        {
            var patients = await _patientRepository.GetAllPatientsAsync();
            
            var metrics = new PopulationHealthMetrics
            {
                Id = Guid.NewGuid(),
                CalculatedAt = DateTime.UtcNow,
                TotalPatients = patients.Count,
                PatientDemographics = CalculatePatientDemographics(patients),
                ChronicConditionPrevalence = CalculateChronicConditionPrevalence(patients),
                RiskStratification = CalculateRiskStratification(patients)
            };

            return metrics;
        }

        public async Task<List<PerformanceMetric>> GeneratePerformanceMetricsAsync()
        {
            var metrics = new List<PerformanceMetric>
            {
                await CalculatePatientAdmissionMetric(),
                await CalculatePrescriptionComplianceMetric(),
                await CalculateRiskManagementMetric()
            };

            return metrics;
        }

        public async Task<List<AuditTrail>> GetRecentAuditTrailsAsync(int days = 30)
        {
            return await _auditRepository.GetAuditTrailsInRangeAsync(
                DateTime.UtcNow.AddDays(-days), 
                DateTime.UtcNow
            );
        }

        private ReportSection CreateVitalSignsSection(Dictionary<DateTime, Dictionary<string, double>> vitalSigns)
        {
            var latestVitals = vitalSigns.OrderByDescending(v => v.Key).FirstOrDefault();
            
            return new ReportSection
            {
                Title = "Vital Signs",
                Summary = "Overview of patient's vital signs",
                Metrics = latestVitals.Value ?? new Dictionary<string, object>(),
                Highlights = GenerateVitalSignHighlights(vitalSigns)
            };
        }

        private ReportSection CreatePrescriptionSection(List<Prescription> prescriptions)
        {
            return new ReportSection
            {
                Title = "Medication History",
                Summary = "Patient's current and past medications",
                Metrics = new Dictionary<string, object>
                {
                    { "TotalPrescriptions", prescriptions.Count },
                    { "ActivePrescriptions", prescriptions.Count(p => p.Status == PrescriptionStatus.Active) }
                },
                Highlights = prescriptions
                    .Where(p => p.Status == PrescriptionStatus.Active)
                    .Select(p => $"{p.MedicationName} - {p.Dosage}")
                    .ToList()
            };
        }

        private ReportSection CreateRiskAssessmentSection(PatientRiskAssessment riskAssessment)
        {
            return new ReportSection
            {
                Title = "Risk Assessment",
                Summary = "Comprehensive patient risk evaluation",
                Metrics = new Dictionary<string, object>
                {
                    { "OverallRiskLevel", riskAssessment.OverallRiskLevel.ToString() }
                },
                Highlights = riskAssessment.RecommendedInterventions
            };
        }

        private List<string> GenerateVitalSignHighlights(Dictionary<DateTime, Dictionary<string, double>> vitalSigns)
        {
            var highlights = new List<string>();
            
            // Add logic to generate meaningful highlights from vital signs
            if (vitalSigns.Any())
            {
                var latestVitals = vitalSigns.OrderByDescending(v => v.Key).First().Value;
                
                if (latestVitals.TryGetValue("bmi", out double bmi))
                {
                    highlights.Add($"BMI: {bmi} - {GetBMICategory(bmi)}");
                }

                if (latestVitals.TryGetValue("bloodPressureSystolic", out double systolic) &&
                    latestVitals.TryGetValue("bloodPressureDiastolic", out double diastolic))
                {
                    highlights.Add($"Blood Pressure: {systolic}/{diastolic} - {GetBloodPressureCategory(systolic, diastolic)}");
                }
            }

            return highlights;
        }

        private string GetBMICategory(double bmi)
        {
            return bmi switch
            {
                < 18.5 => "Underweight",
                >= 18.5 and < 25 => "Normal weight",
                >= 25 and < 30 => "Overweight",
                _ => "Obese"
            };
        }

        private string GetBloodPressureCategory(double systolic, double diastolic)
        {
            return (systolic, diastolic) switch
            {
                (< 120, < 80) => "Normal",
                (>= 120 and < 130, < 80) => "Elevated",
                (>= 130 and < 140, or >= 80 and < 90) => "Hypertension Stage 1",
                (>= 140, or >= 90) => "Hypertension Stage 2",
                _ => "Unknown"
            };
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var age = DateTime.UtcNow.Year - dateOfBirth.Year;
            if (DateTime.UtcNow < dateOfBirth.AddYears(age)) age--;
            return age;
        }

        private Dictionary<string, int> CalculatePatientDemographics(List<Patient> patients)
        {
            return patients
                .GroupBy(p => p.Gender)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private Dictionary<string, double> CalculateChronicConditionPrevalence(List<Patient> patients)
        {
            // This would typically involve more complex logic and data
            return new Dictionary<string, double>
            {
                { "Diabetes", patients.Count(p => p.HasDiabetes) / (double)patients.Count },
                { "Hypertension", patients.Count(p => p.HasHypertension) / (double)patients.Count },
                { "HeartDisease", patients.Count(p => p.HasHeartDisease) / (double)patients.Count }
            };
        }

        private Dictionary<string, double> CalculateRiskStratification(List<Patient> patients)
        {
            return patients
                .Select(p => _cdsService.AssessPatientRiskAsync(p.Id).Result.OverallRiskLevel)
                .GroupBy(risk => risk.ToString())
                .ToDictionary(
                    g => g.Key, 
                    g => g.Count() / (double)patients.Count
                );
        }

        private async Task<PerformanceMetric> CalculatePatientAdmissionMetric()
        {
            // Placeholder implementation
            return new PerformanceMetric
            {
                Id = Guid.NewGuid(),
                MetricName = "Patient Admission Rate",
                Category = "Operational",
                Value = 75.5, // Example value
                Unit = "Percentage",
                CalculatedAt = DateTime.UtcNow,
                Trends = new List<PerformanceTrend>
                {
                    new PerformanceTrend { Timestamp = DateTime.UtcNow.AddMonths(-1), Value = 70.2 },
                    new PerformanceTrend { Timestamp = DateTime.UtcNow.AddMonths(-2), Value = 72.8 }
                }
            };
        }

        private async Task<PerformanceMetric> CalculatePrescriptionComplianceMetric()
        {
            // Placeholder implementation
            return new PerformanceMetric
            {
                Id = Guid.NewGuid(),
                MetricName = "Prescription Compliance",
                Category = "Clinical",
                Value = 85.3, // Example value
                Unit = "Percentage",
                CalculatedAt = DateTime.UtcNow,
                Trends = new List<PerformanceTrend>
                {
                    new PerformanceTrend { Timestamp = DateTime.UtcNow.AddMonths(-1), Value = 82.1 },
                    new PerformanceTrend { Timestamp = DateTime.UtcNow.AddMonths(-2), Value = 83.7 }
                }
            };
        }

        private async Task<PerformanceMetric> CalculateRiskManagementMetric()
        {
            // Placeholder implementation
            return new PerformanceMetric
            {
                Id = Guid.NewGuid(),
                MetricName = "Risk Management Effectiveness",
                Category = "Quality",
                Value = 92.7, // Example value
                Unit = "Percentage",
                CalculatedAt = DateTime.UtcNow,
                Trends = new List<PerformanceTrend>
                {
                    new PerformanceTrend { Timestamp = DateTime.UtcNow.AddMonths(-1), Value = 90.5 },
                    new PerformanceTrend { Timestamp = DateTime.UtcNow.AddMonths(-2), Value = 91.2 }
                }
            };
        }
    }
}
