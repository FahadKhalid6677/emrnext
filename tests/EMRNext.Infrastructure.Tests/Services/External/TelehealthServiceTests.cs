using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using EMRNext.Infrastructure.Services.External;

namespace EMRNext.Infrastructure.Tests.Services.External
{
    public class TelehealthServiceTests
    {
        private readonly Mock<ILogger<TelehealthService>> _loggerMock;
        private readonly Mock<IOptions<ExternalServicesConfiguration>> _configMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public TelehealthServiceTests()
        {
            _loggerMock = new Mock<ILogger<TelehealthService>>();
            _configMock = new Mock<IOptions<ExternalServicesConfiguration>>();
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object);

            _configMock.Setup(x => x.Value).Returns(new ExternalServicesConfiguration
            {
                Telehealth = new TelehealthConfig
                {
                    ApiKey = "test-api-key",
                    ApiSecret = "test-api-secret",
                    AccountId = "test-account"
                }
            });
        }

        [Fact]
        public async Task CreateSessionAsync_ValidRequest_ReturnsSession()
        {
            // Arrange
            var request = new SessionRequest
            {
                ProviderId = "provider-123",
                PatientId = "patient-456",
                ScheduledStartTime = DateTime.UtcNow.AddHours(1),
                DurationMinutes = 30,
                SessionType = "Consultation"
            };

            var expectedResponse = new Session
            {
                SessionId = "session-789",
                ProviderId = request.ProviderId,
                PatientId = request.PatientId,
                Status = "Scheduled",
                ScheduledStartTime = request.ScheduledStartTime
            };

            SetupMockHttpResponse(HttpStatusCode.OK, expectedResponse);

            var service = new TelehealthService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var result = await service.CreateSessionAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("session-789", result.SessionId);
            Assert.Equal(request.ProviderId, result.ProviderId);
            Assert.Equal("Scheduled", result.Status);

            VerifyHttpRequest(HttpMethod.Post, "/api/v2/sessions");
        }

        [Fact]
        public async Task CreateSessionAsync_InvalidRequest_ThrowsArgumentException()
        {
            // Arrange
            var request = new SessionRequest
            {
                ProviderId = "", // Invalid: empty provider ID
                PatientId = "patient-456",
                ScheduledStartTime = DateTime.UtcNow.AddHours(1),
                DurationMinutes = 30
            };

            var service = new TelehealthService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateSessionAsync(request));
        }

        [Fact]
        public async Task GetSessionAsync_ValidId_ReturnsSession()
        {
            // Arrange
            var sessionId = "session-789";
            var expectedSession = new Session
            {
                SessionId = sessionId,
                Status = "InProgress",
                Participants = new List<SessionParticipant>
                {
                    new SessionParticipant
                    {
                        ParticipantId = "participant-123",
                        Role = "Provider",
                        ConnectionStatus = "Connected"
                    }
                }
            };

            SetupMockHttpResponse(HttpStatusCode.OK, expectedSession);

            var service = new TelehealthService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var result = await service.GetSessionAsync(sessionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sessionId, result.SessionId);
            Assert.Equal("InProgress", result.Status);
            Assert.Single(result.Participants);

            VerifyHttpRequest(HttpMethod.Get, $"/api/v2/sessions/{sessionId}");
        }

        [Fact]
        public async Task EndSessionAsync_ValidId_ReturnsSuccess()
        {
            // Arrange
            var sessionId = "session-789";
            SetupMockHttpResponse(HttpStatusCode.OK, null);

            var service = new TelehealthService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var result = await service.EndSessionAsync(sessionId);

            // Assert
            Assert.True(result);
            VerifyHttpRequest(HttpMethod.Post, $"/api/v2/sessions/{sessionId}/end");
        }

        [Fact]
        public async Task GenerateTokenAsync_ValidRequest_ReturnsToken()
        {
            // Arrange
            var sessionId = "session-789";
            var participantId = "participant-123";
            var role = "Provider";

            var expectedToken = new SessionToken
            {
                Token = "test-token-xyz",
                SessionId = sessionId,
                ParticipantId = participantId,
                Role = role,
                ExpirationTime = DateTime.UtcNow.AddHours(1)
            };

            SetupMockHttpResponse(HttpStatusCode.OK, expectedToken);

            var service = new TelehealthService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var result = await service.GenerateTokenAsync(sessionId, participantId, role);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-token-xyz", result.Token);
            Assert.Equal(sessionId, result.SessionId);
            Assert.Equal(participantId, result.ParticipantId);

            VerifyHttpRequest(HttpMethod.Post, "/api/v2/tokens");
        }

        [Fact]
        public async Task GetSessionsByProviderAsync_ValidRequest_ReturnsSessions()
        {
            // Arrange
            var providerId = "provider-123";
            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(7);

            var expectedSessions = new List<Session>
            {
                new Session
                {
                    SessionId = "session-1",
                    ProviderId = providerId,
                    Status = "Completed"
                },
                new Session
                {
                    SessionId = "session-2",
                    ProviderId = providerId,
                    Status = "Scheduled"
                }
            };

            SetupMockHttpResponse(HttpStatusCode.OK, expectedSessions);

            var service = new TelehealthService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var results = await service.GetSessionsByProviderAsync(providerId, startDate, endDate);

            // Assert
            var sessionsList = Assert.IsAssignableFrom<IEnumerable<Session>>(results);
            Assert.Equal(2, sessionsList.Count());

            VerifyHttpRequest(HttpMethod.Get, 
                $"/api/v2/providers/{providerId}/sessions?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
        }

        private void SetupMockHttpResponse<T>(HttpStatusCode statusCode, T content)
        {
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = content != null
                        ? new StringContent(System.Text.Json.JsonSerializer.Serialize(content))
                        : null
                });
        }

        private void VerifyHttpRequest(HttpMethod method, string path, Times? times = null)
        {
            _handlerMock.Protected().Verify(
                "SendAsync",
                times ?? Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri.PathAndQuery == path),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
