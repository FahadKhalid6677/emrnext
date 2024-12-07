using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using EMRNext.Core.Services;
using EMRNext.Core.Infrastructure.Persistence;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Identity;

namespace EMRNext.Core.Tests.Integration
{
    /// <summary>
    /// Integration tests for Patient Service
    /// </summary>
    public class PatientServiceIntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly EMRNextDbContext _dbContext;
        private readonly IPatientService _patientService;

        public PatientServiceIntegrationTests()
        {
            // Create in-memory database for testing
            var services = new ServiceCollection();

            services.AddDbContext<EMRNextDbContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
            });

            // Register services
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            _serviceProvider = services.BuildServiceProvider();
            _dbContext = _serviceProvider.GetRequiredService<EMRNextDbContext>();
            _patientService = _serviceProvider.GetRequiredService<IPatientService>();

            // Ensure database is created
            _dbContext.Database.EnsureCreated();
        }

        [Fact]
        public async Task RegisterPatient_ShouldSuccessfullyAddPatientToDatabase()
        {
            // Arrange
            var patient = new Patient
            {
                Name = new PersonName 
                { 
                    FirstName = "John", 
                    LastName = "Doe" 
                },
                Demographics = new Demographics
                {
                    DateOfBirth = DateTime.Now.AddYears(-30),
                    Gender = Gender.Male
                },
                ContactInformation = new ContactInformation
                {
                    PrimaryPhoneNumber = "1234567890",
                    EmailAddress = "john.doe@example.com"
                }
            };

            // Act
            var registeredPatient = await _patientService.RegisterPatientAsync(patient);

            // Assert
            Assert.NotNull(registeredPatient);
            Assert.NotEqual(Guid.Empty, registeredPatient.Id);
            
            // Verify patient exists in database
            var dbPatient = await _dbContext.Patients.FindAsync(registeredPatient.Id);
            Assert.NotNull(dbPatient);
            Assert.Equal("John", dbPatient.Name.FirstName);
            Assert.Equal("Doe", dbPatient.Name.LastName);
        }

        [Fact]
        public async Task GetPatientById_ExistingPatient_ShouldReturnPatient()
        {
            // Arrange
            var patient = new Patient
            {
                Name = new PersonName 
                { 
                    FirstName = "Jane", 
                    LastName = "Smith" 
                },
                Demographics = new Demographics
                {
                    DateOfBirth = DateTime.Now.AddYears(-25),
                    Gender = Gender.Female
                }
            };

            // Add patient to database first
            _dbContext.Patients.Add(patient);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedPatient = await _patientService.GetPatientByIdAsync(patient.Id);

            // Assert
            Assert.NotNull(retrievedPatient);
            Assert.Equal(patient.Id, retrievedPatient.Id);
            Assert.Equal("Jane", retrievedPatient.Name.FirstName);
            Assert.Equal("Smith", retrievedPatient.Name.LastName);
        }

        [Fact]
        public async Task UpdatePatient_ShouldModifyPatientInDatabase()
        {
            // Arrange
            var patient = new Patient
            {
                Name = new PersonName 
                { 
                    FirstName = "Alice", 
                    LastName = "Johnson" 
                },
                Demographics = new Demographics
                {
                    DateOfBirth = DateTime.Now.AddYears(-35),
                    Gender = Gender.Female
                }
            };

            // Add patient to database
            await _patientService.RegisterPatientAsync(patient);

            // Modify patient
            patient.Name.FirstName = "Alicia";
            patient.Demographics.Gender = Gender.Other;

            // Act
            var updatedPatient = await _patientService.UpdatePatientAsync(patient);

            // Assert
            Assert.NotNull(updatedPatient);
            Assert.Equal("Alicia", updatedPatient.Name.FirstName);
            Assert.Equal(Gender.Other, updatedPatient.Demographics.Gender);

            // Verify in database
            var dbPatient = await _dbContext.Patients.FindAsync(patient.Id);
            Assert.NotNull(dbPatient);
            Assert.Equal("Alicia", dbPatient.Name.FirstName);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            _serviceProvider.Dispose();
        }
    }
}
