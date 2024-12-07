using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using EMRNext.Core.Services;
using EMRNext.Core.Services.FHIR;
using EMRNext.Core.Authorization;

namespace EMRNext.Web.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/v1/fhir")]
    [Produces("application/fhir+json")]
    [ApiVersion("1.0")]
    public class FHIRController : ControllerBase
    {
        private readonly IClinicalService _clinicalService;
        private readonly IFHIRMapper _fhirMapper;
        private readonly ILogger<FHIRController> _logger;

        public FHIRController(
            IClinicalService clinicalService,
            IFHIRMapper fhirMapper,
            ILogger<FHIRController> logger)
        {
            _clinicalService = clinicalService;
            _fhirMapper = fhirMapper;
            _logger = logger;
        }

        [HttpGet("metadata")]
        [AllowAnonymous]
        public ActionResult<CapabilityStatement> GetMetadata()
        {
            var capabilityStatement = new CapabilityStatement
            {
                Status = PublicationStatus.Active,
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Kind = CapabilityStatement.CapabilityStatementKind.Instance,
                Software = new CapabilityStatement.SoftwareComponent
                {
                    Name = "EMRNext",
                    Version = "1.0.0"
                },
                Implementation = new CapabilityStatement.ImplementationComponent
                {
                    Description = "EMRNext FHIR API",
                    Url = $"{Request.Scheme}://{Request.Host}/api/v1/fhir"
                },
                FhirVersion = FHIRVersion.N4_0_1,
                Format = new[] { "application/fhir+json" },
                Rest = new List<CapabilityStatement.RestComponent>
                {
                    new CapabilityStatement.RestComponent
                    {
                        Mode = CapabilityStatement.RestfulCapabilityMode.Server,
                        Resource = new List<CapabilityStatement.ResourceComponent>
                        {
                            CreateResourceComponent("Patient", new[] { "read", "search-type" }),
                            CreateResourceComponent("Encounter", new[] { "read", "search-type" }),
                            CreateResourceComponent("Observation", new[] { "read", "search-type" }),
                            CreateResourceComponent("DocumentReference", new[] { "read", "search-type" }),
                            CreateResourceComponent("MedicationRequest", new[] { "read", "search-type" }),
                            CreateResourceComponent("ServiceRequest", new[] { "read", "search-type" }),
                            CreateResourceComponent("DiagnosticReport", new[] { "read", "search-type" })
                        }
                    }
                }
            };

            return Ok(capabilityStatement);
        }

        [HttpGet("Patient/{id}")]
        [Authorize(Policy = EMRPolicies.PatientView)]
        public async Task<ActionResult<Patient>> GetPatient(string id)
        {
            try
            {
                var patient = await _clinicalService.GetPatientAsync(int.Parse(id));
                if (patient == null)
                    return NotFound();

                var fhirPatient = _fhirMapper.MapToFHIRPatient(patient);
                return Ok(fhirPatient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient {PatientId}", id);
                return StatusCode(500, new OperationOutcome
                {
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Code = OperationOutcome.IssueType.Exception,
                            Details = new CodeableConcept { Text = "An error occurred while processing your request." }
                        }
                    }
                });
            }
        }

        [HttpGet("Encounter/{id}")]
        [Authorize(Policy = EMRPolicies.EncounterView)]
        public async Task<ActionResult<Encounter>> GetEncounter(string id)
        {
            try
            {
                var encounter = await _clinicalService.GetEncounterAsync(int.Parse(id));
                if (encounter == null)
                    return NotFound();

                var fhirEncounter = _fhirMapper.MapToFHIREncounter(encounter);
                return Ok(fhirEncounter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving encounter {EncounterId}", id);
                return StatusCode(500, CreateErrorOutcome("An error occurred while processing your request."));
            }
        }

        [HttpGet("Observation")]
        [Authorize(Policy = EMRPolicies.VitalsView)]
        public async Task<ActionResult<Bundle>> SearchVitals([FromQuery] string patient, [FromQuery] string encounter)
        {
            try
            {
                var vitals = new List<Domain.Entities.Vital>();

                if (!string.IsNullOrEmpty(encounter))
                {
                    vitals = await _clinicalService.GetEncounterVitalsAsync(int.Parse(encounter));
                }
                else if (!string.IsNullOrEmpty(patient))
                {
                    vitals = await _clinicalService.GetPatientVitalsHistoryAsync(int.Parse(patient));
                }
                else
                {
                    return BadRequest(CreateErrorOutcome("Either patient or encounter parameter is required."));
                }

                var observations = vitals.Select(v => _fhirMapper.MapToFHIRObservation(v));
                var bundle = _fhirMapper.CreateFHIRBundle("search", observations);
                return Ok(bundle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching vitals");
                return StatusCode(500, CreateErrorOutcome("An error occurred while processing your request."));
            }
        }

        // Helper methods
        private CapabilityStatement.ResourceComponent CreateResourceComponent(string type, string[] interactions)
        {
            return new CapabilityStatement.ResourceComponent
            {
                Type = type,
                Profile = $"http://hl7.org/fhir/StructureDefinition/{type}",
                Interaction = interactions.Select(i => new CapabilityStatement.ResourceInteractionComponent
                {
                    Code = i switch
                    {
                        "read" => CapabilityStatement.TypeRestfulInteraction.Read,
                        "search-type" => CapabilityStatement.TypeRestfulInteraction.SearchType,
                        _ => CapabilityStatement.TypeRestfulInteraction.Read
                    }
                }).ToList()
            };
        }

        private OperationOutcome CreateErrorOutcome(string message)
        {
            return new OperationOutcome
            {
                Issue = new List<OperationOutcome.IssueComponent>
                {
                    new OperationOutcome.IssueComponent
                    {
                        Severity = OperationOutcome.IssueSeverity.Error,
                        Code = OperationOutcome.IssueType.Invalid,
                        Details = new CodeableConcept { Text = message }
                    }
                }
            };
        }
    }
}
