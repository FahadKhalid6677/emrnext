using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Logging;

namespace EMRNext.Web.Configuration
{
    public static class MonitoringConfig
    {
        public static IServiceCollection AddMonitoringConfiguration(this IServiceCollection services)
        {
            // Configure OpenTelemetry
            services.AddOpenTelemetry()
                .ConfigureResource(builder => builder
                    .AddService("EMRNext")
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector())
                .WithTracing(builder => builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddRedisInstrumentation()
                    .AddOtlpExporter(opts => opts.Endpoint = new Uri("http://localhost:4317")))
                .WithMetrics(builder => builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(opts => opts.Endpoint = new Uri("http://localhost:4317")));

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .WriteTo.Console()
                .WriteTo.File("logs/emrnext-.log", 
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30)
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "emrnext-logs-{0:yyyy.MM}",
                    BatchAction = ElasticOpType.Create
                })
                .CreateLogger();

            services.AddLogging(loggingBuilder =>
                loggingBuilder.ClearProviders()
                            .AddSerilog(dispose: true));

            // Add Health Checks
            services.AddHealthChecks()
                .AddDbContextCheck<EMRNextContext>()
                .AddRedis("localhost:6379")
                .AddUrlGroup(new Uri("https://api.emrnext.com/health"), "API Health")
                .AddCheck<StorageHealthCheck>("Storage");

            // Register monitoring services
            services.AddSingleton<IMetricsService, MetricsService>();
            services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();

            return services;
        }
    }

    public interface IMetricsService
    {
        void RecordApiCall(string endpoint, int statusCode, TimeSpan duration);
        void RecordDatabaseQuery(string operation, TimeSpan duration);
        void RecordCacheOperation(string operation, bool hit);
        Task<PerformanceMetrics> GetCurrentMetrics();
    }

    public class MetricsService : IMetricsService
    {
        private readonly Meter _meter;
        private readonly Counter<long> _apiCallsCounter;
        private readonly Histogram<double> _apiLatencyHistogram;
        private readonly Counter<long> _databaseQueriesCounter;
        private readonly Histogram<double> _queryLatencyHistogram;
        private readonly Counter<long> _cacheHitsCounter;
        private readonly Counter<long> _cacheMissesCounter;

        public MetricsService()
        {
            _meter = new Meter("EMRNext.Metrics");
            
            _apiCallsCounter = _meter.CreateCounter<long>("api.calls.total");
            _apiLatencyHistogram = _meter.CreateHistogram<double>("api.latency");
            _databaseQueriesCounter = _meter.CreateCounter<long>("database.queries.total");
            _queryLatencyHistogram = _meter.CreateHistogram<double>("database.query.latency");
            _cacheHitsCounter = _meter.CreateCounter<long>("cache.hits");
            _cacheMissesCounter = _meter.CreateCounter<long>("cache.misses");
        }

        public void RecordApiCall(string endpoint, int statusCode, TimeSpan duration)
        {
            _apiCallsCounter.Add(1, new KeyValuePair<string, object>[]
            {
                new("endpoint", endpoint),
                new("status_code", statusCode)
            });
            _apiLatencyHistogram.Record(duration.TotalMilliseconds);
        }

        public void RecordDatabaseQuery(string operation, TimeSpan duration)
        {
            _databaseQueriesCounter.Add(1, new KeyValuePair<string, object>[]
            {
                new("operation", operation)
            });
            _queryLatencyHistogram.Record(duration.TotalMilliseconds);
        }

        public void RecordCacheOperation(string operation, bool hit)
        {
            if (hit)
                _cacheHitsCounter.Add(1);
            else
                _cacheMissesCounter.Add(1);
        }

        public async Task<PerformanceMetrics> GetCurrentMetrics()
        {
            // Implement metrics aggregation logic
            return new PerformanceMetrics();
        }
    }

    public class PerformanceMetrics
    {
        public long TotalApiCalls { get; set; }
        public double AverageApiLatency { get; set; }
        public long TotalDatabaseQueries { get; set; }
        public double AverageQueryLatency { get; set; }
        public double CacheHitRate { get; set; }
    }

    public interface IPerformanceMonitor
    {
        void StartOperation(string operationName);
        void EndOperation(string operationName);
        OperationMetrics GetOperationMetrics(string operationName);
    }

    public class PerformanceMonitor : IPerformanceMonitor
    {
        private readonly ConcurrentDictionary<string, OperationMetrics> _metrics = new();

        public void StartOperation(string operationName)
        {
            var metrics = _metrics.GetOrAdd(operationName, _ => new OperationMetrics());
            metrics.StartNewOperation();
        }

        public void EndOperation(string operationName)
        {
            if (_metrics.TryGetValue(operationName, out var metrics))
            {
                metrics.EndOperation();
            }
        }

        public OperationMetrics GetOperationMetrics(string operationName)
        {
            return _metrics.GetOrAdd(operationName, _ => new OperationMetrics());
        }
    }

    public class OperationMetrics
    {
        private long _totalOperations;
        private long _totalDuration;
        private long _currentOperations;
        private readonly Stopwatch _currentOperation = new();

        public void StartNewOperation()
        {
            Interlocked.Increment(ref _currentOperations);
            _currentOperation.Restart();
        }

        public void EndOperation()
        {
            _currentOperation.Stop();
            Interlocked.Increment(ref _totalOperations);
            Interlocked.Add(ref _totalDuration, _currentOperation.ElapsedMilliseconds);
            Interlocked.Decrement(ref _currentOperations);
        }

        public double AverageDuration => _totalOperations > 0 ? (double)_totalDuration / _totalOperations : 0;
        public long TotalOperations => _totalOperations;
        public long CurrentOperations => _currentOperations;
    }
}
