namespace EMRNext.Infrastructure.Data
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string Provider { get; set; }
        public int CommandTimeout { get; set; } = 30;
        public bool EnableDetailedErrors { get; set; }
        public bool EnableSensitiveDataLogging { get; set; }
        public int MaxRetryCount { get; set; } = 3;
        public int MaxRetryDelay { get; set; } = 30;
        public string MigrationsAssembly { get; set; }
        public bool AutoMigrate { get; set; }
    }
}
