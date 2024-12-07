using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Threading;
using Xunit;
using EMRNext.Infrastructure.Services.External;

namespace EMRNext.Infrastructure.Tests.Services.External
{
    public class DrugDatabaseServiceTests
    {
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<ILogger<DrugDatabaseService>> _loggerMock;
        private readonly Mock<IOptions<ExternalServicesConfiguration>> _configMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public DrugDatabaseServiceTests()
        {
            _cacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<DrugDatabaseService>>();
            _configMock = new Mock<IOptions<ExternalServicesConfiguration>>();
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object);

            // Setup configuration
            _configMock.Setup(x => x.Value).Returns(new ExternalServicesConfiguration
            {
                DrugDatabase = new DrugDatabaseConfig
                {
                    BaseUrl = "https://api.drugdb.test",
                    ApiKey = "test-api-key",
                    CacheExpirationMinutes = 60,
                    EnableRealTimeChecks = true
                }
            });
        }

        [Fact]
        public async Task GetDrugInfoAsync_WhenDrugExists_ReturnsDrugInfo()
        {
            // Arrange
            var ndc = "12345-678-90";
            var expectedResponse = new DrugInfo
            {
                NDC = ndc,
                Name = "Test Drug",
                Manufacturer = "Test Labs",
                Form = "Tablet",
                Strength = "10mg"
            };

            SetupMockHttpResponse(HttpStatusCode.OK, expectedResponse);

            var service = new DrugDatabaseService(
                _httpClient,
                _cacheMock.Object,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var result = await service.GetDrugInfoAsync(ndc);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ndc, result.NDC);
            Assert.Equal("Test Drug", result.Name);
            Assert.Equal("Test Labs", result.Manufacturer);

            VerifyHttpRequest(HttpMethod.Get, $"/api/v1/drugs/{ndc}");
        }

        [Fact]
        public async Task GetDrugInfoAsync_WhenDrugNotFound_ThrowsException()
        {
            // Arrange
            var ndc = "nonexistent-ndc";
            SetupMockHttpResponse(HttpStatusCode.NotFound, null);

            var service = new DrugDatabaseService(
                _httpClient,
                _cacheMock.Object,
                _loggerMock.Object,
                _configMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => service.GetDrugInfoAsync(ndc));

            VerifyHttpRequest(HttpMethod.Get, $"/api/v1/drugs/{ndc}");
        }

        [Fact]
        public async Task GetDrugInfoAsync_WhenCached_ReturnsCachedValue()
        {
            // Arrange
            var ndc = "12345-678-90";
            var cachedDrugInfo = new DrugInfo
            {
                NDC = ndc,
                Name = "Cached Drug",
                Manufacturer = "Cached Labs"
            };

            object cached = cachedDrugInfo;
            _cacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out cached))
                .Returns(true);

            _configMock.Setup(x => x.Value).Returns(new ExternalServicesConfiguration
            {
                DrugDatabase = new DrugDatabaseConfig
                {
                    EnableRealTimeChecks = false
                }
            });

            var service = new DrugDatabaseService(
                _httpClient,
                _cacheMock.Object,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var result = await service.GetDrugInfoAsync(ndc);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ndc, result.NDC);
            Assert.Equal("Cached Drug", result.Name);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
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
