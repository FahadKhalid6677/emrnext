using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace EMRNext.Core.Infrastructure.Seeding
{
    public class PerformanceMonitor
    {
        private readonly ConcurrentDictionary<string, OperationMetrics> _operationMetrics = new();
        private readonly object _lockObject = new();

        public void RecordOperation(string operationName, long elapsedMilliseconds)
        {
            _operationMetrics.AddOrUpdate(
                operationName,
                _ => new OperationMetrics(elapsedMilliseconds),
                (_, existingMetrics) => 
                {
                    existingMetrics.AddMeasurement(elapsedMilliseconds);
                    return existingMetrics;
                }
            );
        }

        public OperationMetrics GetMetrics(string operationName)
        {
            return _operationMetrics.TryGetValue(operationName, out var metrics) 
                ? metrics 
                : new OperationMetrics(0);
        }

        public IReadOnlyDictionary<string, OperationMetrics> GetAllMetrics() 
            => new Dictionary<string, OperationMetrics>(_operationMetrics);
    }

    public class OperationMetrics
    {
        public long TotalExecutions { get; private set; }
        public long TotalElapsedMilliseconds { get; private set; }
        public long MaxElapsedMilliseconds { get; private set; }
        public long MinElapsedMilliseconds { get; private set; }

        public OperationMetrics(long initialElapsedMilliseconds)
        {
            TotalExecutions = 1;
            TotalElapsedMilliseconds = initialElapsedMilliseconds;
            MaxElapsedMilliseconds = initialElapsedMilliseconds;
            MinElapsedMilliseconds = initialElapsedMilliseconds;
        }

        public void AddMeasurement(long elapsedMilliseconds)
        {
            TotalExecutions++;
            TotalElapsedMilliseconds += elapsedMilliseconds;
            MaxElapsedMilliseconds = Math.Max(MaxElapsedMilliseconds, elapsedMilliseconds);
            MinElapsedMilliseconds = Math.Min(MinElapsedMilliseconds, elapsedMilliseconds);
        }

        public double AverageElapsedMilliseconds 
            => TotalExecutions > 0 
                ? (double)TotalElapsedMilliseconds / TotalExecutions 
                : 0;
    }
}
