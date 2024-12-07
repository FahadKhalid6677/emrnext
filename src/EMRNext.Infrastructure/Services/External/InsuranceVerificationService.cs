using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EMRNext.Infrastructure.Services.External
{
    public class InsuranceVerificationService : IInsuranceVerificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InsuranceVerificationService> _logger;
        private readonly InsuranceVerificationConfig _config;

        public InsuranceVerificationService(
            HttpClient httpClient,
            ILogger<InsuranceVerificationService> logger,
            IOptions<ExternalServicesConfiguration> config)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config?.Value?.InsuranceVerification ?? throw new ArgumentNullException(nameof(config));

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_config.Username}:{_config.Password}"));
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Basic", authToken);
        }

        public async Task<CoverageVerificationResult> VerifyCoverageAsync(CoverageVerificationRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Verifying coverage for Member: {MemberId}, Group: {GroupNumber}, Service: {ServiceType}",
                    request.MemberId, request.GroupNumber, request.ServiceType);

                var response = await _httpClient.PostAsJsonAsync("/api/v1/coverage/verify", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CoverageVerificationResult>(content);

                if (result.IsActive)
                {
                    _logger.LogInformation(
                        "Coverage verified for Member: {MemberId}. Plan: {PlanName}, Network: {NetworkStatus}",
                        request.MemberId, result.PlanName, result.NetworkStatus);
                }
                else
                {
                    _logger.LogWarning(
                        "Coverage inactive for Member: {MemberId}. Termination Date: {TerminationDate}",
                        request.MemberId, result.TerminationDate);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying coverage for Member: {MemberId}", request.MemberId);
                throw;
            }
        }

        public async Task<EligibilityResponse> CheckEligibilityAsync(EligibilityRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Checking eligibility for Member: {MemberId}, Service: {ServiceType}, Amount: {Amount}",
                    request.MemberId, request.ServiceType, request.EstimatedAmount);

                var response = await _httpClient.PostAsJsonAsync("/api/v1/eligibility/check", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<EligibilityResponse>(content);

                _logger.LogInformation(
                    "Eligibility check completed. Status: {Status}, Patient Responsibility: {PatientAmount}",
                    result.Status, result.EstimatedPatientResponsibility);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking eligibility for Member: {MemberId}", request.MemberId);
                throw;
            }
        }

        public async Task<AuthorizationResponse> RequestAuthorizationAsync(AuthorizationRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Requesting authorization for Member: {MemberId}, Service: {ServiceType}",
                    request.MemberId, request.ServiceType);

                var response = await _httpClient.PostAsJsonAsync("/api/v1/authorization/request", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthorizationResponse>(content);

                _logger.LogInformation(
                    "Authorization request completed. Number: {AuthNumber}, Status: {Status}",
                    result.AuthorizationNumber, result.Status);

                if (result.RequiredDocuments?.Length > 0)
                {
                    _logger.LogWarning(
                        "Additional documents required for authorization {AuthNumber}",
                        result.AuthorizationNumber);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting authorization for Member: {MemberId}", request.MemberId);
                throw;
            }
        }

        public async Task<ClaimStatus> CheckClaimStatusAsync(string claimId)
        {
            try
            {
                _logger.LogInformation("Checking status for Claim: {ClaimId}", claimId);

                var response = await _httpClient.GetAsync($"/api/v1/claims/{claimId}/status");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ClaimStatus>(content);

                _logger.LogInformation(
                    "Claim status retrieved. Status: {Status}, Amount: {Amount}, Paid: {PaidAmount}",
                    result.Status, result.BilledAmount, result.PaidAmount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking status for Claim: {ClaimId}", claimId);
                throw;
            }
        }
    }
}
