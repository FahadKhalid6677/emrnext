using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.Services;
using EMRNext.Core.Models;
using EMRNext.Web.Models.API;
using System.Linq;

namespace EMRNext.Web.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class CareCoordinationController : ControllerBase
    {
        private readonly CareCoordinationService _careCoordinationService;

        public CareCoordinationController(CareCoordinationService careCoordinationService)
        {
            _careCoordinationService = careCoordinationService;
        }

        [HttpPost("referral")]
        public async Task<ActionResult<ReferralDto>> CreateReferral([FromBody] ReferralDto referralDto)
        {
            try
            {
                var referral = new Referral
                {
                    PatientId = referralDto.PatientId,
                    ReferringProviderId = referralDto.ReferringProviderId,
                    ReceivingProviderId = referralDto.ReceivingProviderId,
                    ReferralType = referralDto.ReferralType,
                    Specialty = referralDto.Specialty,
                    Reason = referralDto.Reason,
                    ClinicalNotes = referralDto.ClinicalNotes
                };

                var createdReferral = await _careCoordinationService.CreateReferralAsync(referral);

                return Ok(new ReferralDto
                {
                    Id = createdReferral.Id,
                    PatientId = createdReferral.PatientId,
                    ReferringProviderId = createdReferral.ReferringProviderId,
                    ReceivingProviderId = createdReferral.ReceivingProviderId,
                    ReferralType = createdReferral.ReferralType,
                    Specialty = createdReferral.Specialty,
                    Reason = createdReferral.Reason,
                    ClinicalNotes = createdReferral.ClinicalNotes,
                    Status = createdReferral.Status.ToString(),
                    CreatedAt = createdReferral.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating referral", details = ex.Message });
            }
        }

        [HttpPost("care-transition")]
        public async Task<ActionResult<CareTransitionDto>> InitiateCareTransition([FromBody] CareTransitionDto transitionDto)
        {
            try
            {
                var careTransition = new CareTransition
                {
                    PatientId = transitionDto.PatientId,
                    FromProviderId = transitionDto.FromProviderId,
                    ToProviderId = transitionDto.ToProviderId,
                    TransitionType = transitionDto.TransitionType,
                    Summary = transitionDto.Summary,
                    KeyCommunicationPoints = transitionDto.KeyCommunicationPoints,
                    MedicationChanges = transitionDto.MedicationChanges
                };

                var createdTransition = await _careCoordinationService.InitiateCareTransitionAsync(careTransition);

                return Ok(new CareTransitionDto
                {
                    Id = createdTransition.Id,
                    PatientId = createdTransition.PatientId,
                    FromProviderId = createdTransition.FromProviderId,
                    ToProviderId = createdTransition.ToProviderId,
                    TransitionType = createdTransition.TransitionType,
                    Summary = createdTransition.Summary,
                    KeyCommunicationPoints = createdTransition.KeyCommunicationPoints,
                    MedicationChanges = createdTransition.MedicationChanges,
                    Status = createdTransition.Status.ToString(),
                    TransitionDate = createdTransition.TransitionDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error initiating care transition", details = ex.Message });
            }
        }

        [HttpPost("care-team")]
        public async Task<ActionResult<CareTeamDto>> CreateCareTeam([FromBody] CareTeamDto careTeamDto)
        {
            try
            {
                var careTeam = new CareTeam
                {
                    PatientId = careTeamDto.PatientId,
                    TeamName = careTeamDto.TeamName,
                    Members = careTeamDto.Members.Select(m => new CareTeamMember
                    {
                        ProviderId = m.ProviderId,
                        Name = m.Name,
                        Role = m.Role,
                        Specialty = m.Specialty
                    }).ToList()
                };

                var createdCareTeam = await _careCoordinationService.CreateCareTeamAsync(careTeam);

                return Ok(new CareTeamDto
                {
                    Id = createdCareTeam.Id,
                    PatientId = createdCareTeam.PatientId,
                    TeamName = createdCareTeam.TeamName,
                    CreatedAt = createdCareTeam.CreatedAt,
                    Members = createdCareTeam.Members.Select(m => new CareTeamMemberDto
                    {
                        Id = m.Id,
                        ProviderId = m.ProviderId,
                        Name = m.Name,
                        Role = m.Role,
                        Specialty = m.Specialty,
                        Status = m.Status.ToString()
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error creating care team", details = ex.Message });
            }
        }

        [HttpPost("communication")]
        public async Task<ActionResult<CommunicationLogDto>> SendCommunication([FromBody] CommunicationLogDto communicationDto)
        {
            try
            {
                var communication = new CommunicationLog
                {
                    SenderId = communicationDto.SenderId,
                    ReceiverId = communicationDto.ReceiverId,
                    MessageContent = communicationDto.MessageContent,
                    Type = Enum.Parse<CommunicationType>(communicationDto.Type)
                };

                var sentCommunication = await _careCoordinationService.SendCommunicationAsync(communication);

                return Ok(new CommunicationLogDto
                {
                    Id = sentCommunication.Id,
                    SenderId = sentCommunication.SenderId,
                    ReceiverId = sentCommunication.ReceiverId,
                    MessageContent = sentCommunication.MessageContent,
                    Type = sentCommunication.Type.ToString(),
                    Timestamp = sentCommunication.Timestamp,
                    IsRead = sentCommunication.IsRead
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error sending communication", details = ex.Message });
            }
        }

        [HttpGet("referrals/{patientId}")]
        public async Task<ActionResult<List<ReferralDto>>> GetPatientReferrals(Guid patientId)
        {
            try
            {
                var referrals = await _careCoordinationService.GetPatientReferralsAsync(patientId);

                return Ok(referrals.Select(r => new ReferralDto
                {
                    Id = r.Id,
                    PatientId = r.PatientId,
                    ReferringProviderId = r.ReferringProviderId,
                    ReceivingProviderId = r.ReceivingProviderId,
                    ReferralType = r.ReferralType,
                    Specialty = r.Specialty,
                    Reason = r.Reason,
                    ClinicalNotes = r.ClinicalNotes,
                    Status = r.Status.ToString(),
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    CompletedAt = r.CompletedAt
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving patient referrals", details = ex.Message });
            }
        }

        [HttpPut("referral/{referralId}/status")]
        public async Task<IActionResult> UpdateReferralStatus(Guid referralId, [FromBody] string newStatus)
        {
            try
            {
                var status = Enum.Parse<ReferralStatus>(newStatus);
                await _careCoordinationService.UpdateReferralStatusAsync(referralId, status);
                return Ok(new { message = "Referral status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating referral status", details = ex.Message });
            }
        }
    }
}
