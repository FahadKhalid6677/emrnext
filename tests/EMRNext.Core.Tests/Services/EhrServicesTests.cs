using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Repositories;
using EMRNext.Core.Services.EhrServices;

namespace EMRNext.Core.Tests.Services
{
    public class EhrServicesTests
    {
        private readonly Mock<ILogger<MedicalChartService>> _mockLogger;
        private readonly Mock<IGenericRepository<Patient>> _mockPatientRepository;
        private readonly Mock<IGenericRepository<MedicalChart>> _mockMedicalChartRepository;
        private readonly Mock<IGenericRepository<ProgressNote>> _mockProgressNoteRepository;
        private readonly Mock<IGenericRepository<ClinicalDocument>> _mockDocumentRepository;

        public EhrServicesTests()
        {
            _mockLogger = new Mock<ILogger<MedicalChartService>>();
            _mockPatientRepository = new Mock<IGenericRepository<Patient>>();
            _mockMedicalChartRepository = new Mock<IGenericRepository<MedicalChart>>();
            _mockProgressNoteRepository = new Mock<IGenericRepository<ProgressNote>>();
            _mockDocumentRepository = new Mock<IGenericRepository<ClinicalDocument>>();
        }

        [Fact]
        public async Task CreateMedicalChart_ValidPatient_Succeeds()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = new Patient { Id = patientId };

            _mockPatientRepository
                .Setup(repo => repo.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            var medicalChartService = new MedicalChartService(
                _mockLogger.Object,
                _mockPatientRepository.Object,
                _mockMedicalChartRepository.Object,
                _mockProgressNoteRepository.Object,
                _mockDocumentRepository.Object
            );

            // Act
            var medicalChart = await medicalChartService.CreateMedicalChartAsync(
                patientId, 
                "Initial Consultation"
            );

            // Assert
            Assert.NotNull(medicalChart);
            Assert.Equal(patientId, medicalChart.PatientId);
            Assert.Equal("Initial Consultation", medicalChart.ChartTitle);
            Assert.Equal("Consultation", medicalChart.ChartType);
        }

        [Fact]
        public async Task AddProgressNote_ValidMedicalChart_Succeeds()
        {
            // Arrange
            var medicalChartId = Guid.NewGuid();
            var providerId = Guid.NewGuid();
            var medicalChart = new MedicalChart { Id = medicalChartId };

            _mockMedicalChartRepository
                .Setup(repo => repo.GetByIdAsync(medicalChartId))
                .ReturnsAsync(medicalChart);

            var medicalChartService = new MedicalChartService(
                _mockLogger.Object,
                _mockPatientRepository.Object,
                _mockMedicalChartRepository.Object,
                _mockProgressNoteRepository.Object,
                _mockDocumentRepository.Object
            );

            var progressNote = new ProgressNote
            {
                SubjectiveObservation = "Patient reports mild symptoms",
                ObjectiveObservation = "Vital signs within normal range",
                Assessment = "Stable condition",
                Plan = "Continue current treatment"
            };

            // Act
            var addedProgressNote = await medicalChartService.AddProgressNoteAsync(
                medicalChartId, 
                providerId, 
                progressNote
            );

            // Assert
            Assert.NotNull(addedProgressNote);
            Assert.Equal(medicalChartId, addedProgressNote.MedicalChartId);
            Assert.Equal(providerId, addedProgressNote.ProviderId);
        }

        [Fact]
        public async Task AttachDocumentToProgressNote_ValidProgressNote_Succeeds()
        {
            // Arrange
            var progressNoteId = Guid.NewGuid();
            var progressNote = new ProgressNote { Id = progressNoteId };

            _mockProgressNoteRepository
                .Setup(repo => repo.GetByIdAsync(progressNoteId))
                .ReturnsAsync(progressNote);

            var medicalChartService = new MedicalChartService(
                _mockLogger.Object,
                _mockPatientRepository.Object,
                _mockMedicalChartRepository.Object,
                _mockProgressNoteRepository.Object,
                _mockDocumentRepository.Object
            );

            var document = new ClinicalDocument
            {
                DocumentTitle = "Lab Results",
                DocumentType = "Laboratory",
                FileLocation = "/documents/lab_results.pdf"
            };

            // Act
            var attachedDocument = await medicalChartService.AttachDocumentToProgressNoteAsync(
                progressNoteId, 
                document
            );

            // Assert
            Assert.NotNull(attachedDocument);
            Assert.Equal(progressNoteId, attachedDocument.ProgressNoteId);
            Assert.Equal("Lab Results", attachedDocument.DocumentTitle);
        }

        [Fact]
        public async Task GetPatientMedicalHistory_ValidPatient_Succeeds()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = new Patient
            {
                Id = patientId,
                Encounters = new List<Encounter>(),
                Problems = new List<Problem>(),
                Medications = new List<Medication>(),
                Allergies = new List<Allergy>(),
                Immunizations = new List<Immunization>(),
                MedicalRiskProfile = new Patient.MedicalRiskProfile()
            };

            _mockPatientRepository
                .Setup(repo => repo.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            _mockMedicalChartRepository
                .Setup(repo => repo.FindAsync(It.IsAny<Func<MedicalChart, bool>>()))
                .ReturnsAsync(new List<MedicalChart>());

            var medicalChartService = new MedicalChartService(
                _mockLogger.Object,
                _mockPatientRepository.Object,
                _mockMedicalChartRepository.Object,
                _mockProgressNoteRepository.Object,
                _mockDocumentRepository.Object
            );

            // Act
            var medicalHistory = await medicalChartService.GetPatientMedicalHistoryAsync(patientId);

            // Assert
            Assert.NotNull(medicalHistory);
            Assert.Equal(patientId, medicalHistory.PatientId);
        }
    }
}
