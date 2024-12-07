using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EMRNext.Core.Data;
using System;

namespace EMRNext.IntegrationTests
{
    public abstract class BaseIntegrationTest : IDisposable
    {
        protected readonly WebApplicationFactory<Startup> _factory;
        protected readonly HttpClient _client;

        public BaseIntegrationTest()
        {
            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove the app's DbContext registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<EMRNextContext>));

                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Add in-memory database for testing
                        services.AddDbContext<EMRNextContext>(options =>
                        {
                            options.UseInMemoryDatabase("InMemoryTestDatabase");
                        });

                        // Ensure database is created and seeded
                        var sp = services.BuildServiceProvider();
                        using (var scope = sp.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            var db = scopedServices.GetRequiredService<EMRNextContext>();
                            db.Database.EnsureCreated();
                        }
                    });
                });

            _client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}
