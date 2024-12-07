using System;
using System.Threading.Tasks;
using Xunit;
using EMRNext.Infrastructure.Services.External;

namespace EMRNext.Infrastructure.Tests.Integration
{
    [Collection("Integration Tests")]
    public class TelehealthServiceIntegrationTests : IntegrationTestBase
    {
        private readonly ITelehealthService _telehealthService;

        public TelehealthServiceIntegrationTests()
        {
            _telehealthService = GetService<ITelehealthService>();
        }

        [Fact]
        public async Task CreateAndEndSession_ValidRequest_SuccessfulFlow()
        {
            // Arrange
            var request = new SessionRequest
            {
                ProviderId = "test-provider-123",
                PatientId = "test-patient-456",
                ScheduledStartTime = DateTime.UtcNow.AddHours(1),
                DurationMinutes = 30,
                SessionType = "Consultation",
                EnableRecording = false,
                Settings = new SessionSettings
                {
                    EnableChat = true,
                    EnableScreenShare = true,
                    MuteParticipantsOnEntry = true,
                    MaxParticipants = 2
                }
            };

            // Act - Create Session
            var session = await _telehealthService.CreateSessionAsync(request);

            // Assert - Session Creation
            Assert.NotNull(session);
            Assert.Equal(request.ProviderId, session.ProviderId);
            Assert.Equal(request.PatientId, session.PatientId);
            Assert.Equal("Scheduled", session.Status);

            // Act - Generate Tokens
            var providerToken = await _telehealthService.GenerateTokenAsync(
                session.SessionId, request.ProviderId, "Provider");
            var patientToken = await _telehealthService.GenerateTokenAsync(
                session.SessionId, request.PatientId, "Patient");

            // Assert - Tokens
            Assert.NotNull(providerToken);
            Assert.NotNull(patientToken);
            Assert.NotEqual(providerToken.Token, patientToken.Token);

            // Act - End Session
            var endResult = await _telehealthService.EndSessionAsync(session.SessionId);

            // Assert - Session Ended
            Assert.True(endResult);

            // Verify Final State
            var finalSession = await _telehealthService.GetSessionAsync(session.SessionId);
            Assert.Equal("Ended", finalSession.Status);
        }

        [Fact]
        public async Task GetSessionsByProvider_ValidRequest_ReturnsResults()
        {
            // Arrange
            var providerId = "test-provider-123";
            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(7);

            // Act
            var sessions = await _telehealthService.GetSessionsByProviderAsync(
                providerId, startDate, endDate);

            // Assert
            Assert.NotNull(sessions);
            foreach (var session in sessions)
            {
                Assert.Equal(providerId, session.ProviderId);
                Assert.True(session.ScheduledStartTime >= startDate);
                Assert.True(session.ScheduledStartTime <= endDate);
            }
        }

        [Fact]
        public async Task GetSessionsByPatient_ValidRequest_ReturnsResults()
        {
            // Arrange
            var patientId = "test-patient-456";
            var startDate = DateTime.UtcNow.Date.AddDays(-30);
            var endDate = DateTime.UtcNow.Date;

            // Act
            var sessions = await _telehealthService.GetSessionsByPatientAsync(
                patientId, startDate, endDate);

            // Assert
            Assert.NotNull(sessions);
            foreach (var session in sessions)
            {
                Assert.Equal(patientId, session.PatientId);
                Assert.True(session.ScheduledStartTime >= startDate);
                Assert.True(session.ScheduledStartTime <= endDate);
            }
        }

        [Fact]
        public async Task UpdateSession_ValidChanges_SuccessfulUpdate()
        {
            // Arrange
            var request = new SessionRequest
            {
                ProviderId = "test-provider-123",
                PatientId = "test-patient-456",
                ScheduledStartTime = DateTime.UtcNow.AddHours(2),
                DurationMinutes = 30,
                SessionType = "Consultation"
            };

            var session = await _telehealthService.CreateSessionAsync(request);

            var updateRequest = new SessionUpdateRequest
            {
                ScheduledStartTime = DateTime.UtcNow.AddHours(3),
                DurationMinutes = 45,
                Settings = new SessionSettings
                {
                    EnableChat = true,
                    EnableScreenShare = false
                }
            };

            // Act
            var updateResult = await _telehealthService.UpdateSessionAsync(
                session.SessionId, updateRequest);

            // Assert
            Assert.True(updateResult);

            var updatedSession = await _telehealthService.GetSessionAsync(session.SessionId);
            Assert.Equal(updateRequest.ScheduledStartTime, updatedSession.ScheduledStartTime);
            Assert.Equal(updateRequest.DurationMinutes, updatedSession.DurationMinutes);
            Assert.True(updatedSession.Settings.EnableChat);
            Assert.False(updatedSession.Settings.EnableScreenShare);
        }

        [Fact]
        public async Task MonitorSessionQuality_ValidSession_TracksMetrics()
        {
            // Arrange
            var request = new SessionRequest
            {
                ProviderId = "test-provider-123",
                PatientId = "test-patient-456",
                ScheduledStartTime = DateTime.UtcNow.AddMinutes(5),
                DurationMinutes = 15,
                SessionType = "Consultation"
            };

            // Act
            var session = await _telehealthService.CreateSessionAsync(request);
            
            // Wait for session to start and collect some metrics
            await Task.Delay(TimeSpan.FromMinutes(6));
            
            var activeSession = await _telehealthService.GetSessionAsync(session.SessionId);

            // Assert
            Assert.NotNull(activeSession);
            Assert.NotNull(activeSession.Metrics);
            Assert.NotNull(activeSession.Participants);

            foreach (var participant in activeSession.Participants)
            {
                Assert.NotNull(participant.NetworkStats);
                Assert.True(participant.NetworkStats.Bitrate > 0);
                Assert.NotNull(participant.NetworkStats.Quality);
            }
        }
    }
}
