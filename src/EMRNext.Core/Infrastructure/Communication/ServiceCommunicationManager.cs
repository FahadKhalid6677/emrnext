using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace EMRNext.Core.Infrastructure.Communication
{
    public class ServiceCommunicationManager
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ServiceCommunicationManager> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public ServiceCommunicationManager(
            HttpClient httpClient, 
            ILogger<ServiceCommunicationManager> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Retry Policy
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    3, 
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception, 
                            "Request failed. Waiting {delay}s before retry {retry}",
                            timeSpan.TotalSeconds, 
                            retryCount
                        );
                    }
                );

            // Circuit Breaker Policy
            _circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (exception, duration) =>
                    {
                        _logger.LogError(
                            "Circuit breaker activated for {duration}",
                            duration
                        );
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset");
                    }
                );
        }

        // Generic method for making HTTP GET requests
        public async Task<T> GetAsync<T>(string serviceUrl, string endpoint)
        {
            return await ExecuteWithPoliciesAsync<T>(async () =>
            {
                var response = await _httpClient.GetAsync($"{serviceUrl}/{endpoint}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content);
            });
        }

        // Generic method for making HTTP POST requests
        public async Task<TResponse> PostAsync<TRequest, TResponse>(
            string serviceUrl, 
            string endpoint, 
            TRequest data)
        {
            return await ExecuteWithPoliciesAsync<TResponse>(async () =>
            {
                var jsonContent = JsonSerializer.Serialize(data);
                var httpContent = new StringContent(
                    jsonContent, 
                    System.Text.Encoding.UTF8, 
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{serviceUrl}/{endpoint}", httpContent);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(content);
            });
        }

        // Execute request with retry and circuit breaker policies
        private async Task<T> ExecuteWithPoliciesAsync<T>(Func<Task<T>> action)
        {
            return await _retryPolicy
                .WrapAsync(_circuitBreakerPolicy)
                .ExecuteAsync(action);
        }

        // Service-to-Service Authentication Token Management
        public class ServiceAuthenticationContext
        {
            public string ServiceId { get; set; }
            public string AuthenticationToken { get; set; }
            public DateTime TokenExpiration { get; set; }

            public bool IsTokenValid() => TokenExpiration > DateTime.UtcNow;
        }

        // Generate service-to-service authentication token
        public ServiceAuthenticationContext GenerateServiceToken(string serviceId)
        {
            return new ServiceAuthenticationContext
            {
                ServiceId = serviceId,
                AuthenticationToken = GenerateSecureToken(),
                TokenExpiration = DateTime.UtcNow.AddHours(1)
            };
        }

        // Secure token generation
        private string GenerateSecureToken()
        {
            return Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)
            );
        }
    }

    // Communication-related exceptions
    public class ServiceCommunicationException : Exception
    {
        public string ServiceName { get; }
        public int StatusCode { get; }

        public ServiceCommunicationException(
            string serviceName, 
            string message, 
            int statusCode) 
            : base(message)
        {
            ServiceName = serviceName;
            StatusCode = statusCode;
        }
    }
}
