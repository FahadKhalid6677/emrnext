using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.Services;
using EMRNext.Web.Models.API;
using AutoMapper;

namespace EMRNext.Web.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PatientEngagementController : ControllerBase
    {
        private readonly PatientEngagementService _patientEngagementService;
        private readonly IMapper _mapper;

        public PatientEngagementController(
            PatientEngagementService patientEngagementService,
            IMapper mapper)
        {
            _patientEngagementService = patientEngagementService;
            _mapper = mapper;
        }

        [HttpPost("portal")]
        public async Task<ActionResult<PatientPortalDto>> CreatePatientPortal(Guid patientId)
        {
            var portal = await _patientEngagementService.CreatePatientPortalAccount(patientId);
            return Ok(_mapper.Map<PatientPortalDto>(portal));
        }

        [HttpPost("{patientId}/health-goals")]
        public async Task<ActionResult<HealthGoalDto>> CreateHealthGoal(
            Guid patientId, 
            [FromBody] HealthGoalDto healthGoalDto)
        {
            var healthGoal = _mapper.Map<Core.Models.HealthGoal>(healthGoalDto);
            var createdGoal = await _patientEngagementService.CreateHealthGoal(patientId, healthGoal);
            return Ok(_mapper.Map<HealthGoalDto>(createdGoal));
        }

        [HttpPost("health-goals/{healthGoalId}/progress")]
        public async Task<ActionResult<HealthGoalProgressDto>> AddHealthGoalProgress(
            Guid healthGoalId, 
            [FromBody] HealthGoalProgressDto progressDto)
        {
            var progress = await _patientEngagementService.AddHealthGoalProgress(
                healthGoalId, 
                progressDto.ProgressValue, 
                progressDto.Notes
            );
            return Ok(_mapper.Map<HealthGoalProgressDto>(progress));
        }

        [HttpPost("{patientId}/reminders")]
        public async Task<ActionResult<PatientReminderDto>> CreateReminder(
            Guid patientId, 
            [FromBody] PatientReminderDto reminderDto)
        {
            var reminder = _mapper.Map<Core.Models.PatientReminder>(reminderDto);
            var createdReminder = await _patientEngagementService.CreateReminder(patientId, reminder);
            return Ok(_mapper.Map<PatientReminderDto>(createdReminder));
        }

        [HttpGet("{patientId}/education-resources")]
        public async Task<ActionResult<List<PatientEducationResourceDto>>> GetRecommendedResources(Guid patientId)
        {
            var resources = await _patientEngagementService.GetRecommendedResources(patientId);
            return Ok(_mapper.Map<List<PatientEducationResourceDto>>(resources));
        }

        [HttpPost("{patientId}/surveys")]
        public async Task<ActionResult<PatientSurveyDto>> IssueSurvey(
            Guid patientId, 
            [FromBody] SurveyType surveyType)
        {
            var survey = await _patientEngagementService.IssueSurvey(patientId, surveyType);
            return Ok(_mapper.Map<PatientSurveyDto>(survey));
        }
    }
}
