using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EMRNext.Infrastructure.Services.External
{
    public class TelehealthService : ITelehealthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TelehealthService> _logger;
        private readonly TelehealthConfig _config;

        public TelehealthService(
            HttpClient httpClient,
            ILogger<TelehealthService> logger,
            IOptions<ExternalServicesConfiguration> config)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config?.Value?.Telehealth ?? throw new ArgumentNullException(nameof(config));

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri("https://api.telehealth-provider.com");
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _config.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Account-ID", _config.AccountId);
        }

        public async Task<Session> CreateSessionAsync(SessionRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Creating telehealth session. Provider: {ProviderId}, Patient: {PatientId}, Type: {SessionType}",
                    request.ProviderId, request.PatientId, request.SessionType);

                ValidateSessionRequest(request);
                
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var signature = GenerateSignature(request, timestamp);
                _httpClient.DefaultRequestHeaders.Add("X-Timestamp", timestamp);
                _httpClient.DefaultRequestHeaders.Add("X-Signature", signature);

                var response = await _httpClient.PostAsJsonAsync("/api/v2/sessions", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var session = JsonSerializer.Deserialize<Session>(content);

                _logger.LogInformation(
                    "Telehealth session created. Session ID: {SessionId}, Start Time: {StartTime}",
                    session.SessionId, session.ScheduledStartTime);

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating telehealth session");
                throw;
            }
        }

        public async Task<Session> GetSessionAsync(string sessionId)
        {
            try
            {
                _logger.LogInformation("Retrieving session: {SessionId}", sessionId);

                var response = await _httpClient.GetAsync($"/api/v2/sessions/{sessionId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var session = JsonSerializer.Deserialize<Session>(content);

                LogSessionStatus(session);

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session: {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<bool> EndSessionAsync(string sessionId)
        {
            try
            {
                _logger.LogInformation("Ending session: {SessionId}", sessionId);

                var response = await _httpClient.PostAsync($"/api/v2/sessions/{sessionId}/end", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session: {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<bool> UpdateSessionAsync(string sessionId, SessionUpdateRequest request)
        {
            try
            {
                _logger.LogInformation("Updating session: {SessionId}", sessionId);

                var response = await _httpClient.PatchAsJsonAsync($"/api/v2/sessions/{sessionId}", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session: {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<IEnumerable<Session>> GetSessionsByProviderAsync(
            string providerId,
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                _logger.LogInformation(
                    "Retrieving sessions for Provider: {ProviderId}, Date Range: {StartDate} to {EndDate}",
                    providerId, startDate, endDate);

                var uri = $"/api/v2/providers/{providerId}/sessions" +
                         $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                
                var response = await _httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Session>>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sessions for Provider: {ProviderId}", providerId);
                throw;
            }
        }

        public async Task<IEnumerable<Session>> GetSessionsByPatientAsync(
            string patientId,
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                _logger.LogInformation(
                    "Retrieving sessions for Patient: {PatientId}, Date Range: {StartDate} to {EndDate}",
                    patientId, startDate, endDate);

                var uri = $"/api/v2/patients/{patientId}/sessions" +
                         $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                
                var response = await _httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Session>>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sessions for Patient: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<SessionToken> GenerateTokenAsync(string sessionId, string participantId, string role)
        {
            try
            {
                _logger.LogInformation(
                    "Generating token for Session: {SessionId}, Participant: {ParticipantId}, Role: {Role}",
                    sessionId, participantId, role);

                var request = new
                {
                    SessionId = sessionId,
                    ParticipantId = participantId,
                    Role = role
                };

                var response = await _httpClient.PostAsJsonAsync("/api/v2/tokens", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var token = JsonSerializer.Deserialize<SessionToken>(content);

                _logger.LogInformation("Token generated successfully for Session: {SessionId}", sessionId);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token for Session: {SessionId}", sessionId);
                throw;
            }
        }

        private void ValidateSessionRequest(SessionRequest request)
        {
            if (string.IsNullOrEmpty(request.ProviderId))
                throw new ArgumentException("ProviderId is required", nameof(request));

            if (string.IsNullOrEmpty(request.PatientId))
                throw new ArgumentException("PatientId is required", nameof(request));

            if (request.ScheduledStartTime <= DateTime.Now)
                throw new ArgumentException("ScheduledStartTime must be in the future", nameof(request));

            if (request.DurationMinutes <= 0 || request.DurationMinutes > 240)
                throw new ArgumentException("DurationMinutes must be between 1 and 240", nameof(request));
        }

        private void LogSessionStatus(Session session)
        {
            if (session.Status == "InProgress")
            {
                _logger.LogInformation(
                    "Session {SessionId} is in progress. Participants: {ParticipantCount}",
                    session.SessionId,
                    session.Participants?.Count ?? 0);

                foreach (var participant in session.Participants ?? new List<SessionParticipant>())
                {
                    if (participant.NetworkStats?.Quality == "Poor")
                    {
                        _logger.LogWarning(
                            "Poor connection quality for participant {ParticipantId} in session {SessionId}",
                            participant.ParticipantId,
                            session.SessionId);
                    }
                }
            }
        }

        private string GenerateSignature(SessionRequest request, string timestamp)
        {
            var dataToSign = $"{request.ProviderId}|{request.PatientId}|{timestamp}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.ApiSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
