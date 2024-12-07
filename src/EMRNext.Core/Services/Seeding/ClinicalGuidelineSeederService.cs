using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Repositories;

namespace EMRNext.Core.Services.Seeding
{
    /// <summary>
    /// Background service for seeding clinical guidelines during application startup
    /// </summary>
    public class ClinicalGuidelineSeederService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ClinicalGuidelineSeederService> _logger;

        public ClinicalGuidelineSeederService(
            IServiceProvider serviceProvider,
            ILogger<ClinicalGuidelineSeederService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try 
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var guidelineRepository = scope.ServiceProvider
                        .GetRequiredService<IClinicalGuidelineRepository>();

                    await guidelineRepository.SeedInitialGuidelinesAsync();
                    _logger.LogInformation("Clinical Guidelines Seeding Completed Successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during clinical guidelines seeding");
            }
        }
    }
}
