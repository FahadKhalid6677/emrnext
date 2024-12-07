namespace EMRNext.Core.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration for system health monitoring
    /// </summary>
    public class SystemHealthConfiguration
    {
        /// <summary>
        /// Interval between system health checks in minutes
        /// </summary>
        public int MonitoringIntervalMinutes { get; set; } = 5;

        /// <summary>
        /// CPU usage warning threshold (percentage)
        /// </summary>
        public double CpuWarningThreshold { get; set; } = 75.0;

        /// <summary>
        /// CPU usage critical threshold (percentage)
        /// </summary>
        public double CpuCriticalThreshold { get; set; } = 90.0;

        /// <summary>
        /// Memory usage warning threshold (percentage)
        /// </summary>
        public double MemoryWarningThreshold { get; set; } = 80.0;

        /// <summary>
        /// Memory usage critical threshold (percentage)
        /// </summary>
        public double MemoryCriticalThreshold { get; set; } = 95.0;

        /// <summary>
        /// Disk usage warning threshold (percentage)
        /// </summary>
        public double DiskWarningThreshold { get; set; } = 85.0;

        /// <summary>
        /// Disk usage critical threshold (percentage)
        /// </summary>
        public double DiskCriticalThreshold { get; set; } = 95.0;

        /// <summary>
        /// Maximum number of recent metrics to retain
        /// </summary>
        public int MaxMetricsRetentionCount { get; set; } = 1000;

        /// <summary>
        /// Metrics retention period in days
        /// </summary>
        public int MetricsRetentionDays { get; set; } = 30;

        /// <summary>
        /// Enable or disable system health alerts
        /// </summary>
        public bool EnableAlerts { get; set; } = true;

        /// <summary>
        /// Alert notification channels
        /// </summary>
        public List<AlertChannel> AlertChannels { get; set; } = new List<AlertChannel>
        {
            AlertChannel.Email,
            AlertChannel.SystemLog
        };
    }

    /// <summary>
    /// Defines alert notification channels
    /// </summary>
    public enum AlertChannel
    {
        /// <summary>
        /// Email notifications
        /// </summary>
        Email,

        /// <summary>
        /// System log notifications
        /// </summary>
        SystemLog,

        /// <summary>
        /// SMS notifications
        /// </summary>
        SMS,

        /// <summary>
        /// Slack channel notifications
        /// </summary>
        Slack
    }
}
