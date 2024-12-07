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
    public class LabInterfaceServiceTests
    {
        private readonly Mock<ILogger<LabInterfaceService>> _loggerMock;
        private readonly Mock<IOptions<ExternalServicesConfiguration>> _configMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public LabInterfaceServiceTests()
        {
            _loggerMock = new Mock<ILogger<LabInterfaceService>>();
            _configMock = new Mock<IOptions<ExternalServicesConfiguration>>();
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object);

            _configMock.Setup(x => x.Value).Returns(new ExternalServicesConfiguration
            {
                LabInterface = new LabInterfaceConfig
                {
                    BaseUrl = "https://api.lab-test.com",
                    FacilityId = "test-facility",
                    Username = "test-user",
                    Password = "test-pass"
                }
            });
        }

        [Fact]
        public async Task SubmitLabOrderAsync_ValidOrder_ReturnsOrderResult()
        {
            // Arrange
            var order = new LabOrder
            {
                PatientId = "test-patient",
                ProviderId = "test-provider",
                Tests = new List<OrderedTest>
                {
                    new OrderedTest { TestCode = "CBC", TestName = "Complete Blood Count" }
                }
            };

            var expectedResponse = new OrderResult
            {
                OrderId = "test-order-123",
                Status = "Pending",
                AccessionNumber = "ACC123"
            };

            SetupMockHttpResponse(HttpStatusCode.OK, expectedResponse);

            var service = new LabInterfaceService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var result = await service.SubmitLabOrderAsync(order);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-order-123", result.OrderId);
            Assert.Equal("ACC123", result.AccessionNumber);

            VerifyHttpRequest(HttpMethod.Post, "/api/v2/orders");
        }

        [Fact]
        public async Task SubmitLabOrderAsync_InvalidOrder_ThrowsArgumentException()
        {
            // Arrange
            var order = new LabOrder
            {
                PatientId = "test-patient",
                ProviderId = "test-provider",
                Tests = new List<OrderedTest>() // Empty tests list
            };

            var service = new LabInterfaceService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.SubmitLabOrderAsync(order));
        }

        [Fact]
        public async Task GetLabResultsAsync_ValidOrderId_ReturnsResults()
        {
            // Arrange
            var orderId = "test-order-123";
            var expectedResults = new List<LabResult>
            {
                new LabResult
                {
                    ResultId = "result-123",
                    OrderId = orderId,
                    TestCode = "CBC",
                    Status = "Final",
                    Components = new List<TestComponent>
                    {
                        new TestComponent
                        {
                            Name = "WBC",
                            Value = "7.5",
                            Units = "K/uL",
                            ReferenceRange = "4.5-11.0"
                        }
                    }
                }
            };

            SetupMockHttpResponse(HttpStatusCode.OK, expectedResults);

            var service = new LabInterfaceService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var results = await service.GetLabResultsAsync(orderId);

            // Assert
            Assert.NotNull(results);
            var resultsList = Assert.IsAssignableFrom<IEnumerable<LabResult>>(results);
            var firstResult = Assert.Single(resultsList);
            Assert.Equal("result-123", firstResult.ResultId);
            Assert.Equal("CBC", firstResult.TestCode);

            VerifyHttpRequest(HttpMethod.Get, $"/api/v2/orders/{orderId}/results");
        }

        [Fact]
        public async Task CheckOrderStatusAsync_ValidOrderId_ReturnsStatus()
        {
            // Arrange
            var orderId = "test-order-123";
            var expectedStatus = new OrderStatus
            {
                OrderId = orderId,
                Status = "In Progress",
                CurrentStep = "Specimen Processing",
                LastUpdated = DateTime.UtcNow
            };

            SetupMockHttpResponse(HttpStatusCode.OK, expectedStatus);

            var service = new LabInterfaceService(
                _httpClient,
                _loggerMock.Object,
                _configMock.Object);

            // Act
            var status = await service.CheckOrderStatusAsync(orderId);

            // Assert
            Assert.NotNull(status);
            Assert.Equal(orderId, status.OrderId);
            Assert.Equal("In Progress", status.Status);
            Assert.Equal("Specimen Processing", status.CurrentStep);

            VerifyHttpRequest(HttpMethod.Get, $"/api/v2/orders/{orderId}/status");
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
