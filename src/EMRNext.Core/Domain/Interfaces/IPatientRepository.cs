using System.Threading.Tasks;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Interfaces;

namespace EMRNext.Core.Domain.Repositories
{
    /// <summary>
    /// Repository for patient-specific operations
    /// </summary>
    public interface IPatientRepository : IIntRepository<Patient>
    {
        /// <summary>
        /// Find patients by name
        /// </summary>
        Task<IReadOnlyList<Patient>> FindByNameAsync(string searchTerm);

        /// <summary>
        /// Get patients by age range
        /// </summary>
        Task<IReadOnlyList<Patient>> FindByAgeRangeAsync(int minAge, int maxAge);

        /// <summary>
        /// Get patients by primary care provider
        /// </summary>
        Task<IReadOnlyList<Patient>> FindByPrimaryProviderAsync(string providerId);

        /// <summary>
        /// Check if patient exists by social security number
        /// </summary>
        Task<bool> ExistsBySocialSecurityNumberAsync(string ssn);

        /// <summary>
        /// Get patient with full medical history
        /// </summary>
        Task<Patient> GetPatientWithFullHistoryAsync(int patientId);
    }
}
