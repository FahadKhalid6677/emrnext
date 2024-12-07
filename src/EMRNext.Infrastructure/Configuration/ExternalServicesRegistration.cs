using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EMRNext.Infrastructure.Services.External;

namespace EMRNext.Infrastructure.Configuration
{
    public static class ExternalServicesRegistration
    {
        public static IServiceCollection AddExternalServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind configuration
            services.Configure<ExternalServicesConfiguration>(
                configuration.GetSection("ExternalServices"));

            // Register HTTP clients
            services.AddHttpClient<IDrugDatabaseService, DrugDatabaseService>()
                .ConfigureHttpClient((sp, client) =>
                {
                    // Configure default headers, timeouts, etc.
                    client.Timeout = TimeSpan.FromSeconds(30);
                });

            services.AddHttpClient<IPaymentGatewayService, PaymentGatewayService>()
                .ConfigureHttpClient((sp, client) =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                });

            services.AddHttpClient<IInsuranceVerificationService, InsuranceVerificationService>()
                .ConfigureHttpClient((sp, client) =>
                {
                    client.Timeout = TimeSpan.FromSeconds(45);
                });

            services.AddHttpClient<ILabInterfaceService, LabInterfaceService>()
                .ConfigureHttpClient((sp, client) =>
                {
                    client.Timeout = TimeSpan.FromMinutes(2);
                });

            services.AddHttpClient<ITelehealthService, TelehealthService>()
                .ConfigureHttpClient((sp, client) =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                });

            // Register services
            services.AddScoped<IDrugDatabaseService, DrugDatabaseService>();
            services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
            services.AddScoped<IInsuranceVerificationService, InsuranceVerificationService>();
            services.AddScoped<ILabInterfaceService, LabInterfaceService>();
            services.AddScoped<ITelehealthService, TelehealthService>();

            // Add memory cache for caching responses
            services.AddMemoryCache();

            return services;
        }
    }
}
