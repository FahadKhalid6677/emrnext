using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Validation;
using EMRNext.Core.Exceptions;

namespace EMRNext.Core.Services
{
    public class ClinicalService : IClinicalService
    {
        private readonly IClinicalRepository _clinicalRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ILoggingService _loggingService;
        private readonly EncounterValidator _encounterValidator;
        private readonly ClinicalNoteValidator _noteValidator;
        private readonly OrderValidator _orderValidator;
        private readonly PrescriptionValidator _prescriptionValidator;
        private readonly VitalValidator _vitalValidator;
        private readonly ResultValidator _resultValidator;

        public ClinicalService(
            IClinicalRepository clinicalRepository,
            IPatientRepository patientRepository,
            ILoggingService loggingService)
        {
            _clinicalRepository = clinicalRepository;
            _patientRepository = patientRepository;
            _loggingService = loggingService;
            _encounterValidator = new EncounterValidator();
            _noteValidator = new ClinicalNoteValidator();
            _orderValidator = new OrderValidator();
            _prescriptionValidator = new PrescriptionValidator();
            _vitalValidator = new VitalValidator();
            _resultValidator = new ResultValidator();
        }

        // Encounter Management
        public async Task<Encounter> CreateEncounterAsync(Encounter encounter)
        {
            try
            {
                var validationResult = await _encounterValidator.ValidateAsync(encounter);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

                var patient = await _patientRepository.GetByIdAsync(encounter.PatientId);
                if (patient == null)
                    throw new NotFoundException($"Patient with ID {encounter.PatientId} not found");

                // Set default values and status
                encounter.Status = EncounterStatus.Open;
                encounter.CreatedDate = DateTime.UtcNow;
                encounter.LastModified = DateTime.UtcNow;

                var result = await _clinicalRepository.CreateEncounterAsync(encounter);
                await _loggingService.LogAuditAsync(
                    "CreateEncounter",
                    "Encounter",
                    result.Id.ToString(),
                    $"Created encounter for patient {patient.Id}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error creating encounter for patient {PatientId}", encounter.PatientId);
                throw;
            }
        }

        public async Task<Encounter> UpdateEncounterAsync(int encounterId, Encounter encounter)
        {
            try
            {
                var existingEncounter = await _clinicalRepository.GetEncounterAsync(encounterId);
                if (existingEncounter == null)
                    throw new NotFoundException($"Encounter with ID {encounterId} not found");

                var validationResult = await _encounterValidator.ValidateAsync(encounter);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

                // Preserve original creation date and update modification date
                encounter.CreatedDate = existingEncounter.CreatedDate;
                encounter.LastModified = DateTime.UtcNow;

                var result = await _clinicalRepository.UpdateEncounterAsync(encounterId, encounter);
                await _loggingService.LogAuditAsync(
                    "UpdateEncounter",
                    "Encounter",
                    result.Id.ToString(),
                    $"Updated encounter {result.Id}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error updating encounter {EncounterId}", encounterId);
                throw;
            }
        }

        public async Task<Encounter> GetEncounterAsync(int id)
        {
            try
            {
                var encounter = await _clinicalRepository.GetEncounterAsync(id);
                if (encounter == null)
                    throw new NotFoundException($"Encounter with ID {id} not found");
                return encounter;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving encounter {EncounterId}", id);
                throw;
            }
        }

        public async Task<List<Encounter>> GetPatientEncountersAsync(int patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                    throw new NotFoundException($"Patient with ID {patientId} not found");

                return await _clinicalRepository.GetPatientEncountersAsync(patientId);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving encounters for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<bool> DeleteEncounterAsync(int id)
        {
            try
            {
                var encounter = await _clinicalRepository.GetEncounterAsync(id);
                if (encounter == null)
                    throw new NotFoundException($"Encounter with ID {id} not found");

                var result = await _clinicalRepository.DeleteEncounterAsync(id);
                await _loggingService.LogAuditAsync(
                    "DeleteEncounter",
                    "Encounter",
                    id.ToString(),
                    $"Deleted encounter {id}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error deleting encounter {EncounterId}", id);
                throw;
            }
        }

        // Clinical Documentation
        public async Task<ClinicalNote> CreateClinicalNoteAsync(ClinicalNote note)
        {
            try
            {
                var validationResult = await _noteValidator.ValidateAsync(note);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

                var encounter = await _clinicalRepository.GetEncounterAsync(note.EncounterId);
                if (encounter == null)
                    throw new NotFoundException($"Encounter with ID {note.EncounterId} not found");

                if (encounter.Status != EncounterStatus.Open)
                    throw new BusinessRuleException("Cannot add notes to a closed encounter");

                note.CreatedDate = DateTime.UtcNow;
                note.LastModified = DateTime.UtcNow;

                var result = await _clinicalRepository.CreateClinicalNoteAsync(note);
                await _loggingService.LogAuditAsync(
                    "CreateClinicalNote",
                    "ClinicalNote",
                    result.Id.ToString(),
                    $"Created clinical note for encounter {encounter.Id}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error creating clinical note for encounter {EncounterId}", note.EncounterId);
                throw;
            }
        }

        public async Task<ClinicalNote> GetClinicalNoteAsync(int id)
        {
            try
            {
                var note = await _clinicalRepository.GetClinicalNoteAsync(id);
                if (note == null)
                    throw new NotFoundException($"Clinical note with ID {id} not found");
                return note;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving clinical note {NoteId}", id);
                throw;
            }
        }

        public async Task<List<ClinicalNote>> GetEncounterNotesAsync(int encounterId)
        {
            try
            {
                var encounter = await _clinicalRepository.GetEncounterAsync(encounterId);
                if (encounter == null)
                    throw new NotFoundException($"Encounter with ID {encounterId} not found");

                return await _clinicalRepository.GetEncounterNotesAsync(encounterId);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving notes for encounter {EncounterId}", encounterId);
                throw;
            }
        }

        public async Task<List<ClinicalNote>> GetPatientNotesHistoryAsync(int patientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                    throw new NotFoundException($"Patient with ID {patientId} not found");

                return await _clinicalRepository.GetPatientNotesHistoryAsync(patientId, startDate, endDate);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving notes history for patient {PatientId}", patientId);
                throw;
            }
        }

        // Order Management
        public async Task<Order> CreateOrderAsync(Order order)
        {
            try
            {
                var validationResult = await _orderValidator.ValidateAsync(order);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

                // Check authorization requirements
                if (RequiresPreAuthorization(order))
                {
                    var authorized = await VerifyAuthorizationAsync(order);
                    if (!authorized)
                        throw new AuthorizationException("Order requires pre-authorization");
                }

                // Check for contraindications
                var contraindications = await CheckContraindicationsAsync(order);
                if (contraindications.Any())
                    throw new BusinessRuleException($"Contraindications found: {string.Join(", ", contraindications)}");

                order.Status = OrderStatus.Pending;
                order.CreatedDate = DateTime.UtcNow;
                order.LastModified = DateTime.UtcNow;

                var result = await _clinicalRepository.CreateOrderAsync(order);
                await _loggingService.LogAuditAsync(
                    "CreateOrder",
                    "Order",
                    result.Id.ToString(),
                    $"Created order for encounter {order.EncounterId}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error creating order for encounter {EncounterId}", order.EncounterId);
                throw;
            }
        }

        public async Task<Order> GetOrderAsync(int id)
        {
            try
            {
                var order = await _clinicalRepository.GetOrderAsync(id);
                if (order == null)
                    throw new NotFoundException($"Order with ID {id} not found");
                return order;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving order {OrderId}", id);
                throw;
            }
        }

        public async Task<Order> UpdateOrderStatusAsync(int id, string status)
        {
            try
            {
                var order = await _clinicalRepository.GetOrderAsync(id);
                if (order == null)
                    throw new NotFoundException($"Order with ID {id} not found");

                order.Status = status;
                order.LastModified = DateTime.UtcNow;

                var result = await _clinicalRepository.UpdateOrderAsync(id, order);
                await _loggingService.LogAuditAsync(
                    "UpdateOrderStatus",
                    "Order",
                    id.ToString(),
                    $"Updated order status to {status}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error updating order status {OrderId}", id);
                throw;
            }
        }

        public async Task<List<Order>> GetEncounterOrdersAsync(int encounterId)
        {
            try
            {
                var encounter = await _clinicalRepository.GetEncounterAsync(encounterId);
                if (encounter == null)
                    throw new NotFoundException($"Encounter with ID {encounterId} not found");

                return await _clinicalRepository.GetEncounterOrdersAsync(encounterId);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving orders for encounter {EncounterId}", encounterId);
                throw;
            }
        }

        public async Task<List<Order>> GetPatientOrdersHistoryAsync(int patientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                    throw new NotFoundException($"Patient with ID {patientId} not found");

                return await _clinicalRepository.GetPatientOrdersHistoryAsync(patientId, startDate, endDate);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving orders history for patient {PatientId}", patientId);
                throw;
            }
        }

        // Prescription Management
        public async Task<Prescription> CreatePrescriptionAsync(int encounterId, PrescriptionCreation request)
        {
            try
            {
                var encounter = await _clinicalRepository.GetEncounterAsync(encounterId);
                if (encounter == null)
                    throw new NotFoundException($"Encounter with ID {encounterId} not found");

                var prescription = new Prescription
                {
                    EncounterId = encounterId,
                    PatientId = encounter.PatientId,
                    MedicationCode = request.MedicationCode,
                    MedicationName = request.MedicationName,
                    Dosage = request.Dosage,
                    Route = request.Route,
                    Frequency = request.Frequency,
                    Instructions = request.Instructions,
                    Quantity = request.Quantity,
                    Refills = request.Refills,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsPRN = request.IsPRN,
                    PRNInstructions = request.PRNInstructions,
                    DiagnosisCodes = request.DiagnosisCodes,
                    CustomFields = request.CustomFields,
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                };

                var validationResult = await _prescriptionValidator.ValidateAsync(prescription);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

                // Check for drug interactions
                var interactions = await CheckDrugInteractionsAsync(new[] { prescription });
                if (interactions.Any())
                {
                    throw new BusinessRuleException($"Drug interactions found: {string.Join(", ", interactions.Select(i => i.Message))}");
                }

                var result = await _clinicalRepository.CreatePrescriptionAsync(prescription);
                await _loggingService.LogAuditAsync(
                    "CreatePrescription",
                    "Prescription",
                    result.Id.ToString(),
                    $"Created prescription for encounter {encounterId}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error creating prescription for encounter {EncounterId}", encounterId);
                throw;
            }
        }

        public async Task<Prescription> GetPrescriptionAsync(int id)
        {
            try
            {
                var prescription = await _clinicalRepository.GetPrescriptionAsync(id);
                if (prescription == null)
                    throw new NotFoundException($"Prescription with ID {id} not found");
                return prescription;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving prescription {PrescriptionId}", id);
                throw;
            }
        }

        public async Task<Prescription> UpdatePrescriptionAsync(int id, PrescriptionUpdate request)
        {
            try
            {
                var prescription = await _clinicalRepository.GetPrescriptionAsync(id);
                if (prescription == null)
                    throw new NotFoundException($"Prescription with ID {id} not found");

                prescription.Status = request.Status ?? prescription.Status;
                prescription.RefillsRemaining = request.RefillsRemaining ?? prescription.RefillsRemaining;
                prescription.EndDate = request.EndDate ?? prescription.EndDate;
                prescription.Instructions = request.Instructions ?? prescription.Instructions;
                prescription.CustomFields = request.CustomFields ?? prescription.CustomFields;
                prescription.LastModified = DateTime.UtcNow;

                var validationResult = await _prescriptionValidator.ValidateAsync(prescription);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

                var result = await _clinicalRepository.UpdatePrescriptionAsync(id, prescription);
                await _loggingService.LogAuditAsync(
                    "UpdatePrescription",
                    "Prescription",
                    id.ToString(),
                    $"Updated prescription {id}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error updating prescription {PrescriptionId}", id);
                throw;
            }
        }

        public async Task<List<Prescription>> GetEncounterPrescriptionsAsync(int encounterId)
        {
            try
            {
                var encounter = await _clinicalRepository.GetEncounterAsync(encounterId);
                if (encounter == null)
                    throw new NotFoundException($"Encounter with ID {encounterId} not found");

                return await _clinicalRepository.GetEncounterPrescriptionsAsync(encounterId);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving prescriptions for encounter {EncounterId}", encounterId);
                throw;
            }
        }

        public async Task<List<Prescription>> GetPatientPrescriptionsHistoryAsync(int patientId, bool activeOnly = false)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                    throw new NotFoundException($"Patient with ID {patientId} not found");

                return await _clinicalRepository.GetPatientPrescriptionsHistoryAsync(patientId, activeOnly);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving prescriptions history for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<List<DrugInteraction>> CheckDrugInteractionsAsync(List<string> medicationCodes)
        {
            try
            {
                // This would typically integrate with a drug interaction service
                // For now, return an empty list as a placeholder
                return new List<DrugInteraction>();
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error checking drug interactions");
                throw;
            }
        }

        // Results Management
        public async Task<Result> RecordResultAsync(Result result)
        {
            try
            {
                var validationResult = await _resultValidator.ValidateAsync(result);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

                var order = await _clinicalRepository.GetOrderAsync(result.OrderId);
                if (order == null)
                    throw new NotFoundException($"Order with ID {result.OrderId} not found");

                result.CreatedDate = DateTime.UtcNow;
                result.LastModified = DateTime.UtcNow;

                var savedResult = await _clinicalRepository.RecordResultAsync(result);

                // Check for critical values
                if (IsCriticalResult(savedResult))
                {
                    await HandleCriticalResultAsync(savedResult);
                }

                await _loggingService.LogAuditAsync(
                    "RecordResult",
                    "Result",
                    savedResult.Id.ToString(),
                    $"Recorded result for order {order.Id}"
                );

                return savedResult;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error recording result for order {OrderId}", result.OrderId);
                throw;
            }
        }

        // Vitals Management
        public async Task<Vital> AddVitalsAsync(int encounterId, VitalsCreation request)
        {
            try
            {
                var encounter = await _clinicalRepository.GetEncounterAsync(encounterId);
                if (encounter == null)
                    throw new NotFoundException($"Encounter with ID {encounterId} not found");

                var vital = new Vital
                {
                    EncounterId = encounterId,
                    PatientId = encounter.PatientId,
                    Date = DateTime.UtcNow,
                    Temperature = request.Temperature,
                    TemperatureUnit = request.TemperatureUnit,
                    Pulse = request.Pulse,
                    RespiratoryRate = request.RespiratoryRate,
                    BloodPressureSystolic = request.BloodPressureSystolic,
                    BloodPressureDiastolic = request.BloodPressureDiastolic,
                    BloodPressurePosition = request.BloodPressurePosition,
                    OxygenSaturation = request.OxygenSaturation,
                    InhaledOxygenConcentration = request.InhaledOxygenConcentration,
                    Height = request.Height,
                    HeightUnit = request.HeightUnit,
                    Weight = request.Weight,
                    WeightUnit = request.WeightUnit,
                    WaistCircumference = request.WaistCircumference,
                    WaistCircumferenceUnit = request.WaistCircumferenceUnit,
                    HeadCircumference = request.HeadCircumference,
                    HeadCircumferenceUnit = request.HeadCircumferenceUnit,
                    PulseRhythm = request.PulseRhythm,
                    PulseLocation = request.PulseLocation,
                    Notes = request.Notes
                };

                var validationResult = await _vitalValidator.ValidateAsync(vital);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

                var result = await _clinicalRepository.AddVitalsAsync(vital);
                await _loggingService.LogAuditAsync(
                    "AddVitals",
                    "Vital",
                    result.Id.ToString(),
                    $"Added vitals for encounter {encounterId}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error adding vitals for encounter {EncounterId}", encounterId);
                throw;
            }
        }

        public async Task<List<Vital>> GetPatientVitalsHistoryAsync(int patientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                    throw new NotFoundException($"Patient with ID {patientId} not found");

                return await _clinicalRepository.GetPatientVitalsHistoryAsync(patientId, startDate, endDate);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving vitals history for patient {PatientId}", patientId);
                throw;
            }
        }

        // Clinical Decision Support
        public async Task<IEnumerable<Alert>> GetClinicalAlertsAsync(int patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                    throw new NotFoundException($"Patient with ID {patientId} not found");

                var alerts = new List<Alert>();

                // Check allergies
                var allergies = await _clinicalRepository.GetPatientAllergiesAsync(patientId);
                var medications = await _clinicalRepository.GetActivePrescriptionsAsync(patientId);
                alerts.AddRange(CheckAllergicInteractions(allergies, medications));

                // Check drug interactions
                alerts.AddRange(await CheckDrugInteractionsAsync(medications));

                // Check due preventive care
                alerts.AddRange(await CheckPreventiveCareAlertsAsync(patient));

                // Check abnormal results
                var recentResults = await _clinicalRepository.GetRecentResultsAsync(patientId);
                alerts.AddRange(CheckAbnormalResults(recentResults));

                return alerts;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error getting clinical alerts for patient {PatientId}", patientId);
                throw;
            }
        }

        // Private helper methods
        private bool RequiresPreAuthorization(Order order)
        {
            // Implementation of authorization rules
            return order.OrderType switch
            {
                OrderType.Imaging => true,
                OrderType.Procedure => true,
                OrderType.Referral => true,
                _ => false
            };
        }

        private async Task<bool> VerifyAuthorizationAsync(Order order)
        {
            // Implementation of authorization verification
            // This would typically involve checking with insurance or internal policies
            return true; // Placeholder
        }

        private async Task<List<string>> CheckContraindicationsAsync(Order order)
        {
            // Implementation of contraindication checking
            return new List<string>(); // Placeholder
        }

        private bool IsCriticalResult(Result result)
        {
            // Implementation of critical result checking
            return false; // Placeholder
        }

        private async Task HandleCriticalResultAsync(Result result)
        {
            // Implementation of critical result handling
            // This would typically involve notifications and workflow triggers
        }

        private IEnumerable<Alert> CheckAllergicInteractions(
            IEnumerable<Allergy> allergies,
            IEnumerable<Prescription> medications)
        {
            // Implementation of allergy interaction checking
            return new List<Alert>(); // Placeholder
        }

        private async Task<IEnumerable<Alert>> CheckDrugInteractionsAsync(
            IEnumerable<Prescription> medications)
        {
            // Implementation of drug interaction checking
            return new List<Alert>(); // Placeholder
        }

        private async Task<IEnumerable<Alert>> CheckPreventiveCareAlertsAsync(Patient patient)
        {
            // Implementation of preventive care alert checking
            return new List<Alert>(); // Placeholder
        }

        private IEnumerable<Alert> CheckAbnormalResults(IEnumerable<Result> results)
        {
            // Implementation of abnormal result checking
            return new List<Alert>(); // Placeholder
        }
    }
}
