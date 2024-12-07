using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using EMRNext.Infrastructure.Data;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Services;
using EMRNext.Infrastructure.Data.Repository;
using EMRNext.Infrastructure.Security;
using EMRNext.Infrastructure.Monitoring;

namespace EMRNext.IntegrationTests
{
    public class TestFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; }

        public TestFixture()
        {
            var services = new ServiceCollection();

            // Add DbContext
            services.AddDbContext<EMRDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            // Add Repositories
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IClinicalRepository, ClinicalRepository>();
            services.AddScoped<ISchedulingRepository, SchedulingRepository>();
            services.AddScoped<IBillingRepository, BillingRepository>();

            // Add Services
            services.AddScoped<IClinicalService, ClinicalService>();
            services.AddScoped<ISchedulingService, SchedulingService>();
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<IMonitoringService, MonitoringService>();

            // Add Logging
            services.AddLogging();

            ServiceProvider = services.BuildServiceProvider();

            // Initialize database
            using var scope = ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EMRDbContext>();
            dbContext.Database.EnsureCreated();
            SeedTestData(dbContext);
        }

        private void SeedTestData(EMRDbContext context)
        {
            // Add a test provider
            var provider = new Provider
            {
                FirstName = "Test",
                LastName = "Provider",
                Specialty = "Family Medicine",
                Status = ProviderStatus.Active
            };
            context.Providers.Add(provider);

            // Add test resources
            var resource = new Resource
            {
                ResourceName = "Exam Room 1",
                ResourceType = "Room",
                Location = "Main Clinic",
                Capacity = 1,
                Status = ResourceStatus.Available
            };
            context.Resources.Add(resource);

            context.SaveChanges();
        }

        public void Dispose()
        {
            using var scope = ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EMRDbContext>();
            dbContext.Database.EnsureDeleted();
        }
    }
}
