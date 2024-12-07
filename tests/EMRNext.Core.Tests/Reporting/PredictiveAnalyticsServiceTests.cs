using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Repositories;
using EMRNext.Core.Reporting.Services;

namespace EMRNext.Core.Tests.Reporting
{
    public class PredictiveAnalyticsServiceTests
    {
        private readonly Mock<ILogger<PredictiveAnalyticsService>> _mockLogger;
        private readonly Mock<IGenericRepository<Patient>> _mockPatientRepository;
        private readonly PredictiveAnalyticsService _service;

        public PredictiveAnalyticsServiceTests()
        {
            _mockLogger = new Mock<ILogger<PredictiveAnalyticsService>>();
            _mockPatientRepository = new Mock<IGenericRepository<Patient>>();
            _service = new PredictiveAnalyticsService(_mockLogger.Object, _mockPatientRepository.Object);
        }

        [Fact]
        public async Task PredictDiseaseRiskAsync_ValidPatient_ReturnsRiskPrediction()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = new Patient
            {
                Id = patientId,
                Age = 45,
                Weight = 80,
                Height = 175,
                BloodPressure = 120,
                CholesterolLevel = 200
            };

            _mockPatientRepository
                .Setup(repo => repo.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            // Act
            var result = await _service.PredictDiseaseRiskAsync(patientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(patientId, result.PatientId);
            Assert.InRange(result.DiabetesRisk, 0, 1);
            Assert.InRange(result.HeartDiseaseRisk, 0, 1);
            Assert.InRange(result.HypertensionRisk, 0, 1);
            Assert.InRange(result.PredictionConfidence, 0, 1);
        }

        [Fact]
        public async Task PredictTreatmentAdherenceAsync_ValidPatient_ReturnsAdherencePrediction()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = new Patient
            {
                Id = patientId,
                Age = 55,
                Prescriptions = new List<Prescription>
                {
                    new Prescription { Id = Guid.NewGuid() },
                    new Prescription { Id = Guid.NewGuid() }
                }
            };

            _mockPatientRepository
                .Setup(repo => repo.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            // Act
            var result = await _service.PredictTreatmentAdherenceAsync(patientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(patientId, result.PatientId);
            Assert.InRange(result.AdherenceProbability, 0, 1);
            Assert.InRange(result.PredictionConfidence, 0, 1);
        }

        [Fact]
        public async Task TrainDiseaseRiskModelAsync_ReturnsValidModelTrainingResult()
        {
            // Arrange
            var patients = new List<Patient>
            {
                new Patient { 
                    Age = 40, 
                    Weight = 75, 
                    Height = 170, 
                    BloodPressure = 120, 
                    CholesterolLevel = 180 
                },
                new Patient { 
                    Age = 50, 
                    Weight = 85, 
                    Height = 180, 
                    BloodPressure = 140, 
                    CholesterolLevel = 220 
                }
            };

            _mockPatientRepository
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(patients);

            // Act
            var result = await _service.TrainDiseaseRiskModelAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ModelPath);
            Assert.Equal(patients.Count, result.NumberOfPatients);
            Assert.True(result.TrainingDate <= DateTime.UtcNow);
        }

        [Fact]
        public async Task PredictDiseaseRiskAsync_NonExistentPatient_ThrowsArgumentException()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            _mockPatientRepository
                .Setup(repo => repo.GetByIdAsync(patientId))
                .ReturnsAsync((Patient)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.PredictDiseaseRiskAsync(patientId));
        }
    }
}
