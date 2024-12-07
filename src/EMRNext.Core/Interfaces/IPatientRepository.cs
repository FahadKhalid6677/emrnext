using EMRNext.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMRNext.Core.Interfaces
{
    public interface IPatientRepository
    {
        Task<Patient> GetByIdAsync(int id);
        Task<Patient> GetByPublicIdAsync(Guid publicId);
        Task<IEnumerable<Patient>> SearchAsync(string searchTerm, int skip = 0, int take = 20);
        Task<Patient> CreateAsync(Patient patient);
        Task<Patient> UpdateAsync(Patient patient);
        Task<IEnumerable<Encounter>> GetEncountersAsync(int patientId);
        Task<IEnumerable<Document>> GetDocumentsAsync(int patientId);
        Task<IEnumerable<Insurance>> GetInsurancesAsync(int patientId);
        Task<IEnumerable<Allergy>> GetAllergiesAsync(int patientId);
        Task<IEnumerable<Problem>> GetProblemsAsync(int patientId);
        Task<IEnumerable<Medication>> GetMedicationsAsync(int patientId);
        Task<IEnumerable<Immunization>> GetImmunizationsAsync(int patientId);
        Task<IEnumerable<LabResult>> GetLabResultsAsync(int patientId);
        Task<IEnumerable<Vital>> GetVitalsAsync(int patientId);

        // Vitals Management
        Task<Vital> AddVitalAsync(Vital vital);
        Task<Vital> GetVitalAsync(int patientId, int vitalId);
        Task<IEnumerable<Vital>> GetPatientVitalsAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<Vital> UpdateVitalAsync(Vital vital);
        Task DeleteVitalAsync(Vital vital);

        // Allergies Management
        Task<Allergy> AddAllergyAsync(Allergy allergy);
        Task<Allergy> GetAllergyAsync(int patientId, int allergyId);
        Task<IEnumerable<Allergy>> GetPatientAllergiesAsync(int patientId);
        Task<Allergy> UpdateAllergyAsync(Allergy allergy);
        Task DeleteAllergyAsync(Allergy allergy);

        // Problems Management
        Task<Problem> AddProblemAsync(Problem problem);
        Task<Problem> GetProblemAsync(int patientId, int problemId);
        Task<IEnumerable<Problem>> GetPatientProblemsAsync(int patientId, bool includeResolved = false);
        Task<Problem> UpdateProblemAsync(Problem problem);
        Task DeleteProblemAsync(Problem problem);
    }
}
