using Microsoft.Extensions.DependencyInjection;
using EMRNext.Core.Reporting.Services;

namespace EMRNext.Core.Configuration
{
    /// <summary>
    /// Configuration for predictive analytics services
    /// </summary>
    public static class PredictiveAnalyticsConfiguration
    {
        /// <summary>
        /// Add predictive analytics services to the dependency injection container
        /// </summary>
        public static IServiceCollection AddPredictiveAnalytics(this IServiceCollection services)
        {
            // Register predictive analytics service
            services.AddScoped<PredictiveAnalyticsService>();

            return services;
        }
    }
}
