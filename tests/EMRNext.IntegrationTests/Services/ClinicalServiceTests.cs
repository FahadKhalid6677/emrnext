using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Exceptions;

namespace EMRNext.IntegrationTests.Services
{
    public class ClinicalServiceTests : IClassFixture<TestFixture>
    {
        private readonly IClinicalService _clinicalService;
        private readonly IPatientRepository _patientRepository;
        private readonly IClinicalRepository _clinicalRepository;
        private readonly ILoggingService _loggingService;

        public ClinicalServiceTests(TestFixture fixture)
        {
            _clinicalService = fixture.ServiceProvider.GetRequiredService<IClinicalService>();
            _patientRepository = fixture.ServiceProvider.GetRequiredService<IPatientRepository>();
            _clinicalRepository = fixture.ServiceProvider.GetRequiredService<IClinicalRepository>();
            _loggingService = fixture.ServiceProvider.GetRequiredService<ILoggingService>();
        }

        [Fact]
        public async Task CreateEncounter_WithValidData_ShouldCreateEncounter()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var encounter = new Encounter
            {
                PatientId = patient.Id,
                ProviderId = 1,
                EncounterDate = DateTime.UtcNow,
                EncounterType = "Office Visit",
                Status = EncounterStatus.Open
            };

            // Act
            var result = await _clinicalService.CreateEncounterAsync(encounter);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Status.Should().Be(EncounterStatus.Open);
            result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task CreateEncounter_WithInvalidPatient_ShouldThrowNotFoundException()
        {
            // Arrange
            var encounter = new Encounter
            {
                PatientId = -1,
                ProviderId = 1,
                EncounterDate = DateTime.UtcNow,
                EncounterType = "Office Visit"
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _clinicalService.CreateEncounterAsync(encounter));
        }

        [Fact]
        public async Task CreateClinicalNote_WithValidData_ShouldCreateNote()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var encounter = await CreateTestEncounter(patient.Id);
            var note = new ClinicalNote
            {
                EncounterId = encounter.Id,
                ProviderId = 1,
                NoteText = "Patient presents with...",
                NoteType = "Progress Note"
            };

            // Act
            var result = await _clinicalService.CreateClinicalNoteAsync(note);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task CreateClinicalNote_WithClosedEncounter_ShouldThrowBusinessRuleException()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var encounter = await CreateTestEncounter(patient.Id);
            encounter.Status = EncounterStatus.Closed;
            await _clinicalRepository.UpdateEncounterAsync(encounter.Id, encounter);

            var note = new ClinicalNote
            {
                EncounterId = encounter.Id,
                ProviderId = 1,
                NoteText = "Patient presents with...",
                NoteType = "Progress Note"
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => 
                _clinicalService.CreateClinicalNoteAsync(note));
        }

        [Fact]
        public async Task CreateOrder_WithValidData_ShouldCreateOrder()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var encounter = await CreateTestEncounter(patient.Id);
            var order = new Order
            {
                PatientId = patient.Id,
                EncounterId = encounter.Id,
                OrderingProviderId = 1,
                OrderType = OrderType.Laboratory,
                OrderDate = DateTime.UtcNow,
                Priority = OrderPriority.Routine
            };

            // Act
            var result = await _clinicalService.CreateOrderAsync(order);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Status.Should().Be(OrderStatus.Pending);
            result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task RecordResult_WithValidData_ShouldRecordResult()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var encounter = await CreateTestEncounter(patient.Id);
            var order = await CreateTestOrder(patient.Id, encounter.Id);
            var result = new Result
            {
                OrderId = order.Id,
                ResultDate = DateTime.UtcNow,
                ResultValue = "70",
                Units = "mg/dL",
                ReferenceRange = "65-99",
                ResultType = "Numeric",
                Status = ResultStatus.Final
            };

            // Act
            var recordedResult = await _clinicalService.RecordResultAsync(result);

            // Assert
            recordedResult.Should().NotBeNull();
            recordedResult.Id.Should().BeGreaterThan(0);
            recordedResult.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task GetClinicalAlerts_ShouldReturnAlerts()
        {
            // Arrange
            var patient = await CreateTestPatient();
            await CreateTestAllergies(patient.Id);
            await CreateTestMedications(patient.Id);

            // Act
            var alerts = await _clinicalService.GetClinicalAlertsAsync(patient.Id);

            // Assert
            alerts.Should().NotBeNull();
            alerts.Should().BeOfType<List<Alert>>();
        }

        // Helper methods
        private async Task<Patient> CreateTestPatient()
        {
            var patient = new Patient
            {
                FirstName = "Test",
                LastName = "Patient",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                Gender = "M"
            };
            return await _patientRepository.AddAsync(patient);
        }

        private async Task<Encounter> CreateTestEncounter(int patientId)
        {
            var encounter = new Encounter
            {
                PatientId = patientId,
                ProviderId = 1,
                EncounterDate = DateTime.UtcNow,
                EncounterType = "Office Visit",
                Status = EncounterStatus.Open
            };
            return await _clinicalRepository.CreateEncounterAsync(encounter);
        }

        private async Task<Order> CreateTestOrder(int patientId, int encounterId)
        {
            var order = new Order
            {
                PatientId = patientId,
                EncounterId = encounterId,
                OrderingProviderId = 1,
                OrderType = OrderType.Laboratory,
                OrderDate = DateTime.UtcNow,
                Priority = OrderPriority.Routine,
                Status = OrderStatus.Pending
            };
            return await _clinicalRepository.CreateOrderAsync(order);
        }

        private async Task CreateTestAllergies(int patientId)
        {
            var allergy = new Allergy
            {
                PatientId = patientId,
                AllergenName = "Penicillin",
                AllergyType = "Medication",
                Severity = "Severe",
                Reaction = "Hives"
            };
            await _clinicalRepository.RecordAllergyAsync(allergy);
        }

        private async Task CreateTestMedications(int patientId)
        {
            var prescription = new Prescription
            {
                PatientId = patientId,
                ProviderId = 1,
                MedicationName = "Amoxicillin",
                Dosage = "500mg",
                Frequency = "BID",
                StartDate = DateTime.UtcNow,
                Duration = 10,
                Status = PrescriptionStatus.Active
            };
            await _clinicalRepository.CreatePrescriptionAsync(prescription);
        }
    }
}
