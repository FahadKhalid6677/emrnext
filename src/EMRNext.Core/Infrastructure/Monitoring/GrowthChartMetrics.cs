using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace EMRNext.Core.Infrastructure.Monitoring
{
    public class GrowthChartMetrics : IDisposable
    {
        private readonly Meter _meter;
        private readonly Counter<long> _cacheHits;
        private readonly Counter<long> _cacheMisses;
        private readonly Histogram<double> _queryDuration;
        private readonly Histogram<double> _cacheSize;
        private readonly ILogger<GrowthChartMetrics> _logger;
        private readonly ConcurrentDictionary<string, long> _operationCounts;
        private readonly Timer _metricsTimer;

        public GrowthChartMetrics(ILogger<GrowthChartMetrics> logger)
        {
            _logger = logger;
            _meter = new Meter("EMRNext.GrowthChart");
            _operationCounts = new ConcurrentDictionary<string, long>();

            // Cache metrics
            _cacheHits = _meter.CreateCounter<long>("growth.cache.hits", "hits", "Number of cache hits");
            _cacheMisses = _meter.CreateCounter<long>("growth.cache.misses", "misses", "Number of cache misses");
            _cacheSize = _meter.CreateHistogram<double>("growth.cache.size", "bytes", "Cache size in bytes");

            // Performance metrics
            _queryDuration = _meter.CreateHistogram<double>("growth.query.duration", "ms", "Query duration in milliseconds");

            // Start periodic metrics logging
            _metricsTimer = new Timer(LogMetrics, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        public void RecordCacheHit(string cacheKey)
        {
            _cacheHits.Add(1);
            _operationCounts.AddOrUpdate("cache_hits", 1, (_, count) => count + 1);
            _logger.LogTrace("Cache hit for key: {CacheKey}", cacheKey);
        }

        public void RecordCacheMiss(string cacheKey)
        {
            _cacheMisses.Add(1);
            _operationCounts.AddOrUpdate("cache_misses", 1, (_, count) => count + 1);
            _logger.LogTrace("Cache miss for key: {CacheKey}", cacheKey);
        }

        public void RecordCacheSize(long sizeInBytes)
        {
            _cacheSize.Record(sizeInBytes);
            _logger.LogTrace("Cache size: {SizeInBytes} bytes", sizeInBytes);
        }

        public void RecordQueryDuration(string queryName, double milliseconds)
        {
            _queryDuration.Record(milliseconds, new KeyValuePair<string, object>("query_name", queryName));
            _logger.LogTrace("Query {QueryName} took {Duration}ms", queryName, milliseconds);
        }

        public void RecordOperation(string operation)
        {
            _operationCounts.AddOrUpdate(operation, 1, (_, count) => count + 1);
        }

        private void LogMetrics(object state)
        {
            var hitCount = _operationCounts.GetOrAdd("cache_hits", 0);
            var missCount = _operationCounts.GetOrAdd("cache_misses", 0);
            var hitRatio = hitCount + missCount > 0 
                ? (double)hitCount / (hitCount + missCount) 
                : 0;

            _logger.LogInformation(
                "Growth Chart Metrics - Cache Hit Ratio: {HitRatio:P2}, " +
                "Total Operations: {TotalOps}, " +
                "Cache Hits: {Hits}, " +
                "Cache Misses: {Misses}",
                hitRatio,
                hitCount + missCount,
                hitCount,
                missCount);

            foreach (var operation in _operationCounts)
            {
                if (operation.Key != "cache_hits" && operation.Key != "cache_misses")
                {
                    _logger.LogInformation(
                        "Operation Count - {Operation}: {Count}",
                        operation.Key,
                        operation.Value);
                }
            }
        }

        public void Dispose()
        {
            _metricsTimer?.Dispose();
            _meter?.Dispose();
        }
    }
}
