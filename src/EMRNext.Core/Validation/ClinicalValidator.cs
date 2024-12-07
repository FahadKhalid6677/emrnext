using System;
using FluentValidation;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Validation
{
    public class EncounterValidator : AbstractValidator<Encounter>
    {
        public EncounterValidator()
        {
            RuleFor(e => e.PatientId)
                .NotEmpty().WithMessage("Patient ID is required");

            RuleFor(e => e.ProviderId)
                .NotEmpty().WithMessage("Provider ID is required");

            RuleFor(e => e.EncounterDate)
                .NotEmpty().WithMessage("Encounter date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Encounter date cannot be in the future");

            RuleFor(e => e.EncounterType)
                .NotEmpty().WithMessage("Encounter type is required")
                .MaximumLength(50).WithMessage("Encounter type cannot exceed 50 characters");

            RuleFor(e => e.Status)
                .NotEmpty().WithMessage("Encounter status is required")
                .IsInEnum().WithMessage("Invalid encounter status");
        }
    }

    public class ClinicalNoteValidator : AbstractValidator<ClinicalNote>
    {
        public ClinicalNoteValidator()
        {
            RuleFor(n => n.EncounterId)
                .NotEmpty().WithMessage("Encounter ID is required");

            RuleFor(n => n.ProviderId)
                .NotEmpty().WithMessage("Provider ID is required");

            RuleFor(n => n.NoteText)
                .NotEmpty().WithMessage("Note text is required")
                .MaximumLength(10000).WithMessage("Note text cannot exceed 10000 characters");

            RuleFor(n => n.NoteType)
                .NotEmpty().WithMessage("Note type is required")
                .MaximumLength(50).WithMessage("Note type cannot exceed 50 characters");
        }
    }

    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(o => o.PatientId)
                .NotEmpty().WithMessage("Patient ID is required");

            RuleFor(o => o.OrderingProviderId)
                .NotEmpty().WithMessage("Ordering provider ID is required");

            RuleFor(o => o.OrderType)
                .NotEmpty().WithMessage("Order type is required")
                .MaximumLength(50).WithMessage("Order type cannot exceed 50 characters");

            RuleFor(o => o.OrderDate)
                .NotEmpty().WithMessage("Order date is required");

            RuleFor(o => o.Status)
                .NotEmpty().WithMessage("Order status is required")
                .IsInEnum().WithMessage("Invalid order status");

            RuleFor(o => o.Priority)
                .NotEmpty().WithMessage("Order priority is required")
                .IsInEnum().WithMessage("Invalid order priority");
        }
    }

    public class PrescriptionValidator : AbstractValidator<Prescription>
    {
        public PrescriptionValidator()
        {
            RuleFor(p => p.PatientId)
                .NotEmpty().WithMessage("Patient ID is required");

            RuleFor(p => p.ProviderId)
                .NotEmpty().WithMessage("Provider ID is required");

            RuleFor(p => p.MedicationName)
                .NotEmpty().WithMessage("Medication name is required")
                .MaximumLength(200).WithMessage("Medication name cannot exceed 200 characters");

            RuleFor(p => p.Dosage)
                .NotEmpty().WithMessage("Dosage is required")
                .MaximumLength(100).WithMessage("Dosage cannot exceed 100 characters");

            RuleFor(p => p.Frequency)
                .NotEmpty().WithMessage("Frequency is required")
                .MaximumLength(100).WithMessage("Frequency cannot exceed 100 characters");

            RuleFor(p => p.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(p => p.Duration)
                .NotEmpty().WithMessage("Duration is required");

            RuleFor(p => p.Quantity)
                .NotEmpty().WithMessage("Quantity is required")
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(p => p.Refills)
                .GreaterThanOrEqualTo(0).WithMessage("Refills cannot be negative");
        }
    }

    public class VitalValidator : AbstractValidator<Vital>
    {
        public VitalValidator()
        {
            RuleFor(v => v.PatientId)
                .NotEmpty().WithMessage("Patient ID is required");

            RuleFor(v => v.RecordedByProviderId)
                .NotEmpty().WithMessage("Provider ID is required");

            RuleFor(v => v.RecordedDate)
                .NotEmpty().WithMessage("Recorded date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Recorded date cannot be in the future");

            RuleFor(v => v.Temperature)
                .InclusiveBetween(95.0M, 108.0M)
                .When(v => v.Temperature.HasValue)
                .WithMessage("Temperature must be between 95.0°F and 108.0°F");

            RuleFor(v => v.HeartRate)
                .InclusiveBetween(30, 250)
                .When(v => v.HeartRate.HasValue)
                .WithMessage("Heart rate must be between 30 and 250 BPM");

            RuleFor(v => v.RespiratoryRate)
                .InclusiveBetween(8, 60)
                .When(v => v.RespiratoryRate.HasValue)
                .WithMessage("Respiratory rate must be between 8 and 60 breaths per minute");

            RuleFor(v => v.BloodPressureSystolic)
                .InclusiveBetween(60, 300)
                .When(v => v.BloodPressureSystolic.HasValue)
                .WithMessage("Systolic blood pressure must be between 60 and 300 mmHg");

            RuleFor(v => v.BloodPressureDiastolic)
                .InclusiveBetween(30, 200)
                .When(v => v.BloodPressureDiastolic.HasValue)
                .WithMessage("Diastolic blood pressure must be between 30 and 200 mmHg");

            RuleFor(v => v.OxygenSaturation)
                .InclusiveBetween(50M, 100M)
                .When(v => v.OxygenSaturation.HasValue)
                .WithMessage("Oxygen saturation must be between 50% and 100%");
        }
    }

    public class ResultValidator : AbstractValidator<Result>
    {
        public ResultValidator()
        {
            RuleFor(r => r.OrderId)
                .NotEmpty().WithMessage("Order ID is required");

            RuleFor(r => r.ResultDate)
                .NotEmpty().WithMessage("Result date is required");

            RuleFor(r => r.ResultValue)
                .NotEmpty().WithMessage("Result value is required");

            RuleFor(r => r.Units)
                .NotEmpty().WithMessage("Units are required")
                .When(r => r.ResultType != "Text");

            RuleFor(r => r.ReferenceRange)
                .NotEmpty().WithMessage("Reference range is required")
                .When(r => r.ResultType != "Text");

            RuleFor(r => r.Status)
                .NotEmpty().WithMessage("Result status is required")
                .IsInEnum().WithMessage("Invalid result status");
        }
    }
}
