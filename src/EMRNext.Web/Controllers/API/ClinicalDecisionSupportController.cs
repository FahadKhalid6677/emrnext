using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.Services;
using EMRNext.Core.Models;
using EMRNext.Web.Models.API;

namespace EMRNext.Web.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicalDecisionSupportController : ControllerBase
    {
        private readonly ClinicalDecisionSupportService _cdsService;

        public ClinicalDecisionSupportController(ClinicalDecisionSupportService cdsService)
        {
            _cdsService = cdsService;
        }

        [HttpGet("risk-assessment/{patientId}")]
        public async Task<ActionResult<PatientRiskAssessmentDto>> GetPatientRiskAssessment(Guid patientId)
        {
            try
            {
                var riskAssessment = await _cdsService.AssessPatientRiskAsync(patientId);

                return Ok(new PatientRiskAssessmentDto
                {
                    PatientId = riskAssessment.PatientId,
                    RiskFactors = riskAssessment.RiskFactors.Select(rf => new RiskFactorDto
                    {
                        Name = rf.Name,
                        Value = rf.Value,
                        RiskLevel = rf.RiskLevel.ToString(),
                        Description = rf.Description
                    }).ToList(),
                    OverallRiskLevel = riskAssessment.OverallRiskLevel.ToString(),
                    RecommendedInterventions = riskAssessment.RecommendedInterventions,
                    AssessmentDate = riskAssessment.AssessmentDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error generating risk assessment", details = ex.Message });
            }
        }

        [HttpGet("alerts/{patientId}")]
        public async Task<ActionResult<List<AlertNotificationDto>>> GetPatientAlerts(Guid patientId)
        {
            try
            {
                var alerts = await _cdsService.GeneratePatientAlertsAsync(patientId);

                return Ok(alerts.Select(alert => new AlertNotificationDto
                {
                    Id = alert.Id,
                    PatientId = alert.PatientId,
                    Title = alert.Title,
                    Message = alert.Message,
                    Severity = alert.Severity.ToString(),
                    IsRead = alert.IsRead,
                    CreatedAt = alert.CreatedAt
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error generating patient alerts", details = ex.Message });
            }
        }
    }
}
