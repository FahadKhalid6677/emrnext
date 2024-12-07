using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Portal;
using EMRNext.Web.Models.GroupSeries;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EMRNext.IntegrationTests.Controllers
{
    public class GroupSeriesControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public GroupSeriesControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetAllSeries_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/groupseries");

            // Assert
            response.EnsureSuccessStatusCode();
            var series = await response.Content.ReadFromJsonAsync<IEnumerable<GroupSeriesDto>>();
            Assert.NotNull(series);
        }

        [Fact]
        public async Task GetSeries_WithValidId_ReturnsSeriesDetails()
        {
            // Arrange
            var seriesId = await CreateTestSeries();

            // Act
            var response = await _client.GetAsync($"/api/groupseries/{seriesId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var series = await response.Content.ReadFromJsonAsync<GroupSeriesDto>();
            Assert.NotNull(series);
            Assert.Equal(seriesId, series.Id);
        }

        [Fact]
        public async Task CreateSeries_WithValidData_ReturnsCreatedSeries()
        {
            // Arrange
            var newSeries = new GroupSeriesDto
            {
                Name = "Test Series",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(30),
                RecurrencePattern = "Weekly",
                MaxParticipants = 10,
                Status = "Active",
                ProviderId = Guid.NewGuid(),
                AppointmentTypeId = Guid.NewGuid(),
                Location = "Test Location"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/groupseries", newSeries);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdSeries = await response.Content.ReadFromJsonAsync<GroupSeriesDto>();
            Assert.NotNull(createdSeries);
            Assert.Equal(newSeries.Name, createdSeries.Name);
        }

        [Fact]
        public async Task UpdateSeries_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var seriesId = await CreateTestSeries();
            var updatedSeries = new GroupSeriesDto
            {
                Id = seriesId,
                Name = "Updated Series",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(30),
                RecurrencePattern = "Weekly",
                MaxParticipants = 15,
                Status = "Active",
                ProviderId = Guid.NewGuid(),
                AppointmentTypeId = Guid.NewGuid(),
                Location = "Updated Location"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/groupseries/{seriesId}", updatedSeries);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSeries_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var seriesId = await CreateTestSeries();

            // Act
            var response = await _client.DeleteAsync($"/api/groupseries/{seriesId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task EnrollParticipant_WithValidData_ReturnsParticipant()
        {
            // Arrange
            var seriesId = await CreateTestSeries();
            var patientId = Guid.NewGuid();

            // Act
            var response = await _client.PostAsync($"/api/groupseries/{seriesId}/enroll?patientId={patientId}", null);

            // Assert
            response.EnsureSuccessStatusCode();
            var participant = await response.Content.ReadFromJsonAsync<ParticipantDto>();
            Assert.NotNull(participant);
            Assert.Equal(patientId, participant.PatientId);
        }

        private async Task<Guid> CreateTestSeries()
        {
            var newSeries = new GroupSeriesDto
            {
                Name = "Test Series",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(30),
                RecurrencePattern = "Weekly",
                MaxParticipants = 10,
                Status = "Active",
                ProviderId = Guid.NewGuid(),
                AppointmentTypeId = Guid.NewGuid(),
                Location = "Test Location"
            };

            var response = await _client.PostAsJsonAsync("/api/groupseries", newSeries);
            response.EnsureSuccessStatusCode();
            var createdSeries = await response.Content.ReadFromJsonAsync<GroupSeriesDto>();
            return createdSeries.Id;
        }
    }
}
