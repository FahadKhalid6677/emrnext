using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.Domain.Entities.Portal;
using EMRNext.Core.Interfaces;

namespace EMRNext.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GroupSeriesController : ControllerBase
    {
        private readonly IGroupSeriesService _groupSeriesService;
        private readonly ILogger<GroupSeriesController> _logger;

        public GroupSeriesController(
            IGroupSeriesService groupSeriesService,
            ILogger<GroupSeriesController> logger)
        {
            _groupSeriesService = groupSeriesService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupSeries>>> GetAllSeries()
        {
            try
            {
                var series = await _groupSeriesService.GetAllSeriesAsync();
                return Ok(series);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all group series");
                return StatusCode(500, "An error occurred while retrieving group series");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupSeries>> GetSeries(Guid id)
        {
            try
            {
                var series = await _groupSeriesService.GetSeriesAsync(id);
                if (series == null)
                    return NotFound();

                return Ok(series);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group series {SeriesId}", id);
                return StatusCode(500, "An error occurred while retrieving the group series");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Provider")]
        public async Task<ActionResult<GroupSeries>> CreateSeries(GroupSeries series)
        {
            try
            {
                var createdSeries = await _groupSeriesService.CreateSeriesAsync(series);
                return CreatedAtAction(nameof(GetSeries), new { id = createdSeries.Id }, createdSeries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group series");
                return StatusCode(500, "An error occurred while creating the group series");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Provider")]
        public async Task<IActionResult> UpdateSeries(Guid id, GroupSeries series)
        {
            if (id != series.Id)
                return BadRequest();

            try
            {
                await _groupSeriesService.UpdateSeriesAsync(series);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group series {SeriesId}", id);
                return StatusCode(500, "An error occurred while updating the group series");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Provider")]
        public async Task<IActionResult> DeleteSeries(Guid id)
        {
            try
            {
                await _groupSeriesService.DeleteSeriesAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group series {SeriesId}", id);
                return StatusCode(500, "An error occurred while deleting the group series");
            }
        }

        [HttpGet("{id}/sessions")]
        public async Task<ActionResult<IEnumerable<GroupAppointment>>> GetUpcomingSessions(Guid id)
        {
            try
            {
                var sessions = await _groupSeriesService.GetUpcomingSessionsAsync(id);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming sessions for series {SeriesId}", id);
                return StatusCode(500, "An error occurred while retrieving upcoming sessions");
            }
        }

        [HttpPost("{id}/sessions")]
        [Authorize(Roles = "Admin,Provider")]
        public async Task<ActionResult<IEnumerable<GroupAppointment>>> GenerateSessions(
            Guid id, DateTime startDate, int numberOfSessions)
        {
            try
            {
                var sessions = await _groupSeriesService.GenerateSessionsAsync(id, startDate, numberOfSessions);
                return Ok(sessions);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sessions for series {SeriesId}", id);
                return StatusCode(500, "An error occurred while generating sessions");
            }
        }

        [HttpPost("{id}/enroll")]
        public async Task<ActionResult<GroupParticipant>> EnrollParticipant(Guid id, Guid patientId)
        {
            try
            {
                var participant = await _groupSeriesService.EnrollParticipantAsync(id, patientId);
                return Ok(participant);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling patient {PatientId} in series {SeriesId}", patientId, id);
                return StatusCode(500, "An error occurred while enrolling the participant");
            }
        }

        [HttpPost("{id}/withdraw")]
        public async Task<IActionResult> WithdrawParticipant(Guid id, Guid patientId)
        {
            try
            {
                await _groupSeriesService.WithdrawParticipantAsync(id, patientId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error withdrawing patient {PatientId} from series {SeriesId}", patientId, id);
                return StatusCode(500, "An error occurred while withdrawing the participant");
            }
        }

        [HttpGet("{id}/participants/{sessionId}")]
        public async Task<ActionResult<IEnumerable<GroupParticipant>>> GetSessionParticipants(Guid id, Guid sessionId)
        {
            try
            {
                var participants = await _groupSeriesService.GetSessionParticipantsAsync(sessionId);
                return Ok(participants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving participants for session {SessionId}", sessionId);
                return StatusCode(500, "An error occurred while retrieving session participants");
            }
        }

        [HttpPost("{id}/participants/{sessionId}/status")]
        public async Task<ActionResult<GroupParticipant>> UpdateParticipantStatus(
            Guid id, Guid sessionId, Guid patientId, string status)
        {
            try
            {
                var participant = await _groupSeriesService.UpdateParticipantStatusAsync(sessionId, patientId, status);
                return Ok(participant);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for patient {PatientId} in session {SessionId}", patientId, sessionId);
                return StatusCode(500, "An error occurred while updating participant status");
            }
        }

        [HttpPost("{id}/outcomes")]
        [Authorize(Roles = "Admin,Provider")]
        public async Task<ActionResult<SeriesOutcome>> RecordOutcome(Guid id, Guid patientId, SeriesOutcome outcome)
        {
            try
            {
                var recordedOutcome = await _groupSeriesService.RecordSeriesOutcomeAsync(id, patientId, outcome);
                return Ok(recordedOutcome);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording outcome for patient {PatientId} in series {SeriesId}", patientId, id);
                return StatusCode(500, "An error occurred while recording the outcome");
            }
        }

        [HttpGet("{id}/outcomes/{patientId}")]
        public async Task<ActionResult<IEnumerable<SeriesOutcome>>> GetParticipantOutcomes(Guid id, Guid patientId)
        {
            try
            {
                var outcomes = await _groupSeriesService.GetParticipantOutcomesAsync(id, patientId);
                return Ok(outcomes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving outcomes for patient {PatientId} in series {SeriesId}", patientId, id);
                return StatusCode(500, "An error occurred while retrieving participant outcomes");
            }
        }

        [HttpGet("{id}/reports/{patientId}")]
        [Authorize(Roles = "Admin,Provider")]
        public async Task<ActionResult<ParticipantReport>> GenerateParticipantReport(Guid id, Guid patientId)
        {
            try
            {
                var report = await _groupSeriesService.GenerateParticipantReportAsync(id, patientId);
                return Ok(report);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report for patient {PatientId} in series {SeriesId}", patientId, id);
                return StatusCode(500, "An error occurred while generating the participant report");
            }
        }

        [HttpPost("{id}/holidays")]
        [Authorize(Roles = "Admin,Provider")]
        public async Task<ActionResult<IEnumerable<GroupAppointment>>> AdjustForHolidays(Guid id)
        {
            try
            {
                var adjustedSessions = await _groupSeriesService.AdjustForHolidaysAsync(id);
                return Ok(adjustedSessions);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting sessions for holidays in series {SeriesId}", id);
                return StatusCode(500, "An error occurred while adjusting sessions for holidays");
            }
        }
    }
}
