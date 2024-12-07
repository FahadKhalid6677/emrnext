using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Models.Growth;
using CsvHelper;
using System.Globalization;

namespace EMRNext.Core.Services.Growth
{
    public interface IGrowthDataImportService
    {
        Task<GrowthStandard> ImportWHOStandardsAsync(string dataDirectory);
        Task<GrowthStandard> ImportCDCStandardsAsync(string dataDirectory);
        Task<bool> ValidateDataIntegrityAsync(GrowthStandard standard);
        Task<Dictionary<string, List<DataPoint>>> ParseLMSDataAsync(string filePath);
    }

    public class GrowthDataImportService : IGrowthDataImportService
    {
        private readonly ILogger<GrowthDataImportService> _logger;
        private readonly IGrowthDataRepository _repository;

        public GrowthDataImportService(
            ILogger<GrowthDataImportService> logger,
            IGrowthDataRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<GrowthStandard> ImportWHOStandardsAsync(string dataDirectory)
        {
            try
            {
                var standard = new GrowthStandard
                {
                    Type = GrowthStandardType.WHO,
                    Name = "WHO Child Growth Standards",
                    Version = "2006",
                    EffectiveDate = new DateTime(2006, 1, 1),
                    Percentiles = GetWHOPercentiles(),
                    Charts = new List<ChartDefinition>()
                };

                // Import Weight-for-age data
                await ImportWHODataFileAsync(
                    Path.Combine(dataDirectory, "wfa_boys_0_5.txt"),
                    "Weight-for-age Boys 0-5",
                    MeasurementType.Weight,
                    "M",
                    0,
                    60,
                    "kg",
                    standard);

                await ImportWHODataFileAsync(
                    Path.Combine(dataDirectory, "wfa_girls_0_5.txt"),
                    "Weight-for-age Girls 0-5",
                    MeasurementType.Weight,
                    "F",
                    0,
                    60,
                    "kg",
                    standard);

                // Import Length/Height-for-age data
                await ImportWHODataFileAsync(
                    Path.Combine(dataDirectory, "lhfa_boys_0_5.txt"),
                    "Length/Height-for-age Boys 0-5",
                    MeasurementType.Height,
                    "M",
                    0,
                    60,
                    "cm",
                    standard);

                await ImportWHODataFileAsync(
                    Path.Combine(dataDirectory, "lhfa_girls_0_5.txt"),
                    "Length/Height-for-age Girls 0-5",
                    MeasurementType.Height,
                    "F",
                    0,
                    60,
                    "cm",
                    standard);

                // Import BMI-for-age data
                await ImportWHODataFileAsync(
                    Path.Combine(dataDirectory, "bfa_boys_0_5.txt"),
                    "BMI-for-age Boys 0-5",
                    MeasurementType.BMI,
                    "M",
                    0,
                    60,
                    "kg/m²",
                    standard);

                await ImportWHODataFileAsync(
                    Path.Combine(dataDirectory, "bfa_girls_0_5.txt"),
                    "BMI-for-age Girls 0-5",
                    MeasurementType.BMI,
                    "F",
                    0,
                    60,
                    "kg/m²",
                    standard);

                await _repository.SaveGrowthStandardAsync(standard);
                return standard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing WHO standards from {Directory}", dataDirectory);
                throw;
            }
        }

        public async Task<GrowthStandard> ImportCDCStandardsAsync(string dataDirectory)
        {
            try
            {
                var standard = new GrowthStandard
                {
                    Type = GrowthStandardType.CDC,
                    Name = "CDC Growth Charts",
                    Version = "2000",
                    EffectiveDate = new DateTime(2000, 1, 1),
                    Percentiles = GetCDCPercentiles(),
                    Charts = new List<ChartDefinition>()
                };

                // Import CDC data files
                // Similar to WHO import but with CDC-specific file formats
                // CDC data typically covers ages 2-20 years

                await _repository.SaveGrowthStandardAsync(standard);
                return standard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing CDC standards from {Directory}", dataDirectory);
                throw;
            }
        }

        public async Task<bool> ValidateDataIntegrityAsync(GrowthStandard standard)
        {
            try
            {
                // Validate data completeness
                if (!standard.Charts.Any())
                {
                    _logger.LogError("No charts found in standard {StandardName}", standard.Name);
                    return false;
                }

                foreach (var chart in standard.Charts)
                {
                    // Validate percentile curves
                    if (!chart.PercentileCurves.Any())
                    {
                        _logger.LogError("No percentile curves found in chart {ChartName}", chart.Name);
                        return false;
                    }

                    // Validate data points
                    foreach (var curve in chart.PercentileCurves)
                    {
                        if (!curve.Points.Any())
                        {
                            _logger.LogError("No data points found in curve for percentile {Percentile}", curve.Percentile);
                            return false;
                        }

                        // Validate age range
                        var ages = curve.Points.Select(p => p.AgeMonths).OrderBy(a => a).ToList();
                        if (ages.First() != chart.MinAgeMonths || ages.Last() != chart.MaxAgeMonths)
                        {
                            _logger.LogError("Age range mismatch in chart {ChartName}", chart.Name);
                            return false;
                        }

                        // Validate LMS values
                        if (curve.Points.Any(p => p.L == 0 || p.M == 0 || p.S == 0))
                        {
                            _logger.LogError("Invalid LMS values found in chart {ChartName}", chart.Name);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating growth standard {StandardName}", standard.Name);
                return false;
            }
        }

        public async Task<Dictionary<string, List<DataPoint>>> ParseLMSDataAsync(string filePath)
        {
            try
            {
                var result = new Dictionary<string, List<DataPoint>>();

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    // Skip header
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        var point = new DataPoint
                        {
                            AgeMonths = csv.GetField<int>("Age_Months"),
                            L = csv.GetField<decimal>("L"),
                            M = csv.GetField<decimal>("M"),
                            S = csv.GetField<decimal>("S")
                        };

                        // Calculate values for standard percentiles
                        foreach (var percentile in new[] { 3m, 5m, 10m, 25m, 50m, 75m, 90m, 95m, 97m })
                        {
                            var z = CalculateZScore(percentile);
                            var value = CalculatePercentileValue(point.L, point.M, point.S, z);

                            var key = $"P{percentile}";
                            if (!result.ContainsKey(key))
                            {
                                result[key] = new List<DataPoint>();
                            }

                            result[key].Add(new DataPoint
                            {
                                AgeMonths = point.AgeMonths,
                                Value = value,
                                L = point.L,
                                M = point.M,
                                S = point.S
                            });
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing LMS data from {FilePath}", filePath);
                throw;
            }
        }

        private List<PercentileDefinition> GetWHOPercentiles()
        {
            return new List<PercentileDefinition>
            {
                new PercentileDefinition { Value = 3, Label = "3rd", Color = "#FF0000", IsMainLine = false, LineStyle = "dashed" },
                new PercentileDefinition { Value = 15, Label = "15th", Color = "#FFA500", IsMainLine = false, LineStyle = "dashed" },
                new PercentileDefinition { Value = 50, Label = "50th", Color = "#008000", IsMainLine = true, LineStyle = "solid" },
                new PercentileDefinition { Value = 85, Label = "85th", Color = "#FFA500", IsMainLine = false, LineStyle = "dashed" },
                new PercentileDefinition { Value = 97, Label = "97th", Color = "#FF0000", IsMainLine = false, LineStyle = "dashed" }
            };
        }

        private List<PercentileDefinition> GetCDCPercentiles()
        {
            return new List<PercentileDefinition>
            {
                new PercentileDefinition { Value = 5, Label = "5th", Color = "#FF0000", IsMainLine = false, LineStyle = "dashed" },
                new PercentileDefinition { Value = 10, Label = "10th", Color = "#FFA500", IsMainLine = false, LineStyle = "dashed" },
                new PercentileDefinition { Value = 25, Label = "25th", Color = "#FFD700", IsMainLine = false, LineStyle = "dashed" },
                new PercentileDefinition { Value = 50, Label = "50th", Color = "#008000", IsMainLine = true, LineStyle = "solid" },
                new PercentileDefinition { Value = 75, Label = "75th", Color = "#FFD700", IsMainLine = false, LineStyle = "dashed" },
                new PercentileDefinition { Value = 90, Label = "90th", Color = "#FFA500", IsMainLine = false, LineStyle = "dashed" },
                new PercentileDefinition { Value = 95, Label = "95th", Color = "#FF0000", IsMainLine = false, LineStyle = "dashed" }
            };
        }

        private decimal CalculateZScore(decimal percentile)
        {
            // Convert percentile to z-score using the inverse of the standard normal cumulative distribution
            // This is a simplified approximation
            return (decimal)Math.Sqrt(2) * (decimal)Math.Erfcinv(2 * (1 - (double)percentile / 100));
        }

        private decimal CalculatePercentileValue(decimal L, decimal M, decimal S, decimal Z)
        {
            if (Math.Abs((double)L) < 0.01)
            {
                return M * (decimal)Math.Exp((double)S * (double)Z);
            }
            return M * (decimal)Math.Pow(1 + (double)L * (double)S * (double)Z, 1 / (double)L);
        }

        private async Task ImportWHODataFileAsync(
            string filePath,
            string chartName,
            MeasurementType measurementType,
            string gender,
            int minAge,
            int maxAge,
            string unit,
            GrowthStandard standard)
        {
            var lmsData = await ParseLMSDataAsync(filePath);

            var chart = new ChartDefinition
            {
                Name = chartName,
                MeasurementType = measurementType,
                Gender = gender,
                MinAgeMonths = minAge,
                MaxAgeMonths = maxAge,
                Unit = unit,
                XAxisLabel = "Age (months)",
                YAxisLabel = $"{measurementType} ({unit})",
                PercentileCurves = new List<PercentileCurve>()
            };

            foreach (var kvp in lmsData)
            {
                var percentile = decimal.Parse(kvp.Key.Substring(1));
                chart.PercentileCurves.Add(new PercentileCurve
                {
                    Percentile = percentile,
                    Points = kvp.Value
                });
            }

            standard.Charts.Add(chart);
        }
    }
}
