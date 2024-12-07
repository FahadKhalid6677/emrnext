using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Validation;
using System;
using Xunit;

namespace EMRNext.Tests.Services
{
    public class PatientValidatorTests
    {
        private readonly PatientValidator _validator;

        public PatientValidatorTests()
        {
            _validator = new PatientValidator();
        }

        [Fact]
        public async Task ValidPatient_PassesValidation()
        {
            // Arrange
            var patient = new Patient
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.Parse("1990-01-01"),
                Gender = "Male",
                Email = "john.doe@example.com",
                PhoneCell = "1234567890",
                SocialSecurityNumber = "123-45-6789",
                Street = "123 Main St",
                City = "Anytown",
                State = "CA",
                PostalCode = "12345"
            };

            // Act
            var result = await _validator.ValidateAsync(patient);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task FirstName_WhenNullOrEmpty_FailsValidation(string firstName)
        {
            // Arrange
            var patient = new Patient { FirstName = firstName };

            // Act
            var result = await _validator.ValidateAsync(patient);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Patient.FirstName));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task LastName_WhenNullOrEmpty_FailsValidation(string lastName)
        {
            // Arrange
            var patient = new Patient { LastName = lastName };

            // Act
            var result = await _validator.ValidateAsync(patient);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Patient.LastName));
        }

        [Fact]
        public async Task DateOfBirth_WhenFuture_FailsValidation()
        {
            // Arrange
            var patient = new Patient { DateOfBirth = DateTime.UtcNow.AddDays(1) };

            // Act
            var result = await _validator.ValidateAsync(patient);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Patient.DateOfBirth));
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@example.com")]
        [InlineData("user@")]
        public async Task Email_WhenInvalid_FailsValidation(string email)
        {
            // Arrange
            var patient = new Patient { Email = email };

            // Act
            var result = await _validator.ValidateAsync(patient);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Patient.Email));
        }

        [Theory]
        [InlineData("123-456-7890")]
        [InlineData("1234567890")]
        [InlineData("+11234567890")]
        public async Task PhoneNumber_WhenValid_PassesValidation(string phone)
        {
            // Arrange
            var patient = new Patient { PhoneCell = phone };

            // Act
            var result = await _validator.ValidateAsync(patient);

            // Assert
            Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(Patient.PhoneCell));
        }

        [Theory]
        [InlineData("123-45-6789")]
        [InlineData("123456789")]
        public async Task SSN_WhenValid_PassesValidation(string ssn)
        {
            // Arrange
            var patient = new Patient { SocialSecurityNumber = ssn };

            // Act
            var result = await _validator.ValidateAsync(patient);

            // Assert
            Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(Patient.SocialSecurityNumber));
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("12345-6789")]
        public async Task PostalCode_WhenValid_PassesValidation(string postalCode)
        {
            // Arrange
            var patient = new Patient { PostalCode = postalCode };

            // Act
            var result = await _validator.ValidateAsync(patient);

            // Assert
            Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(Patient.PostalCode));
        }

        [Theory]
        [InlineData("CA")]
        [InlineData("NY")]
        public async Task State_WhenValid_PassesValidation(string state)
        {
            // Arrange
            var patient = new Patient { State = state };

            // Act
            var result = await _validator.ValidateAsync(patient);

            // Assert
            Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(Patient.State));
        }

        [Fact]
        public async Task EmergencyContact_WhenPhoneProvidedWithoutName_FailsValidation()
        {
            // Arrange
            var patient = new Patient
            {
                PhoneEmergency = "1234567890",
                EmergencyContact = "",
                EmergencyRelationship = ""
            };

            // Act
            var result = await _validator.ValidateAsync(patient);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Patient.EmergencyContact));
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Patient.EmergencyRelationship));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-1000)]
        public async Task MonthlyIncome_WhenNegative_FailsValidation(decimal income)
        {
            // Arrange
            var patient = new Patient { MonthlyIncome = income };

            // Act
            var result = await _validator.ValidateAsync(patient);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Patient.MonthlyIncome));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task FamilySize_WhenZeroOrNegative_FailsValidation(int familySize)
        {
            // Arrange
            var patient = new Patient { FamilySize = familySize };

            // Act
            var result = await _validator.ValidateAsync(patient);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Patient.FamilySize));
        }
    }
}
