using EMRNext.Core.FHIR.Profiles;
using EMRNext.Core.Services;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EMRNext.Core.Configuration
{
    public static class FhirConfiguration
    {
        public static IServiceCollection AddFhirServices(this IServiceCollection services, string fhirServerUrl)
        {
            // Register FHIR Client
            services.AddScoped(_ => new FhirClient(fhirServerUrl)
            {
                Settings = new FhirClientSettings
                {
                    VerifyFhirVersion = true,
                    PreferredFormat = ResourceFormat.Json,
                    PreferredReturn = Prefer.ReturnRepresentation
                }
            });

            // Register FHIR Validation Service
            services.AddScoped<IFhirValidationService, FhirValidationService>();

            // Register AutoMapper Profiles
            services.AddAutoMapper(config =>
            {
                config.AddProfile<GroupSeriesFhirProfile>();
                // Add other FHIR profiles here as needed
            });

            return services;
        }
    }
}
