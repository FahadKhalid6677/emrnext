using System;
using System.Threading.Tasks;
using EMRNext.Core.Models;

namespace EMRNext.Core.Services.Interfaces
{
    /// <summary>
    /// Interface for core clinical service operations
    /// </summary>
    public interface IClinicalService
    {
        /// <summary>
        /// Create a new clinical encounter
        /// </summary>
        Task<Encounter> CreateEncounterAsync(int patientId, string encounterType, string diagnosis);

        /// <summary>
        /// Update an existing clinical encounter
        /// </summary>
        Task<Encounter> UpdateEncounterAsync(int encounterId, string diagnosis, string notes);

        /// <summary>
        /// Get patient vitals
        /// </summary>
        Task<object> GetVitalsAsync(int patientId);

        /// <summary>
        /// Get encounter-specific vitals
        /// </summary>
        Task<object> GetEncounterVitalsAsync(int encounterId);

        /// <summary>
        /// Add a clinical note to an encounter
        /// </summary>
        Task<bool> AddClinicalNoteAsync(int encounterId, string note);

        /// <summary>
        /// Create a new clinical order
        /// </summary>
        Task<Order> CreateOrderAsync(int encounterId, string orderType, string description);

        /// <summary>
        /// Get patient alerts
        /// </summary>
        Task<ClinicalAlert[]> GetPatientAlertsAsync(int patientId);

        /// <summary>
        /// Get clinical recommendations
        /// </summary>
        Task<ClinicalRecommendation[]> GetClinicalRecommendationsAsync(int patientId);
    }
}
