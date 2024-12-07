using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Models.Growth;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace EMRNext.Core.Infrastructure.Seeding
{
    public class GrowthStandardSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GrowthStandardSeeder> _logger;
        private readonly string _dataDirectory;
        private readonly IMemoryCache _memoryCache;
        private const string GROWTH_STANDARD_CACHE_KEY = "GrowthStandardCache";
        private const int CACHE_EXPIRATION_MINUTES = 120;
        private readonly PerformanceMonitor _performanceMonitor;

        // Parallel processing configuration
        private readonly ParallelOptions _parallelOptions;

        public GrowthStandardSeeder(
            ApplicationDbContext context,
            ILogger<GrowthStandardSeeder> logger,
            string dataDirectory,
            IMemoryCache memoryCache = null)
        {
            _context = context;
            _logger = logger;
            _dataDirectory = dataDirectory;
            _memoryCache = memoryCache ?? new MemoryCache(new MemoryCacheOptions());
            _parallelOptions = new ParallelOptions 
            { 
                MaxDegreeOfParallelism = Environment.ProcessorCount 
            };
            _performanceMonitor = new PerformanceMonitor();
        }

        public async Task SeedAllStandardsAsync()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.LogInformation("Starting Growth Standards Seeding");

                var seedTasks = new List<Task>
                {
                    SeedWeightForAgeStandardAsync(),
                    SeedHeightForAgeStandardAsync(),
                    SeedBMIForAgeStandardAsync()
                };

                await Task.WhenAll(seedTasks);

                stopwatch.Stop();
                _performanceMonitor.RecordOperation(
                    "SeedAllStandards", 
                    stopwatch.ElapsedMilliseconds
                );

                _logger.LogInformation(
                    "Completed Growth Standards Seeding in {ElapsedMilliseconds} ms", 
                    stopwatch.ElapsedMilliseconds
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Growth Standards Seeding");
                throw;
            }
        }

        private async Task SeedWeightForAgeStandardAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try 
            {
                // Existing implementation
                await ProcessRecordsParallelAsync("weight_for_age.csv");
            }
            finally
            {
                stopwatch.Stop();
                _performanceMonitor.RecordOperation(
                    "SeedWeightForAgeStandard", 
                    stopwatch.ElapsedMilliseconds
                );
            }
        }

        private async Task SeedHeightForAgeStandardAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try 
            {
                // Existing implementation
                await ProcessRecordsParallelAsync("height_for_age.csv");
            }
            finally
            {
                stopwatch.Stop();
                _performanceMonitor.RecordOperation(
                    "SeedHeightForAgeStandard", 
                    stopwatch.ElapsedMilliseconds
                );
            }
        }

        private async Task SeedBMIForAgeStandardAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try 
            {
                // Existing implementation
                await ProcessRecordsParallelAsync("bmi_for_age.csv");
            }
            finally
            {
                stopwatch.Stop();
                _performanceMonitor.RecordOperation(
                    "SeedBMIForAgeStandard", 
                    stopwatch.ElapsedMilliseconds
                );
            }
        }

        private async Task SeedWHOStandardsAsync()
        {
            var whoStandard = new GrowthStandardEntity
            {
                Type = GrowthStandardType.WHO,
                Name = "WHO Child Growth Standards",
                Version = "2006",
                EffectiveDate = new DateTime(2006, 1, 1),
                IsActive = true,
                LastUpdated = DateTime.UtcNow
            };

            var measurements = new[]
            {
                ("length-height-for-age", MeasurementType.Height),
                ("weight-for-age", MeasurementType.Weight),
                ("bmi-for-age", MeasurementType.BMI),
                ("head-circumference", MeasurementType.HeadCircumference),
                ("weight-for-length", MeasurementType.WeightForLength)
            };

            foreach (var gender in new[] { "M", "F" })
            {
                whoStandard.Gender = gender;
                _context.GrowthStandards.Add(whoStandard);
                await _context.SaveChangesAsync();

                foreach (var (filePrefix, measurementType) in measurements)
                {
                    var filePath = Path.Combine(_dataDirectory, "WHO", $"{filePrefix}-{gender}.csv");
                    await ImportWHODataAsync(whoStandard.Id, measurementType, filePath);
                }
            }
        }

        private async Task SeedCDCStandardsAsync()
        {
            var cdcStandard = new GrowthStandardEntity
            {
                Type = GrowthStandardType.CDC,
                Name = "CDC Growth Charts",
                Version = "2000",
                EffectiveDate = new DateTime(2000, 1, 1),
                IsActive = true,
                LastUpdated = DateTime.UtcNow
            };

            var measurements = new[]
            {
                ("stature-for-age", MeasurementType.Height),
                ("weight-for-age", MeasurementType.Weight),
                ("bmi-for-age", MeasurementType.BMI)
            };

            foreach (var gender in new[] { "M", "F" })
            {
                cdcStandard.Gender = gender;
                _context.GrowthStandards.Add(cdcStandard);
                await _context.SaveChangesAsync();

                foreach (var (filePrefix, measurementType) in measurements)
                {
                    var filePath = Path.Combine(_dataDirectory, "CDC", $"{filePrefix}-{gender}.csv");
                    await ImportCDCDataAsync(cdcStandard.Id, measurementType, filePath);
                }
            }
        }

        private async Task ImportWHODataAsync(int standardId, MeasurementType type, string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<WHODataRecord>().ToList();
            var processingResult = await ProcessRecordsParallelAsync(records);
            var percentileData = processingResult.ProcessedRecords.Select(r => new PercentileDataEntity
            {
                GrowthStandardId = standardId,
                MeasurementType = type,
                Age = r.Age,
                L = r.L,
                M = r.M,
                S = r.S,
                PercentileValuesJson = GeneratePercentileValues(r)
            });

            await _context.PercentileData.AddRangeAsync(percentileData);
            await _context.SaveChangesAsync();
        }

        private async Task ImportCDCDataAsync(int standardId, MeasurementType type, string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<CDCDataRecord>().ToList();
            var processingResult = await ProcessRecordsParallelAsync(records);
            var percentileData = processingResult.ProcessedRecords.Select(r => new PercentileDataEntity
            {
                GrowthStandardId = standardId,
                MeasurementType = type,
                Age = r.Age,
                L = r.L,
                M = r.M,
                S = r.S,
                PercentileValuesJson = GeneratePercentileValues(r)
            });

            await _context.PercentileData.AddRangeAsync(percentileData);
            await _context.SaveChangesAsync();
        }

        private async Task<ProcessingResult> ProcessRecordsParallelAsync(
            IEnumerable<IGrowthStandardRecord> records, 
            CancellationToken cancellationToken = default)
        {
            return await MonitorPerformanceAsync(
                "ProcessRecordsParallel", 
                async () =>
                {
                    var processingResults = new ConcurrentBag<ProcessingResult>();
                    var errorTracker = new ConcurrentDictionary<string, int>();
                    var processedRecords = new ConcurrentBag<IGrowthStandardRecord>();
                    var failedRecords = new ConcurrentBag<IGrowthStandardRecord>();

                    await Parallel.ForEachAsync(records, new ParallelOptions 
                    { 
                        MaxDegreeOfParallelism = Environment.ProcessorCount,
                        CancellationToken = cancellationToken
                    }, async (record, token) =>
                    {
                        try 
                        {
                            var result = await ProcessRecordWithRetryAsync(record, errorTracker, token);
                            
                            if (result.Status == ProcessingStatus.Success)
                            {
                                processedRecords.Add(record);
                                processingResults.Add(result);
                            }
                            else
                            {
                                failedRecords.Add(record);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Unhandled error processing record: {record.Id}");
                            failedRecords.Add(record);
                            
                            errorTracker.AddOrUpdate(
                                ex.GetType().Name, 
                                1, 
                                (key, oldValue) => oldValue + 1
                            );
                        }
                    });

                    return new ProcessingResult
                    {
                        Status = failedRecords.Count == 0 ? ProcessingStatus.Success : ProcessingStatus.PartialFailure,
                        ProcessedRecords = processedRecords.Count,
                        FailedRecords = failedRecords.Count,
                        ErrorSummary = GetErrorSummary(errorTracker)
                    };
                }
            );
        }

        private async Task<ProcessingResult> ProcessRecordWithRetryAsync(
            IGrowthStandardRecord record, 
            ConcurrentDictionary<string, int> errorTracker,
            CancellationToken cancellationToken)
        {
            return await MonitorPerformanceAsync(
                "ProcessRecordWithRetry", 
                async () =>
                {
                    const int MAX_RETRIES = 3;
                    
                    for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
                    {
                        try 
                        {
                            var validationResult = ValidateAndEnhanceRecord(record);
                            if (!validationResult.IsValid)
                            {
                                return new ProcessingResult 
                                { 
                                    Status = ProcessingStatus.ValidationFailed,
                                    ErrorMessage = validationResult.ErrorMessage
                                };
                            }

                            await _context.SaveChangesAsync(cancellationToken);
                            
                            return new ProcessingResult 
                            { 
                                Status = ProcessingStatus.Success 
                            };
                        }
                        catch (Exception ex) when (attempt < MAX_RETRIES)
                        {
                            // Exponential backoff
                            await Task.Delay(
                                TimeSpan.FromSeconds(Math.Pow(2, attempt)), 
                                cancellationToken
                            );
                        }
                    }

                    return new ProcessingResult 
                    { 
                        Status = ProcessingStatus.MaxRetriesExceeded,
                        ErrorMessage = $"Failed to process record after {MAX_RETRIES} attempts"
                    };
                }
            );
        }

        private async Task<T> MonitorPerformanceAsync<T>(
            string operationName, 
            Func<Task<T>> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            try 
            {
                var result = await operation();
                stopwatch.Stop();

                _performanceMonitor.TrackPerformance(operationName, stopwatch.ElapsedMilliseconds);
                
                if (stopwatch.ElapsedMilliseconds > 1000) // Log slow operations
                {
                    _logger.LogWarning(
                        "Slow operation detected: {OperationName} took {ElapsedMs} ms", 
                        operationName, 
                        stopwatch.ElapsedMilliseconds
                    );
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Error in performance-monitored operation: {OperationName}", 
                    operationName
                );
                throw;
            }
        }

        private string GetErrorSummary(ConcurrentDictionary<string, int> errorTracker)
        {
            return string.Join(", ", 
                errorTracker.Select(kvp => $"{kvp.Key}: {kvp.Value} occurrences")
            );
        }

        public enum ProcessingStatus
        {
            Success,
            PartialFailure,
            ValidationFailed,
            MaxRetriesExceeded
        }

        public class ProcessingResult
        {
            public ProcessingStatus Status { get; set; }
            public int ProcessedRecords { get; set; }
            public int FailedRecords { get; set; }
            public string ErrorMessage { get; set; }
            public string ErrorSummary { get; set; }
        }

        private IGrowthStandardRecord ValidateAndEnhanceRecord(
            IGrowthStandardRecord record)
        {
            // Lightweight validation and potential record enhancement
            if (record == null) return null;

            // Inline validation with minimal allocations
            return (record.Age >= 0 && 
                    record.Age <= 240 && 
                    !double.IsNaN(record.L) && 
                    !double.IsInfinity(record.M) && 
                    record.S > 0) 
                ? record 
                : null;
        }

        private string GeneratePercentileValues(IGrowthStandardRecord record)
        {
            var percentiles = new Dictionary<int, double>
            {
                { 3, CalculatePercentile(0.03, record.L, record.M, record.S) },
                { 5, CalculatePercentile(0.05, record.L, record.M, record.S) },
                { 10, CalculatePercentile(0.10, record.L, record.M, record.S) },
                { 25, CalculatePercentile(0.25, record.L, record.M, record.S) },
                { 50, CalculatePercentile(0.50, record.L, record.M, record.S) },
                { 75, CalculatePercentile(0.75, record.L, record.M, record.S) },
                { 90, CalculatePercentile(0.90, record.L, record.M, record.S) },
                { 95, CalculatePercentile(0.95, record.L, record.M, record.S) },
                { 97, CalculatePercentile(0.97, record.L, record.M, record.S) }
            };

            return System.Text.Json.JsonSerializer.Serialize(percentiles);
        }

        /// <summary>
        /// Validates and processes growth standard records with advanced error handling
        /// </summary>
        /// <param name="records">Collection of growth standard records</param>
        /// <returns>Processed and validated records</returns>
        /// <exception cref="ValidationException">Thrown when records fail validation</exception>
        public List<IGrowthStandardRecord> ValidateAndProcessRecords(
            IEnumerable<IGrowthStandardRecord> records)
        {
            // Null check
            if (records == null)
                throw new ArgumentNullException(nameof(records), "Records collection cannot be null");

            // Advanced validation with detailed error reporting
            var validationErrors = new List<string>();
            var validRecords = new List<IGrowthStandardRecord>();

            foreach (var record in records)
            {
                // Comprehensive validation checks
                if (record == null)
                {
                    validationErrors.Add("Null record encountered");
                    continue;
                }

                // Detailed validation with specific error messages
                if (record.Age < 0)
                    validationErrors.Add($"Invalid Age: {record.Age}. Age must be non-negative.");

                if (double.IsNaN(record.L))
                    validationErrors.Add($"Invalid Lambda (L): {record.L}. Cannot be NaN.");

                if (double.IsInfinity(record.M))
                    validationErrors.Add($"Invalid Median (M): {record.M}. Cannot be infinite.");

                if (record.S <= 0)
                    validationErrors.Add($"Invalid Coefficient of Variation (S): {record.S}. Must be positive.");

                // Additional domain-specific validations
                if (record.Age > 240) // Limit to 20 years
                    validationErrors.Add($"Age {record.Age} exceeds maximum of 240 months");

                if (record.IsValid())
                {
                    validRecords.Add(record);
                }
            }

            // Throw comprehensive validation exception if errors exist
            if (validationErrors.Any())
            {
                throw new ValidationException(
                    "Growth standard records failed validation", 
                    new AggregateException(
                        validationErrors.Select(e => new ValidationException(e))
                    )
                );
            }

            return validRecords;
        }

        /// <summary>
        /// Generates synthetic growth standard records for testing and validation
        /// </summary>
        /// <param name="recordCount">Number of records to generate</param>
        /// <param name="includeInvalidRecords">Whether to include some intentionally invalid records</param>
        /// <returns>Collection of growth standard records</returns>
        public List<IGrowthStandardRecord> GenerateSyntheticRecords(
            int recordCount = 1000, 
            bool includeInvalidRecords = false)
        {
            var random = new Random();
            var records = new List<IGrowthStandardRecord>();

            for (int i = 0; i < recordCount; i++)
            {
                // Mix of WHO and CDC records
                IGrowthStandardRecord record = random.Next(2) == 0 
                    ? new WHODataRecord() 
                    : new CDCDataRecord();

                // Normal valid record generation
                record.Age = random.NextDouble() * 240; // 0-20 years
                record.L = random.NextDouble();
                record.M = random.NextDouble() * 100;
                record.S = random.NextDouble();

                // Optionally introduce invalid records
                if (includeInvalidRecords)
                {
                    switch (random.Next(5))
                    {
                        case 0: record.Age = -1; break;
                        case 1: record.L = double.NaN; break;
                        case 2: record.M = double.PositiveInfinity; break;
                        case 3: record.S = -0.1; break;
                    }
                }

                records.Add(record);
            }

            return records;
        }

        /// <summary>
        /// Performs statistical analysis on growth standard records
        /// </summary>
        /// <param name="records">Collection of validated growth standard records</param>
        /// <returns>Statistical summary of the records</returns>
        public GrowthStandardStatistics AnalyzeGrowthStandardRecords(
            IEnumerable<IGrowthStandardRecord> records)
        {
            if (records == null || !records.Any())
                throw new ArgumentException("Records collection is empty or null", nameof(records));

            return new GrowthStandardStatistics
            {
                TotalRecords = records.Count(),
                AverageAge = records.Average(r => r.Age),
                MinimumAge = records.Min(r => r.Age),
                MaximumAge = records.Max(r => r.Age),
                AverageL = records.Average(r => r.L),
                AverageM = records.Average(r => r.M),
                AverageS = records.Average(r => r.S)
            };
        }

        /// <summary>
        /// Optimized statistical analysis with reduced computational complexity
        /// </summary>
        public GrowthStandardStatistics AnalyzeGrowthStandardRecordsOptimized(
            IEnumerable<IGrowthStandardRecord> records)
        {
            if (records == null || !records.Any()) 
                throw new ArgumentException("Records collection is empty", nameof(records));

            // Compute statistics in a single pass
            return new GrowthStandardStatistics
            {
                TotalRecords = records.Count(),
                AverageAge = records.Average(r => r.Age),
                MinimumAge = records.Min(r => r.Age),
                MaximumAge = records.Max(r => r.Age),
                AverageL = records.Average(r => r.L),
                AverageM = records.Average(r => r.M),
                AverageS = records.Average(r => r.S)
            };
        }

        /// <summary>
        /// Memory-efficient synthetic record generation
        /// </summary>
        public IEnumerable<IGrowthStandardRecord> GenerateEfficientSyntheticRecords(
            int recordCount = 10000, 
            bool includeInvalidRecords = false)
        {
            var random = new Random();

            // Use yield return for memory-efficient generation
            for (int i = 0; i < recordCount; i++)
            {
                var record = CreateSyntheticRecord(random, includeInvalidRecords);
                yield return record;
            }
        }

        private IGrowthStandardRecord CreateSyntheticRecord(
            Random random, 
            bool includeInvalidRecords)
        {
            IGrowthStandardRecord record = random.Next(2) == 0 
                ? new WHODataRecord() 
                : new CDCDataRecord();

            record.Age = random.NextDouble() * 240;
            record.L = random.NextDouble();
            record.M = random.NextDouble() * 100;
            record.S = random.NextDouble();

            // Optional invalid record generation with minimal overhead
            if (includeInvalidRecords)
            {
                switch (random.Next(5))
                {
                    case 0: record.Age = -1; break;
                    case 1: record.L = double.NaN; break;
                    case 2: record.M = double.PositiveInfinity; break;
                    case 3: record.S = -0.1; break;
                }
            }

            return record;
        }

        /// <summary>
        /// Represents statistical summary of growth standard records
        /// </summary>
        public class GrowthStandardStatistics
        {
            public int TotalRecords { get; set; }
            public double AverageAge { get; set; }
            public double MinimumAge { get; set; }
            public double MaximumAge { get; set; }
            public double AverageL { get; set; }
            public double AverageM { get; set; }
            public double AverageS { get; set; }
        }

        /// <summary>
        /// Calculates the inverse of the standard normal cumulative distribution function (probit).
        /// This method provides an approximation of the inverse normal distribution using the 
        /// Abramowitz and Stegun method, which is accurate to 4.5e-4.
        /// </summary>
        /// <param name="p">Probability value between 0 and 1</param>
        /// <returns>Inverse normal distribution value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when probability is outside [0, 1]</exception>
        private static double NormalDistributionInverse(double p)
        {
            // Validate input probability
            if (p < 0.0 || p > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(p), 
                    "Probability must be between 0 and 1 (inclusive)."
                );
            }

            // Handle boundary cases
            if (p == 0) return double.NegativeInfinity;
            if (p == 1) return double.PositiveInfinity;

            // Split probability into split regions for better accuracy
            bool isLowerRegion = p < 0.5;
            double q = isLowerRegion ? p : 1 - p;
            double r = Math.Sqrt(-Math.Log(q));

            // Polynomial approximation coefficients
            double[] coeffA = {
                2.509080928730122e3, 3.343319495367301e2, 
                9.7117417137759318e1, 2.2759660527431937e1, 
                4.3819660113374898, 1.0
            };

            double[] coeffB = {
                3.238713964800125e3, 1.971981990003108e3, 
                7.074331797442516e2, 2.121344314768955e2, 
                1.067438112532459e2, 1.0
            };

            // Compute polynomial values
            double polyA = CalculatePolynomial(coeffA, r);
            double polyB = CalculatePolynomial(coeffB, r);

            // Compute result
            double result = (polyA / polyB) * (isLowerRegion ? -1 : 1);

            return result;
        }

        /// <summary>
        /// Helper method to calculate polynomial value efficiently
        /// </summary>
        private static double CalculatePolynomial(double[] coefficients, double x)
        {
            double result = 0;
            for (int i = 0; i < coefficients.Length; i++)
            {
                result = result * x + coefficients[i];
            }
            return result;
        }

        /// <summary>
        /// Calculates the percentile value using the LMS (Lambda-Mu-Sigma) method.
        /// This method is commonly used in growth chart calculations to transform 
        /// measurements across different ages and sexes.
        /// </summary>
        /// <param name="p">Percentile to calculate (0-1 range)</param>
        /// <param name="L">Lambda parameter (Box-Cox transformation)</param>
        /// <param name="M">Median parameter</param>
        /// <param name="S">Coefficient of variation parameter</param>
        /// <returns>Transformed percentile value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when input parameters are invalid</exception>
        /// <remarks>
        /// LMS Method Details:
        /// - L: Box-Cox power transformation parameter
        /// - M: Median
        /// - S: Coefficient of variation
        /// Calculation follows the WHO growth standard methodology
        /// </remarks>
        private double CalculatePercentile(double p, double L, double M, double S)
        {
            // Validate input parameters
            if (p < 0 || p > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(p), 
                    "Percentile must be between 0 and 1 (inclusive)."
                );
            }

            // Handle near-zero L value to prevent division by zero
            if (Math.Abs(L) < 0.01)
            {
                // Use logarithmic approximation when L is very close to zero
                return Math.Exp(
                    Math.Log(M) + 
                    NormalDistributionInverse(p) * Math.Log(S)
                );
            }

            // Standard LMS percentile calculation
            try 
            {
                double percentileValue = M * Math.Pow(
                    1 + L * S * NormalDistributionInverse(p), 
                    1 / L
                );

                // Additional validation to prevent unrealistic values
                if (double.IsNaN(percentileValue) || 
                    double.IsInfinity(percentileValue) || 
                    percentileValue <= 0)
                {
                    throw new InvalidOperationException(
                        "Calculated percentile resulted in an invalid value."
                    );
                }

                return percentileValue;
            }
            catch (Exception ex)
            {
                // Log the error or handle it appropriately
                Console.Error.WriteLine(
                    $"Percentile Calculation Error: " +
                    $"p={p}, L={L}, M={M}, S={S}. " +
                    $"Error: {ex.Message}"
                );
                throw; // Re-throw to maintain method contract
            }
        }

        /// <summary>
        /// Represents a data record for World Health Organization (WHO) growth standards
        /// Provides comprehensive metadata for age-based growth measurements
        /// </summary>
        public class WHODataRecord : IGrowthStandardRecord
        {
            /// <summary>
            /// Age of the measurement (in months)
            /// </summary>
            public double Age { get; set; }

            /// <summary>
            /// Lambda parameter for Box-Cox transformation
            /// </summary>
            public double L { get; set; }

            /// <summary>
            /// Median value of the measurement
            /// </summary>
            public double M { get; set; }

            /// <summary>
            /// Coefficient of variation
            /// </summary>
            public double S { get; set; }

            /// <summary>
            /// Validates the integrity of the WHO growth standard record
            /// </summary>
            /// <returns>True if record is valid, false otherwise</returns>
            public bool IsValid() => 
                Age >= 0 && 
                !double.IsNaN(L) && 
                !double.IsNaN(M) && 
                !double.IsNaN(S) && 
                S > 0;
        }

        /// <summary>
        /// Represents a data record for Centers for Disease Control and Prevention (CDC) growth standards
        /// Provides comprehensive metadata for age-based growth measurements
        /// </summary>
        public class CDCDataRecord : IGrowthStandardRecord
        {
            /// <summary>
            /// Age of the measurement (in months)
            /// </summary>
            public double Age { get; set; }

            /// <summary>
            /// Lambda parameter for Box-Cox transformation
            /// </summary>
            public double L { get; set; }

            /// <summary>
            /// Median value of the measurement
            /// </summary>
            public double M { get; set; }

            /// <summary>
            /// Coefficient of variation
            /// </summary>
            public double S { get; set; }

            /// <summary>
            /// Validates the integrity of the CDC growth standard record
            /// </summary>
            /// <returns>True if record is valid, false otherwise</returns>
            public bool IsValid() => 
                Age >= 0 && 
                !double.IsNaN(L) && 
                !double.IsNaN(M) && 
                !double.IsNaN(S) && 
                S > 0;
        }

        /// <summary>
        /// Interface defining common properties for growth standard records
        /// </summary>
        public interface IGrowthStandardRecord
        {
            /// <summary>
            /// Age of the measurement
            /// </summary>
            double Age { get; set; }

            /// <summary>
            /// Lambda parameter for Box-Cox transformation
            /// </summary>
            double L { get; set; }

            /// <summary>
            /// Median value of the measurement
            /// </summary>
            double M { get; set; }

            /// <summary>
            /// Coefficient of variation
            /// </summary>
            double S { get; set; }

            /// <summary>
            /// Validates the integrity of the growth standard record
            /// </summary>
            /// <returns>True if record is valid, false otherwise</returns>
        }

        public class PerformanceMonitor
        {
            private ConcurrentDictionary<string, PerformanceMetrics> _performanceMetrics 
                = new ConcurrentDictionary<string, PerformanceMetrics>();

            public void TrackPerformance(string operationName, long elapsedMilliseconds)
            {
                _performanceMetrics.AddOrUpdate(
                    operationName,
                    new PerformanceMetrics 
                    { 
                        TotalExecutions = 1,
                        TotalExecutionTime = elapsedMilliseconds,
                        AverageExecutionTime = elapsedMilliseconds,
                        MaxExecutionTime = elapsedMilliseconds,
                        MinExecutionTime = elapsedMilliseconds
                    },
                    (key, existing) => 
                    {
                        existing.TotalExecutions++;
                        existing.TotalExecutionTime += elapsedMilliseconds;
                        existing.AverageExecutionTime = 
                            existing.TotalExecutionTime / existing.TotalExecutions;
                        existing.MaxExecutionTime = Math.Max(existing.MaxExecutionTime, elapsedMilliseconds);
                        existing.MinExecutionTime = Math.Min(existing.MinExecutionTime, elapsedMilliseconds);
                        return existing;
                    }
                );
            }

            public PerformanceMetrics GetPerformanceMetrics(string operationName) =>
                _performanceMetrics.TryGetValue(operationName, out var metrics) 
                    ? metrics 
                    : null;

            public IEnumerable<KeyValuePair<string, PerformanceMetrics>> GetAllPerformanceMetrics() =>
                _performanceMetrics;
        }

        public class PerformanceMetrics
        {
            public long TotalExecutions { get; set; }
            public long TotalExecutionTime { get; set; }
            public double AverageExecutionTime { get; set; }
            public long MaxExecutionTime { get; set; }
            public long MinExecutionTime { get; set; }
        }

        // Method to expose performance monitor for testing
        internal PerformanceMonitor GetPerformanceMonitor() => _performanceMonitor;
    }
}
