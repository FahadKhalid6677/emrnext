using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using EMRNext.Infrastructure.Configuration;
using Xunit;

namespace EMRNext.Tests.Infrastructure.Configuration
{
    public class EnvironmentConfigurationTests
    {
        private readonly IConfiguration _configuration;
        private readonly EnvironmentVariableProvider _provider;

        public EnvironmentConfigurationTests()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"EMRNEXT_ENVIRONMENT", "Development"},
                {"EMRNEXT_DB_PROVIDER", "SqlServer"},
                {"EMRNEXT_DB_HOST", "testhost"},
                {"EMRNEXT_DB_NAME", "TestDb"},
                {"EMRNEXT_API_URL", "http://localhost:5000"},
                {"Jwt:SecretKey", "TestSecretKey"},
                {"Database:Provider", "SqlServer"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _provider = new EnvironmentVariableProvider(_configuration);
        }

        [Fact]
        public void ConfigurationValidator_ValidConfiguration_PassesValidation()
        {
            // Arrange
            var validator = new ConfigurationValidator(_configuration);

            // Act
            var isValid = validator.Validate();

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void EnvironmentVariableProvider_GetValue_ReturnsCorrectValue()
        {
            // Act
            var environment = _provider.GetValue("ENVIRONMENT");
            var dbProvider = _provider.GetValue("DB_PROVIDER");

            // Assert
            Assert.Equal("Development", environment);
            Assert.Equal("SqlServer", dbProvider);
        }

        [Fact]
        public void EnvironmentVariableProvider_GetConnectionString_GeneratesValidConnectionString()
        {
            // Act
            var connectionString = _provider.GetConnectionString();

            // Assert
            Assert.Contains("Server=testhost", connectionString);
            Assert.Contains("Database=TestDb", connectionString);
        }

        [Fact]
        public void EnvironmentVariableProvider_IsDevelopment_ReturnsCorrectValue()
        {
            // Act
            var isDevelopment = _provider.IsDevelopment();

            // Assert
            Assert.True(isDevelopment);
        }

        [Theory]
        [InlineData("DB_HOST", "testhost")]
        [InlineData("NONEXISTENT_KEY", "default")]
        public void EnvironmentVariableProvider_GetValue_HandlesDefaultValues(string key, string expected)
        {
            // Act
            var value = _provider.GetValue(key, "default");

            // Assert
            Assert.Equal(expected, value);
        }

        [Fact]
        public void EnvironmentVariableProvider_GetTypedValue_ConvertsProperly()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                {"EMRNEXT_INT_VALUE", "42"},
                {"EMRNEXT_BOOL_VALUE", "true"}
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var provider = new EnvironmentVariableProvider(config);

            // Act
            var intValue = provider.GetValue<int>("INT_VALUE");
            var boolValue = provider.GetValue<bool>("BOOL_VALUE");

            // Assert
            Assert.Equal(42, intValue);
            Assert.True(boolValue);
        }
    }
}
