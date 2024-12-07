using AutoMapper;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMRNext.API.Controllers
{
    [ApiController]
    [Route("api/fhir")]
    [Authorize]
    public class FhirController : ControllerBase
    {
        private readonly IGroupSeriesService _groupSeriesService;
        private readonly IMapper _mapper;
        private readonly FhirClient _fhirClient;

        public FhirController(
            IGroupSeriesService groupSeriesService,
            IMapper mapper,
            FhirClient fhirClient)
        {
            _groupSeriesService = groupSeriesService;
            _mapper = mapper;
            _fhirClient = fhirClient;
        }

        [HttpGet("Group/{id}")]
        public async Task<IActionResult> GetGroup(string id)
        {
            try
            {
                var groupSeries = await _groupSeriesService.GetByIdAsync(int.Parse(id));
                if (groupSeries == null)
                    return NotFound();

                var group = _mapper.Map<Group>(groupSeries);
                return Ok(group);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid ID format");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("Group")]
        public async Task<IActionResult> SearchGroups([FromQuery] string name, [FromQuery] string status)
        {
            try
            {
                var searchResults = await _groupSeriesService.SearchAsync(name, status);
                var bundle = new Bundle
                {
                    Type = Bundle.BundleType.Searchset,
                    Total = searchResults.Count(),
                    Entry = searchResults.Select(gs => new Bundle.EntryComponent
                    {
                        Resource = _mapper.Map<Group>(gs),
                        Search = new Bundle.SearchComponent { Mode = Bundle.SearchEntryMode.Match }
                    }).ToList()
                };

                return Ok(bundle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Group")]
        public async Task<IActionResult> CreateGroup([FromBody] Group group)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var groupSeries = _mapper.Map<GroupSeries>(group);
                var created = await _groupSeriesService.CreateAsync(groupSeries);
                var createdGroup = _mapper.Map<Group>(created);

                return Created($"/api/fhir/Group/{created.Id}", createdGroup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("Group/{id}")]
        public async Task<IActionResult> UpdateGroup(string id, [FromBody] Group group)
        {
            try
            {
                if (id != group.Id)
                    return BadRequest("ID mismatch");

                var groupSeries = _mapper.Map<GroupSeries>(group);
                var updated = await _groupSeriesService.UpdateAsync(groupSeries);
                var updatedGroup = _mapper.Map<Group>(updated);

                return Ok(updatedGroup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("Group/{id}")]
        public async Task<IActionResult> DeleteGroup(string id)
        {
            try
            {
                await _groupSeriesService.DeleteAsync(int.Parse(id));
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // FHIR Operation endpoints
        [HttpPost("Group/{id}/$close-group")]
        public async Task<IActionResult> CloseGroup(string id)
        {
            try
            {
                var groupSeries = await _groupSeriesService.GetByIdAsync(int.Parse(id));
                if (groupSeries == null)
                    return NotFound();

                groupSeries.Status = "Completed";
                var updated = await _groupSeriesService.UpdateAsync(groupSeries);
                var group = _mapper.Map<Group>(updated);

                return Ok(group);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Group/{id}/$add-member")]
        public async Task<IActionResult> AddMember(string id, [FromBody] Parameters parameters)
        {
            try
            {
                var patientRef = parameters.GetSingleValue<ResourceReference>("patient");
                if (patientRef == null)
                    return BadRequest("Patient reference is required");

                var patientId = int.Parse(patientRef.Reference.Split('/')[1]);
                await _groupSeriesService.AddParticipantAsync(int.Parse(id), patientId);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("Encounter/{id}")]
        public async Task<IActionResult> GetEncounter(string id)
        {
            try
            {
                var session = await _groupSeriesService.GetSessionByIdAsync(int.Parse(id));
                if (session == null)
                    return NotFound();

                var encounter = _mapper.Map<Encounter>(session);
                return Ok(encounter);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("Encounter")]
        public async Task<IActionResult> SearchEncounters([FromQuery] string groupId, [FromQuery] string date)
        {
            try
            {
                var searchResults = await _groupSeriesService.SearchSessionsAsync(
                    groupId != null ? int.Parse(groupId) : null,
                    date != null ? DateTime.Parse(date) : null);

                var bundle = new Bundle
                {
                    Type = Bundle.BundleType.Searchset,
                    Total = searchResults.Count(),
                    Entry = searchResults.Select(s => new Bundle.EntryComponent
                    {
                        Resource = _mapper.Map<Encounter>(s),
                        Search = new Bundle.SearchComponent { Mode = Bundle.SearchEntryMode.Match }
                    }).ToList()
                };

                return Ok(bundle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("Observation")]
        public async Task<IActionResult> SearchObservations([FromQuery] string groupId, [FromQuery] string patientId)
        {
            try
            {
                var outcomes = new List<Observation>();

                if (groupId != null)
                {
                    var seriesOutcomes = await _groupSeriesService.GetSeriesOutcomesAsync(int.Parse(groupId));
                    outcomes.AddRange(seriesOutcomes.Select(o => _mapper.Map<Observation>(o)));
                }

                if (patientId != null)
                {
                    var participantOutcomes = await _groupSeriesService.GetParticipantOutcomesAsync(int.Parse(patientId));
                    outcomes.AddRange(participantOutcomes.Select(o => _mapper.Map<Observation>(o)));
                }

                var bundle = new Bundle
                {
                    Type = Bundle.BundleType.Searchset,
                    Total = outcomes.Count,
                    Entry = outcomes.Select(o => new Bundle.EntryComponent
                    {
                        Resource = o,
                        Search = new Bundle.SearchComponent { Mode = Bundle.SearchEntryMode.Match }
                    }).ToList()
                };

                return Ok(bundle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("CarePlan/{id}")]
        public async Task<IActionResult> GetCarePlan(string id)
        {
            try
            {
                var template = await _groupSeriesService.GetTemplateByIdAsync(int.Parse(id));
                if (template == null)
                    return NotFound();

                var carePlan = _mapper.Map<CarePlan>(template);
                return Ok(carePlan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("Group/{id}/$everything")]
        public async Task<IActionResult> GetGroupEverything(string id)
        {
            try
            {
                var groupSeries = await _groupSeriesService.GetByIdAsync(int.Parse(id));
                if (groupSeries == null)
                    return NotFound();

                var bundle = new Bundle
                {
                    Type = Bundle.BundleType.Searchset,
                    Entry = new List<Bundle.EntryComponent>()
                };

                // Add group resource
                bundle.Entry.Add(new Bundle.EntryComponent
                {
                    Resource = _mapper.Map<Group>(groupSeries)
                });

                // Add encounters
                var sessions = await _groupSeriesService.GetSessionsAsync(int.Parse(id));
                bundle.Entry.AddRange(sessions.Select(s => new Bundle.EntryComponent
                {
                    Resource = _mapper.Map<Encounter>(s)
                }));

                // Add outcomes
                var outcomes = await _groupSeriesService.GetSeriesOutcomesAsync(int.Parse(id));
                bundle.Entry.AddRange(outcomes.Select(o => new Bundle.EntryComponent
                {
                    Resource = _mapper.Map<Observation>(o)
                }));

                // Add care plan
                var template = await _groupSeriesService.GetTemplateByIdAsync(groupSeries.GroupSessionTemplateId);
                if (template != null)
                {
                    bundle.Entry.Add(new Bundle.EntryComponent
                    {
                        Resource = _mapper.Map<CarePlan>(template)
                    });
                }

                return Ok(bundle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
