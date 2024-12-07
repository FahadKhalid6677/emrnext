using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Castle.DynamicProxy;

namespace EMRNext.Core.Performance.Interceptors
{
    /// <summary>
    /// Performance interceptor for method execution tracking
    /// </summary>
    public class PerformanceInterceptor : IInterceptor
    {
        private readonly ILogger<PerformanceInterceptor> _logger;

        public PerformanceInterceptor(ILogger<PerformanceInterceptor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Intercept(IInvocation invocation)
        {
            // Skip intercepting certain methods or types
            if (ShouldSkipInterception(invocation))
            {
                invocation.Proceed();
                return;
            }

            // Measure performance
            var stopwatch = Stopwatch.StartNew();

            try 
            {
                // Proceed with method execution
                invocation.Proceed();

                stopwatch.Stop();
                LogPerformance(invocation, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogException(invocation, ex, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Determine if method should be skipped for performance tracking
        /// </summary>
        private bool ShouldSkipInterception(IInvocation invocation)
        {
            // Skip logging for certain methods or types
            var skipAttributes = new[]
            {
                typeof(SkipPerformanceTrackingAttribute)
            };

            return invocation.Method.GetCustomAttributes(true)
                .Any(attr => skipAttributes.Contains(attr.GetType()));
        }

        /// <summary>
        /// Log performance metrics
        /// </summary>
        private void LogPerformance(IInvocation invocation, long elapsedMilliseconds)
        {
            // Performance threshold for logging (configurable)
            const long performanceThresholdMs = 500;

            var methodName = $"{invocation.TargetType.Name}.{invocation.Method.Name}";
            var logLevel = elapsedMilliseconds > performanceThresholdMs 
                ? LogLevel.Warning 
                : LogLevel.Information;

            _logger.Log(logLevel, 
                "Method: {MethodName}, " +
                "Execution Time: {ElapsedMs}ms, " +
                "Performance: {PerformanceStatus}", 
                methodName, 
                elapsedMilliseconds,
                elapsedMilliseconds > performanceThresholdMs ? "SLOW" : "NORMAL"
            );
        }

        /// <summary>
        /// Log exceptions with performance context
        /// </summary>
        private void LogException(IInvocation invocation, Exception ex, long elapsedMilliseconds)
        {
            var methodName = $"{invocation.TargetType.Name}.{invocation.Method.Name}";

            _logger.LogError(ex, 
                "Method: {MethodName}, " +
                "Execution Time: {ElapsedMs}ms, " +
                "Exception: {ExceptionMessage}", 
                methodName, 
                elapsedMilliseconds, 
                ex.Message
            );
        }
    }

    /// <summary>
    /// Attribute to skip performance tracking for specific methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class SkipPerformanceTrackingAttribute : Attribute
    {
    }

    /// <summary>
    /// Performance tracking configuration
    /// </summary>
    public class PerformanceTracking
    {
        /// <summary>
        /// Configure performance tracking for services
        /// </summary>
        public static void ConfigureInterceptors(IServiceCollection services)
        {
            services.AddSingleton<PerformanceInterceptor>();

            services.Decorate<IPatientService>((inner, provider) =>
            {
                var interceptor = provider.GetRequiredService<PerformanceInterceptor>();
                var proxyGenerator = new ProxyGenerator();
                
                return proxyGenerator.CreateInterfaceProxyWithTarget(
                    typeof(IPatientService), 
                    inner, 
                    interceptor
                ) as IPatientService;
            });

            // Add similar decorations for other critical services
        }
    }
}
