using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Services;
using EMRNext.Core.Models;

namespace EMRNext.Tests.Core.Services
{
    public class PredictiveHealthAnalyticsServiceTests
    {
        private readonly Mock<ILogger<PredictiveHealthAnalyticsService>> _loggerMock;
        private readonly Mock<IHealthRiskPredictor> _predictorMock;
        private readonly PredictiveHealthAnalyticsService _service;

        public PredictiveHealthAnalyticsServiceTests()
        {
            _loggerMock = new Mock<ILogger<PredictiveHealthAnalyticsService>>();
            _predictorMock = new Mock<IHealthRiskPredictor>();
            _service = new PredictiveHealthAnalyticsService(_loggerMock.Object, _predictorMock.Object);
        }

        [Fact]
        public async Task PredictHealthRisk_ValidData_ReturnsAssessment()
        {
            // Arrange
            var healthData = new HealthData
            {
                Age = 45,
                BMI = 24.5,
                BloodPressure = 120,
                Cholesterol = 180
            };

            _predictorMock.Setup(x => x.PredictRisk(It.IsAny<HealthData>()))
                .Returns(new HealthRiskAssessment { RiskLevel = "Low", Score = 0.2 });

            // Act
            var result = await _service.PredictHealthRisk(healthData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Low", result.RiskLevel);
            Assert.Equal(0.2, result.Score);
        }

        [Fact]
        public async Task PredictHealthRisk_NullData_ThrowsArgumentNullException()
        {
            // Arrange
            HealthData? healthData = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _service.PredictHealthRisk(healthData));
        }

        [Fact]
        public async Task PredictHealthRisk_PredictorThrowsException_LogsAndRethrows()
        {
            // Arrange
            var healthData = new HealthData { Age = 45 };
            var exception = new Exception("Test exception");
            
            _predictorMock.Setup(x => x.PredictRisk(It.IsAny<HealthData>()))
                .Throws(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() => 
                _service.PredictHealthRisk(healthData));
            
            Assert.Equal(exception.Message, thrownException.Message);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
