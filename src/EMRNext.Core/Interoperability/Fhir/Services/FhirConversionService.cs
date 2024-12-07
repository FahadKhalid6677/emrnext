using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Interoperability.Fhir.Models;
using EMRNext.Core.Repositories;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Interoperability.Fhir.Services
{
    /// <summary>
    /// Service for converting between EMRNext domain models and FHIR resources
    /// </summary>
    public class FhirConversionService
    {
        private readonly ILogger<FhirConversionService> _logger;
        private readonly IGenericRepository<Patient> _patientRepository;
        private readonly FhirJsonParser _fhirJsonParser;
        private readonly FhirJsonSerializer _fhirJsonSerializer;

        public FhirConversionService(
            ILogger<FhirConversionService> logger,
            IGenericRepository<Patient> patientRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _fhirJsonParser = new FhirJsonParser();
            _fhirJsonSerializer = new FhirJsonSerializer();
        }

        /// <summary>
        /// Convert EMRNext Patient to FHIR Patient resource
        /// </summary>
        public FhirPatient ConvertPatientToFhir(Patient patient)
        {
            try 
            {
                return FhirPatient.FromDomainPatient(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error converting patient {patient.Id} to FHIR");
                throw;
            }
        }

        /// <summary>
        /// Convert FHIR Patient resource to EMRNext Patient
        /// </summary>
        public Patient ConvertFhirToPatient(FhirPatient fhirPatient)
        {
            try 
            {
                return fhirPatient.ToDomainPatient();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting FHIR patient to domain model");
                throw;
            }
        }

        /// <summary>
        /// Validate FHIR resource against EMRNext domain rules
        /// </summary>
        public bool ValidateFhirResource(FhirResourceBase resource)
        {
            return resource.Validate();
        }

        /// <summary>
        /// Convert FHIR JSON to EMRNext domain model
        /// </summary>
        public Patient ImportPatientFromFhirJson(string fhirJson)
        {
            try 
            {
                var patient = _fhirJsonParser.Parse<Patient>(fhirJson);
                var fhirPatient = new FhirPatient();
                fhirPatient.FromFhirResource(patient);

                return fhirPatient.ToDomainPatient();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing patient from FHIR JSON");
                throw;
            }
        }

        /// <summary>
        /// Export EMRNext Patient to FHIR JSON
        /// </summary>
        public string ExportPatientToFhirJson(Patient patient)
        {
            try 
            {
                var fhirPatient = FhirPatient.FromDomainPatient(patient);
                return _fhirJsonSerializer.SerializeToString(fhirPatient.ToFhirResource());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting patient {patient.Id} to FHIR JSON");
                throw;
            }
        }

        /// <summary>
        /// Bulk convert multiple patients to FHIR resources
        /// </summary>
        public IEnumerable<FhirPatient> ConvertPatientsToFhir(IEnumerable<Patient> patients)
        {
            return patients.Select(ConvertPatientToFhir);
        }

        /// <summary>
        /// Retrieve and convert patients from repository to FHIR
        /// </summary>
        public async Task<IEnumerable<FhirPatient>> GetAllPatientsAsFhirAsync()
        {
            var patients = await _patientRepository.GetAllAsync();
            return ConvertPatientsToFhir(patients);
        }
    }
}
