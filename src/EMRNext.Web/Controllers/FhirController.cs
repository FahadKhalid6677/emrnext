using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.Interoperability.Fhir.Services;
using EMRNext.Core.Repositories;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Web.Controllers
{
    /// <summary>
    /// FHIR API Endpoint for Resource Interactions
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fhir/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly FhirConversionService _fhirConversionService;
        private readonly IGenericRepository<Patient> _patientRepository;

        public PatientController(
            FhirConversionService fhirConversionService,
            IGenericRepository<Patient> patientRepository)
        {
            _fhirConversionService = fhirConversionService 
                ?? throw new ArgumentNullException(nameof(fhirConversionService));
            _patientRepository = patientRepository 
                ?? throw new ArgumentNullException(nameof(patientRepository));
        }

        /// <summary>
        /// Read a patient by ID in FHIR format
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPatient(Guid id)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            
            if (patient == null)
                return NotFound();

            var fhirPatient = _fhirConversionService.ConvertPatientToFhir(patient);
            return Ok(fhirPatient);
        }

        /// <summary>
        /// Create a new patient from FHIR resource
        /// </summary>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreatePatient([FromBody] string fhirJson)
        {
            try 
            {
                var patient = _fhirConversionService.ImportPatientFromFhirJson(fhirJson);
                
                if (!_fhirConversionService.ValidateFhirResource(
                    _fhirConversionService.ConvertPatientToFhir(patient)))
                {
                    return BadRequest("Invalid patient data");
                }

                var createdPatient = await _patientRepository.AddAsync(patient);
                var fhirPatient = _fhirConversionService.ConvertPatientToFhir(createdPatient);

                return CreatedAtAction(
                    nameof(GetPatient), 
                    new { id = createdPatient.Id }, 
                    fhirPatient
                );
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating patient: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing patient from FHIR resource
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] string fhirJson)
        {
            try 
            {
                var existingPatient = await _patientRepository.GetByIdAsync(id);
                
                if (existingPatient == null)
                    return NotFound();

                var updatedPatient = _fhirConversionService.ImportPatientFromFhirJson(fhirJson);
                updatedPatient.Id = id;

                if (!_fhirConversionService.ValidateFhirResource(
                    _fhirConversionService.ConvertPatientToFhir(updatedPatient)))
                {
                    return BadRequest("Invalid patient data");
                }

                await _patientRepository.UpdateAsync(updatedPatient);
                var fhirPatient = _fhirConversionService.ConvertPatientToFhir(updatedPatient);

                return Ok(fhirPatient);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating patient: {ex.Message}");
            }
        }

        /// <summary>
        /// Search patients based on various criteria
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SearchPatients(
            [FromQuery] string identifier = null,
            [FromQuery] string name = null,
            [FromQuery] string birthDate = null)
        {
            var patients = await _patientRepository.GetAllAsync(p => 
                (identifier == null || p.MedicalRecordNumber == identifier) &&
                (name == null || p.FirstName.Contains(name) || p.LastName.Contains(name)) &&
                (birthDate == null || p.DateOfBirth.ToString("yyyy-MM-dd") == birthDate)
            );

            var fhirPatients = _fhirConversionService.ConvertPatientsToFhir(patients);
            return Ok(fhirPatients);
        }
    }
}
