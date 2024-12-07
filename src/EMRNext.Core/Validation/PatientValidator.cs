using EMRNext.Core.Domain.Entities;
using FluentValidation;
using System;

namespace EMRNext.Core.Validation
{
    public class PatientValidator : AbstractValidator<Patient>
    {
        public PatientValidator()
        {
            RuleFor(p => p.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(p => p.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(p => p.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required")
                .LessThan(DateTime.UtcNow).WithMessage("Date of birth cannot be in the future");

            RuleFor(p => p.Gender)
                .NotEmpty().WithMessage("Gender is required")
                .MaximumLength(20).WithMessage("Gender cannot exceed 20 characters");

            RuleFor(p => p.SocialSecurityNumber)
                .MaximumLength(11).WithMessage("Social Security Number cannot exceed 11 characters")
                .Matches(@"^\d{3}-?\d{2}-?\d{4}$").When(p => !string.IsNullOrEmpty(p.SocialSecurityNumber))
                .WithMessage("Invalid Social Security Number format");

            RuleFor(p => p.Email)
                .EmailAddress().When(p => !string.IsNullOrEmpty(p.Email))
                .WithMessage("Invalid email address format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(p => p.PhoneHome)
                .Matches(@"^\+?1?\d{10,14}$").When(p => !string.IsNullOrEmpty(p.PhoneHome))
                .WithMessage("Invalid home phone number format");

            RuleFor(p => p.PhoneCell)
                .Matches(@"^\+?1?\d{10,14}$").When(p => !string.IsNullOrEmpty(p.PhoneCell))
                .WithMessage("Invalid cell phone number format");

            RuleFor(p => p.PhoneWork)
                .Matches(@"^\+?1?\d{10,14}$").When(p => !string.IsNullOrEmpty(p.PhoneWork))
                .WithMessage("Invalid work phone number format");

            RuleFor(p => p.PhoneEmergency)
                .Matches(@"^\+?1?\d{10,14}$").When(p => !string.IsNullOrEmpty(p.PhoneEmergency))
                .WithMessage("Invalid emergency phone number format");

            RuleFor(p => p.EmergencyContact)
                .NotEmpty().When(p => !string.IsNullOrEmpty(p.PhoneEmergency))
                .WithMessage("Emergency contact name is required when emergency phone is provided")
                .MaximumLength(100).WithMessage("Emergency contact name cannot exceed 100 characters");

            RuleFor(p => p.EmergencyRelationship)
                .NotEmpty().When(p => !string.IsNullOrEmpty(p.PhoneEmergency))
                .WithMessage("Emergency contact relationship is required when emergency phone is provided")
                .MaximumLength(50).WithMessage("Emergency contact relationship cannot exceed 50 characters");

            RuleFor(p => p.Street)
                .MaximumLength(100).WithMessage("Street address cannot exceed 100 characters");

            RuleFor(p => p.City)
                .MaximumLength(50).WithMessage("City cannot exceed 50 characters");

            RuleFor(p => p.State)
                .MaximumLength(2).WithMessage("State must be a 2-letter code")
                .Matches(@"^[A-Z]{2}$").When(p => !string.IsNullOrEmpty(p.State))
                .WithMessage("State must be a valid 2-letter code");

            RuleFor(p => p.PostalCode)
                .Matches(@"^\d{5}(-\d{4})?$").When(p => !string.IsNullOrEmpty(p.PostalCode))
                .WithMessage("Invalid postal code format");

            RuleFor(p => p.Country)
                .MaximumLength(50).WithMessage("Country cannot exceed 50 characters");

            RuleFor(p => p.Language)
                .MaximumLength(50).WithMessage("Language cannot exceed 50 characters");

            RuleFor(p => p.Race)
                .MaximumLength(50).WithMessage("Race cannot exceed 50 characters");

            RuleFor(p => p.Ethnicity)
                .MaximumLength(50).WithMessage("Ethnicity cannot exceed 50 characters");

            RuleFor(p => p.Religion)
                .MaximumLength(50).WithMessage("Religion cannot exceed 50 characters");

            RuleFor(p => p.Occupation)
                .MaximumLength(100).WithMessage("Occupation cannot exceed 100 characters");

            RuleFor(p => p.Employer)
                .MaximumLength(100).WithMessage("Employer cannot exceed 100 characters");

            RuleFor(p => p.MonthlyIncome)
                .GreaterThanOrEqualTo(0).When(p => p.MonthlyIncome.HasValue)
                .WithMessage("Monthly income cannot be negative");

            RuleFor(p => p.FamilySize)
                .GreaterThan(0).When(p => p.FamilySize.HasValue)
                .WithMessage("Family size must be greater than 0");

            RuleFor(p => p.PrimaryCareProvider)
                .MaximumLength(100).WithMessage("Primary care provider name cannot exceed 100 characters");

            RuleFor(p => p.ReferredBy)
                .MaximumLength(100).WithMessage("Referring provider name cannot exceed 100 characters");

            RuleFor(p => p.PreferredPharmacy)
                .MaximumLength(100).WithMessage("Preferred pharmacy name cannot exceed 100 characters");
        }
    }
}
