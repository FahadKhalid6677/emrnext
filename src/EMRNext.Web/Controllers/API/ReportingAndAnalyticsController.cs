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
    public class ReportingAndAnalyticsController : ControllerBase
    {
        private readonly ReportingAndAnalyticsService _reportingService;

        public ReportingAndAnalyticsController(ReportingAndAnalyticsService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpPost("clinical-report")]
        public async Task<ActionResult<ClinicalReportDto>> GenerateClinicalReport([FromBody] ReportRequestDto reportRequest)
        {
            try
            {
                if (!reportRequest.PatientId.HasValue)
                {
                    return BadRequest("Patient ID is required for clinical report generation");
                }

                var report = await _reportingService.GeneratePatientClinicalReportAsync(
                    reportRequest.PatientId.Value, 
                    reportRequest.StartDate, 
                    reportRequest.EndDate
                );

                return Ok(new ClinicalReportDto
                {
                    Id = report.Id,
                    PatientId = report.PatientId,
                    ReportType = report.ReportType,
                    GeneratedAt = report.GeneratedAt,
                    StartDate = report.StartDate,
                    EndDate = report.EndDate,
                    ReportData = report.ReportData,
                    Sections = report.Sections.Select(s => new ReportSectionDto
                    {
                        Title = s.Title,
                        Summary = s.Summary,
                        Metrics = s.Metrics,
                        Highlights = s.Highlights
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error generating clinical report", details = ex.Message });
            }
        }

        [HttpGet("population-health")]
        public async Task<ActionResult<PopulationHealthMetricsDto>> GetPopulationHealthMetrics()
        {
            try
            {
                var metrics = await _reportingService.CalculatePopulationHealthMetricsAsync();

                return Ok(new PopulationHealthMetricsDto
                {
                    Id = metrics.Id,
                    CalculatedAt = metrics.CalculatedAt,
                    TotalPatients = metrics.TotalPatients,
                    PatientDemographics = metrics.PatientDemographics,
                    ChronicConditionPrevalence = metrics.ChronicConditionPrevalence,
                    RiskStratification = metrics.RiskStratification
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error calculating population health metrics", details = ex.Message });
            }
        }

        [HttpGet("performance-metrics")]
        public async Task<ActionResult<List<PerformanceMetricDto>>> GetPerformanceMetrics()
        {
            try
            {
                var metrics = await _reportingService.GeneratePerformanceMetricsAsync();

                return Ok(metrics.Select(m => new PerformanceMetricDto
                {
                    Id = m.Id,
                    MetricName = m.MetricName,
                    Category = m.Category,
                    Value = m.Value,
                    Unit = m.Unit,
                    CalculatedAt = m.CalculatedAt,
                    Trends = m.Trends.Select(t => new PerformanceTrendDto
                    {
                        Timestamp = t.Timestamp,
                        Value = t.Value
                    }).ToList()
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error generating performance metrics", details = ex.Message });
            }
        }

        [HttpGet("audit-trail")]
        public async Task<ActionResult<List<AuditTrailDto>>> GetRecentAuditTrails([FromQuery] int days = 30)
        {
            try
            {
                var auditTrails = await _reportingService.GetRecentAuditTrailsAsync(days);

                return Ok(auditTrails.Select(a => new AuditTrailDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    Action = a.Action,
                    Entity = a.Entity,
                    Details = a.Details,
                    Timestamp = a.Timestamp,
                    IPAddress = a.IPAddress
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving audit trails", details = ex.Message });
            }
        }
    }
}
