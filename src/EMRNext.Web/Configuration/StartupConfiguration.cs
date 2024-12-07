using Microsoft.Extensions.DependencyInjection;
using EMRNext.Core.Configuration;

namespace EMRNext.Web.Configuration
{
    /// <summary>
    /// Central configuration for application startup and dependency injection
    /// </summary>
    public static class StartupConfiguration
    {
        /// <summary>
        /// Configure all core services for the application
        /// </summary>
        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services)
        {
            // Add core services
            services
                .AddReportingServices()
                .AddPredictiveAnalytics()
                .AddCachingServices()
                .AddInteroperabilityServices();

            return services;
        }

        /// <summary>
        /// Add reporting-related services
        /// </summary>
        private static IServiceCollection AddReportingServices(this IServiceCollection services)
        {
            // Register reporting services
            services.AddScoped<ReportingService>();
            
            return services;
        }

        /// <summary>
        /// Add caching services
        /// </summary>
        private static IServiceCollection AddCachingServices(this IServiceCollection services)
        {
            // Configure distributed caching
            services.AddDistributedMemoryCache();
            
            return services;
        }

        /// <summary>
        /// Add interoperability services
        /// </summary>
        private static IServiceCollection AddInteroperabilityServices(this IServiceCollection services)
        {
            // Register FHIR conversion services
            services.AddScoped<FhirConversionService>();
            
            return services;
        }
    }
}
