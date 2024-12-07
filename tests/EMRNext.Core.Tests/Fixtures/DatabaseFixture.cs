using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EMRNext.Core.Infrastructure.Persistence;
using EMRNext.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace EMRNext.Core.Tests.Fixtures
{
    public class DatabaseFixture : IDisposable
    {
        public ServiceProvider ServiceProvider { get; private set; }
        public ApplicationDbContext Context { get; private set; }

        public DatabaseFixture()
        {
            // Create a new service collection
            var services = new ServiceCollection();

            // Create configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Encryption:Key", Convert.ToBase64String(GenerateTestKey()))
                })
                .Build();

            // Configure in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName: $"EMRNextTestDb_{Guid.NewGuid()}");
            });

            // Add configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Register dependencies
            EhrConfiguration.ConfigureServices(services);

            // Build service provider
            ServiceProvider = services.BuildServiceProvider();

            // Create database context
            Context = ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            // Clean up the database after tests
            Context.Database.EnsureDeleted();
            Context.Dispose();
            ServiceProvider.Dispose();
        }

        /// <summary>
        /// Generates a test encryption key
        /// </summary>
        private byte[] GenerateTestKey()
        {
            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                aes.GenerateKey();
                return aes.Key;
            }
        }
    }
}
