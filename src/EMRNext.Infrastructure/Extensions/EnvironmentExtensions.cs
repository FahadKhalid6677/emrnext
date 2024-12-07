using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Configuration;

namespace EMRNext.Infrastructure.Extensions
{
    public static class EnvironmentExtensions
    {
        public static IServiceCollection AddEnvironmentConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Validate configuration
            var validator = new ConfigurationValidator(configuration);
            if (!validator.Validate())
            {
                throw new InvalidOperationException(validator.GetValidationErrors());
            }

            // Register configuration services
            services.AddSingleton<ISecretManager, SecretManager>();

            // Configure strongly typed settings
            services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            // Add environment-specific services
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment?.ToLower() == "development")
            {
                services.AddDevelopmentServices(configuration);
            }
            else
            {
                services.AddProductionServices(configuration);
            }

            return services;
        }

        private static IServiceCollection AddDevelopmentServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add development-specific services
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(configuration["EMRNEXT_CLIENT_URL"])
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            // Enable detailed errors and sensitive data logging
            if (bool.TryParse(configuration["EMRNEXT_ENABLE_DETAILED_ERRORS"], out bool enableDetailedErrors) && enableDetailedErrors)
            {
                services.AddDatabaseDeveloperPageExceptionFilter();
            }

            return services;
        }

        private static IServiceCollection AddProductionServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add production-specific services
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(configuration["EMRNEXT_CORS_ORIGINS"].Split(','))
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Add health checks
            services.AddHealthChecks()
                   .AddDbContextCheck<ApplicationDbContext>()
                   .AddUrlGroup(new Uri(configuration["EMRNEXT_API_URL"]), name: "api-check");

            return services;
        }
    }
}
