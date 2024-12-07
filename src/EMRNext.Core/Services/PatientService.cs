using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMRNext.Core.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<PatientService> _logger;
        private readonly PatientValidator _validator;
        private readonly ICurrentUserService _currentUserService;

        public PatientService(
            IPatientRepository patientRepository,
            ILogger<PatientService> logger,
            PatientValidator validator,
            ICurrentUserService currentUserService)
        {
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<Patient> GetByIdAsync(int id)
        {
            try
            {
                return await _patientRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient with ID: {PatientId}", id);
                throw;
            }
        }

        public async Task<Patient> GetByPublicIdAsync(Guid publicId)
        {
            try
            {
                return await _patientRepository.GetByPublicIdAsync(publicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient with Public ID: {PublicId}", publicId);
                throw;
            }
        }

        public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm, int skip = 0, int take = 20)
        {
            try
            {
                return await _patientRepository.SearchAsync(searchTerm, skip, take);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching patients with term: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            try
            {
                if (!await ValidatePatientDataAsync(patient))
                {
                    throw new ValidationException("Patient data validation failed");
                }

                if (await CheckDuplicatePatientAsync(patient))
                {
                    throw new DuplicatePatientException("A patient with similar details already exists");
                }

                patient.CreatedAt = DateTime.UtcNow;
                patient.IsActive = true;
                patient.PublicId = Guid.NewGuid();

                return await _patientRepository.CreateAsync(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient: {PatientName}", $"{patient.FirstName} {patient.LastName}");
                throw;
            }
        }

        public async Task<Patient> UpdatePatientAsync(Patient patient)
        {
            try
            {
                if (!await ValidatePatientDataAsync(patient))
                {
                    throw new ValidationException("Patient data validation failed");
                }

                patient.ModifiedAt = DateTime.UtcNow;
                return await _patientRepository.UpdateAsync(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient with ID: {PatientId}", patient.Id);
                throw;
            }
        }

        public async Task<bool> DeactivatePatientAsync(int id)
        {
            try
            {
                var patient = await GetByIdAsync(id);
                if (patient == null)
                {
                    return false;
                }

                patient.IsActive = false;
                patient.ModifiedAt = DateTime.UtcNow;
                await _patientRepository.UpdateAsync(patient);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating patient with ID: {PatientId}", id);
                throw;
            }
        }

        public async Task<bool> ActivatePatientAsync(int id)
        {
            try
            {
                var patient = await GetByIdAsync(id);
                if (patient == null)
                {
                    return false;
                }

                patient.IsActive = true;
                patient.ModifiedAt = DateTime.UtcNow;
                await _patientRepository.UpdateAsync(patient);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating patient with ID: {PatientId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Encounter>> GetPatientEncountersAsync(int patientId)
        {
            try
            {
                return await _patientRepository.GetEncountersAsync(patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving encounters for patient ID: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<Document>> GetPatientDocumentsAsync(int patientId)
        {
            try
            {
                return await _patientRepository.GetDocumentsAsync(patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents for patient ID: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<Insurance>> GetPatientInsurancesAsync(int patientId)
        {
            try
            {
                return await _patientRepository.GetInsurancesAsync(patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving insurances for patient ID: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<Allergy>> GetPatientAllergiesAsync(int patientId)
        {
            try
            {
                return await _patientRepository.GetAllergiesAsync(patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving allergies for patient ID: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<Problem>> GetPatientProblemsAsync(int patientId)
        {
            try
            {
                return await _patientRepository.GetProblemsAsync(patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving problems for patient ID: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<Medication>> GetPatientMedicationsAsync(int patientId)
        {
            try
            {
                return await _patientRepository.GetMedicationsAsync(patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medications for patient ID: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<Immunization>> GetPatientImmunizationsAsync(int patientId)
        {
            try
            {
                return await _patientRepository.GetImmunizationsAsync(patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving immunizations for patient ID: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<LabResult>> GetPatientLabResultsAsync(int patientId)
        {
            try
            {
                return await _patientRepository.GetLabResultsAsync(patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lab results for patient ID: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<Vital>> GetPatientVitalsAsync(int patientId)
        {
            try
            {
                return await _patientRepository.GetVitalsAsync(patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vitals for patient ID: {PatientId}", patientId);
                throw;
            }
        }

        // Vitals Management
        public async Task<Vital> AddVitalAsync(int patientId, Vital vital)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
                throw new NotFoundException($"Patient with ID {patientId} not found");

            vital.PatientId = patientId;
            vital.CreatedAt = DateTime.UtcNow;
            vital.CreatedBy = _currentUserService.UserId;

            return await _patientRepository.AddVitalAsync(vital);
        }

        public async Task<Vital> GetVitalAsync(int patientId, int vitalId)
        {
            var vital = await _patientRepository.GetVitalAsync(patientId, vitalId);
            if (vital == null)
                throw new NotFoundException($"Vital with ID {vitalId} not found for patient {patientId}");

            return vital;
        }

        public async Task<IEnumerable<Vital>> GetPatientVitalsAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _patientRepository.GetPatientVitalsAsync(patientId, fromDate, toDate);
        }

        public async Task<Vital> UpdateVitalAsync(int patientId, int vitalId, Vital vital)
        {
            var existingVital = await GetVitalAsync(patientId, vitalId);
            
            vital.Id = vitalId;
            vital.PatientId = patientId;
            vital.CreatedAt = existingVital.CreatedAt;
            vital.CreatedBy = existingVital.CreatedBy;
            vital.UpdatedAt = DateTime.UtcNow;
            vital.UpdatedBy = _currentUserService.UserId;

            return await _patientRepository.UpdateVitalAsync(vital);
        }

        public async Task DeleteVitalAsync(int patientId, int vitalId)
        {
            var vital = await GetVitalAsync(patientId, vitalId);
            await _patientRepository.DeleteVitalAsync(vital);
        }

        // Allergies Management
        public async Task<Allergy> AddAllergyAsync(int patientId, Allergy allergy)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
                throw new NotFoundException($"Patient with ID {patientId} not found");

            allergy.PatientId = patientId;
            allergy.CreatedAt = DateTime.UtcNow;
            allergy.CreatedBy = _currentUserService.UserId;

            return await _patientRepository.AddAllergyAsync(allergy);
        }

        public async Task<Allergy> GetAllergyAsync(int patientId, int allergyId)
        {
            var allergy = await _patientRepository.GetAllergyAsync(patientId, allergyId);
            if (allergy == null)
                throw new NotFoundException($"Allergy with ID {allergyId} not found for patient {patientId}");

            return allergy;
        }

        public async Task<IEnumerable<Allergy>> GetPatientAllergiesAsync(int patientId)
        {
            return await _patientRepository.GetPatientAllergiesAsync(patientId);
        }

        public async Task<Allergy> UpdateAllergyAsync(int patientId, int allergyId, Allergy allergy)
        {
            var existingAllergy = await GetAllergyAsync(patientId, allergyId);
            
            allergy.Id = allergyId;
            allergy.PatientId = patientId;
            allergy.CreatedAt = existingAllergy.CreatedAt;
            allergy.CreatedBy = existingAllergy.CreatedBy;
            allergy.UpdatedAt = DateTime.UtcNow;
            allergy.UpdatedBy = _currentUserService.UserId;

            return await _patientRepository.UpdateAllergyAsync(allergy);
        }

        public async Task DeleteAllergyAsync(int patientId, int allergyId)
        {
            var allergy = await GetAllergyAsync(patientId, allergyId);
            await _patientRepository.DeleteAllergyAsync(allergy);
        }

        // Problems Management
        public async Task<Problem> AddProblemAsync(int patientId, Problem problem)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
                throw new NotFoundException($"Patient with ID {patientId} not found");

            problem.PatientId = patientId;
            problem.CreatedAt = DateTime.UtcNow;
            problem.CreatedBy = _currentUserService.UserId;

            return await _patientRepository.AddProblemAsync(problem);
        }

        public async Task<Problem> GetProblemAsync(int patientId, int problemId)
        {
            var problem = await _patientRepository.GetProblemAsync(patientId, problemId);
            if (problem == null)
                throw new NotFoundException($"Problem with ID {problemId} not found for patient {patientId}");

            return problem;
        }

        public async Task<IEnumerable<Problem>> GetPatientProblemsAsync(int patientId, bool includeResolved = false)
        {
            return await _patientRepository.GetPatientProblemsAsync(patientId, includeResolved);
        }

        public async Task<Problem> UpdateProblemAsync(int patientId, int problemId, Problem problem)
        {
            var existingProblem = await GetProblemAsync(patientId, problemId);
            
            problem.Id = problemId;
            problem.PatientId = patientId;
            problem.CreatedAt = existingProblem.CreatedAt;
            problem.CreatedBy = existingProblem.CreatedBy;
            problem.UpdatedAt = DateTime.UtcNow;
            problem.UpdatedBy = _currentUserService.UserId;

            return await _patientRepository.UpdateProblemAsync(problem);
        }

        public async Task DeleteProblemAsync(int patientId, int problemId)
        {
            var problem = await GetProblemAsync(patientId, problemId);
            await _patientRepository.DeleteProblemAsync(problem);
        }

        public async Task<bool> ValidatePatientDataAsync(Patient patient)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(patient);
                return validationResult.IsValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating patient data");
                throw;
            }
        }

        public async Task<bool> CheckDuplicatePatientAsync(Patient patient)
        {
            try
            {
                // Check for potential duplicates based on name, DOB, and SSN
                var potentialDuplicates = await _patientRepository.SearchAsync(
                    $"{patient.FirstName} {patient.LastName}", 0, 10);

                return potentialDuplicates.Any(p =>
                    p.DateOfBirth == patient.DateOfBirth &&
                    (p.SocialSecurityNumber == patient.SocialSecurityNumber ||
                     (p.FirstName.Equals(patient.FirstName, StringComparison.OrdinalIgnoreCase) &&
                      p.LastName.Equals(patient.LastName, StringComparison.OrdinalIgnoreCase))));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for duplicate patient");
                throw;
            }
        }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    public class DuplicatePatientException : Exception
    {
        public DuplicatePatientException(string message) : base(message) { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
