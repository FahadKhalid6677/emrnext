using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using EMRNext.Core.Models;
using EMRNext.Core.Domain.Interfaces;

namespace EMRNext.Core.Domain.Repositories
{
    /// <summary>
    /// Repository for prescription-specific operations
    /// </summary>
    public interface IPrescriptionRepository : IIntRepository<Prescription>
    {
        /// <summary>
        /// Get active prescriptions for a patient
        /// </summary>
        Task<IReadOnlyList<Prescription>> GetActivePatientPrescriptionsAsync(int patientId);

        /// <summary>
        /// Get prescriptions by medication name
        /// </summary>
        Task<IReadOnlyList<Prescription>> FindByMedicationNameAsync(string medicationName);

        /// <summary>
        /// Get prescriptions due for refill
        /// </summary>
        Task<IReadOnlyList<Prescription>> GetPrescriptionsDueForRefillAsync();

        /// <summary>
        /// Check for potential drug interactions
        /// </summary>
        Task<IReadOnlyList<DrugInteraction>> CheckDrugInteractionsAsync(List<string> medications);

        /// <summary>
        /// Get prescription with fill history
        /// </summary>
        Task<Prescription> GetPrescriptionWithFillHistoryAsync(int prescriptionId);
    }
}
