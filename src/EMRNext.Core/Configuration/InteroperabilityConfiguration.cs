using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EMRNext.Core.Infrastructure.Interoperability;

namespace EMRNext.Core.Configuration
{
    public static class InteroperabilityConfiguration
    {
        public static IServiceCollection AddDataInteroperability(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Register Data Interoperability Service
            services.AddScoped<DataInteroperabilityService>();

            // Configure Default Mapping Configurations
            services.Configure<List<DataInteroperabilityService.DataMappingConfiguration>>(
                configuration.GetSection("DataMappingConfigurations")
            );

            // Add External Data Source Providers
            services.AddHttpClient("FHIRClient", client =>
            {
                client.BaseAddress = new Uri(
                    configuration["ExternalServices:FHIRServerUrl"]
                );
            });

            services.AddHttpClient("HL7V2Client", client =>
            {
                client.BaseAddress = new Uri(
                    configuration["ExternalServices:HL7V2ServerUrl"]
                );
            });

            // Configure Transformation Options
            services.AddOptions<InteroperabilityOptions>()
                .Configure(options =>
                {
                    options.EnableDetailedLogging = true;
                    options.DefaultTransformationStrategy = 
                        DataInteroperabilityService.TransformationStrategy.Normalized;
                    options.SupportedDataSources = new[]
                    {
                        DataInteroperabilityService.DataSourceType.FHIR,
                        DataInteroperabilityService.DataSourceType.HL7V2,
                        DataInteroperabilityService.DataSourceType.DICOM
                    };
                });

            return services;
        }

        // Interoperability Configuration Options
        public class InteroperabilityOptions
        {
            public bool EnableDetailedLogging { get; set; }
            public DataInteroperabilityService.TransformationStrategy DefaultTransformationStrategy { get; set; }
            public DataInteroperabilityService.DataSourceType[] SupportedDataSources { get; set; }
        }
    }
}
