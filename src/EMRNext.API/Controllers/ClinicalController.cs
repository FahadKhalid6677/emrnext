using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.Services;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Authorization;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace EMRNext.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClinicalController : ControllerBase
    {
        private readonly IClinicalService _clinicalService;
        private readonly ILoggingService _loggingService;

        public ClinicalController(IClinicalService clinicalService, ILoggingService loggingService)
        {
            _clinicalService = clinicalService;
            _loggingService = loggingService;
        }

        [HttpGet("encounters/{patientId}")]
        [Authorize(Policy = EMRAuthorizationPolicies.ViewClinicalData)]
        public async Task<ActionResult<IEnumerable<Encounter>>> GetEncounters(int patientId)
        {
            try
            {
                var encounters = await _clinicalService.GetPatientEncountersAsync(patientId);
                await _loggingService.LogAuditAsync("GetEncounters", $"Retrieved encounters for patient {patientId}", User.Identity.Name);
                return Ok(encounters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("GetEncounters", ex.Message, ex);
                return StatusCode(500, "An error occurred while retrieving encounters");
            }
        }

        [HttpPost("encounters")]
        [Authorize(Policy = EMRAuthorizationPolicies.ModifyClinicalData)]
        public async Task<ActionResult<Encounter>> CreateEncounter([FromBody] Encounter encounter)
        {
            try
            {
                var result = await _clinicalService.CreateEncounterAsync(encounter);
                await _loggingService.LogAuditAsync("CreateEncounter", $"Created encounter for patient {encounter.PatientId}", User.Identity.Name);
                return CreatedAtAction(nameof(GetEncounter), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("CreateEncounter", ex.Message, ex);
                return StatusCode(500, "An error occurred while creating the encounter");
            }
        }

        [HttpGet("encounters/detail/{id}")]
        [Authorize(Policy = EMRAuthorizationPolicies.ViewClinicalData)]
        public async Task<ActionResult<Encounter>> GetEncounter(int id)
        {
            try
            {
                var encounter = await _clinicalService.GetEncounterAsync(id);
                if (encounter == null)
                    return NotFound();

                await _loggingService.LogAuditAsync("GetEncounter", $"Retrieved encounter {id}", User.Identity.Name);
                return Ok(encounter);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("GetEncounter", ex.Message, ex);
                return StatusCode(500, "An error occurred while retrieving the encounter");
            }
        }

        [HttpPost("orders")]
        [Authorize(Policy = EMRAuthorizationPolicies.ModifyClinicalData)]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] Order order)
        {
            try
            {
                var result = await _clinicalService.CreateOrderAsync(order);
                await _loggingService.LogAuditAsync("CreateOrder", $"Created order for encounter {order.EncounterId}", User.Identity.Name);
                return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("CreateOrder", ex.Message, ex);
                return StatusCode(500, "An error occurred while creating the order");
            }
        }

        [HttpGet("orders/{id}")]
        [Authorize(Policy = EMRAuthorizationPolicies.ViewClinicalData)]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            try
            {
                var order = await _clinicalService.GetOrderAsync(id);
                if (order == null)
                    return NotFound();

                await _loggingService.LogAuditAsync("GetOrder", $"Retrieved order {id}", User.Identity.Name);
                return Ok(order);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("GetOrder", ex.Message, ex);
                return StatusCode(500, "An error occurred while retrieving the order");
            }
        }

        [HttpPost("clinicalnotes")]
        [Authorize(Policy = EMRAuthorizationPolicies.ModifyClinicalData)]
        public async Task<ActionResult<ClinicalNote>> CreateClinicalNote([FromBody] ClinicalNote note)
        {
            try
            {
                var result = await _clinicalService.CreateClinicalNoteAsync(note);
                await _loggingService.LogAuditAsync("CreateClinicalNote", $"Created note for encounter {note.EncounterId}", User.Identity.Name);
                return CreatedAtAction(nameof(GetClinicalNote), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("CreateClinicalNote", ex.Message, ex);
                return StatusCode(500, "An error occurred while creating the clinical note");
            }
        }

        [HttpGet("clinicalnotes/{id}")]
        [Authorize(Policy = EMRAuthorizationPolicies.ViewClinicalData)]
        public async Task<ActionResult<ClinicalNote>> GetClinicalNote(int id)
        {
            try
            {
                var note = await _clinicalService.GetClinicalNoteAsync(id);
                if (note == null)
                    return NotFound();

                await _loggingService.LogAuditAsync("GetClinicalNote", $"Retrieved clinical note {id}", User.Identity.Name);
                return Ok(note);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("GetClinicalNote", ex.Message, ex);
                return StatusCode(500, "An error occurred while retrieving the clinical note");
            }
        }
    }
}
