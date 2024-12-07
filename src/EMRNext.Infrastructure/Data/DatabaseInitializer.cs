using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EMRNext.Infrastructure.Data
{
    public class DatabaseInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitializer> _logger;
        private readonly DatabaseSettings _settings;

        public DatabaseInitializer(
            IServiceProvider serviceProvider,
            ILogger<DatabaseInitializer> logger,
            DatabaseSettings settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (_settings.AutoMigrate)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    _logger.LogInformation("Starting database migration...");
                    await context.Database.MigrateAsync();
                    _logger.LogInformation("Database migration completed successfully");

                    await SeedInitialDataAsync(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
        }

        private async Task SeedInitialDataAsync(ApplicationDbContext context)
        {
            try
            {
                _logger.LogInformation("Starting initial data seeding...");

                // Add any initial data seeding here
                if (!await context.Roles.AnyAsync())
                {
                    context.Roles.AddRange(
                        new Core.Domain.Entities.Role { Name = "Admin", Description = "System Administrator" },
                        new Core.Domain.Entities.Role { Name = "Doctor", Description = "Medical Doctor" },
                        new Core.Domain.Entities.Role { Name = "Nurse", Description = "Medical Nurse" },
                        new Core.Domain.Entities.Role { Name = "Patient", Description = "Patient User" }
                    );
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Initial data seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding initial data");
                throw;
            }
        }
    }
}
