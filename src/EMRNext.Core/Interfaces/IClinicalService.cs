using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Interfaces
{
    public interface IClinicalService
    {
        // Encounter Management
        Task<Encounter> CreateEncounterAsync(Encounter encounter);
        Task<Encounter> UpdateEncounterAsync(int encounterId, Encounter encounter);
        Task<Encounter> GetEncounterAsync(int encounterId);
        Task<IEnumerable<Encounter>> GetPatientEncountersAsync(int patientId);
        
        // Clinical Documentation
        Task<ClinicalNote> CreateClinicalNoteAsync(ClinicalNote note);
        Task<ClinicalNote> UpdateClinicalNoteAsync(int noteId, ClinicalNote note);
        Task<IEnumerable<ClinicalNote>> GetEncounterNotesAsync(int encounterId);
        Task<IEnumerable<ClinicalNote>> GetPatientNotesAsync(int patientId);
        
        // Vital Signs
        Task<Vital> RecordVitalsAsync(Vital vitals);
        Task<IEnumerable<Vital>> GetPatientVitalsHistoryAsync(int patientId);
        Task<Vital> GetLatestVitalsAsync(int patientId);
        
        // Orders Management
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderStatusAsync(int orderId, string status);
        Task<IEnumerable<Order>> GetPendingOrdersAsync(int patientId);
        Task<IEnumerable<Order>> GetOrdersByEncounterAsync(int encounterId);
        
        // Results Management
        Task<Result> RecordResultAsync(Result result);
        Task<Result> UpdateResultAsync(int resultId, Result result);
        Task<IEnumerable<Result>> GetOrderResultsAsync(int orderId);
        Task<IEnumerable<Result>> GetPatientResultsAsync(int patientId);
        
        // Prescription Management
        Task<Prescription> CreatePrescriptionAsync(Prescription prescription);
        Task<Prescription> UpdatePrescriptionAsync(int prescriptionId, Prescription prescription);
        Task<IEnumerable<Prescription>> GetActivePrescriptionsAsync(int patientId);
        Task<bool> VerifyPrescriptionInteractionsAsync(Prescription prescription);
        
        // Clinical Decision Support
        Task<IEnumerable<Alert>> GetClinicalAlertsAsync(int patientId);
        Task<IEnumerable<Recommendation>> GetCareRecommendationsAsync(int patientId);
        
        // Problem List Management
        Task<Problem> AddProblemAsync(Problem problem);
        Task<Problem> UpdateProblemStatusAsync(int problemId, string status);
        Task<IEnumerable<Problem>> GetActiveProblemListAsync(int patientId);
        
        // Allergy Management
        Task<Allergy> RecordAllergyAsync(Allergy allergy);
        Task<IEnumerable<Allergy>> GetPatientAllergiesAsync(int patientId);
        Task<bool> VerifyAllergicInteractionsAsync(int patientId, string medication);
        
        // Immunization Management
        Task<Immunization> RecordImmunizationAsync(Immunization immunization);
        Task<IEnumerable<Immunization>> GetImmunizationHistoryAsync(int patientId);
        Task<IEnumerable<Immunization>> GetDueImmunizationsAsync(int patientId);
    }
}
