using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

using EMRNext.Core.Infrastructure.Seeding;
using EMRNext.Core.Data;
using EMRNext.Core.Models;

namespace EMRNext.Core.IntegrationTests.Infrastructure.Seeding
{
    /// <summary>
    /// Integration tests for Growth Standard Seeding mechanism
    /// Validates end-to-end seeding process and database interactions
    /// </summary>
    public class GrowthStandardSeederIntegrationTests : IDisposable
    {
        private readonly DbContext _dbContext;
        private readonly GrowthStandardSeeder _seeder;
        private readonly Mock<ILogger<GrowthStandardSeeder>> _mockLogger;

        public GrowthStandardSeederIntegrationTests()
        {
            // Setup in-memory database for testing
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new DbContext(options);
            _mockLogger = new Mock<ILogger<GrowthStandardSeeder>>();
            _seeder = new GrowthStandardSeeder(_mockLogger.Object, _dbContext);
        }

        [Fact]
        public async Task SeedStandards_ValidData_SuccessfulImport()
        {
            // Arrange
            var syntheticRecords = _seeder.GenerateSyntheticRecords(1000);
            var csvContent = GenerateMockCsvData(syntheticRecords);

            // Act
            using var stream = new MemoryStream(csvContent);
            await _seeder.ImportWHODataAsync(stream, "Height", 1);

            // Assert
            var importedRecords = _dbContext.Set<PercentileDataEntity>()
                .Where(p => p.GrowthStandardId == 1 && p.MeasurementType == "Height")
                .ToList();

            Assert.NotEmpty(importedRecords);
        }

        [Fact]
        public void AnalyzeGrowthStandards_ProducesStatistics()
        {
            // Arrange
            var syntheticRecords = _seeder.GenerateSyntheticRecords(5000);

            // Act
            var statistics = _seeder.AnalyzeGrowthStandardRecords(syntheticRecords);

            // Assert
            Assert.NotNull(statistics);
            Assert.True(statistics.TotalRecords > 0);
            Assert.True(statistics.AverageAge >= 0 && statistics.AverageAge <= 240);
        }

        [Fact]
        public async Task LargeScaleSeeding_PerformanceTest()
        {
            // Arrange
            var syntheticRecords = _seeder.GenerateSyntheticRecords(50000);
            var csvContent = GenerateMockCsvData(syntheticRecords);

            // Act
            using var stream = new MemoryStream(csvContent);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await _seeder.ImportWHODataAsync(stream, "Height", 3);
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 30000, 
                "Seeding 50,000 records took too long");
        }

        private byte[] GenerateMockCsvData(
            System.Collections.Generic.IEnumerable<IGrowthStandardRecord> records)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            
            // CSV Header
            writer.WriteLine("Age,L,M,S");
            
            // Write valid records
            foreach (var record in records.Where(r => r.IsValid()))
            {
                writer.WriteLine(
                    $"{record.Age},{record.L},{record.M},{record.S}"
                );
            }
            
            writer.Flush();
            return stream.ToArray();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
