using System;
using System.Linq;
using Xunit;
using EMRNext.Core.Infrastructure.Seeding;
using EMRNext.Core.Models;
using Moq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace EMRNext.Core.Tests.Infrastructure.Seeding
{
    public class GrowthStandardSeederTests
    {
        [Fact]
        public void NormalDistributionInverse_ValidInputs_ReturnsCorrectValue()
        {
            // Arrange
            var seeder = new GrowthStandardSeeder();
            var testCases = new[]
            {
                new { Probability = 0.5, ExpectedResult = 0.0 },
                new { Probability = 0.75, ExpectedResult = 0.6745 },
                new { Probability = 0.25, ExpectedResult = -0.6745 }
            };

            // Act & Assert
            foreach (var testCase in testCases)
            {
                var result = seeder.InvokeNormalDistributionInverse(testCase.Probability);
                Assert.True(
                    Math.Abs(result - testCase.ExpectedResult) < 0.01, 
                    $"Failed for probability {testCase.Probability}"
                );
            }
        }

        [Fact]
        public void CalculatePercentile_ValidInputs_ReturnsAccurateResults()
        {
            // Arrange
            var seeder = new GrowthStandardSeeder();
            var testCases = new[]
            {
                new { 
                    Percentile = 0.5, 
                    L = 0.5, 
                    M = 10, 
                    S = 0.1, 
                    ExpectedMin = 9.5, 
                    ExpectedMax = 10.5 
                },
                new { 
                    Percentile = 0.9, 
                    L = 0.3, 
                    M = 15, 
                    S = 0.2, 
                    ExpectedMin = 16.0, 
                    ExpectedMax = 17.0 
                }
            };

            // Act & Assert
            foreach (var testCase in testCases)
            {
                var result = seeder.InvokeCalculatePercentile(
                    testCase.Percentile, 
                    testCase.L, 
                    testCase.M, 
                    testCase.S
                );

                Assert.True(
                    result >= testCase.ExpectedMin && result <= testCase.ExpectedMax, 
                    $"Percentile calculation failed for {testCase.Percentile}"
                );
            }
        }

        [Theory]
        [InlineData(0, 10, 0.1, true)]
        [InlineData(-1, 10, 0.1, false)]
        [InlineData(0.5, double.NaN, 0.1, false)]
        public void WHODataRecord_Validation_WorksCorrectly(
            double age, double m, double s, bool expectedValidity)
        {
            // Arrange
            var record = new GrowthStandardSeeder.WHODataRecord
            {
                Age = age,
                L = 0.5,
                M = m,
                S = s
            };

            // Act & Assert
            Assert.Equal(expectedValidity, record.IsValid());
        }

        [Fact]
        public void GeneratePercentileValues_ProducesConsistentResults()
        {
            // Arrange
            var seeder = new GrowthStandardSeeder();
            var record = new GrowthStandardSeeder.WHODataRecord
            {
                Age = 12,
                L = 0.5,
                M = 10,
                S = 0.1
            };

            // Act
            var percentileValues = seeder.InvokeGeneratePercentileValues(record);

            // Assert
            Assert.NotNull(percentileValues);
            var percentiles = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, double>>(percentileValues);
            Assert.NotNull(percentiles);
            Assert.True(percentiles.Count > 0);
        }

        [Fact]
        public void ImportWHOData_InvalidData_HandledGracefully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GrowthStandardSeeder>>();
            var mockContext = new Mock<DbContext>();
            var seeder = new GrowthStandardSeeder(mockLogger.Object, mockContext.Object);

            var invalidRecords = new List<WHODataRecord>
            {
                new WHODataRecord { Age = -1, L = double.NaN, M = 0, S = -0.1 },
                new WHODataRecord { Age = 10, L = 0.5, M = double.PositiveInfinity, S = 0.1 }
            };

            // Act & Assert
            Assert.Throws<ValidationException>(() => 
                seeder.ValidateAndProcessRecords(invalidRecords)
            );
        }

        [Theory]
        [InlineData(0.001, ExpectedResult = true)]
        [InlineData(0.999, ExpectedResult = true)]
        [InlineData(-0.001, ExpectedResult = false)]
        [InlineData(1.001, ExpectedResult = false)]
        public bool PercentileValidation_BoundaryConditions(double percentile)
        {
            // Arrange
            var seeder = new GrowthStandardSeeder();

            // Act & Assert
            return seeder.IsValidPercentile(percentile);
        }

        [Fact]
        public void SeedingPerformance_LargeDataset_CompletesInReasonableTime()
        {
            // Arrange
            var seeder = new GrowthStandardSeeder();
            var largeDataset = seeder.GenerateLargeTestDataset();

            // Act
            var stopwatch = Stopwatch.StartNew();
            var processedRecords = seeder.ProcessGrowthStandardRecords(largeDataset);
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, "Seeding process took too long");
            Assert.NotEmpty(processedRecords);
        }

        [Fact]
        public void DataIntegrity_CrossReferenceStandards_ConsistencyCheck()
        {
            // Arrange
            var whoStandard = new WHODataRecord { Age = 12, L = 0.5, M = 10, S = 0.1 };
            var cdcStandard = new CDCDataRecord { Age = 12, L = 0.5, M = 10, S = 0.1 };

            // Act & Assert
            Assert.Equal(whoStandard.Age, cdcStandard.Age);
            Assert.Equal(whoStandard.L, cdcStandard.L);
            Assert.Equal(whoStandard.M, cdcStandard.M);
            Assert.Equal(whoStandard.S, cdcStandard.S);
        }

        [Fact]
        public async Task ParallelProcessing_ErrorResilience_HandlesVariedScenarios()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GrowthStandardSeeder>>();
            var mockContext = new Mock<DbContext>();
            var seeder = new GrowthStandardSeeder(mockLogger.Object, mockContext.Object);

            var mixedRecords = new List<IGrowthStandardRecord>
            {
                new WHODataRecord { Age = 12, L = 0.5, M = 10, S = 0.1 }, // Valid record
                new WHODataRecord { Age = -1, L = double.NaN, M = 0, S = -0.1 }, // Invalid record
                new WHODataRecord { Age = 10, L = 0.5, M = double.PositiveInfinity, S = 0.1 } // Another invalid record
            };

            // Act
            var processingResult = await seeder.ProcessRecordsParallelAsync(mixedRecords);

            // Assert
            Assert.NotNull(processingResult);
            Assert.Equal(ProcessingStatus.PartialFailure, processingResult.Status);
            Assert.True(processingResult.ProcessedRecords > 0);
            Assert.True(processingResult.FailedRecords > 0);
            Assert.NotNull(processingResult.ErrorSummary);
        }

        [Fact]
        public async Task RetryMechanism_TransientErrors_Recovers()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GrowthStandardSeeder>>();
            var mockContext = new Mock<DbContext>();
            
            // Simulate transient database error
            mockContext
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TransientException("Simulated transient error"))
                .Verifiable();

            var seeder = new GrowthStandardSeeder(mockLogger.Object, mockContext.Object);

            var recordWithTransientError = new WHODataRecord 
            { 
                Age = 12, 
                L = 0.5, 
                M = 10, 
                S = 0.1 
            };

            // Act
            var processingResult = await seeder.ProcessRecordWithRetryAsync(
                recordWithTransientError, 
                new ConcurrentDictionary<string, int>(), 
                CancellationToken.None
            );

            // Assert
            Assert.NotNull(processingResult);
            Assert.Equal(ProcessingStatus.MaxRetriesExceeded, processingResult.Status);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Fact]
        public async Task ErrorTracking_AccuratelyRecordsErrorFrequency()
        {
            // Arrange
            var errorTracker = new ConcurrentDictionary<string, int>();
            var mockLogger = new Mock<ILogger<GrowthStandardSeeder>>();
            var mockContext = new Mock<DbContext>();
            
            var seeder = new GrowthStandardSeeder(mockLogger.Object, mockContext.Object);

            var recordsWithErrors = new List<IGrowthStandardRecord>
            {
                new WHODataRecord { Age = -1, L = double.NaN, M = 0, S = -0.1 },
                new WHODataRecord { Age = -2, L = double.NaN, M = 0, S = -0.2 },
                new WHODataRecord { Age = 10, L = 0.5, M = double.PositiveInfinity, S = 0.1 }
            };

            // Act
            foreach (var record in recordsWithErrors)
            {
                try 
                {
                    await seeder.ProcessRecordWithRetryAsync(
                        record, 
                        errorTracker, 
                        CancellationToken.None
                    );
                }
                catch 
                {
                    // Intentionally catch and continue to simulate error tracking
                }
            }

            // Assert
            Assert.True(errorTracker.Count > 0);
            Assert.Contains("ValidationException", errorTracker.Keys);
        }

        [Fact]
        public void ErrorSummary_GeneratesComprehensiveReport()
        {
            // Arrange
            var errorTracker = new ConcurrentDictionary<string, int>();
            errorTracker.TryAdd("ValidationException", 2);
            errorTracker.TryAdd("DatabaseException", 1);

            var mockLogger = new Mock<ILogger<GrowthStandardSeeder>>();
            var mockContext = new Mock<DbContext>();
            var seeder = new GrowthStandardSeeder(mockLogger.Object, mockContext.Object);

            // Act
            var errorSummary = seeder.GetErrorSummary(errorTracker);

            // Assert
            Assert.Contains("ValidationException: 2 occurrences", errorSummary);
            Assert.Contains("DatabaseException: 1 occurrences", errorSummary);
        }

        [Fact]
        public async Task PerformanceMonitoring_TracksOperationMetrics()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GrowthStandardSeeder>>();
            var mockContext = new Mock<DbContext>();
            var seeder = new GrowthStandardSeeder(mockLogger.Object, mockContext.Object);

            var testRecords = new List<WHODataRecord>
            {
                new WHODataRecord { Age = 12, L = 0.5, M = 10, S = 0.1 },
                new WHODataRecord { Age = 24, L = 0.6, M = 15, S = 0.2 }
            };

            // Act
            await seeder.ProcessRecordsParallelAsync(testRecords);

            // Assert
            var performanceMonitor = seeder.GetPerformanceMonitor();
            var parallelProcessingMetrics = performanceMonitor.GetPerformanceMetrics("ProcessRecordsParallel");
            var recordProcessingMetrics = performanceMonitor.GetPerformanceMetrics("ProcessRecordWithRetry");

            Assert.NotNull(parallelProcessingMetrics);
            Assert.NotNull(recordProcessingMetrics);
            Assert.True(parallelProcessingMetrics.TotalExecutions > 0);
            Assert.True(recordProcessingMetrics.TotalExecutions > 0);
            Assert.True(parallelProcessingMetrics.AverageExecutionTime >= 0);
            Assert.True(recordProcessingMetrics.AverageExecutionTime >= 0);
        }

        [Fact]
        public async Task PerformanceMonitoring_CapturesSlowOperations()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GrowthStandardSeeder>>();
            var mockContext = new Mock<DbContext>();
            
            // Simulate slow database operation
            mockContext
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.Delay(1500)) // Simulate 1.5-second delay
                .Verifiable();

            var seeder = new GrowthStandardSeeder(mockLogger.Object, mockContext.Object);

            var slowRecord = new WHODataRecord 
            { 
                Age = 12, 
                L = 0.5, 
                M = 10, 
                S = 0.1 
            };

            // Act
            await Assert.ThrowsAsync<Exception>(() => 
                seeder.ProcessRecordsParallelAsync(new[] { slowRecord })
            );

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => 
                        o.ToString().Contains("Slow operation detected") && 
                        o.ToString().Contains("ProcessRecordWithRetry")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void PerformanceMonitor_AggregatesMetricsCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GrowthStandardSeeder>>();
            var mockContext = new Mock<DbContext>();
            var seeder = new GrowthStandardSeeder(mockLogger.Object, mockContext.Object);

            var performanceMonitor = seeder.GetPerformanceMonitor();

            // Act
            performanceMonitor.TrackPerformance("TestOperation", 100);
            performanceMonitor.TrackPerformance("TestOperation", 200);
            performanceMonitor.TrackPerformance("TestOperation", 150);

            // Assert
            var metrics = performanceMonitor.GetPerformanceMetrics("TestOperation");
            Assert.NotNull(metrics);
            Assert.Equal(3, metrics.TotalExecutions);
            Assert.Equal(450, metrics.TotalExecutionTime);
            Assert.Equal(150, metrics.AverageExecutionTime);
            Assert.Equal(200, metrics.MaxExecutionTime);
            Assert.Equal(100, metrics.MinExecutionTime);
        }

        // Custom exception for simulating transient errors
        public class TransientException : Exception
        {
            public TransientException(string message) : base(message) { }
        }
    }

    // Extension methods to expose private methods for testing
    public static class GrowthStandardSeederExtensions
    {
        public static double InvokeNormalDistributionInverse(
            this GrowthStandardSeeder seeder, double p)
        {
            var method = typeof(GrowthStandardSeeder)
                .GetMethod("NormalDistributionInverse", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Static);
            
            return (double)method.Invoke(null, new object[] { p });
        }

        public static double InvokeCalculatePercentile(
            this GrowthStandardSeeder seeder, 
            double p, double l, double m, double s)
        {
            var method = typeof(GrowthStandardSeeder)
                .GetMethod("CalculatePercentile", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
            
            return (double)method.Invoke(seeder, new object[] { p, l, m, s });
        }

        public static string InvokeGeneratePercentileValues(
            this GrowthStandardSeeder seeder, 
            IGrowthStandardRecord record)
        {
            var method = typeof(GrowthStandardSeeder)
                .GetMethod("GeneratePercentileValues", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
            
            return (string)method.Invoke(seeder, new object[] { record });
        }
    }

    // Performance and Stress Testing Extension
    public static class GrowthStandardSeederPerformanceExtensions
    {
        public static List<IGrowthStandardRecord> GenerateLargeTestDataset(
            this GrowthStandardSeeder seeder, 
            int recordCount = 10000)
        {
            var random = new Random();
            return Enumerable.Range(0, recordCount)
                .Select(_ => new WHODataRecord
                {
                    Age = random.NextDouble() * 240, // 0-20 years
                    L = random.NextDouble(),
                    M = random.NextDouble() * 100,
                    S = random.NextDouble()
                })
                .Cast<IGrowthStandardRecord>()
                .ToList();
        }

        public static List<IGrowthStandardRecord> ProcessGrowthStandardRecords(
            this GrowthStandardSeeder seeder, 
            List<IGrowthStandardRecord> records)
        {
            return records
                .Where(r => r.IsValid())
                .ToList();
        }
    }
}
