using Microsoft.Extensions.DependencyInjection;
using EMRNext.Core.Infrastructure.Reporting;
using EMRNext.Core.Services.Reporting;

namespace EMRNext.Core.Configuration
{
    public static class ReportingConfiguration
    {
        public static IServiceCollection AddReportingServices(
            this IServiceCollection services)
        {
            // Register Reporting Engine Components
            services.AddScoped<ReportingEngine.ReportingService>();
            
            // Register Data Sources
            services.AddScoped<ReportingEngine.IReportDataSource, ClinicalReportService>();
            
            // Register Filters
            services.AddScoped<ReportingEngine.IReportFilter, ClinicalReportService>();
            
            // Register Aggregators
            services.AddScoped<ReportingEngine.IReportAggregator, ClinicalReportService>();
            
            // Register Visualization Service
            services.AddScoped<ReportVisualizationService>();

            // Configure Default Reporting Settings
            services.AddOptions<ReportingEngine.ReportConfiguration>()
                .Configure(config =>
                {
                    config.ReportId = "DEFAULT_REPORT";
                    config.ReportName = "Default System Report";
                    config.Type = ReportingEngine.ReportType.Standard;
                });

            return services;
        }
    }
}
