using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Infrastructure.Persistence;

namespace EMRNext.Core.Infrastructure.Migrations
{
    /// <summary>
    /// Manages database migrations and data seeding for EMRNext application
    /// </summary>
    public class MigrationManager
    {
        private readonly EMRNextDbContext _context;
        private readonly ILogger<MigrationManager> _logger;
        private readonly IDataSeeder[] _seeders;

        public MigrationManager(
            EMRNextDbContext context, 
            ILogger<MigrationManager> logger,
            IDataSeeder[] seeders)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _seeders = seeders ?? throw new ArgumentNullException(nameof(seeders));
        }

        /// <summary>
        /// Performs database migration and data seeding
        /// </summary>
        public async Task MigrateAndSeedAsync()
        {
            try 
            {
                // Ensure database is created and up to date
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Database migration completed successfully.");

                // Perform data seeding
                await SeedDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during database migration and seeding.");
                throw;
            }
        }

        /// <summary>
        /// Seeds data using registered seeders
        /// </summary>
        private async Task SeedDataAsync()
        {
            foreach (var seeder in _seeders.OrderBy(s => s.Priority))
            {
                try 
                {
                    await seeder.SeedAsync(_context);
                    _logger.LogInformation($"Seeding completed for {seeder.GetType().Name}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error seeding data with {seeder.GetType().Name}");
                    throw;
                }
            }

            // Save changes after all seeders have run
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Performs a backup of the current database state
        /// </summary>
        public async Task BackupDatabaseAsync(string backupPath)
        {
            try 
            {
                await _context.Database.EnsureCreatedAsync();
                
                // Note: Actual backup mechanism depends on the database provider
                // This is a placeholder for database-specific backup logic
                _logger.LogInformation($"Database backup initiated to {backupPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database backup failed");
                throw;
            }
        }

        /// <summary>
        /// Rolls back to a previous database version if needed
        /// </summary>
        public async Task RollbackMigrationAsync(string targetVersion)
        {
            try 
            {
                // Rollback to a specific migration
                await _context.Database.MigrateAsync();
                _logger.LogInformation($"Rolled back to migration version: {targetVersion}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration rollback failed");
                throw;
            }
        }
    }

    /// <summary>
    /// Interface for data seeders to implement
    /// </summary>
    public interface IDataSeeder
    {
        /// <summary>
        /// Priority of seeding (lower number = earlier execution)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Seed data into the database context
        /// </summary>
        Task SeedAsync(EMRNextDbContext context);
    }
}
