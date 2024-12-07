using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EMRNext.Core.Services;
using EMRNext.Core.Repositories;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Events;

namespace EMRNext.Core.Tests.Services
{
    /// <summary>
    /// Unit tests for Patient Service
    /// </summary>
    public class PatientServiceTests
    {
        private readonly Mock<IGenericRepository<Patient>> _mockPatientRepository;
        private readonly Mock<IDomainEventDispatcher> _mockEventDispatcher;
        private readonly PatientService _patientService;

        public PatientServiceTests()
        {
            _mockPatientRepository = new Mock<IGenericRepository<Patient>>();
            _mockEventDispatcher = new Mock<IDomainEventDispatcher>();
            _patientService = new PatientService(
                _mockPatientRepository.Object, 
                _mockEventDispatcher.Object
            );
        }

        [Fact]
        public async Task RegisterPatientAsync_ShouldAddPatientAndDispatchEvent()
        {
            // Arrange
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                Name = new PersonName { FirstName = "John", LastName = "Doe" },
                Demographics = new Demographics 
                { 
                    DateOfBirth = DateTime.Now.AddYears(-30) 
                }
            };

            _mockPatientRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Patient>()))
                .Returns(Task.CompletedTask);

            _mockEventDispatcher
                .Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<PatientRegisteredEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _patientService.RegisterPatientAsync(patient);

            // Assert
            _mockPatientRepository.Verify(repo => repo.AddAsync(patient), Times.Once);
            _mockEventDispatcher.Verify(
                dispatcher => dispatcher.DispatchAsync(
                    It.Is<PatientRegisteredEvent>(
                        e => e.PatientId == patient.Id && 
                             e.FirstName == patient.Name.FirstName
                    )
                ), 
                Times.Once
            );
            Assert.Equal(patient, result);
        }

        [Fact]
        public async Task GetPatientByIdAsync_ExistingPatient_ShouldReturnPatient()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var expectedPatient = new Patient { Id = patientId };

            _mockPatientRepository
                .Setup(repo => repo.GetByIdAsync(patientId))
                .ReturnsAsync(expectedPatient);

            // Act
            var result = await _patientService.GetPatientByIdAsync(patientId);

            // Assert
            Assert.Equal(expectedPatient, result);
        }
    }
}
