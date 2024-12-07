using System;
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
    public class PaymentGatewayServiceTests
    {
        private readonly Mock<ILogger<PaymentGatewayService>> _loggerMock;
        private readonly Mock<IOptions<ExternalServicesConfiguration>> _configMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public PaymentGatewayServiceTests()
        {
            _loggerMock = new Mock<ILogger<PaymentGatewayService>>();
            _configMock = new Mock<IOptions<ExternalServicesConfiguration>>();
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object);

            // Setup configuration
            _configMock.Setup(x => x.Value).Returns(new ExternalServicesConfiguration
            {
                PaymentGateway = new PaymentGatewayConfig
                {
                    MerchantId = "test-merchant",
                    PublicKey = "test-public-key",
                    PrivateKey = "test-private-key",
                    UseSandbox = true,
                    WebhookSecret = "test-webhook-secret"
                }
            });
        }

        [Fact]
        public async Task ProcessPaymentAsync_WhenValid_ReturnsPaymentResult()
        {
            // Arrange
            var request = new PaymentRequest
            {
                PaymentMethodId = "pm_test123",
                Amount = 100.00m,
                Currency = "USD",
                CustomerId = "cust_test123",
                Description = "Test payment"
            };

            var expectedResponse = new PaymentResult
            {
                TransactionId = "tx_test123",
                Status = "succeeded",
                Amount = 100.00m,
                Currency = "USD",
                PaymentMethodId = "pm_test123"
            };

            SetupMockHttpResponse(HttpStatusCode.OK, expectedResponse);

            var service = new PaymentGatewayService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var result = await service.ProcessPaymentAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("tx_test123", result.TransactionId);
            Assert.Equal("succeeded", result.Status);
            Assert.Equal(100.00m, result.Amount);

            VerifyHttpRequest(HttpMethod.Post, "/api/v1/payments");
        }

        [Fact]
        public async Task ProcessPaymentAsync_WhenError_ThrowsException()
        {
            // Arrange
            var request = new PaymentRequest
            {
                PaymentMethodId = "invalid_pm",
                Amount = 100.00m,
                Currency = "USD",
                CustomerId = "cust_test123"
            };

            SetupMockHttpResponse(HttpStatusCode.BadRequest, null);

            var service = new PaymentGatewayService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => service.ProcessPaymentAsync(request));

            VerifyHttpRequest(HttpMethod.Post, "/api/v1/payments");
        }

        [Fact]
        public async Task RefundPaymentAsync_WhenValid_ReturnsRefundResult()
        {
            // Arrange
            var transactionId = "tx_test123";
            var amount = 50.00m;

            var expectedResponse = new PaymentResult
            {
                TransactionId = "refund_tx_test123",
                Status = "succeeded",
                Amount = amount,
                Currency = "USD"
            };

            SetupMockHttpResponse(HttpStatusCode.OK, expectedResponse);

            var service = new PaymentGatewayService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var result = await service.RefundPaymentAsync(transactionId, amount);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("refund_tx_test123", result.TransactionId);
            Assert.Equal("succeeded", result.Status);
            Assert.Equal(amount, result.Amount);

            VerifyHttpRequest(HttpMethod.Post, "/api/v1/refunds");
        }

        [Fact]
        public async Task GetTransactionDetailsAsync_WhenExists_ReturnsTransaction()
        {
            // Arrange
            var transactionId = "tx_test123";
            var expectedResponse = new PaymentTransaction
            {
                TransactionId = transactionId,
                Type = "payment",
                Status = "succeeded",
                Amount = 100.00m,
                Currency = "USD"
            };

            SetupMockHttpResponse(HttpStatusCode.OK, expectedResponse);

            var service = new PaymentGatewayService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var result = await service.GetTransactionDetailsAsync(transactionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionId, result.TransactionId);
            Assert.Equal("succeeded", result.Status);

            VerifyHttpRequest(HttpMethod.Get, $"/api/v1/transactions/{transactionId}");
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
