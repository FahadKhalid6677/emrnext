using System;
using System.Threading.Tasks;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Models;
using EMRNext.Core.Services;
using EMRNext.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EMRNext.Tests.PackageCompatibility
{
    public class HealthAnalyticsCompatibilityTests
    {
        private readonly IServiceProvider _serviceProvider;

        public HealthAnalyticsCompatibilityTests()
        {
            var services = new ServiceCollection();
            
            // Register core services
            services.AddScoped<IPredictiveHealthAnalyticsService, PredictiveHealthAnalyticsService>();
            services.AddScoped<IHealthRiskPredictor, BasicHealthRiskPredictor>();
            
            // Register other dependencies
            services.AddLogging();
            services.AddAutoMapper(typeof(PredictiveHealthAnalyticsService).Assembly);
            
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task VerifyServiceResolution_AllDependenciesResolve()
        {
            // Arrange & Act
            var analyticsService = _serviceProvider.GetService<IPredictiveHealthAnalyticsService>();
            var riskPredictor = _serviceProvider.GetService<IHealthRiskPredictor>();

            // Assert
            Assert.NotNull(analyticsService);
            Assert.NotNull(riskPredictor);
        }

        [Fact]
        public async Task VerifyHealthRiskPrediction_ExecutesWithoutErrors()
        {
            // Arrange
            var analyticsService = _serviceProvider.GetRequiredService<IPredictiveHealthAnalyticsService>();
            var healthData = new HealthData
            {
                PatientId = Guid.NewGuid(),
                BloodPressureSystolic = 120,
                BloodPressureDiastolic = 80,
                HeartRate = 75,
                Temperature = 37.0m
            };

            // Act
            var result = await analyticsService.PredictHealthRisk(healthData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(healthData.PatientId, result.PatientId);
            Assert.NotNull(result.Recommendations);
        }

        [Fact]
        public void VerifyAutomapperConfiguration_ValidMappings()
        {
            // Arrange
            var mapper = _serviceProvider.GetService<AutoMapper.IMapper>();

            // Act & Assert
            Assert.NotNull(mapper);
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void VerifyEntityFrameworkCore_DatabaseContext()
        {
            // Arrange & Act
            var exception = Record.Exception(() => 
            {
                using var scope = _serviceProvider.CreateScope();
                // Verify EF Core types are properly resolved
                var contextType = Type.GetType("Microsoft.EntityFrameworkCore.DbContext, Microsoft.EntityFrameworkCore");
                Assert.NotNull(contextType);
            });

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void VerifyJsonSerialization_HealthDataModels()
        {
            // Arrange
            var healthData = new HealthData
            {
                PatientId = Guid.NewGuid(),
                BloodPressureSystolic = 120,
                BloodPressureDiastolic = 80,
                HeartRate = 75,
                Temperature = 37.0m
            };

            // Act
            var serialized = System.Text.Json.JsonSerializer.Serialize(healthData);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<HealthData>(serialized);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(healthData.PatientId, deserialized.PatientId);
            Assert.Equal(healthData.BloodPressureSystolic, deserialized.BloodPressureSystolic);
        }
    }
}
