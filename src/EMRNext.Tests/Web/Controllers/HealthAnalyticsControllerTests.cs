using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Models;
using EMRNext.Web.Controllers;

namespace EMRNext.Tests.Web.Controllers
{
    public class HealthAnalyticsControllerTests
    {
        private readonly Mock<ILogger<HealthAnalyticsController>> _loggerMock;
        private readonly Mock<IPredictiveHealthAnalyticsService> _serviceMock;
        private readonly HealthAnalyticsController _controller;

        public HealthAnalyticsControllerTests()
        {
            _loggerMock = new Mock<ILogger<HealthAnalyticsController>>();
            _serviceMock = new Mock<IPredictiveHealthAnalyticsService>();
            _controller = new HealthAnalyticsController(_loggerMock.Object, _serviceMock.Object);
        }

        [Fact]
        public async Task GetHealthRisk_ValidInput_ReturnsOkResult()
        {
            // Arrange
            var healthData = new HealthData
            {
                Age = 45,
                BMI = 24.5,
                BloodPressure = 120,
                Cholesterol = 180
            };

            _serviceMock.Setup(x => x.PredictHealthRisk(It.IsAny<HealthData>()))
                .ReturnsAsync(new HealthRiskAssessment { RiskLevel = "Low", Score = 0.2 });

            // Act
            var result = await _controller.GetHealthRisk(healthData);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<HealthRiskAssessment>(okResult.Value);
            Assert.Equal("Low", returnValue.RiskLevel);
        }

        [Fact]
        public async Task GetHealthRisk_InvalidInput_ReturnsBadRequest()
        {
            // Arrange
            HealthData? healthData = null;

            // Act
            var result = await _controller.GetHealthRisk(healthData);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task GetHealthRisk_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var healthData = new HealthData { Age = 45 };
            _serviceMock.Setup(x => x.PredictHealthRisk(It.IsAny<HealthData>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetHealthRisk(healthData);

            // Assert
            Assert.IsType<StatusCodeResult>(result);
            var statusCodeResult = (StatusCodeResult)result;
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
