using Microsoft.Extensions.DependencyInjection;
using Xunit;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Services;
using EMRNext.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EMRNext.Tests.Infrastructure
{
    public class DependencyTests
    {
        private readonly IServiceCollection _services;
        private readonly IConfiguration _configuration;

        public DependencyTests()
        {
            _services = new ServiceCollection();
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.test.json", optional: false)
                .AddEnvironmentVariables();
            
            _configuration = builder.Build();
        }

        [Fact]
        public void CoreServices_ShouldResolve()
        {
            // Arrange
            _services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            
            _services.AddScoped<IHealthRiskPredictor, HealthRiskPredictor>();
            _services.AddScoped<IPredictiveHealthAnalyticsService, PredictiveHealthAnalyticsService>();

            var serviceProvider = _services.BuildServiceProvider();

            // Act & Assert
            Assert.NotNull(serviceProvider.GetService<IHealthRiskPredictor>());
            Assert.NotNull(serviceProvider.GetService<IPredictiveHealthAnalyticsService>());
        }

        [Fact]
        public void EntityFramework_ShouldInitialize()
        {
            // Arrange
            _services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            var serviceProvider = _services.BuildServiceProvider();

            // Act
            var dbContext = serviceProvider.GetService<ApplicationDbContext>();

            // Assert
            Assert.NotNull(dbContext);
            Assert.True(dbContext.Database.IsInMemory());
        }

        [Fact]
        public void ML_Dependencies_ShouldResolve()
        {
            // Arrange
            var predictor = new HealthRiskPredictor();

            // Act & Assert
            Assert.NotNull(predictor);
        }
    }
}
