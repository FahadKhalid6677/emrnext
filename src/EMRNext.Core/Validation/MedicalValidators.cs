using FluentValidation;
using EMRNext.Core.Entities;

namespace EMRNext.Core.Validation
{
    public class VitalValidator : AbstractValidator<Vital>
    {
        public VitalValidator()
        {
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Vital type is required")
                .MaximumLength(50).WithMessage("Vital type cannot exceed 50 characters");

            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Vital value is required");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Unit is required")
                .MaximumLength(20).WithMessage("Unit cannot exceed 20 characters");

            RuleFor(x => x.MeasurementDate)
                .NotEmpty().WithMessage("Measurement date is required")
                .LessThanOrEqualTo(System.DateTime.UtcNow).WithMessage("Measurement date cannot be in the future");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");
        }
    }

    public class AllergyValidator : AbstractValidator<Allergy>
    {
        public AllergyValidator()
        {
            RuleFor(x => x.Allergen)
                .NotEmpty().WithMessage("Allergen is required")
                .MaximumLength(100).WithMessage("Allergen cannot exceed 100 characters");

            RuleFor(x => x.AllergenType)
                .NotEmpty().WithMessage("Allergen type is required")
                .MaximumLength(50).WithMessage("Allergen type cannot exceed 50 characters");

            RuleFor(x => x.Severity)
                .NotEmpty().WithMessage("Severity is required")
                .MaximumLength(20).WithMessage("Severity cannot exceed 20 characters");

            RuleFor(x => x.Reaction)
                .NotEmpty().WithMessage("Reaction is required")
                .MaximumLength(200).WithMessage("Reaction cannot exceed 200 characters");

            RuleFor(x => x.OnsetDate)
                .NotEmpty().WithMessage("Onset date is required")
                .LessThanOrEqualTo(System.DateTime.UtcNow).WithMessage("Onset date cannot be in the future");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .MaximumLength(20).WithMessage("Status cannot exceed 20 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");
        }
    }

    public class ProblemValidator : AbstractValidator<Problem>
    {
        public ProblemValidator()
        {
            RuleFor(x => x.ProblemName)
                .NotEmpty().WithMessage("Problem name is required")
                .MaximumLength(200).WithMessage("Problem name cannot exceed 200 characters");

            RuleFor(x => x.IcdCode)
                .NotEmpty().WithMessage("ICD code is required")
                .MaximumLength(20).WithMessage("ICD code cannot exceed 20 characters");

            RuleFor(x => x.OnsetDate)
                .NotEmpty().WithMessage("Onset date is required")
                .LessThanOrEqualTo(System.DateTime.UtcNow).WithMessage("Onset date cannot be in the future");

            RuleFor(x => x.EndDate)
                .Must((problem, endDate) => !endDate.HasValue || endDate.Value >= problem.OnsetDate)
                .WithMessage("End date must be after onset date");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .MaximumLength(20).WithMessage("Status cannot exceed 20 characters");

            RuleFor(x => x.Severity)
                .NotEmpty().WithMessage("Severity is required")
                .MaximumLength(20).WithMessage("Severity cannot exceed 20 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");
        }
    }
}
