using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EMRNext.Infrastructure.Data;
using EMRNext.Core.Services.Portal;
using EMRNext.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace EMRNext.IntegrationTests
{
    public class TestDatabaseFixture : IDisposable
    {
        private const string ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=EMRNextIntegrationTests;Trusted_Connection=True;MultipleActiveResultSets=true";
        
        public TestDatabaseFixture()
        {
            var services = new ServiceCollection();

            // Add DbContext
            services.AddDbContext<EMRNextDbContext>(options =>
                options.UseSqlServer(ConnectionString));

            // Add required services
            services.AddScoped<ILogger<GroupSeriesService>>(provider => 
                provider.GetRequiredService<ILoggerFactory>().CreateLogger<GroupSeriesService>());
            services.AddScoped<IResourceManagementService, ResourceManagementService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<GroupSeriesService>();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
            });

            ServiceProvider = services.BuildServiceProvider();

            // Ensure database is created
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EMRNextDbContext>();
            context.Database.EnsureCreated();
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EMRNextDbContext>();
            context.Database.EnsureDeleted();
        }
    }
}
