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
    public class GeneticRiskProgressTrackingTests
    {
        private readonly Mock<ILogger<GeneticRiskAssessmentService>> _mockLogger;
        private readonly Mock<IGenericRepository<Patient>> _mockPatientRepository;
        private readonly Mock<IGenericRepository<FamilyMedicalHistory>> _mockFamilyHistoryRepository;
        private readonly Mock<IGenericRepository<GeneticRiskProgressTracking>> _mockProgressTrackingRepository;

        public GeneticRiskProgressTrackingTests()
        {
            _mockLogger = new Mock<ILogger<GeneticRiskAssessmentService>>();
            _mockPatientRepository = new Mock<IGenericRepository<Patient>>();
            _mockFamilyHistoryRepository = new Mock<IGenericRepository<FamilyMedicalHistory>>();
            _mockProgressTrackingRepository = new Mock<IGenericRepository<GeneticRiskProgressTracking>>();
        }

        [Fact]
        public async Task TrackGeneticRiskProgress_FirstAssessment_CreatesInitialTracking()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = CreateSamplePatient(patientId, 45, "Female");
            var familyHistory = CreateSampleFamilyHistory(patientId);

            SetupRepositoryMocks(patient, familyHistory);

            var service = CreateGeneticRiskAssessmentService();

            // Act
            var progressTracking = await service.TrackGeneticRiskProgressAsync(patientId);

            // Assert
            Assert.NotNull(progressTracking);
            Assert.Equal(patientId, progressTracking.PatientId);
            Assert.NotNull(progressTracking.Recommendations);
            Assert.True(progressTracking.Recommendations.Any());
            Assert.NotNull(progressTracking.Interventions);
            Assert.True(progressTracking.Interventions.All(i => 
                i.Status == InterventionStatus.Recommended));
        }

        [Fact]
        public async Task TrackGeneticRiskProgress_MultipleAssessments_TracksRiskChanges()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = CreateSamplePatient(patientId, 45, "Female");
            var familyHistory = CreateSampleFamilyHistory(patientId);

            // Setup previous tracking
            var previousTracking = new GeneticRiskProgressTracking
            {
                PatientId = patientId,
                AssessmentDate = DateTime.UtcNow.AddMonths(-3),
                RiskProfile = new Patient.MedicalRiskProfile
                {
                    DiabetesRisk = 0.2,
                    CardiovascularRisk = 0.3,
                    CancerRisk = 0.4
                }
            };

            _mockProgressTrackingRepository
                .Setup(repo => repo.FindAsync(It.IsAny<Func<GeneticRiskProgressTracking, bool>>()))
                .ReturnsAsync(new List<GeneticRiskProgressTracking> { previousTracking });

            SetupRepositoryMocks(patient, familyHistory);

            var service = CreateGeneticRiskAssessmentService();

            // Act
            var progressTracking = await service.TrackGeneticRiskProgressAsync(patientId);

            // Assert
            Assert.NotNull(progressTracking.RiskFactorTrends);
            Assert.NotEqual(0, progressTracking.RiskFactorTrends.DiabetesRiskChange);
            Assert.NotEqual(0, progressTracking.RiskFactorTrends.CardiovascularRiskChange);
            Assert.NotEqual(0, progressTracking.RiskFactorTrends.CancerRiskChange);
        }

        [Fact]
        public async Task TrackInterventions_ExistingRecommendations_ProgressesStatus()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = CreateSamplePatient(patientId, 45, "Female");
            var familyHistory = CreateSampleFamilyHistory(patientId);

            // Setup previous tracking with existing interventions
            var previousTracking = new GeneticRiskProgressTracking
            {
                PatientId = patientId,
                AssessmentDate = DateTime.UtcNow.AddMonths(-3),
                Interventions = new List<RiskInterventionProgress>
                {
                    new RiskInterventionProgress
                    {
                        InterventionDescription = "Schedule diabetes screening",
                        RecommendedDate = DateTime.UtcNow.AddMonths(-3),
                        Status = InterventionStatus.Recommended
                    }
                }
            };

            _mockProgressTrackingRepository
                .Setup(repo => repo.FindAsync(It.IsAny<Func<GeneticRiskProgressTracking, bool>>()))
                .ReturnsAsync(new List<GeneticRiskProgressTracking> { previousTracking });

            SetupRepositoryMocks(patient, familyHistory);

            var service = CreateGeneticRiskAssessmentService();

            // Act
            var progressTracking = await service.TrackGeneticRiskProgressAsync(patientId);

            // Assert
            var diabetesScreeningIntervention = progressTracking.Interventions
                .FirstOrDefault(i => i.InterventionDescription == "Schedule diabetes screening");
            
            Assert.NotNull(diabetesScreeningIntervention);
            Assert.Equal(InterventionStatus.Initiated, diabetesScreeningIntervention.Status);
        }

        private void SetupRepositoryMocks(Patient patient, List<FamilyMedicalHistory> familyHistory)
        {
            _mockPatientRepository
                .Setup(repo => repo.GetByIdAsync(patient.Id))
                .ReturnsAsync(patient);

            _mockFamilyHistoryRepository
                .Setup(repo => repo.FindAsync(It.IsAny<Func<FamilyMedicalHistory, bool>>()))
                .ReturnsAsync(familyHistory);

            _mockProgressTrackingRepository
                .Setup(repo => repo.AddAsync(It.IsAny<GeneticRiskProgressTracking>()))
                .Returns(Task.CompletedTask);
        }

        private GeneticRiskAssessmentService CreateGeneticRiskAssessmentService()
        {
            return new GeneticRiskAssessmentService(
                _mockLogger.Object,
                _mockPatientRepository.Object,
                _mockFamilyHistoryRepository.Object,
                _mockProgressTrackingRepository.Object
            );
        }

        private Patient CreateSamplePatient(Guid id, int age, string gender)
        {
            return new Patient
            {
                Id = id,
                Gender = gender,
                DateOfBirth = DateTime.Now.AddYears(-age),
                Medications = new List<string>() 
            };
        }

        private List<FamilyMedicalHistory> CreateSampleFamilyHistory(Guid patientId)
        {
            return new List<FamilyMedicalHistory>
            {
                new FamilyMedicalHistory
                {
                    PatientId = patientId,
                    Relationship = "Parent",
                    Condition = "Diabetes",
                    AgeOfOnset = 50
                }
            };
        }
    }
}
