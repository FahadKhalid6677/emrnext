using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EMRNext.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace EMRNext.Infrastructure.Migrations
{
    public class DatabaseMigrationManager
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseMigrationManager> _logger;

        public DatabaseMigrationManager(
            ApplicationDbContext context, 
            ILogger<DatabaseMigrationManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task MigrateAsync()
        {
            try 
            {
                _logger.LogInformation("Starting database migration...");
                
                // Ensure database is created and all pending migrations are applied
                await _context.Database.MigrateAsync();
                
                // Optional: Seed initial data if needed
                await SeedDataAsync();

                _logger.LogInformation("Database migration completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during database migration.");
                throw;
            }
        }

        private async Task SeedDataAsync()
        {
            // Implement data seeding logic
            var dataSeeder = new DataSeeder(_context);
            await dataSeeder.SeedAsync();
        }
    }
}
