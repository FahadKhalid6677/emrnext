using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EMRNext.Infrastructure.Services.External
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentGatewayService> _logger;
        private readonly PaymentGatewayConfig _config;

        public PaymentGatewayService(
            HttpClient httpClient,
            ILogger<PaymentGatewayService> logger,
            IOptions<ExternalServicesConfiguration> config)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config?.Value?.PaymentGateway ?? throw new ArgumentNullException(nameof(config));

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            var baseUrl = _config.UseSandbox ? "https://api.sandbox.payment-gateway.com" : "https://api.payment-gateway.com";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-Merchant-ID", _config.MerchantId);
            _httpClient.DefaultRequestHeaders.Add("X-Public-Key", _config.PublicKey);
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing payment for customer: {CustomerId}, Amount: {Amount} {Currency}",
                    request.CustomerId, request.Amount, request.Currency);

                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var signature = GenerateSignature(request, timestamp);

                _httpClient.DefaultRequestHeaders.Add("X-Timestamp", timestamp);
                _httpClient.DefaultRequestHeaders.Add("X-Signature", signature);

                var response = await _httpClient.PostAsJsonAsync("/api/v1/payments", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PaymentResult>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for customer: {CustomerId}", request.CustomerId);
                throw;
            }
        }

        public async Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount)
        {
            try
            {
                _logger.LogInformation("Processing refund for transaction: {TransactionId}, Amount: {Amount}",
                    transactionId, amount);

                var request = new { TransactionId = transactionId, Amount = amount };
                var response = await _httpClient.PostAsJsonAsync("/api/v1/refunds", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PaymentResult>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for transaction: {TransactionId}", transactionId);
                throw;
            }
        }

        public async Task<PaymentMethod> SavePaymentMethodAsync(PaymentMethodRequest request)
        {
            try
            {
                _logger.LogInformation("Saving payment method for customer: {CustomerId}", request.CustomerId);

                var response = await _httpClient.PostAsJsonAsync("/api/v1/payment-methods", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PaymentMethod>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving payment method for customer: {CustomerId}", request.CustomerId);
                throw;
            }
        }

        public async Task<PaymentMethod> GetPaymentMethodAsync(string paymentMethodId)
        {
            try
            {
                _logger.LogInformation("Retrieving payment method: {PaymentMethodId}", paymentMethodId);

                var response = await _httpClient.GetAsync($"/api/v1/payment-methods/{paymentMethodId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PaymentMethod>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment method: {PaymentMethodId}", paymentMethodId);
                throw;
            }
        }

        public async Task<bool> DeletePaymentMethodAsync(string paymentMethodId)
        {
            try
            {
                _logger.LogInformation("Deleting payment method: {PaymentMethodId}", paymentMethodId);

                var response = await _httpClient.DeleteAsync($"/api/v1/payment-methods/{paymentMethodId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment method: {PaymentMethodId}", paymentMethodId);
                throw;
            }
        }

        public async Task<PaymentTransaction> GetTransactionDetailsAsync(string transactionId)
        {
            try
            {
                _logger.LogInformation("Retrieving transaction details: {TransactionId}", transactionId);

                var response = await _httpClient.GetAsync($"/api/v1/transactions/{transactionId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PaymentTransaction>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction details: {TransactionId}", transactionId);
                throw;
            }
        }

        private string GenerateSignature(PaymentRequest request, string timestamp)
        {
            var dataToSign = $"{_config.MerchantId}|{request.Amount}|{request.Currency}|{timestamp}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.PrivateKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
