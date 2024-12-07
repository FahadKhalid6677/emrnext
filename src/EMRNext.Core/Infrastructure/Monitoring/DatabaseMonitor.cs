using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EMRNext.Core.Infrastructure.Monitoring
{
    public class DatabaseMonitor
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseMonitor> _logger;
        private readonly Timer _monitorTimer;
        private readonly Dictionary<string, long> _tableRowCounts;
        private readonly Dictionary<string, long> _tableSizes;

        public DatabaseMonitor(
            ApplicationDbContext context,
            ILogger<DatabaseMonitor> logger)
        {
            _context = context;
            _logger = logger;
            _tableRowCounts = new Dictionary<string, long>();
            _tableSizes = new Dictionary<string, long>();

            // Start monitoring every 30 minutes
            _monitorTimer = new Timer(
                async _ => await MonitorDatabaseMetricsAsync(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(30));
        }

        private async Task MonitorDatabaseMetricsAsync()
        {
            try
            {
                await MonitorTableMetricsAsync();
                await MonitorIndexUsageAsync();
                await MonitorQueryPerformanceAsync();
                await MonitorConnectionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring database metrics");
            }
        }

        private async Task MonitorTableMetricsAsync()
        {
            var sql = @"
                SELECT 
                    schemaname || '.' || tablename as table_name,
                    n_live_tup as row_count,
                    pg_total_relation_size(schemaname || '.' || tablename) as total_bytes
                FROM pg_stat_user_tables
                WHERE schemaname = 'public'
                  AND tablename IN ('GrowthStandards', 'PercentileData', 'PatientMeasurements', 'GrowthAlerts');";

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            await _context.Database.OpenConnectionAsync();

            try
            {
                using var result = await command.ExecuteReaderAsync();
                while (await result.ReadAsync())
                {
                    var tableName = result.GetString(0);
                    var rowCount = result.GetInt64(1);
                    var totalBytes = result.GetInt64(2);

                    _tableRowCounts[tableName] = rowCount;
                    _tableSizes[tableName] = totalBytes;

                    _logger.LogInformation(
                        "Table {TableName} - Rows: {RowCount}, Size: {SizeInMB:F2}MB",
                        tableName,
                        rowCount,
                        totalBytes / (1024.0 * 1024.0));
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        private async Task MonitorIndexUsageAsync()
        {
            var sql = @"
                SELECT 
                    schemaname || '.' || tablename as table_name,
                    indexrelname as index_name,
                    idx_scan as index_scans,
                    idx_tup_read as tuples_read,
                    idx_tup_fetch as tuples_fetched
                FROM pg_stat_user_indexes
                WHERE schemaname = 'public'
                  AND tablename IN ('GrowthStandards', 'PercentileData', 'PatientMeasurements', 'GrowthAlerts');";

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            await _context.Database.OpenConnectionAsync();

            try
            {
                using var result = await command.ExecuteReaderAsync();
                while (await result.ReadAsync())
                {
                    var tableName = result.GetString(0);
                    var indexName = result.GetString(1);
                    var indexScans = result.GetInt64(2);
                    var tuplesRead = result.GetInt64(3);
                    var tuplesFetched = result.GetInt64(4);

                    _logger.LogInformation(
                        "Index Usage - Table: {TableName}, Index: {IndexName}, " +
                        "Scans: {Scans}, Reads: {Reads}, Fetches: {Fetches}",
                        tableName,
                        indexName,
                        indexScans,
                        tuplesRead,
                        tuplesFetched);
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        private async Task MonitorQueryPerformanceAsync()
        {
            var sql = @"
                SELECT 
                    query,
                    calls,
                    total_time / calls as avg_time,
                    rows / calls as avg_rows
                FROM pg_stat_statements
                WHERE query ILIKE '%growthstandards%'
                   OR query ILIKE '%percentiledata%'
                   OR query ILIKE '%patientmeasurements%'
                   OR query ILIKE '%growthalerts%'
                ORDER BY total_time DESC
                LIMIT 10;";

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            await _context.Database.OpenConnectionAsync();

            try
            {
                using var result = await command.ExecuteReaderAsync();
                while (await result.ReadAsync())
                {
                    var query = result.GetString(0);
                    var calls = result.GetInt64(1);
                    var avgTime = result.GetDouble(2);
                    var avgRows = result.GetDouble(3);

                    _logger.LogInformation(
                        "Query Performance - Calls: {Calls}, Avg Time: {AvgTime:F2}ms, " +
                        "Avg Rows: {AvgRows:F2}\nQuery: {Query}",
                        calls,
                        avgTime,
                        avgRows,
                        query);
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        private async Task MonitorConnectionsAsync()
        {
            var sql = @"
                SELECT 
                    count(*) as total_connections,
                    count(*) filter (where state = 'active') as active_connections,
                    count(*) filter (where state = 'idle') as idle_connections
                FROM pg_stat_activity
                WHERE datname = current_database();";

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            await _context.Database.OpenConnectionAsync();

            try
            {
                using var result = await command.ExecuteReaderAsync();
                if (await result.ReadAsync())
                {
                    var totalConnections = result.GetInt64(0);
                    var activeConnections = result.GetInt64(1);
                    var idleConnections = result.GetInt64(2);

                    _logger.LogInformation(
                        "Database Connections - Total: {Total}, Active: {Active}, Idle: {Idle}",
                        totalConnections,
                        activeConnections,
                        idleConnections);
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public void Dispose()
        {
            _monitorTimer?.Dispose();
        }
    }
}
