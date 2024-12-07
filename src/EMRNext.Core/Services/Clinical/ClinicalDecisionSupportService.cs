using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities.Clinical;
using EMRNext.Core.Models.Clinical;

namespace EMRNext.Core.Services.Clinical
{
    public class ClinicalDecisionSupportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClinicalDecisionSupportService> _logger;
        private readonly IClinicalAlertService _alertService;
        private readonly DrugAllergyService _drugAllergyService;

        public ClinicalDecisionSupportService(
            ApplicationDbContext context,
            ILogger<ClinicalDecisionSupportService> logger,
            IClinicalAlertService alertService,
            DrugAllergyService drugAllergyService)
        {
            _context = context;
            _logger = logger;
            _alertService = alertService;
            _drugAllergyService = drugAllergyService;
        }

        public async Task<List<ClinicalSuggestion>> EvaluatePatientConditionAsync(
            int patientId,
            string condition,
            List<string> symptoms,
            Dictionary<string, string> vitalSigns,
            List<string> labResults)
        {
            var suggestions = new List<ClinicalSuggestion>();

            try
            {
                // Get patient's relevant clinical data
                var patientData = await GetPatientClinicalDataAsync(patientId);

                // Apply clinical rules
                suggestions.AddRange(await ApplyDiagnosticRulesAsync(
                    condition, symptoms, vitalSigns, labResults));

                // Check medication-related rules
                suggestions.AddRange(await EvaluateMedicationRulesAsync(
                    patientId, condition, patientData));

                // Apply preventive care rules
                suggestions.AddRange(await ApplyPreventiveCareRulesAsync(
                    patientId, patientData));

                // Generate alerts for high-priority suggestions
                foreach (var suggestion in suggestions.Where(s => s.Priority == "High"))
                {
                    await _alertService.CreateAlertAsync(new ClinicalAlert
                    {
                        PatientId = patientId,
                        AlertType = "Clinical Suggestion",
                        Severity = suggestion.Priority,
                        Message = suggestion.Title,
                        Details = suggestion.Description,
                        RecommendedAction = suggestion.Recommendation,
                        RequiresAcknowledgment = true
                    });
                }

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating patient condition");
                throw;
            }
        }

        private async Task<PatientClinicalData> GetPatientClinicalDataAsync(int patientId)
        {
            return new PatientClinicalData
            {
                Demographics = await _context.PatientDemographics
                    .FirstOrDefaultAsync(d => d.PatientId == patientId),
                
                ActiveProblems = await _context.Problems
                    .Where(p => p.PatientId == patientId && p.IsActive)
                    .ToListAsync(),
                
                Medications = await _context.PatientMedications
                    .Where(m => m.PatientId == patientId && m.IsActive)
                    .ToListAsync(),
                
                Allergies = await _drugAllergyService.GetPatientAllergiesAsync(patientId),
                
                LabResults = await _context.LabResults
                    .Where(l => l.PatientId == patientId)
                    .OrderByDescending(l => l.ResultDate)
                    .Take(10)
                    .ToListAsync(),
                
                VitalSigns = await _context.VitalSigns
                    .Where(v => v.PatientId == patientId)
                    .OrderByDescending(v => v.MeasurementDate)
                    .Take(5)
                    .ToListAsync()
            };
        }

        private async Task<List<ClinicalSuggestion>> ApplyDiagnosticRulesAsync(
            string condition,
            List<string> symptoms,
            Dictionary<string, string> vitalSigns,
            List<string> labResults)
        {
            var suggestions = new List<ClinicalSuggestion>();

            // Get relevant diagnostic rules
            var rules = await _context.DiagnosticRules
                .Where(r => r.Condition == condition || r.Symptoms.Any(s => symptoms.Contains(s)))
                .ToListAsync();

            foreach (var rule in rules)
            {
                if (EvaluateRule(rule, symptoms, vitalSigns, labResults))
                {
                    suggestions.Add(new ClinicalSuggestion
                    {
                        Title = rule.Title,
                        Description = rule.Description,
                        Recommendation = rule.Recommendation,
                        Priority = rule.Priority,
                        Category = "Diagnostic",
                        EvidenceLevel = rule.EvidenceLevel,
                        References = rule.References
                    });
                }
            }

            return suggestions;
        }

        private async Task<List<ClinicalSuggestion>> EvaluateMedicationRulesAsync(
            int patientId,
            string condition,
            PatientClinicalData patientData)
        {
            var suggestions = new List<ClinicalSuggestion>();

            // Get medication rules for the condition
            var rules = await _context.MedicationRules
                .Where(r => r.Condition == condition)
                .ToListAsync();

            foreach (var rule in rules)
            {
                if (EvaluateMedicationRule(rule, patientData))
                {
                    // Check for drug interactions
                    if (rule.RecommendedDrugId.HasValue)
                    {
                        var interactions = await _drugAllergyService.CheckDrugInteractionsAsync(
                            patientId,
                            rule.RecommendedDrugId.Value,
                            patientData.Medications.Select(m => m.DrugId).ToList());

                        if (interactions.Any(i => i.RequiresOverride))
                            continue; // Skip this recommendation if there are severe interactions
                    }

                    suggestions.Add(new ClinicalSuggestion
                    {
                        Title = rule.Title,
                        Description = rule.Description,
                        Recommendation = rule.Recommendation,
                        Priority = rule.Priority,
                        Category = "Medication",
                        EvidenceLevel = rule.EvidenceLevel,
                        References = rule.References
                    });
                }
            }

            return suggestions;
        }

        private async Task<List<ClinicalSuggestion>> ApplyPreventiveCareRulesAsync(
            int patientId,
            PatientClinicalData patientData)
        {
            var suggestions = new List<ClinicalSuggestion>();

            // Get age and gender-appropriate preventive care rules
            var rules = await _context.PreventiveCareRules
                .Where(r => 
                    r.MinAge <= patientData.Demographics.Age &&
                    r.MaxAge >= patientData.Demographics.Age &&
                    (r.Gender == "A" || r.Gender == patientData.Demographics.Gender))
                .ToListAsync();

            foreach (var rule in rules)
            {
                if (EvaluatePreventiveCareRule(rule, patientData))
                {
                    suggestions.Add(new ClinicalSuggestion
                    {
                        Title = rule.Title,
                        Description = rule.Description,
                        Recommendation = rule.Recommendation,
                        Priority = rule.Priority,
                        Category = "Preventive Care",
                        EvidenceLevel = rule.EvidenceLevel,
                        References = rule.References
                    });
                }
            }

            return suggestions;
        }

        private bool EvaluateRule(
            DiagnosticRule rule,
            List<string> symptoms,
            Dictionary<string, string> vitalSigns,
            List<string> labResults)
        {
            // Implement rule evaluation logic
            // This is a simplified example - actual implementation would be more complex
            return rule.Symptoms.All(s => symptoms.Contains(s)) &&
                   EvaluateVitalSigns(rule.VitalSignCriteria, vitalSigns) &&
                   EvaluateLabResults(rule.LabResultCriteria, labResults);
        }

        private bool EvaluateMedicationRule(
            MedicationRule rule,
            PatientClinicalData patientData)
        {
            // Implement medication rule evaluation logic
            return true; // Simplified for example
        }

        private bool EvaluatePreventiveCareRule(
            PreventiveCareRule rule,
            PatientClinicalData patientData)
        {
            // Implement preventive care rule evaluation logic
            return true; // Simplified for example
        }

        private bool EvaluateVitalSigns(
            Dictionary<string, string> criteria,
            Dictionary<string, string> actualVitals)
        {
            // Implement vital signs evaluation logic
            return true; // Simplified for example
        }

        private bool EvaluateLabResults(
            Dictionary<string, string> criteria,
            List<string> actualResults)
        {
            // Implement lab results evaluation logic
            return true; // Simplified for example
        }
    }

    public class ClinicalSuggestion
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
        public string Priority { get; set; }
        public string Category { get; set; }
        public string EvidenceLevel { get; set; }
        public string References { get; set; }
    }

    public class PatientClinicalData
    {
        public PatientDemographics Demographics { get; set; }
        public List<ProblemEntity> ActiveProblems { get; set; }
        public List<PatientMedicationEntity> Medications { get; set; }
        public List<AllergyEntity> Allergies { get; set; }
        public List<LabResultEntity> LabResults { get; set; }
        public List<VitalSignEntity> VitalSigns { get; set; }
    }
}
