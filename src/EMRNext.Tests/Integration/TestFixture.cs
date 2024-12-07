using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EMRNext.Core.Services;
using EMRNext.Infrastructure;

namespace EMRNext.Tests.Integration
{
    public class TestFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; }

        public TestFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            // Add configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Add services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IDocumentationService, DocumentationService>();
            services.AddScoped<IOrderService, OrderService>();

            // Add DbContext
            services.AddDbContext<EMRNextContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            ServiceProvider = services.BuildServiceProvider();
        }

        public void Dispose()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
