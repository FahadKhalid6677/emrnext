using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Infrastructure.Reporting;
using EMRNext.Core.Services.Reporting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EMRNext.Core.Tests.Services.Reporting
{
    public class ReportingEngineTests
    {
        private readonly Mock<ILogger<ReportingEngine.ReportingService>> _mockLogger;
        private readonly Mock<ReportingEngine.IReportDataSource> _mockDataSource;
        private readonly Mock<ReportingEngine.IReportFilter> _mockFilter;
        private readonly Mock<ReportingEngine.IReportAggregator> _mockAggregator;

        public ReportingEngineTests()
        {
            _mockLogger = new Mock<ILogger<ReportingEngine.ReportingService>>();
            _mockDataSource = new Mock<ReportingEngine.IReportDataSource>();
            _mockFilter = new Mock<ReportingEngine.IReportFilter>();
            _mockAggregator = new Mock<ReportingEngine.IReportAggregator>();
        }

        [Fact]
        public async Task GenerateReport_ValidConfiguration_ReturnsSuccessfulReport()
        {
            // Arrange
            var testData = new List<object>
            {
                new { Id = 1, Name = "Test1" },
                new { Id = 2, Name = "Test2" }
            }.AsQueryable();

            _mockDataSource
                .Setup(ds => ds.GetDataAsync())
                .ReturnsAsync(testData);

            _mockFilter
                .Setup(f => f.ApplyFilter(It.IsAny<IQueryable<object>>()))
                .Returns(testData);

            _mockAggregator
                .Setup(a => a.Aggregate(It.IsAny<IQueryable<object>>()))
                .Returns(testData);

            var reportService = new ReportingEngine.ReportingService(
                _mockLogger.Object,
                new[] { _mockDataSource.Object },
                new[] { _mockFilter.Object },
                new[] { _mockAggregator.Object }
            );

            var context = new ReportingEngine.ReportGenerationContext
            {
                Configuration = new ReportingEngine.ReportConfiguration
                {
                    ReportId = "TEST_REPORT",
                    ReportName = "Test Report",
                    Type = ReportingEngine.ReportType.Standard,
                    Parameters = new List<ReportingEngine.ReportParameter>()
                },
                InputParameters = new Dictionary<string, object>(),
                GeneratedBy = "TestUser"
            };

            // Act
            var result = await reportService.GenerateReportAsync(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST_REPORT", result.ReportId);
            Assert.Empty(result.ValidationIssues);
            Assert.NotNull(result.RawData);
            Assert.NotNull(result.ProcessedData);
            Assert.NotNull(result.Metadata);
        }

        [Fact]
        public async Task GenerateReport_MissingRequiredParameter_ThrowsArgumentException()
        {
            // Arrange
            var reportService = new ReportingEngine.ReportingService(
                _mockLogger.Object,
                new[] { _mockDataSource.Object },
                new[] { _mockFilter.Object },
                new[] { _mockAggregator.Object }
            );

            var context = new ReportingEngine.ReportGenerationContext
            {
                Configuration = new ReportingEngine.ReportConfiguration
                {
                    ReportId = "TEST_REPORT",
                    ReportName = "Test Report",
                    Type = ReportingEngine.ReportType.Standard,
                    Parameters = new List<ReportingEngine.ReportParameter>
                    {
                        new ReportingEngine.ReportParameter
                        {
                            Name = "RequiredParam",
                            IsRequired = true
                        }
                    }
                },
                InputParameters = new Dictionary<string, object>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => reportService.GenerateReportAsync(context)
            );
        }

        [Fact]
        public async Task ReportVisualizationService_GenerateVisualization_ProducesValidImage()
        {
            // Arrange
            var visualizationService = new ReportVisualizationService();
            var config = new ReportVisualizationService.VisualizationConfig
            {
                Type = ReportVisualizationService.VisualizationType.Bar,
                Title = "Test Visualization",
                Data = new Dictionary<string, object>
                {
                    { "Category1", 10.0 },
                    { "Category2", 20.0 },
                    { "Category3", 15.0 }
                },
                Colors = new Dictionary<string, string>
                {
                    { "Category1", "#FF0000" },
                    { "Category2", "#00FF00" },
                    { "Category3", "#0000FF" }
                }
            };

            // Act
            var imageBytes = visualizationService.GenerateVisualization(config);

            // Assert
            Assert.NotNull(imageBytes);
            Assert.True(imageBytes.Length > 0);
        }
    }
}
