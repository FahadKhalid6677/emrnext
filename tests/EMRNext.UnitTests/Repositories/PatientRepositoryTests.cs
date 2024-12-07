using System;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Infrastructure.Data;
using EMRNext.Infrastructure.Repositories;
using EMRNext.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EMRNext.UnitTests.Repositories
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

        [Fact]
        public async Task AddPatient_ShouldSuccessfullyAddPatient()
        {
            // Arrange
            var patient = MockDataGenerator.GeneratePatients(1).First();

            // Act
            await _repository.AddAsync(patient);
            await _context.SaveChangesAsync();

            // Assert
            var addedPatient = await _context.Patients.FindAsync(patient.Id);
            Assert.NotNull(addedPatient);
            Assert.Equal(patient.FirstName, addedPatient.FirstName);
        }

        [Fact]
        public async Task GetPatientById_ShouldReturnCorrectPatient()
        {
            // Arrange
            var patient = MockDataGenerator.GeneratePatients(1).First();
            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();

            // Act
            var retrievedPatient = await _repository.GetByIdAsync(patient.Id);

            // Assert
            Assert.NotNull(retrievedPatient);
            Assert.Equal(patient.Id, retrievedPatient.Id);
        }

        [Fact]
        public async Task UpdatePatient_ShouldModifyPatientDetails()
        {
            // Arrange
            var patient = MockDataGenerator.GeneratePatients(1).First();
            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();

            // Act
            patient.FirstName = "UpdatedName";
            await _repository.UpdateAsync(patient);
            await _context.SaveChangesAsync();

            // Assert
            var updatedPatient = await _context.Patients.FindAsync(patient.Id);
            Assert.Equal("UpdatedName", updatedPatient.FirstName);
        }

        [Fact]
        public async Task DeletePatient_ShouldRemovePatientFromDatabase()
        {
            // Arrange
            var patient = MockDataGenerator.GeneratePatients(1).First();
            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(patient.Id);
            await _context.SaveChangesAsync();

            // Assert
            var deletedPatient = await _context.Patients.FindAsync(patient.Id);
            Assert.Null(deletedPatient);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
