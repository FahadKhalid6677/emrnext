using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using System.Collections.Generic;

namespace EMRNext.Core.Monitoring
{
    public interface IMonitoringService
    {
        void RecordMetric(string name, double value, params KeyValuePair<string, object>[] tags);
        void StartOperation(string operationName);
        void EndOperation(string operationName, bool success);
        Task AlertAsync(AlertLevel level, string message, Exception exception = null);
    }

    public class MonitoringService : IMonitoringService
    {
        private readonly ILogger<MonitoringService> _logger;
        private readonly Meter _meter;
        private readonly Dictionary<string, DateTime> _operationTimings;

        public MonitoringService(ILogger<MonitoringService> logger)
        {
            _logger = logger;
            _meter = new Meter("EMRNext.Metrics", "1.0.0");
            _operationTimings = new Dictionary<string, DateTime>();
        }

        public void RecordMetric(string name, double value, params KeyValuePair<string, object>[] tags)
        {
            try
            {
                var counter = _meter.CreateCounter<double>(name);
                counter.Add(value, tags);
                
                _logger.LogInformation("Metric recorded: {Name} = {Value}", name, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording metric {Name}", name);
            }
        }

        public void StartOperation(string operationName)
        {
            _operationTimings[operationName] = DateTime.UtcNow;
            _logger.LogInformation("Operation started: {OperationName}", operationName);
        }

        public void EndOperation(string operationName, bool success)
        {
            if (_operationTimings.TryGetValue(operationName, out DateTime startTime))
            {
                var duration = DateTime.UtcNow - startTime;
                _operationTimings.Remove(operationName);

                RecordMetric(
                    $"{operationName}.duration",
                    duration.TotalMilliseconds,
                    new KeyValuePair<string, object>("success", success)
                );

                _logger.LogInformation(
                    "Operation completed: {OperationName}, Duration: {Duration}ms, Success: {Success}",
                    operationName,
                    duration.TotalMilliseconds,
                    success
                );
            }
        }

        public async Task AlertAsync(AlertLevel level, string message, Exception exception = null)
        {
            var logMessage = $"Alert [{level}]: {message}";
            
            switch (level)
            {
                case AlertLevel.Critical:
                    _logger.LogCritical(exception, logMessage);
                    // Send immediate notification
                    await SendNotificationAsync(level, message, exception);
                    break;
                
                case AlertLevel.Error:
                    _logger.LogError(exception, logMessage);
                    await SendNotificationAsync(level, message, exception);
                    break;
                
                case AlertLevel.Warning:
                    _logger.LogWarning(logMessage);
                    // Only send notification if threshold exceeded
                    if (ShouldSendWarningNotification(message))
                    {
                        await SendNotificationAsync(level, message, exception);
                    }
                    break;
                
                case AlertLevel.Info:
                    _logger.LogInformation(logMessage);
                    break;
            }

            // Record alert metric
            RecordMetric(
                "alerts.count",
                1,
                new KeyValuePair<string, object>("level", level.ToString()),
                new KeyValuePair<string, object>("type", exception?.GetType().Name ?? "None")
            );
        }

        private bool ShouldSendWarningNotification(string message)
        {
            // Implement warning threshold logic
            // For example, only alert if similar warning occurs multiple times within a time window
            return true; // Placeholder implementation
        }

        private async Task SendNotificationAsync(AlertLevel level, string message, Exception exception)
        {
            // Implement notification logic (email, SMS, Slack, etc.)
            // This is a placeholder for actual implementation
            await Task.CompletedTask;
        }
    }

    public enum AlertLevel
    {
        Info,
        Warning,
        Error,
        Critical
    }
}
