using System;
using FluentValidation;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Validation
{
    public class AppointmentValidator : AbstractValidator<Appointment>
    {
        public AppointmentValidator()
        {
            RuleFor(a => a.PatientId)
                .NotEmpty().WithMessage("Patient ID is required");

            RuleFor(a => a.ProviderId)
                .NotEmpty().WithMessage("Provider ID is required");

            RuleFor(a => a.StartTime)
                .NotEmpty().WithMessage("Start time is required")
                .GreaterThan(DateTime.UtcNow).WithMessage("Appointment must be in the future");

            RuleFor(a => a.EndTime)
                .NotEmpty().WithMessage("End time is required")
                .GreaterThan(a => a.StartTime).WithMessage("End time must be after start time");

            RuleFor(a => a.AppointmentType)
                .NotEmpty().WithMessage("Appointment type is required")
                .MaximumLength(50).WithMessage("Appointment type cannot exceed 50 characters");

            RuleFor(a => a.Status)
                .NotEmpty().WithMessage("Appointment status is required")
                .IsInEnum().WithMessage("Invalid appointment status");

            RuleFor(a => a.Location)
                .NotEmpty().WithMessage("Location is required")
                .MaximumLength(100).WithMessage("Location cannot exceed 100 characters");
        }
    }

    public class TimeSlotValidator : AbstractValidator<TimeSlot>
    {
        public TimeSlotValidator()
        {
            RuleFor(t => t.ProviderId)
                .NotEmpty().WithMessage("Provider ID is required");

            RuleFor(t => t.StartTime)
                .NotEmpty().WithMessage("Start time is required");

            RuleFor(t => t.EndTime)
                .NotEmpty().WithMessage("End time is required")
                .GreaterThan(t => t.StartTime).WithMessage("End time must be after start time");

            RuleFor(t => t.Status)
                .NotEmpty().WithMessage("Time slot status is required")
                .IsInEnum().WithMessage("Invalid time slot status");
        }
    }

    public class ResourceValidator : AbstractValidator<Resource>
    {
        public ResourceValidator()
        {
            RuleFor(r => r.ResourceName)
                .NotEmpty().WithMessage("Resource name is required")
                .MaximumLength(100).WithMessage("Resource name cannot exceed 100 characters");

            RuleFor(r => r.ResourceType)
                .NotEmpty().WithMessage("Resource type is required")
                .MaximumLength(50).WithMessage("Resource type cannot exceed 50 characters");

            RuleFor(r => r.Location)
                .NotEmpty().WithMessage("Location is required")
                .MaximumLength(100).WithMessage("Location cannot exceed 100 characters");

            RuleFor(r => r.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be greater than 0");
        }
    }

    public class WaitlistEntryValidator : AbstractValidator<WaitlistEntry>
    {
        public WaitlistEntryValidator()
        {
            RuleFor(w => w.PatientId)
                .NotEmpty().WithMessage("Patient ID is required");

            RuleFor(w => w.RequestedServiceType)
                .NotEmpty().WithMessage("Requested service type is required")
                .MaximumLength(50).WithMessage("Service type cannot exceed 50 characters");

            RuleFor(w => w.PreferredProviderId)
                .NotEmpty().WithMessage("Preferred provider ID is required");

            RuleFor(w => w.Priority)
                .NotEmpty().WithMessage("Priority is required")
                .IsInEnum().WithMessage("Invalid priority level");

            RuleFor(w => w.RequestDate)
                .NotEmpty().WithMessage("Request date is required");

            RuleFor(w => w.PreferredTimeRanges)
                .NotEmpty().WithMessage("At least one preferred time range is required");
        }
    }

    public class ReminderValidator : AbstractValidator<Reminder>
    {
        public ReminderValidator()
        {
            RuleFor(r => r.AppointmentId)
                .NotEmpty().WithMessage("Appointment ID is required");

            RuleFor(r => r.ReminderType)
                .NotEmpty().WithMessage("Reminder type is required")
                .IsInEnum().WithMessage("Invalid reminder type");

            RuleFor(r => r.ReminderTime)
                .NotEmpty().WithMessage("Reminder time is required")
                .GreaterThan(DateTime.UtcNow).WithMessage("Reminder time must be in the future");

            RuleFor(r => r.Status)
                .NotEmpty().WithMessage("Reminder status is required")
                .IsInEnum().WithMessage("Invalid reminder status");
        }
    }

    public class ConflictValidator : AbstractValidator<Conflict>
    {
        public ConflictValidator()
        {
            RuleFor(c => c.ConflictType)
                .NotEmpty().WithMessage("Conflict type is required")
                .IsInEnum().WithMessage("Invalid conflict type");

            RuleFor(c => c.Description)
                .NotEmpty().WithMessage("Conflict description is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(c => c.AffectedResourceIds)
                .NotEmpty().WithMessage("Affected resource IDs are required");

            RuleFor(c => c.DetectedTime)
                .NotEmpty().WithMessage("Conflict detection time is required");

            RuleFor(c => c.Status)
                .NotEmpty().WithMessage("Conflict status is required")
                .IsInEnum().WithMessage("Invalid conflict status");
        }
    }
}
