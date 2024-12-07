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
    public class VitalSignsController : ControllerBase
    {
        private readonly VitalSignsService _vitalSignsService;
        private readonly IMapper _mapper;

        public VitalSignsController(
            VitalSignsService vitalSignsService,
            IMapper mapper)
        {
            _vitalSignsService = vitalSignsService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Policy = "CreateVitalSigns")]
        public async Task<ActionResult<VitalSignsRecordDto>> RecordVitalSigns(
            [FromBody] VitalSignsRecordDto vitalSignsDto)
        {
            var vitalSigns = _mapper.Map<Core.Models.VitalSignsRecord>(vitalSignsDto);
            var recordedVitalSigns = await _vitalSignsService.RecordVitalSignsAsync(vitalSigns);
            return Ok(_mapper.Map<VitalSignsRecordDto>(recordedVitalSigns));
        }

        [HttpGet("{patientId}/history")]
        [Authorize(Policy = "ViewPatientVitalSigns")]
        public async Task<ActionResult<List<VitalSignsRecordDto>>> GetVitalSignsHistory(
            string patientId, 
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            var history = await _vitalSignsService.GetPatientVitalSignHistoryAsync(
                patientId, 
                startDate, 
                endDate
            );
            return Ok(_mapper.Map<List<VitalSignsRecordDto>>(history));
        }

        [HttpGet("{patientId}/trends")]
        [Authorize(Policy = "ViewPatientVitalSigns")]
        public async Task<ActionResult<VitalSignsTrendDto>> GetVitalSignsTrends(string patientId)
        {
            var trends = await _vitalSignsService.AnalyzeVitalSignTrendsAsync(patientId);
            return Ok(_mapper.Map<VitalSignsTrendDto>(trends));
        }

        [HttpGet("{patientId}/alerts")]
        [Authorize(Policy = "ViewPatientVitalSigns")]
        public async Task<ActionResult<List<VitalSignAlertDto>>> GetRecentVitalSignAlerts(string patientId)
        {
            var history = await _vitalSignsService.GetPatientVitalSignHistoryAsync(
                patientId, 
                startDate: DateTime.UtcNow.AddDays(-7)
            );

            var alerts = history
                .Where(h => h.Alerts?.Any() == true)
                .SelectMany(h => h.Alerts)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            return Ok(_mapper.Map<List<VitalSignAlertDto>>(alerts));
        }
    }
}
