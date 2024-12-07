using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EMRNext.Infrastructure.Services.External
{
    public class LabInterfaceService : ILabInterfaceService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LabInterfaceService> _logger;
        private readonly LabInterfaceConfig _config;

        public LabInterfaceService(
            HttpClient httpClient,
            ILogger<LabInterfaceService> logger,
            IOptions<ExternalServicesConfiguration> config)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config?.Value?.LabInterface ?? throw new ArgumentNullException(nameof(config));

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_config.Username}:{_config.Password}"));
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Basic", authToken);
            _httpClient.DefaultRequestHeaders.Add("X-Facility-ID", _config.FacilityId);
        }

        public async Task<OrderResult> SubmitLabOrderAsync(LabOrder order)
        {
            try
            {
                _logger.LogInformation(
                    "Submitting lab order for Patient: {PatientId}, Provider: {ProviderId}, Tests: {TestCount}",
                    order.PatientId, order.ProviderId, order.Tests?.Count ?? 0);

                ValidateLabOrder(order);

                var response = await _httpClient.PostAsJsonAsync("/api/v2/orders", order);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OrderResult>(content);

                _logger.LogInformation(
                    "Lab order submitted successfully. Order ID: {OrderId}, Accession: {AccessionNumber}",
                    result.OrderId, result.AccessionNumber);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting lab order for Patient: {PatientId}", order.PatientId);
                throw;
            }
        }

        public async Task<IEnumerable<LabResult>> GetLabResultsAsync(string orderId)
        {
            try
            {
                _logger.LogInformation("Retrieving results for Order: {OrderId}", orderId);

                var response = await _httpClient.GetAsync($"/api/v2/orders/{orderId}/results");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<IEnumerable<LabResult>>(content);

                foreach (var result in results)
                {
                    LogResultFlags(result);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving results for Order: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<LabTest>> GetAvailableTestsAsync(string facilityId)
        {
            try
            {
                _logger.LogInformation("Retrieving available tests for Facility: {FacilityId}", facilityId);

                var response = await _httpClient.GetAsync($"/api/v2/facilities/{facilityId}/tests");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<LabTest>>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available tests for Facility: {FacilityId}", facilityId);
                throw;
            }
        }

        public async Task<OrderStatus> CheckOrderStatusAsync(string orderId)
        {
            try
            {
                _logger.LogInformation("Checking status for Order: {OrderId}", orderId);

                var response = await _httpClient.GetAsync($"/api/v2/orders/{orderId}/status");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var status = JsonSerializer.Deserialize<OrderStatus>(content);

                _logger.LogInformation(
                    "Order status retrieved. Status: {Status}, Current Step: {Step}",
                    status.Status, status.CurrentStep);

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking status for Order: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(string orderId, string reason)
        {
            try
            {
                _logger.LogInformation("Cancelling Order: {OrderId}, Reason: {Reason}", orderId, reason);

                var request = new { Reason = reason };
                var response = await _httpClient.PostAsJsonAsync($"/api/v2/orders/{orderId}/cancel", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling Order: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<LabResult>> GetResultsByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            string patientId = null)
        {
            try
            {
                _logger.LogInformation(
                    "Retrieving results from {StartDate} to {EndDate}, Patient: {PatientId}",
                    startDate, endDate, patientId ?? "All");

                var uri = $"/api/v2/results?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                if (!string.IsNullOrEmpty(patientId))
                {
                    uri += $"&patientId={patientId}";
                }

                var response = await _httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<LabResult>>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving results for date range");
                throw;
            }
        }

        private void ValidateLabOrder(LabOrder order)
        {
            if (string.IsNullOrEmpty(order.PatientId))
                throw new ArgumentException("PatientId is required", nameof(order));

            if (string.IsNullOrEmpty(order.ProviderId))
                throw new ArgumentException("ProviderId is required", nameof(order));

            if (order.Tests == null || order.Tests.Count == 0)
                throw new ArgumentException("At least one test is required", nameof(order));

            if (order.CollectionDate.HasValue && order.CollectionDate.Value > DateTime.Now)
                throw new ArgumentException("Collection date cannot be in the future", nameof(order));
        }

        private void LogResultFlags(LabResult result)
        {
            if (result.Flags?.Length > 0)
            {
                _logger.LogWarning(
                    "Result {ResultId} has flags: {Flags}",
                    result.ResultId,
                    string.Join(", ", result.Flags));
            }

            foreach (var component in result.Components)
            {
                if (!string.IsNullOrEmpty(component.Flag))
                {
                    _logger.LogWarning(
                        "Result {ResultId}, Component {Component} flagged: {Flag}",
                        result.ResultId,
                        component.Name,
                        component.Flag);
                }
            }
        }
    }
}
