using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities.Pharmacy;
using EMRNext.Core.Domain.Models.Pharmacy;
using EMRNext.Core.Infrastructure;
using EMRNext.Core.Services.Interface;
using EMRNext.Core.Services.Document;
using EMRNext.Core.Services.Notification;
using EMRNext.Core.Services.Clinical;

namespace EMRNext.Core.Services.Pharmacy
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly EMRNextDbContext _context;
        private readonly IPharmacyInterface _pharmacyInterface;
        private readonly IDrugDatabase _drugDatabase;
        private readonly IDocumentService _documentService;
        private readonly INotificationService _notificationService;
        private readonly IClinicalService _clinicalService;
        private readonly IAuditService _auditService;
        private readonly IQualityService _qualityService;

        public PrescriptionService(
            EMRNextDbContext context,
            IPharmacyInterface pharmacyInterface,
            IDrugDatabase drugDatabase,
            IDocumentService documentService,
            INotificationService notificationService,
            IClinicalService clinicalService,
            IAuditService auditService,
            IQualityService qualityService)
        {
            _context = context;
            _pharmacyInterface = pharmacyInterface;
            _drugDatabase = drugDatabase;
            _documentService = documentService;
            _notificationService = notificationService;
            _clinicalService = clinicalService;
            _auditService = auditService;
            _qualityService = qualityService;
        }

        public async Task<Prescription> CreatePrescriptionAsync(PrescriptionRequest request)
        {
            // Validate prescription request
            await ValidatePrescriptionRequestAsync(request);

            var prescription = new Prescription
            {
                PrescriptionNumber = await GeneratePrescriptionNumberAsync(),
                PatientId = request.PatientId,
                ProviderId = request.ProviderId,
                EncounterId = request.EncounterId,
                PrescriptionDate = DateTime.UtcNow,
                Status = "Pending",
                Priority = request.Priority,
                DiagnosisCodes = request.DiagnosisCodes,
                ClinicalNotes = request.ClinicalNotes,
                RequiresApproval = request.RequiresApproval
            };

            // Add prescription items
            foreach (var item in request.Items)
            {
                // Validate medication and dosage
                await ValidateMedicationAsync(item.MedicationId, request.PatientId);
                await ValidateDosageAsync(item);

                var prescriptionItem = new PrescriptionItem
                {
                    MedicationId = item.MedicationId,
                    Dosage = item.Dosage,
                    Route = item.Route,
                    Frequency = item.Frequency,
                    Instructions = item.Instructions,
                    Quantity = item.Quantity,
                    DaysSupply = item.DaysSupply,
                    Refills = item.Refills,
                    RefillsRemaining = item.Refills,
                    ExpirationDate = DateTime.UtcNow.AddMonths(6),
                    IsActive = true,
                    IsPrn = item.IsPrn,
                    PrnInstructions = item.PrnInstructions
                };

                // Check for interactions
                var interactions = await CheckInteractionsAsync(item.MedicationId, request.PatientId);
                if (interactions.Any())
                {
                    prescriptionItem.HasInteractions = true;
                    foreach (var interaction in interactions)
                    {
                        prescriptionItem.Interactions.Add(interaction);
                    }
                }

                prescription.Items.Add(prescriptionItem);
            }

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            // Create initial documents
            await CreatePrescriptionDocumentsAsync(prescription, request.Documents);

            // Send notifications
            await NotifyPrescriptionCreationAsync(prescription);

            // Audit trail
            await _auditService.LogActivityAsync(
                "PrescriptionCreation",
                $"Created prescription: {prescription.PrescriptionNumber}",
                prescription);

            return prescription;
        }

        public async Task<bool> SendToPrescriptionAsync(int prescriptionId)
        {
            var prescription = await GetPrescriptionWithDetailsAsync(prescriptionId);
            if (prescription == null)
                throw new NotFoundException("Prescription not found");

            // Validate prescription readiness
            await ValidatePrescriptionForSubmissionAsync(prescription);

            // Check if controlled substance
            if (prescription.IsControlled)
            {
                await ValidateControlledSubstanceAsync(prescriptionId);
                await RecordControlledSubstanceAsync(prescriptionId);
            }

            // Send to pharmacy
            if (prescription.PharmacyId.HasValue)
            {
                var success = await _pharmacyInterface.SendPrescriptionAsync(prescription);
                if (!success)
                    throw new InterfaceException("Failed to send prescription to pharmacy");
            }

            prescription.Status = "Sent";
            await _context.SaveChangesAsync();

            // Notify relevant parties
            await NotifyPrescriptionSubmissionAsync(prescription);

            return true;
        }

        public async Task<PrescriptionFill> ProcessRefillRequestAsync(RefillRequest request)
        {
            var prescriptionItem = await _context.PrescriptionItems
                .Include(p => p.Prescription)
                .FirstOrDefaultAsync(p => p.Id == request.PrescriptionItemId);

            if (prescriptionItem == null)
                throw new NotFoundException("Prescription item not found");

            // Validate refill eligibility
            await ValidateRefillEligibilityAsync(prescriptionItem);

            var fill = new PrescriptionFill
            {
                PrescriptionItemId = prescriptionItem.Id,
                FillDate = DateTime.UtcNow,
                Quantity = request.Quantity,
                DaysSupply = request.DaysSupply,
                Status = "Pending",
                PharmacyId = request.PharmacyId,
                PharmacistNotes = request.Notes
            };

            _context.PrescriptionFills.Add(fill);

            // Update prescription item
            prescriptionItem.RefillsRemaining--;
            prescriptionItem.LastFilledDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Process documents
            await ProcessFillDocumentsAsync(fill, request.Documents);

            // Notify provider if needed
            if (request.RequiresAuthorization)
            {
                await NotifyRefillAuthorizationRequestAsync(fill);
            }

            return fill;
        }

        public async Task<IEnumerable<DrugInteraction>> CheckInteractionsAsync(
            int medicationId,
            int patientId)
        {
            var interactions = new List<DrugInteraction>();

            // Get patient's active medications
            var activeMedications = await GetActivePatientMedicationsAsync(patientId);

            // Check interactions with each active medication
            foreach (var activeMed in activeMedications)
            {
                var drugInteractions = await _drugDatabase.CheckInteractionAsync(
                    medicationId,
                    activeMed.MedicationId);

                interactions.AddRange(drugInteractions);
            }

            // Check drug-allergy interactions
            var allergyInteractions = await CheckAllergyInteractionsAsync(
                medicationId,
                patientId);

            // Check drug-condition interactions
            var conditionInteractions = await CheckConditionInteractionsAsync(
                medicationId,
                patientId);

            return interactions
                .OrderByDescending(i => i.Severity)
                .ToList();
        }

        public async Task<bool> ValidateControlledSubstanceAsync(int prescriptionId)
        {
            var prescription = await GetPrescriptionWithDetailsAsync(prescriptionId);
            if (prescription == null)
                throw new NotFoundException("Prescription not found");

            // Validate provider DEA number
            await ValidateProviderDEAAsync(prescription.ProviderId);

            // Check prescription monitoring program
            await CheckPrescriptionMonitoringProgramAsync(prescription);

            // Validate quantity limits
            foreach (var item in prescription.Items)
            {
                await ValidateControlledSubstanceQuantityAsync(item);
            }

            // Record in controlled substance log
            await RecordControlledSubstanceLogAsync(prescription);

            return true;
        }

        private async Task ValidatePrescriptionRequestAsync(PrescriptionRequest request)
        {
            // Validate patient
            var patient = await _context.Patients.FindAsync(request.PatientId);
            if (patient == null)
                throw new ValidationException("Invalid patient");

            // Validate provider
            var provider = await _context.Providers.FindAsync(request.ProviderId);
            if (provider == null)
                throw new ValidationException("Invalid provider");

            // Validate medications
            foreach (var item in request.Items)
            {
                var medication = await _context.Medications.FindAsync(item.MedicationId);
                if (medication == null)
                    throw new ValidationException($"Invalid medication: {item.MedicationId}");

                if (!medication.IsActive)
                    throw new ValidationException($"Medication is inactive: {medication.Name}");
            }

            // Validate clinical requirements
            await _clinicalService.ValidatePrescriptionAsync(request);
        }

        private async Task<string> GeneratePrescriptionNumberAsync()
        {
            var prefix = "RX";
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var sequence = await _context.Prescriptions
                .Where(p => p.PrescriptionNumber.StartsWith($"{prefix}{date}"))
                .CountAsync() + 1;

            return $"{prefix}{date}{sequence:D4}";
        }

        private async Task CreatePrescriptionDocumentsAsync(
            Prescription prescription,
            IEnumerable<DocumentRequest> documents)
        {
            foreach (var doc in documents)
            {
                var document = new PrescriptionDocument
                {
                    PrescriptionId = prescription.Id,
                    DocumentType = doc.DocumentType,
                    Description = doc.Description
                };

                // Process document content
                document.DocumentPath = await _documentService.StoreDocumentAsync(
                    doc.Content,
                    doc.DocumentType);

                _context.PrescriptionDocuments.Add(document);
            }

            await _context.SaveChangesAsync();
        }

        private async Task NotifyPrescriptionCreationAsync(Prescription prescription)
        {
            // Notify prescribing provider
            await _notificationService.SendProviderNotificationAsync(
                prescription.ProviderId,
                "Prescription Created",
                $"Prescription {prescription.PrescriptionNumber} has been created");

            // Notify pharmacy if assigned
            if (prescription.PharmacyId.HasValue)
            {
                await _notificationService.SendPharmacyNotificationAsync(
                    prescription.PharmacyId.Value,
                    "New Prescription",
                    $"New prescription received: {prescription.PrescriptionNumber}");
            }
        }

        private async Task ValidatePrescriptionForSubmissionAsync(Prescription prescription)
        {
            // Check approval if required
            if (prescription.RequiresApproval && prescription.ApprovalStatus != "Approved")
                throw new ValidationException("Prescription requires approval");

            // Validate insurance if needed
            if (prescription.Items.Any(i => i.Medication.RequiresPreAuthorization))
            {
                await ValidateInsuranceAuthorizationAsync(prescription);
            }

            // Validate controlled substance requirements
            if (prescription.IsControlled)
            {
                await ValidateControlledSubstanceRequirementsAsync(prescription);
            }
        }

        private async Task ValidateRefillEligibilityAsync(PrescriptionItem item)
        {
            // Check if refills remaining
            if (item.RefillsRemaining <= 0)
                throw new ValidationException("No refills remaining");

            // Check if prescription is active
            if (!item.IsActive)
                throw new ValidationException("Prescription is not active");

            // Check expiration
            if (item.ExpirationDate <= DateTime.UtcNow)
                throw new ValidationException("Prescription has expired");

            // Check for controlled substance restrictions
            if (item.Medication.IsControlled)
            {
                await ValidateControlledSubstanceRefillAsync(item);
            }
        }

        private async Task NotifyRefillAuthorizationRequestAsync(PrescriptionFill fill)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Provider)
                .FirstOrDefaultAsync(p => p.Items.Any(i => i.Id == fill.PrescriptionItemId));

            if (prescription != null)
            {
                await _notificationService.SendProviderNotificationAsync(
                    prescription.ProviderId,
                    "Refill Authorization Request",
                    $"Refill authorization requested for prescription {prescription.PrescriptionNumber}");
            }
        }

        private async Task<IEnumerable<PrescriptionItem>> GetActivePatientMedicationsAsync(
            int patientId)
        {
            return await _context.PrescriptionItems
                .Include(p => p.Prescription)
                .Include(p => p.Medication)
                .Where(p => p.Prescription.PatientId == patientId &&
                           p.IsActive &&
                           p.Prescription.Status != "Cancelled")
                .ToListAsync();
        }
    }
}
