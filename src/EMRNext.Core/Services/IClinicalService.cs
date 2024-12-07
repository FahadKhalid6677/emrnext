using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services
{
    public interface IClinicalService
    {
        // Encounter Management
        Task<Encounter> CreateEncounterAsync(EncounterCreation request);
        Task<Encounter> GetEncounterAsync(int id);
        Task<Encounter> UpdateEncounterAsync(int id, EncounterUpdate request);
        Task<List<Encounter>> GetPatientEncountersAsync(int patientId);
        Task<bool> DeleteEncounterAsync(int id);

        // Vitals Management
        Task<Vital> AddVitalsAsync(int encounterId, VitalsCreation request);
        Task<Vital> GetVitalsAsync(int id);
        Task<List<Vital>> GetEncounterVitalsAsync(int encounterId);
        Task<List<Vital>> GetPatientVitalsHistoryAsync(int patientId, DateTime? startDate = null, DateTime? endDate = null);

        // Clinical Notes
        Task<ClinicalNote> AddClinicalNoteAsync(int encounterId, ClinicalNoteCreation request);
        Task<ClinicalNote> GetClinicalNoteAsync(int id);
        Task<List<ClinicalNote>> GetEncounterNotesAsync(int encounterId);
        Task<List<ClinicalNote>> GetPatientNotesHistoryAsync(int patientId, DateTime? startDate = null, DateTime? endDate = null);

        // Orders Management
        Task<Order> CreateOrderAsync(int encounterId, OrderCreation request);
        Task<Order> GetOrderAsync(int id);
        Task<Order> UpdateOrderStatusAsync(int id, string status);
        Task<List<Order>> GetEncounterOrdersAsync(int encounterId);
        Task<List<Order>> GetPatientOrdersHistoryAsync(int patientId, DateTime? startDate = null, DateTime? endDate = null);

        // Prescriptions
        Task<Prescription> CreatePrescriptionAsync(int encounterId, PrescriptionCreation request);
        Task<Prescription> GetPrescriptionAsync(int id);
        Task<Prescription> UpdatePrescriptionAsync(int id, PrescriptionUpdate request);
        Task<List<Prescription>> GetEncounterPrescriptionsAsync(int encounterId);
        Task<List<Prescription>> GetPatientPrescriptionsHistoryAsync(int patientId, bool activeOnly = false);

        // Clinical Decision Support
        Task<List<Alert>> GetPatientAlertsAsync(int patientId);
        Task<List<Recommendation>> GetClinicalRecommendationsAsync(int encounterId);
        Task<List<DrugInteraction>> CheckDrugInteractionsAsync(List<string> medicationCodes);
    }

    public class EncounterCreation
    {
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int? SupervisorId { get; set; }
        public int FacilityId { get; set; }
        public DateTime Date { get; set; }
        public string ClassCode { get; set; }
        public string Type { get; set; }
        public string Reason { get; set; }
        public string ChiefComplaint { get; set; }
        public string ReferralSource { get; set; }
        public int? AppointmentCategoryId { get; set; }
    }

    public class EncounterUpdate
    {
        public string Type { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string ChiefComplaint { get; set; }
        public string SubjectiveNotes { get; set; }
        public string ObjectiveNotes { get; set; }
        public string AssessmentNotes { get; set; }
        public string PlanNotes { get; set; }
        public string AdditionalNotes { get; set; }
        public string DischargeDisposition { get; set; }
        public DateTime? DischargeDate { get; set; }
        public string DischargeNotes { get; set; }
    }

    public class VitalsCreation
    {
        public decimal? Temperature { get; set; }
        public string TemperatureUnit { get; set; }
        public decimal? Pulse { get; set; }
        public decimal? RespiratoryRate { get; set; }
        public decimal? BloodPressureSystolic { get; set; }
        public decimal? BloodPressureDiastolic { get; set; }
        public decimal? BloodPressurePosition { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public decimal? InhaledOxygenConcentration { get; set; }
        public decimal? Height { get; set; }
        public string HeightUnit { get; set; }
        public decimal? Weight { get; set; }
        public string WeightUnit { get; set; }
        public decimal? WaistCircumference { get; set; }
        public string WaistCircumferenceUnit { get; set; }
        public decimal? HeadCircumference { get; set; }
        public string HeadCircumferenceUnit { get; set; }
        public string PulseRhythm { get; set; }
        public string PulseLocation { get; set; }
        public string Notes { get; set; }
    }

    public class ClinicalNoteCreation
    {
        public string Type { get; set; }
        public string Content { get; set; }
        public List<string> DiagnosisCodes { get; set; }
        public List<string> ProcedureCodes { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class OrderCreation
    {
        public string OrderType { get; set; }
        public string OrderCode { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Frequency { get; set; }
        public string Priority { get; set; }
        public List<string> DiagnosisCodes { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
    }

    public class PrescriptionCreation
    {
        public string MedicationCode { get; set; }
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public string Route { get; set; }
        public string Frequency { get; set; }
        public string Instructions { get; set; }
        public int Quantity { get; set; }
        public int Refills { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsPRN { get; set; }
        public string PRNInstructions { get; set; }
        public List<string> DiagnosisCodes { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
    }

    public class PrescriptionUpdate
    {
        public string Status { get; set; }
        public int? RefillsRemaining { get; set; }
        public DateTime? EndDate { get; set; }
        public string Instructions { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
    }
}
