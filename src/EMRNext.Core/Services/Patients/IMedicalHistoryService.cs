using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Models;

namespace EMRNext.Core.Services.Patients
{
    public interface IMedicalHistoryService
    {
        Task<MedicalHistory> AddMedicalHistoryAsync(MedicalHistoryRequest request);
        Task<IEnumerable<MedicalHistory>> GetPatientMedicalHistoryAsync(string patientId);
        Task<Allergy> AddAllergyAsync(AllergyRequest request);
        Task<IEnumerable<Allergy>> GetPatientAllergiesAsync(string patientId);
        Task<Problem> AddProblemAsync(ProblemRequest request);
        Task<IEnumerable<Problem>> GetPatientProblemsAsync(string patientId);
        Task<Medication> AddMedicationAsync(MedicationRequest request);
        Task<IEnumerable<Medication>> GetPatientMedicationsAsync(string patientId);
        Task<bool> UpdateProblemStatusAsync(string problemId, ProblemStatus status);
        Task<bool> UpdateMedicationStatusAsync(string medicationId, MedicationStatus status);
    }
}
