using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Services;
using EMRNext.Core.Validation;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EMRNext.Tests.Services
{
    public class PatientServiceTests
    {
        private readonly Mock<IPatientRepository> _mockRepository;
        private readonly Mock<ILogger<PatientService>> _mockLogger;
        private readonly Mock<PatientValidator> _mockValidator;
        private readonly PatientService _service;

        public PatientServiceTests()
        {
            _mockRepository = new Mock<IPatientRepository>();
            _mockLogger = new Mock<ILogger<PatientService>>();
            _mockValidator = new Mock<PatientValidator>();
            _service = new PatientService(_mockRepository.Object, _mockLogger.Object, _mockValidator.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsPatient()
        {
            // Arrange
            var expectedPatient = new Patient { Id = 1, FirstName = "John", LastName = "Doe" };
            _mockRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(expectedPatient);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPatient.Id, result.Id);
            Assert.Equal(expectedPatient.FirstName, result.FirstName);
            Assert.Equal(expectedPatient.LastName, result.LastName);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Patient)null);

            // Act
            var result = await _service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreatePatientAsync_ValidPatient_ReturnsCreatedPatient()
        {
            // Arrange
            var patient = new Patient
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.Parse("1990-01-01"),
                Gender = "Male"
            };

            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<Patient>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<Patient>()))
                .ReturnsAsync((Patient p) => p);

            // Act
            var result = await _service.CreatePatientAsync(patient);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(patient.FirstName, result.FirstName);
            Assert.Equal(patient.LastName, result.LastName);
            Assert.True(result.IsActive);
            Assert.NotEqual(Guid.Empty, result.PublicId);
        }

        [Fact]
        public async Task CreatePatientAsync_DuplicatePatient_ThrowsDuplicatePatientException()
        {
            // Arrange
            var patient = new Patient
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.Parse("1990-01-01"),
                SocialSecurityNumber = "123-45-6789"
            };

            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<Patient>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _mockRepository.Setup(repo => repo.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Patient> { new Patient
                {
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = DateTime.Parse("1990-01-01"),
                    SocialSecurityNumber = "123-45-6789"
                }});

            // Act & Assert
            await Assert.ThrowsAsync<DuplicatePatientException>(
                () => _service.CreatePatientAsync(patient));
        }

        [Fact]
        public async Task UpdatePatientAsync_ValidPatient_ReturnsUpdatedPatient()
        {
            // Arrange
            var patient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.Parse("1990-01-01")
            };

            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<Patient>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Patient>()))
                .ReturnsAsync((Patient p) => p);

            // Act
            var result = await _service.UpdatePatientAsync(patient);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(patient.Id, result.Id);
            Assert.Equal(patient.FirstName, result.FirstName);
            Assert.Equal(patient.LastName, result.LastName);
            Assert.NotNull(result.ModifiedAt);
        }

        [Fact]
        public async Task SearchPatientsAsync_ValidSearchTerm_ReturnsMatchingPatients()
        {
            // Arrange
            var searchTerm = "John";
            var expectedPatients = new List<Patient>
            {
                new Patient { FirstName = "John", LastName = "Doe" },
                new Patient { FirstName = "Johnny", LastName = "Smith" }
            };

            _mockRepository.Setup(repo => repo.SearchAsync(searchTerm, 0, 20))
                .ReturnsAsync(expectedPatients);

            // Act
            var result = await _service.SearchPatientsAsync(searchTerm);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.FirstName == "John");
            Assert.Contains(result, p => p.FirstName == "Johnny");
        }

        [Fact]
        public async Task GetPatientEncountersAsync_ValidPatientId_ReturnsEncounters()
        {
            // Arrange
            var patientId = 1;
            var expectedEncounters = new List<Encounter>
            {
                new Encounter { Id = 1, PatientId = patientId, EncounterDate = DateTime.Today },
                new Encounter { Id = 2, PatientId = patientId, EncounterDate = DateTime.Today.AddDays(-1) }
            };

            _mockRepository.Setup(repo => repo.GetEncountersAsync(patientId))
                .ReturnsAsync(expectedEncounters);

            // Act
            var result = await _service.GetPatientEncountersAsync(patientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, e => Assert.Equal(patientId, e.PatientId));
        }

        [Fact]
        public async Task DeactivatePatientAsync_ExistingPatient_ReturnsTrue()
        {
            // Arrange
            var patientId = 1;
            var patient = new Patient { Id = patientId, IsActive = true };

            _mockRepository.Setup(repo => repo.GetByIdAsync(patientId))
                .ReturnsAsync(patient);
            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Patient>()))
                .ReturnsAsync((Patient p) => p);

            // Act
            var result = await _service.DeactivatePatientAsync(patientId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.Is<Patient>(p => 
                p.Id == patientId && !p.IsActive)), Times.Once);
        }

        [Fact]
        public async Task ActivatePatientAsync_ExistingPatient_ReturnsTrue()
        {
            // Arrange
            var patientId = 1;
            var patient = new Patient { Id = patientId, IsActive = false };

            _mockRepository.Setup(repo => repo.GetByIdAsync(patientId))
                .ReturnsAsync(patient);
            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Patient>()))
                .ReturnsAsync((Patient p) => p);

            // Act
            var result = await _service.ActivatePatientAsync(patientId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.Is<Patient>(p => 
                p.Id == patientId && p.IsActive)), Times.Once);
        }
    }
}
