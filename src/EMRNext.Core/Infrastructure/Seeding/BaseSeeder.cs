using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Infrastructure.Persistence;
using EMRNext.Core.Infrastructure.Migrations;

namespace EMRNext.Core.Infrastructure.Seeding
{
    /// <summary>
    /// Base abstract class for data seeders with common functionality
    /// </summary>
    public abstract class BaseSeeder : IDataSeeder
    {
        protected readonly ILogger<BaseSeeder> _logger;

        public BaseSeeder(ILogger<BaseSeeder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Priority of seeding (lower number = earlier execution)
        /// </summary>
        public abstract int Priority { get; }

        /// <summary>
        /// Abstract method to seed data
        /// </summary>
        public abstract Task SeedAsync(EMRNextDbContext context);

        /// <summary>
        /// Helper method to check if data already exists
        /// </summary>
        protected async Task<bool> IsDataEmptyAsync<T>(
            EMRNextDbContext context, 
            Func<IQueryable<T>, IQueryable<T>> customQuery = null) 
            where T : class
        {
            var query = context.Set<T>().AsQueryable();
            
            if (customQuery != null)
            {
                query = customQuery(query);
            }

            return await query.CountAsync() == 0;
        }

        /// <summary>
        /// Log seeding progress
        /// </summary>
        protected void LogSeedingProgress(string entityType, int count)
        {
            _logger.LogInformation($"Seeded {count} {entityType} records.");
        }
    }
}
