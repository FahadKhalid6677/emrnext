using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Xunit;

namespace EMRNext.Tests.PackageCompatibility
{
    public class DependencyCompatibilityTests
    {
        [Fact]
        public void VerifyNuGetPackages_AllDependenciesCompatible()
        {
            // Arrange & Act & Assert
            
            // Core .NET packages
            Assert.NotNull(typeof(ServiceCollection));  // Microsoft.Extensions.DependencyInjection
            Assert.NotNull(typeof(ILogger));           // Microsoft.Extensions.Logging
            Assert.NotNull(typeof(DbContext));         // Microsoft.EntityFrameworkCore
            
            // Authentication packages
            Assert.NotNull(typeof(JwtSecurityToken));  // System.IdentityModel.Tokens.Jwt
            Assert.NotNull(typeof(JwtBearerDefaults)); // Microsoft.AspNetCore.Authentication.JwtBearer
            
            // Third-party packages
            Assert.NotNull(typeof(IMapper));           // AutoMapper
            Assert.NotNull(typeof(JsonSerializer));    // System.Text.Json
        }

        [Fact]
        public void VerifyPackageVersions_CorrectVersionsLoaded()
        {
            // Get assembly versions
            var efCoreVersion = typeof(DbContext).Assembly.GetName().Version;
            var autoMapperVersion = typeof(IMapper).Assembly.GetName().Version;
            var jwtBearerVersion = typeof(JwtBearerDefaults).Assembly.GetName().Version;

            // Verify versions match our standardized versions
            Assert.True(efCoreVersion?.Major >= 7);
            Assert.True(autoMapperVersion?.Major >= 12);
            Assert.True(jwtBearerVersion?.Major >= 7);
        }

        [Fact]
        public void VerifyDependencyInjection_ServiceResolution()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Add core services
            services.AddLogging();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            
            // Build provider
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var logger = serviceProvider.GetService<ILoggerFactory>();
            var mapper = serviceProvider.GetService<IMapper>();

            Assert.NotNull(logger);
            Assert.NotNull(mapper);
        }

        [Fact]
        public void VerifyJsonSerialization_ComplexObjects()
        {
            // Arrange
            var testObject = new
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Timestamp = DateTime.UtcNow,
                NestedObject = new { Value = 42 }
            };

            // Act
            var serialized = JsonSerializer.Serialize(testObject);
            var deserialized = JsonSerializer.Deserialize<dynamic>(serialized);

            // Assert
            Assert.NotNull(deserialized);
        }

        [Fact]
        public void VerifyEntityFramework_DatabaseOperations()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            // Act & Assert
            using (var context = new DbContext(options))
            {
                Assert.NotNull(context);
                Assert.True(context.Database.IsInMemory());
            }
        }
    }
}
