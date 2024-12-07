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

namespace EMRNext.Web.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiVersion("1.0")]
    public class EncountersController : ControllerBase
    {
        private readonly IClinicalService _clinicalService;
        private readonly ILogger<EncountersController> _logger;

        public EncountersController(
            IClinicalService clinicalService,
            ILogger<EncountersController> logger)
        {
            _clinicalService = clinicalService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new encounter
        /// </summary>
        /// <param name="request">Encounter creation information</param>
        /// <returns>Newly created encounter information</returns>
        [HttpPost]
        [Authorize(Policy = EMRPolicies.EncounterCreate)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<EncounterResponse>> CreateEncounter([FromBody] EncounterCreationRequest request)
        {
            try
            {
                var encounter = await _clinicalService.CreateEncounterAsync(new EncounterCreation
                {
                    PatientId = request.PatientId,
                    ProviderId = request.ProviderId,
                    EncounterType = request.EncounterType,
                    EncounterDate = request.EncounterDate,
                    Department = request.Department,
                    Facility = request.Facility,
                    ChiefComplaint = request.ChiefComplaint,
                    DiagnosisCodes = request.DiagnosisCodes,
                    ProcedureCodes = request.ProcedureCodes,
                    CustomFields = request.CustomFields
                });

                var response = MapToEncounterResponse(encounter);
                return CreatedAtAction(nameof(GetEncounter), new { id = encounter.Id }, response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Invalid encounter creation request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating encounter");
                throw;
            }
        }

        /// <summary>
        /// Get encounter by ID
        /// </summary>
        /// <param name="id">Encounter ID</param>
        /// <returns>Encounter information</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = EMRPolicies.EncounterView)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EncounterResponse>> GetEncounter(int id)
        {
            var encounter = await _clinicalService.GetEncounterAsync(id);
            if (encounter == null)
                return NotFound();

            return MapToEncounterResponse(encounter);
        }

        /// <summary>
        /// Update encounter information
        /// </summary>
        /// <param name="id">Encounter ID</param>
        /// <param name="request">Updated encounter information</param>
        /// <returns>Updated encounter information</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = EMRPolicies.EncounterUpdate)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EncounterResponse>> UpdateEncounter(
            int id,
            [FromBody] EncounterUpdateRequest request)
        {
            try
            {
                var encounter = await _clinicalService.UpdateEncounterAsync(id, new EncounterUpdate
                {
                    EncounterType = request.EncounterType,
                    Department = request.Department,
                    Facility = request.Facility,
                    ChiefComplaint = request.ChiefComplaint,
                    DiagnosisCodes = request.DiagnosisCodes,
                    ProcedureCodes = request.ProcedureCodes,
                    Status = request.Status,
                    CustomFields = request.CustomFields
                });

                if (encounter == null)
                    return NotFound();

                return MapToEncounterResponse(encounter);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Invalid encounter update request");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get patient encounters
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <returns>List of patient encounters</returns>
        [HttpGet("patient/{patientId}")]
        [Authorize(Policy = EMRPolicies.EncounterView)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EncounterResponse>>> GetPatientEncounters(int patientId)
        {
            var encounters = await _clinicalService.GetPatientEncountersAsync(patientId);
            return encounters.Select(MapToEncounterResponse).ToList();
        }

        /// <summary>
        /// Add clinical note to encounter
        /// </summary>
        /// <param name="id">Encounter ID</param>
        /// <param name="request">Clinical note information</param>
        /// <returns>Created clinical note information</returns>
        [HttpPost("{id}/notes")]
        [Authorize(Policy = EMRPolicies.ClinicalNoteCreate)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClinicalNoteResponse>> AddClinicalNote(
            int id,
            [FromBody] ClinicalNoteRequest request)
        {
            try
            {
                var note = await _clinicalService.AddClinicalNoteAsync(id, new ClinicalNoteCreation
                {
                    NoteType = request.NoteType,
                    Content = request.Content,
                    DiagnosisCodes = request.DiagnosisCodes,
                    ProcedureCodes = request.ProcedureCodes,
                    Metadata = request.Metadata
                });

                var response = MapToClinicalNoteResponse(note);
                return CreatedAtAction(
                    nameof(GetClinicalNote),
                    new { id = id, noteId = note.Id },
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
        /// Add vitals to encounter
        /// </summary>
        /// <param name="id">Encounter ID</param>
        /// <param name="request">Vitals information</param>
        /// <returns>Created vitals information</returns>
        [HttpPost("{id}/vitals")]
        [Authorize(Policy = EMRPolicies.VitalsCreate)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VitalsResponse>> AddVitals(
            int id,
            [FromBody] VitalsRequest request)
        {
            try
            {
                var vitals = await _clinicalService.AddVitalsAsync(id, new VitalsCreation
                {
                    Temperature = request.Temperature,
                    HeartRate = request.HeartRate,
                    RespiratoryRate = request.RespiratoryRate,
                    SystolicBP = request.SystolicBP,
                    DiastolicBP = request.DiastolicBP,
                    Weight = request.Weight,
                    Height = request.Height,
                    BMI = request.BMI,
                    OxygenSaturation = request.OxygenSaturation,
                    Pain = request.Pain,
                    CustomMeasurements = request.CustomMeasurements
                });

                var response = MapToVitalsResponse(vitals);
                return CreatedAtAction(
                    nameof(GetVitals),
                    new { id = id, vitalsId = vitals.Id },
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

        private static EncounterResponse MapToEncounterResponse(Encounter encounter)
        {
            return new EncounterResponse
            {
                Id = encounter.Id,
                PatientId = encounter.PatientId,
                ProviderId = encounter.ProviderId,
                EncounterType = encounter.EncounterType,
                EncounterDate = encounter.EncounterDate,
                Department = encounter.Department,
                Facility = encounter.Facility,
                ChiefComplaint = encounter.ChiefComplaint,
                Status = encounter.Status,
                DiagnosisCodes = encounter.DiagnosisCodes,
                ProcedureCodes = encounter.ProcedureCodes,
                Vitals = encounter.Vitals?.Select(MapToVitalsResponse).ToList(),
                Notes = encounter.Notes?.Select(MapToClinicalNoteResponse).ToList(),
                Orders = encounter.Orders?.Select(MapToOrderResponse).ToList(),
                Prescriptions = encounter.Prescriptions?.Select(MapToPrescriptionResponse).ToList(),
                CustomFields = encounter.CustomFields,
                CreatedDate = encounter.CreatedDate,
                LastUpdatedDate = encounter.LastUpdatedDate
            };
        }

        private static ClinicalNoteResponse MapToClinicalNoteResponse(ClinicalNote note)
        {
            return new ClinicalNoteResponse
            {
                Id = note.Id,
                NoteType = note.NoteType,
                Content = note.Content,
                DiagnosisCodes = note.DiagnosisCodes,
                ProcedureCodes = note.ProcedureCodes,
                Metadata = note.Metadata,
                CreatedBy = note.CreatedBy,
                CreatedDate = note.CreatedDate,
                LastUpdatedBy = note.LastUpdatedBy,
                LastUpdatedDate = note.LastUpdatedDate
            };
        }

        private static VitalsResponse MapToVitalsResponse(Vitals vitals)
        {
            return new VitalsResponse
            {
                Id = vitals.Id,
                Temperature = vitals.Temperature,
                HeartRate = vitals.HeartRate,
                RespiratoryRate = vitals.RespiratoryRate,
                SystolicBP = vitals.SystolicBP,
                DiastolicBP = vitals.DiastolicBP,
                Weight = vitals.Weight,
                Height = vitals.Height,
                BMI = vitals.BMI,
                OxygenSaturation = vitals.OxygenSaturation,
                Pain = vitals.Pain,
                CustomMeasurements = vitals.CustomMeasurements,
                RecordedBy = vitals.RecordedBy,
                RecordedDate = vitals.RecordedDate
            };
        }

        private static OrderResponse MapToOrderResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                OrderType = order.OrderType,
                OrderCode = order.OrderCode,
                Description = order.Description,
                Instructions = order.Instructions,
                StartDate = order.StartDate,
                EndDate = order.EndDate,
                Frequency = order.Frequency,
                Priority = order.Priority,
                Status = order.Status,
                DiagnosisCodes = order.DiagnosisCodes,
                Results = order.Results?.Select(MapToResultResponse).ToList(),
                CustomFields = order.CustomFields,
                OrderedBy = order.OrderedBy,
                OrderedDate = order.OrderedDate
            };
        }

        private static ResultResponse MapToResultResponse(Result result)
        {
            return new ResultResponse
            {
                Id = result.Id,
                ResultType = result.ResultType,
                Value = result.Value,
                Unit = result.Unit,
                ReferenceRange = result.ReferenceRange,
                Interpretation = result.Interpretation,
                Status = result.Status,
                Comments = result.Comments,
                CustomFields = result.CustomFields,
                RecordedBy = result.RecordedBy,
                RecordedDate = result.RecordedDate
            };
        }

        private static PrescriptionResponse MapToPrescriptionResponse(Prescription prescription)
        {
            return new PrescriptionResponse
            {
                Id = prescription.Id,
                MedicationCode = prescription.MedicationCode,
                MedicationName = prescription.MedicationName,
                Dosage = prescription.Dosage,
                Route = prescription.Route,
                Frequency = prescription.Frequency,
                Instructions = prescription.Instructions,
                Quantity = prescription.Quantity,
                Refills = prescription.Refills,
                RefillsRemaining = prescription.RefillsRemaining,
                StartDate = prescription.StartDate,
                EndDate = prescription.EndDate,
                IsPRN = prescription.IsPRN,
                PRNInstructions = prescription.PRNInstructions,
                Status = prescription.Status,
                DiagnosisCodes = prescription.DiagnosisCodes,
                CustomFields = prescription.CustomFields,
                PrescribedBy = prescription.PrescribedBy,
                PrescribedDate = prescription.PrescribedDate
            };
        }
    }
}
