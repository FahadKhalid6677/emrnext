using Microsoft.AspNetCore.Mvc;

namespace EMRNext.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public HealthController(
            ILogger<HealthController> logger, 
            ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try 
            {
                // Check database connection
                var canConnect = _dbContext.Database.CanConnect();
                
                _logger.LogInformation($"Health check performed. Database connection: {canConnect}");

                return canConnect 
                    ? Ok(new { 
                        Status = "Healthy", 
                        DatabaseConnection = true,
                        Timestamp = DateTime.UtcNow 
                    }) 
                    : StatusCode(503, new { 
                        Status = "Unhealthy", 
                        DatabaseConnection = false 
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new { 
                    Status = "Unhealthy", 
                    Error = ex.Message 
                });
            }
        }
    }
}
