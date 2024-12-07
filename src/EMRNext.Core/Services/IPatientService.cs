using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Models;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services
{
    /// <summary>
    /// Service interface for patient-related operations
    /// </summary>
    public interface IPatientService
    {
        /// <summary>
        /// Register a new patient
        /// </summary>
        Task<Patient> RegisterPatientAsync(Patient patient);

        /// <summary>
        /// Update existing patient information
        /// </summary>
        Task<Patient> UpdatePatientAsync(Patient patient);

        /// <summary>
        /// Get patient by unique identifier
        /// </summary>
        Task<Patient> GetPatientByIdAsync(Guid patientId);

        /// <summary>
        /// Search patients based on various criteria
        /// </summary>
        Task<IEnumerable<Patient>> SearchPatientsAsync(
            string firstName = null, 
            string lastName = null, 
            DateTime? dateOfBirth = null);

        /// <summary>
        /// Add medical record to patient
        /// </summary>
        Task AddMedicalRecordAsync(Guid patientId, MedicalRecord medicalRecord);

        /// <summary>
        /// Add encounter to patient
        /// </summary>
        Task AddEncounterAsync(Guid patientId, Encounter encounter);

        /// <summary>
        /// Check if patient exists
        /// </summary>
        Task<bool> PatientExistsAsync(Guid patientId);

        /// <summary>
        /// Soft delete a patient
        /// </summary>
        Task DeletePatientAsync(Guid patientId);

        Task<Patient> GetByIdAsync(int id);
        Task<Patient> GetByPublicIdAsync(Guid publicId);
        Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm, int skip = 0, int take = 20);
        Task<Patient> CreatePatientAsync(Patient patient);
        Task<Patient> UpdatePatientAsync(Patient patient);
        Task<bool> DeactivatePatientAsync(int id);
        Task<bool> ActivatePatientAsync(int id);
        Task<IEnumerable<Encounter>> GetPatientEncountersAsync(int patientId);
        Task<IEnumerable<Document>> GetPatientDocumentsAsync(int patientId);
        Task<IEnumerable<Insurance>> GetPatientInsurancesAsync(int patientId);
        Task<IEnumerable<Allergy>> GetPatientAllergiesAsync(int patientId);
        Task<IEnumerable<Problem>> GetPatientProblemsAsync(int patientId);
        Task<IEnumerable<Medication>> GetPatientMedicationsAsync(int patientId);
        Task<IEnumerable<Immunization>> GetPatientImmunizationsAsync(int patientId);
        Task<IEnumerable<LabResult>> GetPatientLabResultsAsync(int patientId);
        Task<IEnumerable<Vital>> GetPatientVitalsAsync(int patientId);
        Task<bool> ValidatePatientDataAsync(Patient patient);
        Task<bool> CheckDuplicatePatientAsync(Patient patient);

        // Vitals Management
        Task<Vital> AddVitalAsync(int patientId, Vital vital);
        Task<Vital> GetVitalAsync(int patientId, int vitalId);
        Task<IEnumerable<Vital>> GetPatientVitalsAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<Vital> UpdateVitalAsync(int patientId, int vitalId, Vital vital);
        Task DeleteVitalAsync(int patientId, int vitalId);

        // Allergies Management
        Task<Allergy> AddAllergyAsync(int patientId, Allergy allergy);
        Task<Allergy> GetAllergyAsync(int patientId, int allergyId);
        Task<IEnumerable<Allergy>> GetPatientAllergiesAsync(int patientId);
        Task<Allergy> UpdateAllergyAsync(int patientId, int allergyId, Allergy allergy);
        Task DeleteAllergyAsync(int patientId, int allergyId);

        // Problems Management
        Task<Problem> AddProblemAsync(int patientId, Problem problem);
        Task<Problem> GetProblemAsync(int patientId, int problemId);
        Task<IEnumerable<Problem>> GetPatientProblemsAsync(int patientId, bool includeResolved = false);
        Task<Problem> UpdateProblemAsync(int patientId, int problemId, Problem problem);
        Task DeleteProblemAsync(int patientId, int problemId);
    }

    /// <summary>
    /// Implementation of patient service
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly IGenericRepository<Patient> _patientRepository;
        private readonly IDomainEventDispatcher _eventDispatcher;

        public PatientService(
            IGenericRepository<Patient> patientRepository,
            IDomainEventDispatcher eventDispatcher)
        {
            _patientRepository = patientRepository;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<Patient> RegisterPatientAsync(Patient patient)
        {
            await _patientRepository.AddAsync(patient);
            
            // Dispatch domain event
            await _eventDispatcher.DispatchAsync(new PatientRegisteredEvent
            {
                PatientId = patient.Id,
                FirstName = patient.Name.FirstName,
                LastName = patient.Name.LastName,
                DateOfBirth = patient.Demographics.DateOfBirth
            });

            return patient;
        }

        // Implement other methods...
    }
}
