using System;
using System.Threading.Tasks;
using EMRNext.Core.Infrastructure.Seeding;
using EMRNext.Core.Infrastructure.Monitoring;
using EMRNext.Core.Infrastructure.Repositories;
using EMRNext.Core.Models.Growth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EMRNext.Core.Tests.Growth
{
    public class GrowthChartIntegrationTests : IDisposable
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly Mock<ILogger<GrowthDataRepository>> _repoLoggerMock;
        private readonly Mock<ILogger<GrowthStandardSeeder>> _seederLoggerMock;
        private readonly Mock<ILogger<GrowthChartMetrics>> _metricsLoggerMock;
        private readonly GrowthDataRepository _repository;
        private readonly GrowthStandardSeeder _seeder;
        private readonly GrowthChartMetrics _metrics;
        private readonly string _testDataDirectory;

        public GrowthChartIntegrationTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql("Host=localhost;Database=emrnext_test;Username=test;Password=test")
                .Options;

            _context = new ApplicationDbContext(_options);
            _cache = new MemoryCache(new MemoryCacheOptions());
            _repoLoggerMock = new Mock<ILogger<GrowthDataRepository>>();
            _seederLoggerMock = new Mock<ILogger<GrowthStandardSeeder>>();
            _metricsLoggerMock = new Mock<ILogger<GrowthChartMetrics>>();
            
            _testDataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
            
            _repository = new GrowthDataRepository(_context, _cache, _repoLoggerMock.Object);
            _seeder = new GrowthStandardSeeder(_context, _seederLoggerMock.Object, _testDataDirectory);
            _metrics = new GrowthChartMetrics(_metricsLoggerMock.Object);
        }

        [Fact]
        public async Task CompleteWorkflow_SuccessfullyProcessesGrowthData()
        {
            // Arrange
            await _context.Database.MigrateAsync();
            await _seeder.SeedAllStandardsAsync();

            var patientId = 1;
            var measurements = new[]
            {
                new GrowthMeasurement
                {
                    PatientId = patientId,
                    Type = MeasurementType.Height,
                    Value = 100.0,
                    Date = DateTime.UtcNow.AddMonths(-6),
                    Source = "Initial"
                },
                new GrowthMeasurement
                {
                    PatientId = patientId,
                    Type = MeasurementType.Height,
                    Value = 105.0,
                    Date = DateTime.UtcNow,
                    Source = "Follow-up"
                }
            };

            // Act & Assert - Step 1: Save Measurements
            foreach (var measurement in measurements)
            {
                await _repository.SaveMeasurementAsync(measurement);
            }

            var savedMeasurements = await _repository.GetMeasurementsAsync(
                patientId,
                MeasurementType.Height,
                DateTime.UtcNow.AddYears(-1),
                DateTime.UtcNow);

            Assert.Equal(2, savedMeasurements.Count);

            // Act & Assert - Step 2: Calculate Velocity
            var velocity = await _repository.CalculateVelocityAsync(
                patientId,
                MeasurementType.Height,
                DateTime.UtcNow.AddYears(-1),
                DateTime.UtcNow);

            Assert.NotNull(velocity);
            Assert.True(velocity.VelocityPerYear > 0);

            // Act & Assert - Step 3: Get Growth Standard
            var standard = await _repository.GetGrowthStandardAsync(GrowthStandardType.WHO);
            Assert.NotNull(standard);
            Assert.True(standard.PercentileData.Count > 0);

            // Act & Assert - Step 4: Verify Cache
            var cachedStandard = await _repository.GetGrowthStandardAsync(GrowthStandardType.WHO);
            Assert.Same(standard, cachedStandard); // Should be the same instance from cache
        }

        [Fact]
        public async Task DatabaseMigration_SuccessfullyAppliesAllChanges()
        {
            // Act
            await _context.Database.MigrateAsync();

            // Assert
            var tables = new[]
            {
                "GrowthStandards",
                "PercentileData",
                "PatientMeasurements",
                "GrowthAlerts"
            };

            foreach (var table in tables)
            {
                var tableExists = await TableExistsAsync(table);
                Assert.True(tableExists, $"Table {table} should exist");
            }
        }

        [Fact]
        public async Task DataSeeding_SuccessfullyImportsStandardData()
        {
            // Arrange
            await _context.Database.MigrateAsync();

            // Act
            await _seeder.SeedAllStandardsAsync();

            // Assert
            var whoStandards = await _context.GrowthStandards
                .Where(s => s.Type == GrowthStandardType.WHO)
                .ToListAsync();

            var cdcStandards = await _context.GrowthStandards
                .Where(s => s.Type == GrowthStandardType.CDC)
                .ToListAsync();

            Assert.True(whoStandards.Count > 0, "WHO standards should be seeded");
            Assert.True(cdcStandards.Count > 0, "CDC standards should be seeded");

            // Verify percentile data
            var percentileData = await _context.PercentileData
                .Where(p => p.GrowthStandardId == whoStandards.First().Id)
                .ToListAsync();

            Assert.True(percentileData.Count > 0, "Percentile data should be seeded");
        }

        [Fact]
        public async Task ConcurrentAccess_HandlesMultipleOperationsCorrectly()
        {
            // Arrange
            await _context.Database.MigrateAsync();
            var patientId = 1;
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var measurement = new GrowthMeasurement
                {
                    PatientId = patientId,
                    Type = MeasurementType.Height,
                    Value = 100.0 + i,
                    Date = DateTime.UtcNow.AddDays(i),
                    Source = $"Concurrent-{i}"
                };

                tasks.Add(_repository.SaveMeasurementAsync(measurement));
            }

            await Task.WhenAll(tasks);

            // Assert
            var measurements = await _repository.GetMeasurementsAsync(
                patientId,
                MeasurementType.Height,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(10));

            Assert.Equal(10, measurements.Count);
        }

        private async Task<bool> TableExistsAsync(string tableName)
        {
            var sql = @"
                SELECT EXISTS (
                    SELECT FROM information_schema.tables 
                    WHERE table_schema = 'public'
                    AND table_name = @tableName
                );";

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new Npgsql.NpgsqlParameter("@tableName", tableName.ToLower()));

            await _context.Database.OpenConnectionAsync();
            try
            {
                using var result = await command.ExecuteReaderAsync();
                await result.ReadAsync();
                return result.GetBoolean(0);
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _cache.Dispose();
        }
    }
}
