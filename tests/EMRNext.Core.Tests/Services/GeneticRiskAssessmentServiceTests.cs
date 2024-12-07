using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Repositories;
using EMRNext.Core.Services;

namespace EMRNext.Core.Tests.Services
{
    public class GeneticRiskAssessmentServiceTests
    {
        private readonly Mock<ILogger<GeneticRiskAssessmentService>> _mockLogger;
        private readonly Mock<IGenericRepository<Patient>> _mockPatientRepository;
        private readonly Mock<IGenericRepository<FamilyMedicalHistory>> _mockFamilyHistoryRepository;

        public GeneticRiskAssessmentServiceTests()
        {
            _mockLogger = new Mock<ILogger<GeneticRiskAssessmentService>>();
            _mockPatientRepository = new Mock<IGenericRepository<Patient>>();
            _mockFamilyHistoryRepository = new Mock<IGenericRepository<FamilyMedicalHistory>>();
        }

        [Fact]
        public async Task AssessGeneticRisk_PatientWithNoFamilyHistory_ReturnsBaseRisk()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = CreateSamplePatient(patientId, 55, "Male");

            _mockPatientRepository
                .Setup(repo => repo.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            _mockFamilyHistoryRepository
                .Setup(repo => repo.FindAsync(It.IsAny<Func<FamilyMedicalHistory, bool>>()))
                .ReturnsAsync(new List<FamilyMedicalHistory>());

            var service = CreateGeneticRiskAssessmentService();

            // Act
            var riskProfile = await service.AssessGeneticRiskAsync(patientId);

            // Assert
            Assert.NotNull(riskProfile);
            Assert.True(riskProfile.DiabetesRisk > 0.1);
            Assert.True(riskProfile.CardiovascularRisk > 0.1);
        }

        [Fact]
        public async Task AssessGeneticRisk_FamilyHistoryOfCancer_IncreasedRisk()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = CreateSamplePatient(patientId, 40, "Female");

            var familyHistory = new List<FamilyMedicalHistory>
            {
                new FamilyMedicalHistory 
                { 
                    PatientId = patientId, 
                    Relationship = "Parent", 
                    Condition = "Breast Cancer",
                    AgeOfOnset = 45
                }
            };

            _mockPatientRepository
                .Setup(repo => repo.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            _mockFamilyHistoryRepository
                .Setup(repo => repo.FindAsync(It.IsAny<Func<FamilyMedicalHistory, bool>>()))
                .ReturnsAsync(familyHistory);

            var service = CreateGeneticRiskAssessmentService();

            // Act
            var riskProfile = await service.AssessGeneticRiskAsync(patientId);

            // Assert
            Assert.NotNull(riskProfile);
            Assert.True(riskProfile.CancerRisk > 0.5);
            Assert.Contains("Breast Cancer", riskProfile.GeneticPredispositions);
        }

        [Fact]
        public async Task GenerateRiskMitigationRecommendations_HighRisk_ReturnsDetailedRecommendations()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = CreateSamplePatient(patientId, 50, "Male");

            var familyHistory = new List<FamilyMedicalHistory>
            {
                new FamilyMedicalHistory 
                { 
                    PatientId = patientId, 
                    Relationship = "Parent", 
                    Condition = "HeartDisease",
                    AgeOfOnset = 55
                }
            };

            _mockPatientRepository
                .Setup(repo => repo.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            _mockFamilyHistoryRepository
                .Setup(repo => repo.FindAsync(It.IsAny<Func<FamilyMedicalHistory, bool>>()))
                .ReturnsAsync(familyHistory);

            var service = CreateGeneticRiskAssessmentService();

            // Act
            var recommendations = await service.GenerateRiskMitigationRecommendationsAsync(patientId);

            // Assert
            Assert.NotNull(recommendations);
            Assert.Contains("cardiovascular", recommendations.First(), StringComparison.OrdinalIgnoreCase);
        }

        private GeneticRiskAssessmentService CreateGeneticRiskAssessmentService()
        {
            return new GeneticRiskAssessmentService(
                _mockLogger.Object,
                _mockPatientRepository.Object,
                _mockFamilyHistoryRepository.Object
            );
        }

        private Patient CreateSamplePatient(Guid id, int age, string gender)
        {
            return new Patient
            {
                Id = id,
                Gender = gender,
                DateOfBirth = DateTime.Now.AddYears(-age),
                Medications = new List<string>() // Optional medication list
            };
        }
    }
}
