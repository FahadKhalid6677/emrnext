using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.Services;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Authorization;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EMRNext.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SchedulingController : ControllerBase
    {
        private readonly ISchedulingService _schedulingService;
        private readonly ILoggingService _loggingService;

        public SchedulingController(ISchedulingService schedulingService, ILoggingService loggingService)
        {
            _schedulingService = schedulingService;
            _loggingService = loggingService;
        }

        [HttpGet("appointments")]
        [Authorize(Policy = EMRAuthorizationPolicies.ViewSchedule)]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var appointments = await _schedulingService.GetAppointmentsAsync(startDate, endDate);
                await _loggingService.LogAuditAsync("GetAppointments", $"Retrieved appointments from {startDate} to {endDate}", User.Identity.Name);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("GetAppointments", ex.Message, ex);
                return StatusCode(500, "An error occurred while retrieving appointments");
            }
        }

        [HttpPost("appointments")]
        [Authorize(Policy = EMRAuthorizationPolicies.ModifySchedule)]
        public async Task<ActionResult<Appointment>> CreateAppointment([FromBody] Appointment appointment)
        {
            try
            {
                var result = await _schedulingService.CreateAppointmentAsync(appointment);
                await _loggingService.LogAuditAsync("CreateAppointment", 
                    $"Created appointment for patient {appointment.PatientId} with provider {appointment.ProviderId}", 
                    User.Identity.Name);
                return CreatedAtAction(nameof(GetAppointment), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("CreateAppointment", ex.Message, ex);
                return StatusCode(500, "An error occurred while creating the appointment");
            }
        }

        [HttpGet("appointments/{id}")]
        [Authorize(Policy = EMRAuthorizationPolicies.ViewSchedule)]
        public async Task<ActionResult<Appointment>> GetAppointment(int id)
        {
            try
            {
                var appointment = await _schedulingService.GetAppointmentAsync(id);
                if (appointment == null)
                    return NotFound();

                await _loggingService.LogAuditAsync("GetAppointment", $"Retrieved appointment {id}", User.Identity.Name);
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("GetAppointment", ex.Message, ex);
                return StatusCode(500, "An error occurred while retrieving the appointment");
            }
        }

        [HttpPut("appointments/{id}")]
        [Authorize(Policy = EMRAuthorizationPolicies.ModifySchedule)]
        public async Task<ActionResult<Appointment>> UpdateAppointment(int id, [FromBody] Appointment appointment)
        {
            try
            {
                if (id != appointment.Id)
                    return BadRequest();

                var result = await _schedulingService.UpdateAppointmentAsync(appointment);
                await _loggingService.LogAuditAsync("UpdateAppointment", $"Updated appointment {id}", User.Identity.Name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("UpdateAppointment", ex.Message, ex);
                return StatusCode(500, "An error occurred while updating the appointment");
            }
        }

        [HttpDelete("appointments/{id}")]
        [Authorize(Policy = EMRAuthorizationPolicies.ModifySchedule)]
        public async Task<ActionResult> CancelAppointment(int id)
        {
            try
            {
                await _schedulingService.CancelAppointmentAsync(id);
                await _loggingService.LogAuditAsync("CancelAppointment", $"Cancelled appointment {id}", User.Identity.Name);
                return NoContent();
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("CancelAppointment", ex.Message, ex);
                return StatusCode(500, "An error occurred while cancelling the appointment");
            }
        }

        [HttpGet("providers/{providerId}/availability")]
        [Authorize(Policy = EMRAuthorizationPolicies.ViewSchedule)]
        public async Task<ActionResult<IEnumerable<TimeSlot>>> GetProviderAvailability(int providerId, [FromQuery] DateTime date)
        {
            try
            {
                var availability = await _schedulingService.GetProviderAvailabilityAsync(providerId, date);
                await _loggingService.LogAuditAsync("GetProviderAvailability", 
                    $"Retrieved availability for provider {providerId} on {date}", 
                    User.Identity.Name);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("GetProviderAvailability", ex.Message, ex);
                return StatusCode(500, "An error occurred while retrieving provider availability");
            }
        }

        [HttpPost("providers/{providerId}/schedule")]
        [Authorize(Policy = EMRAuthorizationPolicies.ModifySchedule)]
        public async Task<ActionResult> SetProviderSchedule(int providerId, [FromBody] List<Schedule> schedule)
        {
            try
            {
                await _schedulingService.SetProviderScheduleAsync(providerId, schedule);
                await _loggingService.LogAuditAsync("SetProviderSchedule", 
                    $"Updated schedule for provider {providerId}", 
                    User.Identity.Name);
                return Ok();
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("SetProviderSchedule", ex.Message, ex);
                return StatusCode(500, "An error occurred while setting provider schedule");
            }
        }
    }
}
