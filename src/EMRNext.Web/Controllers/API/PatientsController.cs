using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.Services;
using EMRNext.Web.Models.API;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Authorization;
using System.Net.Mime;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace EMRNext.Web.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiVersion("1.0")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IDocumentService _documentService;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(
            IPatientService patientService,
            IDocumentService documentService,
            ILogger<PatientsController> logger)
        {
            _patientService = patientService;
            _documentService = documentService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new patient
        /// </summary>
        /// <param name="request">Patient registration information</param>
        /// <returns>Newly created patient information</returns>
        /// <response code="201">Returns the newly created patient</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized</response>
        [HttpPost]
        [Authorize(Policy = EMRPolicies.PatientRegistration)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PatientResponse>> RegisterPatient([FromBody] PatientRegistrationRequest request)
        {
            try
            {
                var patient = await _patientService.RegisterPatientAsync(
                    new PatientRegistration
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        DateOfBirth = request.DateOfBirth,
                        Gender = request.Gender,
                        Email = request.Email,
                        Phone = request.Phone,
                        Address = request.Address,
                        City = request.City,
                        State = request.State,
                        ZipCode = request.ZipCode,
                        SSN = request.SSN,
                        EmergencyContact = request.EmergencyContact,
                        EmergencyPhone = request.EmergencyPhone,
                        PreferredLanguage = request.PreferredLanguage,
                        MaritalStatus = request.MaritalStatus,
                        EmploymentStatus = request.EmploymentStatus,
                        Insurances = request.Insurances?.Select(i => new InsuranceRegistration
                        {
                            PayerId = i.PayerId,
                            PolicyNumber = i.PolicyNumber,
                            GroupNumber = i.GroupNumber,
                            SubscriberId = i.SubscriberId,
                            SubscriberName = i.SubscriberName,
                            EffectiveDate = i.EffectiveDate,
                            TerminationDate = i.TerminationDate,
                            Priority = i.Priority
                        }).ToList()
                    });

                var response = MapToPatientResponse(patient);
                return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Invalid patient registration request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering patient");
                throw;
            }
        }

        /// <summary>
        /// Get patient by ID
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <returns>Patient information</returns>
        /// <response code="200">Returns the patient information</response>
        /// <response code="404">If patient is not found</response>
        [HttpGet("{id}")]
        [Authorize(Policy = EMRPolicies.PatientView)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientResponse>> GetPatient(int id)
        {
            var patient = await _patientService.GetPatientAsync(id);
            if (patient == null)
                return NotFound();

            return MapToPatientResponse(patient);
        }

        /// <summary>
        /// Update patient information
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <param name="request">Updated patient information</param>
        /// <returns>Updated patient information</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = EMRPolicies.PatientUpdate)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientResponse>> UpdatePatient(int id, [FromBody] PatientUpdateRequest request)
        {
            try
            {
                var patient = await _patientService.UpdatePatientAsync(id, new PatientUpdate
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    ZipCode = request.ZipCode,
                    EmergencyContact = request.EmergencyContact,
                    EmergencyPhone = request.EmergencyPhone,
                    PreferredLanguage = request.PreferredLanguage,
                    MaritalStatus = request.MaritalStatus,
                    EmploymentStatus = request.EmploymentStatus
                });

                if (patient == null)
                    return NotFound();

                return MapToPatientResponse(patient);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Invalid patient update request");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Search for patients
        /// </summary>
        /// <param name="request">Search criteria</param>
        /// <returns>List of matching patients</returns>
        [HttpGet("search")]
        [Authorize(Policy = EMRPolicies.PatientSearch)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PatientSearchResponse>> SearchPatients([FromQuery] PatientSearchRequest request)
        {
            var searchResult = await _patientService.SearchPatientsAsync(new PatientSearchCriteria
            {
                SearchTerm = request.SearchTerm,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                PageNumber = request.PageNumber ?? 1,
                PageSize = request.PageSize ?? 20,
                SortBy = request.SortBy,
                SortDescending = request.SortDescending
            });

            return new PatientSearchResponse
            {
                Patients = searchResult.Patients.Select(MapToPatientResponse).ToList(),
                TotalCount = searchResult.TotalCount,
                PageNumber = searchResult.PageNumber,
                PageSize = searchResult.PageSize,
                TotalPages = searchResult.TotalPages
            };
        }

        /// <summary>
        /// Upload patient document
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <param name="request">Document information</param>
        /// <returns>Uploaded document information</returns>
        [HttpPost("{id}/documents")]
        [Authorize(Policy = EMRPolicies.PatientDocumentUpload)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientDocumentResponse>> UploadDocument(
            int id,
            [FromBody] PatientDocumentRequest request)
        {
            try
            {
                var document = await _documentService.UploadPatientDocumentAsync(
                    id,
                    new DocumentUpload
                    {
                        DocumentType = request.DocumentType,
                        DocumentContent = request.DocumentContent,
                        Description = request.Description,
                        Metadata = request.Metadata
                    });

                var response = new PatientDocumentResponse
                {
                    Id = document.Id,
                    DocumentType = document.DocumentType,
                    DocumentPath = document.DocumentPath,
                    Description = document.Description,
                    Metadata = document.Metadata,
                    UploadDate = document.UploadDate,
                    UploadedBy = document.UploadedBy
                };

                return CreatedAtAction(
                    nameof(GetDocument),
                    new { id = id, documentId = document.Id },
                    response);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get patient document
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <param name="documentId">Document ID</param>
        /// <returns>Document information</returns>
        [HttpGet("{id}/documents/{documentId}")]
        [Authorize(Policy = EMRPolicies.PatientDocumentView)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientDocumentResponse>> GetDocument(int id, int documentId)
        {
            var document = await _documentService.GetPatientDocumentAsync(id, documentId);
            if (document == null)
                return NotFound();

            return new PatientDocumentResponse
            {
                Id = document.Id,
                DocumentType = document.DocumentType,
                DocumentPath = document.DocumentPath,
                Description = document.Description,
                Metadata = document.Metadata,
                UploadDate = document.UploadDate,
                UploadedBy = document.UploadedBy
            };
        }

        /// <summary>
        /// Get patient vitals history
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <returns>List of patient vitals</returns>
        [HttpGet("{id}/vitals")]
        [Authorize(Policy = EMRPolicies.PatientVitalsView)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<VitalResponse>>> GetVitals(int id)
        {
            var vitals = await _patientService.GetPatientVitalsAsync(id);
            if (vitals == null)
                return NotFound();

            return Ok(vitals.Select(v => new VitalResponse
            {
                Id = v.Id,
                Type = v.Type,
                Value = v.Value,
                Unit = v.Unit,
                MeasurementDate = v.MeasurementDate,
                MeasuredBy = v.MeasuredBy,
                Notes = v.Notes
            }));
        }

        /// <summary>
        /// Record new vital signs
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <param name="request">Vital signs information</param>
        /// <returns>Recorded vital signs</returns>
        [HttpPost("{id}/vitals")]
        [Authorize(Policy = EMRPolicies.PatientVitalsCreate)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VitalResponse>> RecordVitals(
            int id,
            [FromBody] VitalRequest request)
        {
            try
            {
                var vital = await _patientService.RecordVitalsAsync(id, new Vital
                {
                    PatientId = id,
                    Type = request.Type,
                    Value = request.Value,
                    Unit = request.Unit,
                    MeasurementDate = request.MeasurementDate,
                    MeasuredBy = User.Identity.Name,
                    Notes = request.Notes
                });

                var response = new VitalResponse
                {
                    Id = vital.Id,
                    Type = vital.Type,
                    Value = vital.Value,
                    Unit = vital.Unit,
                    MeasurementDate = vital.MeasurementDate,
                    MeasuredBy = vital.MeasuredBy,
                    Notes = vital.Notes
                };

                return CreatedAtAction(
                    nameof(GetVitals),
                    new { id = id },
                    response);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get patient allergies
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <returns>List of patient allergies</returns>
        [HttpGet("{id}/allergies")]
        [Authorize(Policy = EMRPolicies.PatientAllergiesView)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AllergyResponse>>> GetAllergies(int id)
        {
            var allergies = await _patientService.GetPatientAllergiesAsync(id);
            if (allergies == null)
                return NotFound();

            return Ok(allergies.Select(a => new AllergyResponse
            {
                Id = a.Id,
                Allergen = a.Allergen,
                AllergenType = a.AllergenType,
                Severity = a.Severity,
                Reaction = a.Reaction,
                OnsetDate = a.OnsetDate,
                Status = a.Status,
                Notes = a.Notes
            }));
        }

        /// <summary>
        /// Record new allergy
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <param name="request">Allergy information</param>
        /// <returns>Recorded allergy</returns>
        [HttpPost("{id}/allergies")]
        [Authorize(Policy = EMRPolicies.PatientAllergiesCreate)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AllergyResponse>> RecordAllergy(
            int id,
            [FromBody] AllergyRequest request)
        {
            try
            {
                var allergy = await _patientService.RecordAllergyAsync(id, new Allergy
                {
                    PatientId = id,
                    Allergen = request.Allergen,
                    AllergenType = request.AllergenType,
                    Severity = request.Severity,
                    Reaction = request.Reaction,
                    OnsetDate = request.OnsetDate,
                    Status = request.Status,
                    Notes = request.Notes
                });

                var response = new AllergyResponse
                {
                    Id = allergy.Id,
                    Allergen = allergy.Allergen,
                    AllergenType = allergy.AllergenType,
                    Severity = allergy.Severity,
                    Reaction = allergy.Reaction,
                    OnsetDate = allergy.OnsetDate,
                    Status = allergy.Status,
                    Notes = allergy.Notes
                };

                return CreatedAtAction(
                    nameof(GetAllergies),
                    new { id = id },
                    response);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get patient problems/conditions
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <returns>List of patient problems</returns>
        [HttpGet("{id}/problems")]
        [Authorize(Policy = EMRPolicies.PatientProblemsView)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ProblemResponse>>> GetProblems(int id)
        {
            var problems = await _patientService.GetPatientProblemsAsync(id);
            if (problems == null)
                return NotFound();

            return Ok(problems.Select(p => new ProblemResponse
            {
                Id = p.Id,
                ProblemName = p.ProblemName,
                IcdCode = p.IcdCode,
                OnsetDate = p.OnsetDate,
                EndDate = p.EndDate,
                Status = p.Status,
                Severity = p.Severity,
                Notes = p.Notes
            }));
        }

        /// <summary>
        /// Record new problem/condition
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <param name="request">Problem information</param>
        /// <returns>Recorded problem</returns>
        [HttpPost("{id}/problems")]
        [Authorize(Policy = EMRPolicies.PatientProblemsCreate)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProblemResponse>> RecordProblem(
            int id,
            [FromBody] ProblemRequest request)
        {
            try
            {
                var problem = await _patientService.RecordProblemAsync(id, new Problem
                {
                    PatientId = id,
                    ProblemName = request.ProblemName,
                    IcdCode = request.IcdCode,
                    OnsetDate = request.OnsetDate,
                    EndDate = request.EndDate,
                    Status = request.Status,
                    Severity = request.Severity,
                    Notes = request.Notes
                });

                var response = new ProblemResponse
                {
                    Id = problem.Id,
                    ProblemName = problem.ProblemName,
                    IcdCode = problem.IcdCode,
                    OnsetDate = problem.OnsetDate,
                    EndDate = problem.EndDate,
                    Status = problem.Status,
                    Severity = problem.Severity,
                    Notes = problem.Notes
                };

                return CreatedAtAction(
                    nameof(GetProblems),
                    new { id = id },
                    response);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Vitals Management
        [HttpPost("{patientId}/vitals")]
        [ProducesResponseType(typeof(VitalResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddVital(int patientId, [FromBody] VitalRequest request)
        {
            try
            {
                var vital = _mapper.Map<Vital>(request);
                var result = await _patientService.AddVitalAsync(patientId, vital);
                var response = _mapper.Map<VitalResponse>(result);
                return CreatedAtAction(nameof(GetVital), new { patientId, vitalId = response.Id }, response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding vital for patient {PatientId}", patientId);
                return BadRequest("Failed to add vital");
            }
        }

        [HttpGet("{patientId}/vitals/{vitalId}")]
        [ProducesResponseType(typeof(VitalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVital(int patientId, int vitalId)
        {
            try
            {
                var vital = await _patientService.GetVitalAsync(patientId, vitalId);
                var response = _mapper.Map<VitalResponse>(vital);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{patientId}/vitals")]
        [ProducesResponseType(typeof(IEnumerable<VitalResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPatientVitals(int patientId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var vitals = await _patientService.GetPatientVitalsAsync(patientId, fromDate, toDate);
            var response = _mapper.Map<IEnumerable<VitalResponse>>(vitals);
            return Ok(response);
        }

        [HttpPut("{patientId}/vitals/{vitalId}")]
        [ProducesResponseType(typeof(VitalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVital(int patientId, int vitalId, [FromBody] VitalRequest request)
        {
            try
            {
                var vital = _mapper.Map<Vital>(request);
                var result = await _patientService.UpdateVitalAsync(patientId, vitalId, vital);
                var response = _mapper.Map<VitalResponse>(result);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vital {VitalId} for patient {PatientId}", vitalId, patientId);
                return BadRequest("Failed to update vital");
            }
        }

        [HttpDelete("{patientId}/vitals/{vitalId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVital(int patientId, int vitalId)
        {
            try
            {
                await _patientService.DeleteVitalAsync(patientId, vitalId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Allergies Management
        [HttpPost("{patientId}/allergies")]
        [ProducesResponseType(typeof(AllergyResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAllergy(int patientId, [FromBody] AllergyRequest request)
        {
            try
            {
                var allergy = _mapper.Map<Allergy>(request);
                var result = await _patientService.AddAllergyAsync(patientId, allergy);
                var response = _mapper.Map<AllergyResponse>(result);
                return CreatedAtAction(nameof(GetAllergy), new { patientId, allergyId = response.Id }, response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding allergy for patient {PatientId}", patientId);
                return BadRequest("Failed to add allergy");
            }
        }

        [HttpGet("{patientId}/allergies/{allergyId}")]
        [ProducesResponseType(typeof(AllergyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllergy(int patientId, int allergyId)
        {
            try
            {
                var allergy = await _patientService.GetAllergyAsync(patientId, allergyId);
                var response = _mapper.Map<AllergyResponse>(allergy);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{patientId}/allergies")]
        [ProducesResponseType(typeof(IEnumerable<AllergyResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPatientAllergies(int patientId)
        {
            var allergies = await _patientService.GetPatientAllergiesAsync(patientId);
            var response = _mapper.Map<IEnumerable<AllergyResponse>>(allergies);
            return Ok(response);
        }

        [HttpPut("{patientId}/allergies/{allergyId}")]
        [ProducesResponseType(typeof(AllergyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAllergy(int patientId, int allergyId, [FromBody] AllergyRequest request)
        {
            try
            {
                var allergy = _mapper.Map<Allergy>(request);
                var result = await _patientService.UpdateAllergyAsync(patientId, allergyId, allergy);
                var response = _mapper.Map<AllergyResponse>(result);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating allergy {AllergyId} for patient {PatientId}", allergyId, patientId);
                return BadRequest("Failed to update allergy");
            }
        }

        [HttpDelete("{patientId}/allergies/{allergyId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAllergy(int patientId, int allergyId)
        {
            try
            {
                await _patientService.DeleteAllergyAsync(patientId, allergyId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Problems Management
        [HttpPost("{patientId}/problems")]
        [ProducesResponseType(typeof(ProblemResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddProblem(int patientId, [FromBody] ProblemRequest request)
        {
            try
            {
                var problem = _mapper.Map<Problem>(request);
                var result = await _patientService.AddProblemAsync(patientId, problem);
                var response = _mapper.Map<ProblemResponse>(result);
                return CreatedAtAction(nameof(GetProblem), new { patientId, problemId = response.Id }, response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding problem for patient {PatientId}", patientId);
                return BadRequest("Failed to add problem");
            }
        }

        [HttpGet("{patientId}/problems/{problemId}")]
        [ProducesResponseType(typeof(ProblemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProblem(int patientId, int problemId)
        {
            try
            {
                var problem = await _patientService.GetProblemAsync(patientId, problemId);
                var response = _mapper.Map<ProblemResponse>(problem);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{patientId}/problems")]
        [ProducesResponseType(typeof(IEnumerable<ProblemResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPatientProblems(int patientId, [FromQuery] bool includeResolved = false)
        {
            var problems = await _patientService.GetPatientProblemsAsync(patientId, includeResolved);
            var response = _mapper.Map<IEnumerable<ProblemResponse>>(problems);
            return Ok(response);
        }

        [HttpPut("{patientId}/problems/{problemId}")]
        [ProducesResponseType(typeof(ProblemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProblem(int patientId, int problemId, [FromBody] ProblemRequest request)
        {
            try
            {
                var problem = _mapper.Map<Problem>(request);
                var result = await _patientService.UpdateProblemAsync(patientId, problemId, problem);
                var response = _mapper.Map<ProblemResponse>(result);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating problem {ProblemId} for patient {PatientId}", problemId, patientId);
                return BadRequest("Failed to update problem");
            }
        }

        [HttpDelete("{patientId}/problems/{problemId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProblem(int patientId, int problemId)
        {
            try
            {
                await _patientService.DeleteProblemAsync(patientId, problemId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private static PatientResponse MapToPatientResponse(Patient patient)
        {
            return new PatientResponse
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                Email = patient.Email,
                Phone = patient.Phone,
                Address = patient.Address,
                City = patient.City,
                State = patient.State,
                ZipCode = patient.ZipCode,
                EmergencyContact = patient.EmergencyContact,
                EmergencyPhone = patient.EmergencyPhone,
                PreferredLanguage = patient.PreferredLanguage,
                MaritalStatus = patient.MaritalStatus,
                EmploymentStatus = patient.EmploymentStatus,
                CreatedDate = patient.CreatedDate,
                LastUpdatedDate = patient.LastUpdatedDate,
                Insurances = patient.Insurances?.Select(i => new InsuranceResponse
                {
                    Id = i.Id,
                    PayerId = i.PayerId,
                    PayerName = i.PayerName,
                    PolicyNumber = i.PolicyNumber,
                    GroupNumber = i.GroupNumber,
                    SubscriberId = i.SubscriberId,
                    SubscriberName = i.SubscriberName,
                    EffectiveDate = i.EffectiveDate,
                    TerminationDate = i.TerminationDate,
                    Priority = i.Priority,
                    IsActive = i.IsActive,
                    IsVerified = i.IsVerified,
                    LastVerificationDate = i.LastVerificationDate
                }).ToList()
            };
        }
    }
}
