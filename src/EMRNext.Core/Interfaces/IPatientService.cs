using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Interfaces
{
    public interface IPatientService
    {
        Task<Patient> RegisterPatientAsync(Patient patient);
        Task<Patient> UpdatePatientAsync(int patientId, Patient updatedPatient);
        Task<Patient> GetPatientAsync(int patientId);
        Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm);
        Task<bool> VerifyInsuranceAsync(int patientId, int insuranceId);
        Task<IEnumerable<Insurance>> GetActiveInsurancesAsync(int patientId);
        Task<IEnumerable<Document>> GetPatientDocumentsAsync(int patientId);
        Task<IEnumerable<AuditLog>> GetPatientHistoryAsync(int patientId);
    }
}
