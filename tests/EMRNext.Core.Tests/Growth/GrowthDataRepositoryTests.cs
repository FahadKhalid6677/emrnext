using System;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Infrastructure.Repositories;
using EMRNext.Core.Models.Growth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EMRNext.Core.Tests.Growth
{
    public class GrowthDataRepositoryTests : IDisposable
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly Mock<ILogger<GrowthDataRepository>> _loggerMock;
        private readonly GrowthDataRepository _repository;

        public GrowthDataRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(_options);
            _cache = new MemoryCache(new MemoryCacheOptions());
            _loggerMock = new Mock<ILogger<GrowthDataRepository>>();
            _repository = new GrowthDataRepository(_context, _cache, _loggerMock.Object);
        }

        [Fact]
        public async Task GetGrowthStandard_WhenExists_ReturnsCorrectStandard()
        {
            // Arrange
            var standard = new GrowthStandardEntity
            {
                Type = GrowthStandardType.WHO,
                Name = "WHO Standards",
                Version = "2006",
                Gender = "M",
                EffectiveDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.GrowthStandards.Add(standard);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetGrowthStandardAsync(GrowthStandardType.WHO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(GrowthStandardType.WHO, result.Type);
            Assert.Equal("WHO Standards", result.Name);
        }

        [Fact]
        public async Task GetGrowthStandard_WhenCached_ReturnsCachedValue()
        {
            // Arrange
            var standard = new GrowthStandardEntity
            {
                Type = GrowthStandardType.WHO,
                Name = "WHO Standards",
                Version = "2006",
                Gender = "M",
                EffectiveDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.GrowthStandards.Add(standard);
            await _context.SaveChangesAsync();

            // Act
            var firstResult = await _repository.GetGrowthStandardAsync(GrowthStandardType.WHO);
            _context.GrowthStandards.Remove(standard);
            await _context.SaveChangesAsync();
            var secondResult = await _repository.GetGrowthStandardAsync(GrowthStandardType.WHO);

            // Assert
            Assert.NotNull(secondResult);
            Assert.Equal(firstResult.Name, secondResult.Name);
        }

        [Fact]
        public async Task SaveMeasurement_PersistsCorrectly()
        {
            // Arrange
            var measurement = new GrowthMeasurement
            {
                PatientId = 1,
                Type = MeasurementType.Height,
                Value = 150.5,
                Date = DateTime.UtcNow,
                Source = "Manual Entry"
            };

            // Act
            await _repository.SaveMeasurementAsync(measurement);
            var savedMeasurement = await _context.PatientMeasurements
                .FirstOrDefaultAsync(m => m.PatientId == 1);

            // Assert
            Assert.NotNull(savedMeasurement);
            Assert.Equal(measurement.Value, savedMeasurement.Value);
            Assert.Equal(measurement.Type, savedMeasurement.Type);
        }

        [Fact]
        public async Task GetMeasurements_ReturnsCorrectDateRange()
        {
            // Arrange
            var patientId = 1;
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;

            var measurements = new[]
            {
                new PatientMeasurementEntity
                {
                    PatientId = patientId,
                    Type = MeasurementType.Height,
                    Value = 150.5,
                    MeasurementDate = startDate.AddDays(1)
                },
                new PatientMeasurementEntity
                {
                    PatientId = patientId,
                    Type = MeasurementType.Height,
                    Value = 151.0,
                    MeasurementDate = startDate.AddDays(15)
                },
                new PatientMeasurementEntity
                {
                    PatientId = patientId,
                    Type = MeasurementType.Height,
                    Value = 151.5,
                    MeasurementDate = endDate.AddDays(1) // Outside range
                }
            };

            await _context.PatientMeasurements.AddRangeAsync(measurements);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMeasurementsAsync(
                patientId, MeasurementType.Height, startDate, endDate);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, m => Assert.True(
                m.Date >= startDate && m.Date <= endDate));
        }

        [Fact]
        public async Task CalculateVelocity_ReturnsCorrectValue()
        {
            // Arrange
            var patientId = 1;
            var startDate = DateTime.UtcNow.AddYears(-1);
            var endDate = DateTime.UtcNow;

            var measurements = new[]
            {
                new PatientMeasurementEntity
                {
                    PatientId = patientId,
                    Type = MeasurementType.Height,
                    Value = 150.0,
                    MeasurementDate = startDate
                },
                new PatientMeasurementEntity
                {
                    PatientId = patientId,
                    Type = MeasurementType.Height,
                    Value = 160.0,
                    MeasurementDate = endDate
                }
            };

            await _context.PatientMeasurements.AddRangeAsync(measurements);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.CalculateVelocityAsync(
                patientId, MeasurementType.Height, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10.0, result.VelocityPerYear, 2); // 10cm/year
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _cache.Dispose();
        }
    }
}
