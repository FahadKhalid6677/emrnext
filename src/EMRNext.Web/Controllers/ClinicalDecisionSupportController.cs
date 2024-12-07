using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.ClinicalDecisionSupport.Services;
using EMRNext.Core.ClinicalDecisionSupport.Models;

namespace EMRNext.Web.Controllers
{
    /// <summary>
    /// Controller for Clinical Decision Support operations
    /// </summary>
    [Authorize(Roles = "Physician,Nurse")]
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicalDecisionSupportController : ControllerBase
    {
        private readonly ClinicalDecisionSupportService _cdssService;

        public ClinicalDecisionSupportController(
            ClinicalDecisionSupportService cdssService)
        {
            _cdssService = cdssService ?? throw new ArgumentNullException(nameof(cdssService));
        }

        /// <summary>
        /// Evaluate clinical rules for a specific patient
        /// </summary>
        [HttpGet("evaluate/{patientId}")]
        public async Task<IActionResult> EvaluatePatientRules(Guid patientId)
        {
            var result = await _cdssService.EvaluatePatientRulesAsync(patientId);

            if (!result.Success)
            {
                return NotFound($"Patient with ID {patientId} not found");
            }

            return Ok(new 
            {
                PatientId = patientId,
                RuleResults = result.RuleResults,
                OverallRecommendations = result.OverallRecommendations
            });
        }

        /// <summary>
        /// Create a new clinical decision support rule
        /// </summary>
        [Authorize(Roles = "Administrator,Physician")]
        [HttpPost("rules")]
        public async Task<IActionResult> CreateRule([FromBody] ClinicalRule rule)
        {
            if (rule == null)
            {
                return BadRequest("Invalid rule data");
            }

            try 
            {
                var createdRule = await _cdssService.CreateRuleAsync(rule);
                return CreatedAtAction(
                    nameof(GetRule), 
                    new { id = createdRule.Id }, 
                    createdRule
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating rule: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific clinical decision support rule
        /// </summary>
        [HttpGet("rules/{id}")]
        public IActionResult GetRule(Guid id)
        {
            // Placeholder for rule retrieval logic
            // Would typically involve repository lookup
            return Ok();
        }
    }
}
