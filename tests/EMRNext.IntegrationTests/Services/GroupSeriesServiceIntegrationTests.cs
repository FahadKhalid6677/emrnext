using System;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Entities.Portal;
using EMRNext.Core.Services.Portal;
using EMRNext.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EMRNext.IntegrationTests.Services
{
    public class GroupSeriesServiceIntegrationTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;

        public GroupSeriesServiceIntegrationTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreateAndUpdateSeries_Success()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<GroupSeriesService>();
            var context = scope.ServiceProvider.GetRequiredService<EMRNextDbContext>();

            var series = new GroupSeries
            {
                Name = "Integration Test Series",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(3),
                SessionDurationMinutes = 60,
                MaxParticipants = 10,
                Description = "Test Description"
            };

            // Act - Create
            var createdSeries = await service.CreateSeriesAsync(series);

            // Assert - Create
            Assert.NotNull(createdSeries);
            Assert.NotEqual(0, createdSeries.Id);
            Assert.Equal("Integration Test Series", createdSeries.Name);

            // Act - Update
            createdSeries.Name = "Updated Series Name";
            var updatedSeries = await service.UpdateSeriesAsync(createdSeries);

            // Assert - Update
            Assert.NotNull(updatedSeries);
            Assert.Equal("Updated Series Name", updatedSeries.Name);

            // Verify in database
            var dbSeries = await context.GroupSeries.FindAsync(createdSeries.Id);
            Assert.Equal("Updated Series Name", dbSeries.Name);
        }

        [Fact]
        public async Task EnrollAndWithdrawParticipant_Success()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<GroupSeriesService>();
            var context = scope.ServiceProvider.GetRequiredService<EMRNextDbContext>();

            var series = await CreateTestSeriesAsync(context);
            var patient = await CreateTestPatientAsync(context);

            // Act - Enroll
            var participant = await service.EnrollParticipantAsync(series.Id, patient.Id);

            // Assert - Enroll
            Assert.NotNull(participant);
            Assert.Equal(series.Id, participant.GroupSeriesId);
            Assert.Equal(patient.Id, participant.PatientId);
            Assert.Equal("Active", participant.EnrollmentStatus);

            // Act - Withdraw
            var withdrawResult = await service.WithdrawParticipantAsync(series.Id, patient.Id);

            // Assert - Withdraw
            Assert.True(withdrawResult);
            var dbParticipant = await context.SeriesParticipants
                .FirstOrDefaultAsync(p => p.GroupSeriesId == series.Id && p.PatientId == patient.Id);
            Assert.Equal("Withdrawn", dbParticipant.EnrollmentStatus);
        }

        [Fact]
        public async Task RecordAndRetrieveOutcomes_Success()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<GroupSeriesService>();
            var context = scope.ServiceProvider.GetRequiredService<EMRNextDbContext>();

            var series = await CreateTestSeriesAsync(context);
            var patient = await CreateTestPatientAsync(context);
            var participant = await service.EnrollParticipantAsync(series.Id, patient.Id);

            var outcome = new ParticipantOutcome
            {
                OutcomeType = "Progress",
                OutcomeValue = "Improved",
                Notes = "Integration test outcome"
            };

            // Act - Record
            var recordedOutcome = await service.RecordSeriesOutcomeAsync(series.Id, patient.Id, outcome);

            // Assert - Record
            Assert.NotNull(recordedOutcome);
            Assert.Equal("Progress", recordedOutcome.OutcomeType);
            Assert.Equal("Improved", recordedOutcome.OutcomeValue);

            // Act - Retrieve
            var outcomes = await service.GetParticipantOutcomesAsync(series.Id);

            // Assert - Retrieve
            Assert.NotNull(outcomes);
            Assert.Contains(outcomes, o => o.OutcomeType == "Progress" && o.OutcomeValue == "Improved");
        }

        [Fact]
        public async Task GenerateAndRetrieveReport_Success()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<GroupSeriesService>();
            var context = scope.ServiceProvider.GetRequiredService<EMRNextDbContext>();

            var series = await CreateTestSeriesAsync(context);
            var patient = await CreateTestPatientAsync(context);
            await service.EnrollParticipantAsync(series.Id, patient.Id);

            // Act
            var report = await service.GenerateParticipantReportAsync(series.Id, patient.Id);

            // Assert
            Assert.NotNull(report);
            Assert.True(report.Length > 0);
        }

        private async Task<GroupSeries> CreateTestSeriesAsync(EMRNextDbContext context)
        {
            var series = new GroupSeries
            {
                Name = "Test Series",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1),
                SessionDurationMinutes = 60,
                MaxParticipants = 10
            };

            context.GroupSeries.Add(series);
            await context.SaveChangesAsync();
            return series;
        }

        private async Task<Patient> CreateTestPatientAsync(EMRNextDbContext context)
        {
            var patient = new Patient
            {
                FirstName = "Test",
                LastName = "Patient",
                DateOfBirth = DateTime.Today.AddYears(-30),
                Gender = "M"
            };

            context.Patients.Add(patient);
            await context.SaveChangesAsync();
            return patient;
        }
    }
}
