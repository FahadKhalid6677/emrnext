using EMRNext.Core.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EMRNext.Core.Validation.Rules
{
    public class GroupSessionTemplateValidationRules : IValidationRule<GroupSessionTemplate>
    {
        private readonly IProviderService _providerService;
        private readonly ILocationService _locationService;

        public GroupSessionTemplateValidationRules(
            IProviderService providerService,
            ILocationService locationService)
        {
            _providerService = providerService;
            _locationService = locationService;
        }

        public async Task<ValidationResult> ValidateAsync(GroupSessionTemplate entity, ValidationContext context)
        {
            var result = new ValidationResult();

            // Basic validation
            if (string.IsNullOrWhiteSpace(entity.Name))
            {
                result.AddError(nameof(entity.Name), "Name is required");
            }
            else if (entity.Name.Length > 100)
            {
                result.AddError(nameof(entity.Name), "Name cannot exceed 100 characters");
            }

            // Duration validation
            if (entity.DefaultDurationMinutes <= 0)
            {
                result.AddError(nameof(entity.DefaultDurationMinutes), "Duration must be greater than 0");
            }
            else if (entity.DefaultDurationMinutes > 480) // 8 hours max
            {
                result.AddError(nameof(entity.DefaultDurationMinutes), "Duration cannot exceed 8 hours");
            }

            // Participant limits validation
            if (entity.MinParticipants < 2)
            {
                result.AddError(nameof(entity.MinParticipants), "Minimum participants must be at least 2");
            }

            if (entity.MaxParticipants < entity.MinParticipants)
            {
                result.AddError(nameof(entity.MaxParticipants), "Maximum participants must be greater than minimum participants");
            }

            if (entity.MaxParticipants > 50) // Example business rule
            {
                result.AddError(nameof(entity.MaxParticipants), "Maximum participants cannot exceed 50");
            }

            // Provider validation
            if (entity.DefaultProviderId > 0)
            {
                var provider = await _providerService.GetProviderByIdAsync(entity.DefaultProviderId);
                if (provider == null)
                {
                    result.AddError(nameof(entity.DefaultProviderId), "Invalid provider");
                }
                else
                {
                    // Check provider qualifications
                    if (!string.IsNullOrEmpty(entity.RequiredQualifications))
                    {
                        var hasQualifications = await _providerService.CheckQualificationsAsync(
                            provider.Id, entity.RequiredQualifications.Split(','));

                        if (!hasQualifications)
                        {
                            result.AddError(nameof(entity.DefaultProviderId), 
                                "Provider does not meet required qualifications",
                                ValidationSeverity.Warning);
                        }
                    }
                }
            }

            // Location validation
            if (entity.DefaultLocationId > 0)
            {
                var location = await _locationService.GetLocationByIdAsync(entity.DefaultLocationId);
                if (location == null)
                {
                    result.AddError(nameof(entity.DefaultLocationId), "Invalid location");
                }
                else
                {
                    // Check location capacity
                    if (location.Capacity < entity.MaxParticipants)
                    {
                        result.AddError(nameof(entity.DefaultLocationId), 
                            $"Location capacity ({location.Capacity}) is less than maximum participants ({entity.MaxParticipants})",
                            ValidationSeverity.Warning);
                    }

                    // Check location facilities
                    if (!string.IsNullOrEmpty(entity.ClinicalProtocol))
                    {
                        var hasFacilities = await _locationService.CheckFacilitiesAsync(
                            location.Id, entity.ClinicalProtocol);

                        if (!hasFacilities)
                        {
                            result.AddError(nameof(entity.DefaultLocationId), 
                                "Location does not have required facilities for the clinical protocol",
                                ValidationSeverity.Warning);
                        }
                    }
                }
            }

            // Material templates validation
            if (entity.MaterialTemplates?.Any() == true)
            {
                foreach (var material in entity.MaterialTemplates)
                {
                    if (string.IsNullOrWhiteSpace(material.Name))
                    {
                        result.AddError("MaterialTemplates", "Material name is required");
                    }

                    if (string.IsNullOrWhiteSpace(material.StoragePath))
                    {
                        result.AddError("MaterialTemplates", "Material storage path is required");
                    }
                }
            }

            // Documentation templates validation
            if (entity.DocumentationTemplates?.Any() == true)
            {
                foreach (var doc in entity.DocumentationTemplates)
                {
                    if (string.IsNullOrWhiteSpace(doc.Name))
                    {
                        result.AddError("DocumentationTemplates", "Documentation template name is required");
                    }

                    if (string.IsNullOrWhiteSpace(doc.Template))
                    {
                        result.AddError("DocumentationTemplates", "Documentation template content is required");
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

                    if (string.IsNullOrWhiteSpace(measure.MetricDefinition))
                    {
                        result.AddError("OutcomeMeasures", "Outcome measure metric definition is required");
                    }
                }
            }

            // Follow-up protocols validation
            if (entity.FollowUpProtocols?.Any() == true)
            {
                foreach (var protocol in entity.FollowUpProtocols)
                {
                    if (string.IsNullOrWhiteSpace(protocol.Name))
                    {
                        result.AddError("FollowUpProtocols", "Follow-up protocol name is required");
                    }

                    if (protocol.TimeframeInDays <= 0)
                    {
                        result.AddError("FollowUpProtocols", "Follow-up timeframe must be greater than 0 days");
                    }
                }
            }

            return result;
        }
    }
}
