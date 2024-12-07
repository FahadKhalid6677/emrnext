using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EMRNext.Infrastructure.Configuration;

namespace EMRNext.Infrastructure.Tests.Integration
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly IConfiguration Configuration;
        protected readonly HttpClient HttpClient;

        protected IntegrationTestBase()
        {
            // Build configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.integration.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            // Setup service collection
            var services = new ServiceCollection();
            
            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(Configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.AddDebug();
            });

            // Add configuration
            services.AddSingleton(Configuration);

            // Add external services
            services.AddExternalServices(Configuration);

            // Add HTTP client
            services.AddHttpClient();

            // Build service provider
            ServiceProvider = services.BuildServiceProvider();
            HttpClient = ServiceProvider.GetRequiredService<HttpClient>();
        }

        public void Dispose()
        {
            if (HttpClient != null)
            {
                HttpClient.Dispose();
            }

            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        protected T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}
