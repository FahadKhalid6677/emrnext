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
    public class DrugAllergyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DrugAllergyService> _logger;
        private readonly IClinicalAlertService _alertService;

        public DrugAllergyService(
            ApplicationDbContext context,
            ILogger<DrugAllergyService> logger,
            IClinicalAlertService alertService)
        {
            _context = context;
            _logger = logger;
            _alertService = alertService;
        }

        public async Task<List<DrugInteractionResult>> CheckDrugInteractionsAsync(
            int patientId,
            int newDrugId,
            List<int> currentMedicationIds)
        {
            var results = new List<DrugInteractionResult>();

            // Check drug-drug interactions
            var drugInteractions = await GetDrugInteractionsAsync(newDrugId, currentMedicationIds);

            foreach (var interaction in drugInteractions)
            {
                results.Add(new DrugInteractionResult
                {
                    InteractionType = "Drug-Drug",
                    Drug1 = interaction.Drug1.GenericName,
                    Drug2 = interaction.Drug2.GenericName,
                    Severity = interaction.SeverityLevel,
                    Mechanism = interaction.InteractionMechanism,
                    ClinicalEffects = interaction.ClinicalEffects,
                    Management = interaction.ManagementStrategy,
                    RequiresOverride = interaction.SeverityLevel == "Severe"
                });
            }

            // Check drug-allergy interactions
            var allergies = await GetPatientAllergiesAsync(patientId);

            foreach (var allergy in allergies)
            {
                var allergyInteraction = allergy.Interactions
                    .FirstOrDefault(ai => ai.DrugId == newDrugId);

                if (allergyInteraction != null)
                {
                    results.Add(new DrugInteractionResult
                    {
                        InteractionType = "Drug-Allergy",
                        Drug1 = allergyInteraction.Drug.GenericName,
                        Drug2 = allergy.Allergen,
                        Severity = "Severe",
                        Mechanism = "Allergic Cross-Reactivity",
                        ClinicalEffects = allergyInteraction.ClinicalEvidence,
                        Management = allergyInteraction.RecommendedAction,
                        RequiresOverride = allergyInteraction.RequiresOverride
                    });
                }
            }

            // Generate alerts for severe interactions
            foreach (var result in results.Where(r => r.RequiresOverride))
            {
                await _alertService.CreateAlertAsync(new ClinicalAlert
                {
                    PatientId = patientId,
                    AlertType = "Drug Interaction",
                    Severity = "High",
                    Message = $"Severe {result.InteractionType} interaction detected: " +
                             $"{result.Drug1} with {result.Drug2}",
                    Details = result.ClinicalEffects,
                    RecommendedAction = result.Management,
                    RequiresAcknowledgment = true
                });
            }

            return results;
        }

        private async Task<List<DrugInteraction>> GetDrugInteractionsAsync(
            int newDrugId,
            List<int> currentMedicationIds)
        {
            return await _context.DrugInteractions
                .Include(di => di.Drug1)
                .Include(di => di.Drug2)
                .Where(di =>
                    (di.Drug1Id == newDrugId && currentMedicationIds.Contains(di.Drug2Id)) ||
                    (di.Drug2Id == newDrugId && currentMedicationIds.Contains(di.Drug1Id)))
                .ToListAsync();
        }

        public async Task<bool> ProcessOverrideRequestAsync(DrugAllergyOverride overrideRequest)
        {
            try
            {
                var overrideEntity = new AllergyOverrideEntity
                {
                    AllergyId = overrideRequest.AllergyId,
                    DrugId = overrideRequest.DrugId,
                    ProviderId = overrideRequest.ProviderId,
                    PatientId = overrideRequest.PatientId,
                    OverrideDate = DateTime.UtcNow,
                    Reason = overrideRequest.Reason,
                    AlternativeConsidered = overrideRequest.AlternativeConsidered,
                    RiskMitigationPlan = overrideRequest.RiskMitigationPlan,
                    PatientConsentNotes = overrideRequest.PatientConsentNotes,
                    IsActive = true,
                    ExpirationDate = overrideRequest.ExpirationDate
                };

                _context.AllergyOverrides.Add(overrideEntity);
                await _context.SaveChangesAsync();

                await _alertService.CreateAlertAsync(new ClinicalAlert
                {
                    PatientId = overrideRequest.PatientId,
                    AlertType = "Allergy Override",
                    Severity = "High",
                    Message = $"Allergy override approved for patient",
                    Details = $"Override reason: {overrideRequest.Reason}\n" +
                             $"Risk mitigation: {overrideRequest.RiskMitigationPlan}",
                    RequiresAcknowledgment = true
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing allergy override request");
                return false;
            }
        }

        public async Task<List<AllergyEntity>> GetPatientAllergiesAsync(int patientId)
        {
            return await _context.Allergies
                .Include(a => a.Interactions)
                .ThenInclude(i => i.Drug)
                .Where(a => a.PatientId == patientId && a.IsActive)
                .OrderByDescending(a => a.OnsetDate)
                .ToListAsync();
        }

        public async Task<bool> AddAllergyAsync(AllergyEntity allergy)
        {
            try
            {
                _context.Allergies.Add(allergy);
                await _context.SaveChangesAsync();

                // Check for potential interactions with current medications
                var currentMedications = await _context.PatientMedications
                    .Where(m => m.PatientId == allergy.PatientId && m.IsActive)
                    .Select(m => m.DrugId)
                    .ToListAsync();

                if (currentMedications.Any())
                {
                    await CheckDrugInteractionsAsync(
                        allergy.PatientId,
                        currentMedications.First(),
                        currentMedications.Skip(1).ToList());
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding allergy");
                return false;
            }
        }

        public async Task<bool> UpdateAllergyAsync(AllergyEntity allergy)
        {
            try
            {
                var existing = await _context.Allergies
                    .FirstOrDefaultAsync(a => a.Id == allergy.Id);

                if (existing == null)
                    return false;

                _context.Entry(existing).CurrentValues.SetValues(allergy);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating allergy");
                return false;
            }
        }

        public async Task<bool> DeactivateAllergyAsync(int allergyId, string reason)
        {
            try
            {
                var allergy = await _context.Allergies
                    .FirstOrDefaultAsync(a => a.Id == allergyId);

                if (allergy == null)
                    return false;

                allergy.IsActive = false;
                allergy.ClinicalNotes += $"\nDeactivated: {DateTime.UtcNow} - {reason}";
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating allergy");
                return false;
            }
        }
    }

    public class DrugInteractionResult
    {
        public string InteractionType { get; set; }
        public string Drug1 { get; set; }
        public string Drug2 { get; set; }
        public string Severity { get; set; }
        public string Mechanism { get; set; }
        public string ClinicalEffects { get; set; }
        public string Management { get; set; }
        public bool RequiresOverride { get; set; }
    }

    public class DrugAllergyOverride
    {
        public int AllergyId { get; set; }
        public int DrugId { get; set; }
        public int ProviderId { get; set; }
        public int PatientId { get; set; }
        public string Reason { get; set; }
        public string AlternativeConsidered { get; set; }
        public string RiskMitigationPlan { get; set; }
        public string PatientConsentNotes { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
