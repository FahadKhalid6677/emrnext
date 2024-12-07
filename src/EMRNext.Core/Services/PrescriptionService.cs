using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Models;
using EMRNext.Core.Repositories;

namespace EMRNext.Core.Services
{
    public class PrescriptionService
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IDrugInteractionRepository _drugInteractionRepository;

        public PrescriptionService(
            IPrescriptionRepository prescriptionRepository,
            IDrugInteractionRepository drugInteractionRepository)
        {
            _prescriptionRepository = prescriptionRepository;
            _drugInteractionRepository = drugInteractionRepository;
        }

        public async Task<Prescription> CreatePrescriptionAsync(Prescription prescription)
        {
            // Check for drug interactions before creating prescription
            var existingPrescriptions = await _prescriptionRepository.GetPatientActivePrescriptionsAsync(prescription.PatientId);
            var interactions = await CheckDrugInteractionsAsync(existingPrescriptions, prescription);

            if (interactions.Any(i => i.Severity == InteractionSeverity.Contraindicated))
            {
                throw new InvalidOperationException("Prescription cannot be created due to severe drug interactions.");
            }

            prescription.Id = Guid.NewGuid();
            prescription.PrescriptionDate = DateTime.UtcNow;
            prescription.Status = PrescriptionStatus.Active;
            prescription.EndDate = prescription.PrescriptionDate.AddDays(prescription.Duration);

            return await _prescriptionRepository.AddPrescriptionAsync(prescription);
        }

        public async Task<List<DrugInteraction>> CheckDrugInteractionsAsync(
            List<Prescription> existingPrescriptions, 
            Prescription newPrescription)
        {
            var allMedications = existingPrescriptions
                .Select(p => p.MedicationName)
                .Append(newPrescription.MedicationName)
                .Distinct()
                .ToList();

            return await _drugInteractionRepository.GetDrugInteractionsAsync(allMedications);
        }

        public async Task<Prescription> DiscontinuePrescriptionAsync(Guid prescriptionId)
        {
            var prescription = await _prescriptionRepository.GetPrescriptionByIdAsync(prescriptionId);
            
            if (prescription == null)
            {
                throw new ArgumentException("Prescription not found");
            }

            prescription.Status = PrescriptionStatus.Discontinued;
            prescription.EndDate = DateTime.UtcNow;

            return await _prescriptionRepository.UpdatePrescriptionAsync(prescription);
        }

        public async Task<List<Prescription>> GetPatientPrescriptionsAsync(Guid patientId)
        {
            return await _prescriptionRepository.GetPatientPrescriptionsAsync(patientId);
        }
    }
}
