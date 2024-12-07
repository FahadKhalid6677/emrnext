using EMRNext.Core.Domain.Entities;
using EMRNext.Infrastructure.Data;
using EMRNext.Infrastructure.Data.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EMRNext.Tests.Services
{
    public class PatientRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PatientRepository _repository;

        public PatientRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new PatientRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task<Patient> CreateTestPatient()
        {
            var patient = new Patient
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.Parse("1990-01-01"),
                Gender = "Male",
                Email = "john.doe@example.com",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                PublicId = Guid.NewGuid()
            };

            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        [Fact]
        public async Task GetByIdAsync_ExistingPatient_ReturnsPatient()
        {
            // Arrange
            var patient = await CreateTestPatient();

            // Act
            var result = await _repository.GetByIdAsync(patient.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(patient.Id, result.Id);
            Assert.Equal(patient.FirstName, result.FirstName);
            Assert.Equal(patient.LastName, result.LastName);
        }

        [Fact]
        public async Task GetByPublicIdAsync_ExistingPatient_ReturnsPatient()
        {
            // Arrange
            var patient = await CreateTestPatient();

            // Act
            var result = await _repository.GetByPublicIdAsync(patient.PublicId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(patient.PublicId, result.PublicId);
            Assert.Equal(patient.FirstName, result.FirstName);
        }

        [Fact]
        public async Task SearchAsync_MatchingTerm_ReturnsMatchingPatients()
        {
            // Arrange
            await CreateTestPatient();
            await _context.Patients.AddAsync(new Patient
            {
                FirstName = "Jane",
                LastName = "Doe",
                DateOfBirth = DateTime.Parse("1992-01-01"),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                PublicId = Guid.NewGuid()
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchAsync("Doe");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Contains("Doe", p.LastName));
        }

        [Fact]
        public async Task CreateAsync_ValidPatient_CreatesAndReturnsPatient()
        {
            // Arrange
            var patient = new Patient
            {
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = DateTime.Parse("1990-01-01"),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                PublicId = Guid.NewGuid()
            };

            // Act
            var result = await _repository.CreateAsync(patient);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(0, result.Id);
            var dbPatient = await _context.Patients.FindAsync(result.Id);
            Assert.NotNull(dbPatient);
            Assert.Equal(patient.FirstName, dbPatient.FirstName);
        }

        [Fact]
        public async Task UpdateAsync_ExistingPatient_UpdatesAndReturnsPatient()
        {
            // Arrange
            var patient = await CreateTestPatient();
            patient.FirstName = "Updated";
            patient.LastName = "Name";

            // Act
            var result = await _repository.UpdateAsync(patient);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result.FirstName);
            Assert.Equal("Name", result.LastName);
            var dbPatient = await _context.Patients.FindAsync(patient.Id);
            Assert.Equal("Updated", dbPatient.FirstName);
            Assert.Equal("Name", dbPatient.LastName);
        }

        [Fact]
        public async Task GetEncountersAsync_ExistingPatient_ReturnsEncounters()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var encounters = new List<Encounter>
            {
                new Encounter { PatientId = patient.Id, EncounterDate = DateTime.Today },
                new Encounter { PatientId = patient.Id, EncounterDate = DateTime.Today.AddDays(-1) }
            };
            await _context.Encounters.AddRangeAsync(encounters);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEncountersAsync(patient.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, e => Assert.Equal(patient.Id, e.PatientId));
        }

        [Fact]
        public async Task GetAllergiesAsync_ExistingPatient_ReturnsAllergies()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var allergies = new List<Allergy>
            {
                new Allergy { PatientId = patient.Id, Severity = "Mild" },
                new Allergy { PatientId = patient.Id, Severity = "Severe" }
            };
            await _context.Allergies.AddRangeAsync(allergies);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllergiesAsync(patient.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, a => Assert.Equal(patient.Id, a.PatientId));
        }

        [Fact]
        public async Task GetMedicationsAsync_ExistingPatient_ReturnsMedications()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var medications = new List<Medication>
            {
                new Medication { PatientId = patient.Id, StartDate = DateTime.Today },
                new Medication { PatientId = patient.Id, StartDate = DateTime.Today.AddDays(-1) }
            };
            await _context.Medications.AddRangeAsync(medications);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMedicationsAsync(patient.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, m => Assert.Equal(patient.Id, m.PatientId));
        }
    }
}
