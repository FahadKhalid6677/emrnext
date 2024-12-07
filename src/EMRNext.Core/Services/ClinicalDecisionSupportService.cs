using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EMRNext.Core.Services
{
    public class ClinicalDecisionSupportService : IClinicalDecisionSupportService
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IPatientRepository _patientRepository;
        private readonly IClinicalGuidelineRepository _guidelineRepository;
        private readonly IVitalSignRepository _vitalSignRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;

        public ClinicalDecisionSupportService(
            IApplicationDbContext context,
            INotificationService notificationService,
            IAuditService auditService,
            IPatientRepository patientRepository,
            IClinicalGuidelineRepository guidelineRepository,
            IVitalSignRepository vitalSignRepository,
            IPrescriptionRepository prescriptionRepository)
        {
            _context = context;
            _notificationService = notificationService;
            _auditService = auditService;
            _patientRepository = patientRepository;
            _guidelineRepository = guidelineRepository;
            _vitalSignRepository = vitalSignRepository;
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<ClinicalRule> CreateRuleAsync(ClinicalRule rule)
        {
            rule.CreatedAt = DateTime.UtcNow;
            rule.Version = 1;
            rule.IsActive = true;

            _context.ClinicalRules.Add(rule);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("ClinicalRule", rule.Id, "Create");

            return rule;
        }

        public async Task<ClinicalRule> UpdateRuleAsync(ClinicalRule rule)
        {
            var existingRule = await _context.ClinicalRules.FindAsync(rule.Id);
            if (existingRule == null)
                throw new KeyNotFoundException($"Rule {rule.Id} not found");

            // Create new version
            rule.Version = existingRule.Version + 1;
            rule.ModifiedAt = DateTime.UtcNow;
            
            _context.Entry(existingRule).CurrentValues.SetValues(rule);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("ClinicalRule", rule.Id, "Update");

            return rule;
        }

        public async Task<IEnumerable<RuleEvaluation>> EvaluatePatientRulesAsync(int patientId, string context)
        {
            var patient = await _context.Patients
                .Include(p => p.Encounters)
                .Include(p => p.Problems)
                .Include(p => p.Medications)
                .Include(p => p.Allergies)
                .Include(p => p.Vitals)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
                throw new KeyNotFoundException($"Patient {patientId} not found");

            var activeRules = await _context.ClinicalRules
                .Where(r => r.IsActive && r.TriggerEvent == context)
                .ToListAsync();

            var evaluations = new List<RuleEvaluation>();

            foreach (var rule in activeRules)
            {
                var evaluation = await EvaluateRuleAsync(rule, patient);
                if (evaluation != null)
                {
                    evaluations.Add(evaluation);
                    if (evaluation.EvaluationResult == "Triggered")
                    {
                        await SendAlertNotificationAsync(evaluation);
                    }
                }
            }

            return evaluations;
        }

        private async Task<RuleEvaluation> EvaluateRuleAsync(ClinicalRule rule, Patient patient)
        {
            try
            {
                var conditions = JsonConvert.DeserializeObject<Dictionary<string, object>>(rule.Condition);
                bool isTriggered = EvaluateConditions(conditions, patient);

                var evaluation = new RuleEvaluation
                {
                    ClinicalRuleId = rule.Id,
                    PatientId = patient.Id,
                    EvaluationTime = DateTime.UtcNow,
                    EvaluationResult = isTriggered ? "Triggered" : "Not Triggered",
                    ContextData = JsonConvert.SerializeObject(new
                    {
                        PatientAge = (DateTime.UtcNow - patient.DateOfBirth).TotalYears,
                        RecentVitals = patient.Vitals.OrderByDescending(v => v.Date).FirstOrDefault(),
                        ActiveProblems = patient.Problems.Where(p => p.IsActive).Select(p => p.Code),
                        CurrentMedications = patient.Medications.Where(m => m.IsActive).Select(m => m.Code)
                    })
                };

                if (isTriggered)
                {
                    evaluation.AlertMessage = rule.Recommendation;
                    evaluation.NotificationSent = false;
                }

                _context.RuleEvaluations.Add(evaluation);
                await _context.SaveChangesAsync();

                return evaluation;
            }
            catch (Exception ex)
            {
                await _auditService.LogErrorAsync("RuleEvaluation", 
                    $"Error evaluating rule {rule.Id} for patient {patient.Id}: {ex.Message}");
                return null;
            }
        }

        private bool EvaluateConditions(Dictionary<string, object> conditions, Patient patient)
        {
            // Implement condition evaluation logic here
            // This would evaluate the rule conditions against patient data
            // Return true if conditions are met, false otherwise
            return true; // Placeholder
        }

        public async Task<RuleOverride> CreateOverrideAsync(RuleOverride ruleOverride)
        {
            ruleOverride.CreatedAt = DateTime.UtcNow;
            ruleOverride.IsReviewed = false;

            _context.RuleOverrides.Add(ruleOverride);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("RuleOverride", ruleOverride.Id, "Create");

            return ruleOverride;
        }

        public async Task SendAlertNotificationAsync(RuleEvaluation evaluation)
        {
            var rule = await _context.ClinicalRules.FindAsync(evaluation.ClinicalRuleId);
            var patient = await _context.Patients.FindAsync(evaluation.PatientId);

            var notification = new Notification
            {
                Type = "ClinicalAlert",
                Priority = rule.Severity,
                Message = evaluation.AlertMessage,
                RecipientId = evaluation.ProviderId.ToString(),
                Data = JsonConvert.SerializeObject(new
                {
                    RuleId = rule.Id,
                    RuleName = rule.Name,
                    PatientId = patient.Id,
                    PatientName = $"{patient.LastName}, {patient.FirstName}",
                    EvaluationId = evaluation.Id
                })
            };

            await _notificationService.SendNotificationAsync(notification);

            evaluation.NotificationSent = true;
            evaluation.NotificationTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<PatientRiskAssessment> AssessPatientRiskAsync(Guid patientId)
        {
            var patient = await _patientRepository.GetPatientByIdAsync(patientId);
            var vitalSigns = await _vitalSignRepository.GetLatestVitalSignsAsync(patientId);
            var prescriptions = await _prescriptionRepository.GetPatientActivePrescriptionsAsync(patientId);

            var riskFactors = new List<RiskFactor>();

            // BMI Risk Assessment
            if (vitalSigns.TryGetValue("bmi", out var bmiValue))
            {
                riskFactors.Add(AssessBMIRisk(bmiValue));
            }

            // Blood Pressure Risk Assessment
            if (vitalSigns.TryGetValue("bloodPressureSystolic", out var systolic) &&
                vitalSigns.TryGetValue("bloodPressureDiastolic", out var diastolic))
            {
                riskFactors.Add(AssessBloodPressureRisk(systolic, diastolic));
            }

            // Medication Interaction Risk
            var medicationRisk = AssessMedicationRisk(prescriptions);
            if (medicationRisk != null)
            {
                riskFactors.Add(medicationRisk);
            }

            // Calculate Overall Risk
            var overallRiskLevel = CalculateOverallRiskLevel(riskFactors);

            return new PatientRiskAssessment
            {
                PatientId = patientId,
                RiskFactors = riskFactors,
                OverallRiskLevel = overallRiskLevel,
                RecommendedInterventions = GetRecommendedInterventions(overallRiskLevel),
                AssessmentDate = DateTime.UtcNow
            };
        }

        private RiskFactor AssessBMIRisk(double bmi)
        {
            RiskLevel riskLevel;
            string description;

            if (bmi < 18.5)
            {
                riskLevel = RiskLevel.Medium;
                description = "Underweight - potential nutritional risks";
            }
            else if (bmi >= 18.5 && bmi < 25)
            {
                riskLevel = RiskLevel.Low;
                description = "Normal weight range";
            }
            else if (bmi >= 25 && bmi < 30)
            {
                riskLevel = RiskLevel.Medium;
                description = "Overweight - increased health risks";
            }
            else
            {
                riskLevel = RiskLevel.High;
                description = "Obese - significant health risks";
            }

            return new RiskFactor
            {
                Name = "BMI",
                Value = bmi,
                RiskLevel = riskLevel,
                Description = description
            };
        }

        private RiskFactor AssessBloodPressureRisk(double systolic, double diastolic)
        {
            RiskLevel riskLevel;
            string description;

            if (systolic < 120 && diastolic < 80)
            {
                riskLevel = RiskLevel.Low;
                description = "Normal blood pressure";
            }
            else if ((systolic >= 120 && systolic < 130) && (diastolic < 80))
            {
                riskLevel = RiskLevel.Low;
                description = "Elevated blood pressure";
            }
            else if ((systolic >= 130 && systolic < 140) || (diastolic >= 80 && diastolic < 90))
            {
                riskLevel = RiskLevel.Medium;
                description = "Stage 1 Hypertension";
            }
            else
            {
                riskLevel = RiskLevel.High;
                description = "Stage 2 Hypertension - Immediate medical attention recommended";
            }

            return new RiskFactor
            {
                Name = "Blood Pressure",
                Value = systolic, // Using systolic as primary value
                RiskLevel = riskLevel,
                Description = description
            };
        }

        private RiskFactor AssessMedicationRisk(List<Prescription> prescriptions)
        {
            if (prescriptions.Count > 5)
            {
                return new RiskFactor
                {
                    Name = "Medication Complexity",
                    Value = prescriptions.Count,
                    RiskLevel = RiskLevel.High,
                    Description = "High number of concurrent medications - potential interaction risks"
                };
            }
            return null;
        }

        private RiskLevel CalculateOverallRiskLevel(List<RiskFactor> riskFactors)
        {
            if (riskFactors.Any(r => r.RiskLevel == RiskLevel.VeryHigh))
                return RiskLevel.VeryHigh;

            var riskLevels = riskFactors.Select(r => r.RiskLevel);
            var highestRisk = riskLevels.Max();

            return highestRisk;
        }

        private List<string> GetRecommendedInterventions(RiskLevel riskLevel)
        {
            return riskLevel switch
            {
                RiskLevel.Low => new List<string> 
                { 
                    "Continue regular health monitoring",
                    "Maintain current lifestyle and preventive measures"
                },
                RiskLevel.Medium => new List<string>
                {
                    "Schedule comprehensive health review",
                    "Consider lifestyle modifications",
                    "Increase frequency of health screenings"
                },
                RiskLevel.High => new List<string>
                {
                    "Immediate medical consultation recommended",
                    "Comprehensive diagnostic workup",
                    "Develop targeted intervention plan",
                    "Consider specialist referral"
                },
                RiskLevel.VeryHigh => new List<string>
                {
                    "URGENT: Immediate medical intervention required",
                    "Emergency diagnostic assessment",
                    "Intensive monitoring and treatment plan",
                    "Specialist consultation mandatory"
                },
                _ => new List<string>()
            };
        }

        public async Task<List<AlertNotification>> GeneratePatientAlertsAsync(Guid patientId)
        {
            var riskAssessment = await AssessPatientRiskAsync(patientId);
            var alerts = new List<AlertNotification>();

            foreach (var riskFactor in riskAssessment.RiskFactors)
            {
                if (riskFactor.RiskLevel >= RiskLevel.High)
                {
                    alerts.Add(new AlertNotification
                    {
                        Id = Guid.NewGuid(),
                        PatientId = patientId,
                        Title = $"High Risk Alert: {riskFactor.Name}",
                        Message = riskFactor.Description,
                        Severity = riskFactor.RiskLevel == RiskLevel.VeryHigh 
                            ? AlertSeverity.Critical 
                            : AlertSeverity.Warning,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            return alerts;
        }
    }
}
