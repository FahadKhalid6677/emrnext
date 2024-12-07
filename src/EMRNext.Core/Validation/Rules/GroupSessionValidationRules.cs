using EMRNext.Core.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EMRNext.Core.Validation.Rules
{
    public class GroupSessionValidationRules : IValidationRule<GroupSession>
    {
        private readonly IProviderService _providerService;
        private readonly ILocationService _locationService;
        private readonly IGroupSeriesService _groupSeriesService;
        private readonly IParticipantService _participantService;

        public GroupSessionValidationRules(
            IProviderService providerService,
            ILocationService locationService,
            IGroupSeriesService groupSeriesService,
            IParticipantService participantService)
        {
            _providerService = providerService;
            _locationService = locationService;
            _groupSeriesService = groupSeriesService;
            _participantService = participantService;
        }

        public async Task<ValidationResult> ValidateAsync(GroupSession entity, ValidationContext context)
        {
            var result = new ValidationResult();

            // Basic validation
            if (entity.StartTime == default)
            {
                result.AddError(nameof(entity.StartTime), "Start time is required");
            }

            if (entity.EndTime == default)
            {
                result.AddError(nameof(entity.EndTime), "End time is required");
            }

            if (entity.StartTime >= entity.EndTime)
            {
                result.AddError(nameof(entity.StartTime), "Start time must be before end time");
            }

            // Duration validation
            var duration = entity.EndTime - entity.StartTime;
            if (duration.TotalMinutes > 480) // 8 hours max
            {
                result.AddError(nameof(entity.EndTime), "Session duration cannot exceed 8 hours");
            }

            // Group series validation
            if (entity.GroupSeriesId > 0)
            {
                var groupSeries = await _groupSeriesService.GetByIdAsync(entity.GroupSeriesId);
                if (groupSeries == null)
                {
                    result.AddError(nameof(entity.GroupSeriesId), "Invalid group series");
                }
                else
                {
                    // Check if session falls within series date range
                    if (entity.StartTime < groupSeries.StartDate || entity.EndTime > groupSeries.EndDate)
                    {
                        result.AddError("Session", "Session must fall within the group series date range");
                    }

                    // Check if session follows the series schedule pattern
                    if (!await _groupSeriesService.ValidateSessionScheduleAsync(groupSeries.Id, entity.StartTime))
                    {
                        result.AddError("Session", "Session does not follow the group series schedule pattern");
                    }
                }
            }

            // Provider validation
            if (entity.ProviderId > 0)
            {
                var provider = await _providerService.GetProviderByIdAsync(entity.ProviderId);
                if (provider == null)
                {
                    result.AddError(nameof(entity.ProviderId), "Invalid provider");
                }
                else
                {
                    // Check provider availability
                    var isAvailable = await _providerService.CheckAvailabilityAsync(
                        provider.Id, entity.StartTime, entity.EndTime);

                    if (!isAvailable)
                    {
                        result.AddError(nameof(entity.ProviderId), "Provider is not available for this time slot");
                    }

                    // Check provider qualifications if template exists
                    if (entity.TemplateId > 0)
                    {
                        var hasQualifications = await _providerService.CheckTemplateQualificationsAsync(
                            provider.Id, entity.TemplateId);

                        if (!hasQualifications)
                        {
                            result.AddError(nameof(entity.ProviderId), 
                                "Provider does not meet required qualifications for this template",
                                ValidationSeverity.Warning);
                        }
                    }
                }
            }

            // Location validation
            if (entity.LocationId > 0)
            {
                var location = await _locationService.GetLocationByIdAsync(entity.LocationId);
                if (location == null)
                {
                    result.AddError(nameof(entity.LocationId), "Invalid location");
                }
                else
                {
                    // Check location availability
                    var isAvailable = await _locationService.CheckAvailabilityAsync(
                        location.Id, entity.StartTime, entity.EndTime);

                    if (!isAvailable)
                    {
                        result.AddError(nameof(entity.LocationId), "Location is not available for this time slot");
                    }

                    // Check location capacity against participants
                    if (entity.Participants?.Count > location.Capacity)
                    {
                        result.AddError(nameof(entity.LocationId), 
                            "Number of participants exceeds location capacity");
                    }
                }
            }

            // Participants validation
            if (entity.Participants?.Any() == true)
            {
                foreach (var participant in entity.Participants)
                {
                    // Check if participant exists
                    var isValid = await _participantService.ValidateParticipantAsync(participant.Id);
                    if (!isValid)
                    {
                        result.AddError("Participants", $"Invalid participant: {participant.Id}");
                        continue;
                    }

                    // Check participant availability
                    var isAvailable = await _participantService.CheckAvailabilityAsync(
                        participant.Id, entity.StartTime, entity.EndTime);

                    if (!isAvailable)
                    {
                        result.AddError("Participants", 
                            $"Participant {participant.Id} has a scheduling conflict",
                            ValidationSeverity.Warning);
                    }

                    // Check participant eligibility if template exists
                    if (entity.TemplateId > 0)
                    {
                        var isEligible = await _participantService.CheckTemplateEligibilityAsync(
                            participant.Id, entity.TemplateId);

                        if (!isEligible)
                        {
                            result.AddError("Participants", 
                                $"Participant {participant.Id} does not meet eligibility criteria for this template",
                                ValidationSeverity.Warning);
                        }
                    }
                }

                // Check minimum participants if template exists
                if (entity.TemplateId > 0)
                {
                    var template = await _groupSeriesService.GetTemplateByIdAsync(entity.TemplateId);
                    if (template != null && entity.Participants.Count < template.MinParticipants)
                    {
                        result.AddError("Participants", 
                            $"Number of participants is below the minimum required ({template.MinParticipants})");
                    }
                }
            }

            // Materials validation
            if (entity.Materials?.Any() == true)
            {
                foreach (var material in entity.Materials)
                {
                    if (string.IsNullOrWhiteSpace(material.Name))
                    {
                        result.AddError("Materials", "Material name is required");
                    }

                    if (material.Quantity <= 0)
                    {
                        result.AddError("Materials", "Material quantity must be greater than 0");
                    }
                }
            }

            // Outcome measures validation
            if (entity.OutcomeMeasures?.Any() == true)
            {
                foreach (var measure in entity.OutcomeMeasures)
                {
                    if (string.IsNullOrWhiteSpace(measure.Name))
                    {
                        result.AddError("OutcomeMeasures", "Outcome measure name is required");
                    }

                    if (!measure.Value.HasValue && !measure.IsOptional)
                    {
                        result.AddError("OutcomeMeasures", 
                            $"Value is required for outcome measure: {measure.Name}");
                    }
                }
            }

            return result;
        }
    }
}
