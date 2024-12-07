using BenchmarkDotNet.Attributes;
using System.Net.Http;
using System.Threading.Tasks;
using EMRNext.Core.Services;

namespace EMRNext.PerformanceTests
{
    [MemoryDiagnoser]
    public class PerformanceBenchmarks
    {
        private readonly HttpClient _httpClient;
        private readonly PatientService _patientService;

        public PerformanceBenchmarks()
        {
            _httpClient = new HttpClient();
            _patientService = new PatientService();
        }

        [Benchmark]
        public async Task PatientRegistrationPerformance()
        {
            var patient = new PatientRegistrationModel
            {
                FirstName = "Performance",
                LastName = "Test",
                Email = $"perf-{Guid.NewGuid()}@emrnext.com"
            };

            await _patientService.RegisterPatient(patient);
        }

        [Benchmark]
        public async Task APIResponseTimeTest()
        {
            var response = await _httpClient.GetAsync("https://emrnext.railway.app/api/health");
            response.EnsureSuccessStatusCode();
        }

        [Benchmark]
        public void DatabaseQueryPerformance()
        {
            var patients = _patientService.GetPatientsWithinDateRange(
                DateTime.Now.AddMonths(-1), 
                DateTime.Now
            );
        }

        [Benchmark]
        public async Task AuthenticationPerformance()
        {
            var loginRequest = new LoginModel
            {
                Email = "test@emrnext.com",
                Password = "TestPassword123!"
            };

            await _httpClient.PostAsJsonAsync(
                "https://emrnext.railway.app/api/auth/login", 
                loginRequest
            );
        }
    }

    // Supporting models for benchmarks
    public class PatientRegistrationModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
