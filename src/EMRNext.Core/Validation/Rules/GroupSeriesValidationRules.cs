using EMRNext.Core.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace EMRNext.Core.Validation.Rules
{
    public class GroupSeriesValidationRules : IValidationRule<GroupSeries>
    {
        private readonly IGroupSeriesService _groupSeriesService;
        private readonly IProviderService _providerService;
        private readonly ILocationService _locationService;

        public GroupSeriesValidationRules(
            IGroupSeriesService groupSeriesService,
            IProviderService providerService,
            ILocationService locationService)
        {
            _groupSeriesService = groupSeriesService;
            _providerService = providerService;
            _locationService = locationService;
        }

        public async Task<ValidationResult> ValidateAsync(GroupSeries entity, ValidationContext context)
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

            // Date validation
            if (entity.StartDate < DateTime.Today)
            {
                result.AddError(nameof(entity.StartDate), "Start date cannot be in the past");
            }

            if (entity.EndDate.HasValue && entity.EndDate.Value < entity.StartDate)
            {
                result.AddError(nameof(entity.EndDate), "End date must be after start date");
            }

            // Session count validation
            if (entity.NumberOfSessions <= 0)
            {
                result.AddError(nameof(entity.NumberOfSessions), "Number of sessions must be greater than 0");
            }
            else if (entity.NumberOfSessions > 52) // Example business rule: max 52 sessions per series
            {
                result.AddError(nameof(entity.NumberOfSessions), "Number of sessions cannot exceed 52");
            }

            // Template validation
            var template = await _groupSeriesService.GetTemplateByIdAsync(entity.GroupSessionTemplateId);
            if (template == null)
            {
                result.AddError(nameof(entity.GroupSessionTemplateId), "Invalid template");
            }
            else
            {
                // Validate against template constraints
                if (entity.MinimumAttendance < template.MinParticipants)
                {
                    result.AddError(nameof(entity.MinimumAttendance), 
                        $"Minimum attendance cannot be less than template minimum ({template.MinParticipants})");
                }
            }

            // Complex business rules
            if (context.IsNew)
            {
                // Check for scheduling conflicts
                var hasConflicts = await _groupSeriesService.CheckForSchedulingConflictsAsync(entity);
                if (hasConflicts)
                {
                    result.AddError("Scheduling", "The proposed schedule conflicts with existing series");
                }

                // Check provider availability
                var provider = await _providerService.GetProviderByIdAsync(template?.DefaultProviderId ?? 0);
                if (provider != null)
                {
                    var isAvailable = await _providerService.CheckAvailabilityAsync(
                        provider.Id, entity.StartDate, entity.EndDate ?? entity.StartDate.AddMonths(6));
                    
                    if (!isAvailable)
                    {
                        result.AddError("Provider", "Selected provider is not available for the entire series duration",
                            ValidationSeverity.Warning);
                    }
                }

                // Check location capacity
                var location = await _locationService.GetLocationByIdAsync(template?.DefaultLocationId ?? 0);
                if (location != null && template != null)
                {
                    if (location.Capacity < template.MaxParticipants)
                    {
                        result.AddError("Location", 
                            $"Location capacity ({location.Capacity}) is less than maximum participants ({template.MaxParticipants})",
                            ValidationSeverity.Warning);
                    }
                }
            }

            // Holiday handling validation
            if (entity.AutoAdjustForHolidays && string.IsNullOrWhiteSpace(entity.HolidayHandlingStrategy))
            {
                result.AddError(nameof(entity.HolidayHandlingStrategy), 
                    "Holiday handling strategy is required when auto-adjust for holidays is enabled");
            }

            // Recurrence pattern validation
            if (!string.IsNullOrWhiteSpace(entity.RecurrencePattern))
            {
                try
                {
                    // Validate RRULE format
                    var isValid = await _groupSeriesService.ValidateRecurrencePatternAsync(entity.RecurrencePattern);
                    if (!isValid)
                    {
                        result.AddError(nameof(entity.RecurrencePattern), "Invalid recurrence pattern format");
                    }
                }
                catch (Exception)
                {
                    result.AddError(nameof(entity.RecurrencePattern), "Invalid recurrence pattern");
                }
            }

            return result;
        }
    }
}
