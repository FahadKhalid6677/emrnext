using System;
using System.Collections.Generic;
using System.Linq;
using EMRNext.Core.Models;

namespace EMRNext.Core.Services.Clinical
{
    public interface IGrowthChartService
    {
        decimal GetBmiPercentile(decimal bmi, int ageInMonths, string gender);
        decimal GetHeightPercentile(decimal heightInCm, int ageInMonths, string gender);
        decimal GetWeightPercentile(decimal weightInKg, int ageInMonths, string gender);
        byte[] GenerateGrowthChart(int patientId, string measurementType);
    }

    public class GrowthChartService : IGrowthChartService
    {
        private readonly Dictionary<string, List<GrowthChartDataPoint>> _bmiPercentileData;
        private readonly Dictionary<string, List<GrowthChartDataPoint>> _heightPercentileData;
        private readonly Dictionary<string, List<GrowthChartDataPoint>> _weightPercentileData;

        public GrowthChartService()
        {
            // Initialize percentile data
            _bmiPercentileData = LoadBmiPercentileData();
            _heightPercentileData = LoadHeightPercentileData();
            _weightPercentileData = LoadWeightPercentileData();
        }

        public decimal GetBmiPercentile(decimal bmi, int ageInMonths, string gender)
        {
            var key = gender.ToLower();
            if (!_bmiPercentileData.ContainsKey(key))
                throw new ArgumentException("Invalid gender specified");

            var ageData = _bmiPercentileData[key]
                .Where(d => d.AgeInMonths == ageInMonths)
                .OrderBy(d => d.Value)
                .ToList();

            return CalculatePercentile(bmi, ageData);
        }

        public decimal GetHeightPercentile(decimal heightInCm, int ageInMonths, string gender)
        {
            var key = gender.ToLower();
            if (!_heightPercentileData.ContainsKey(key))
                throw new ArgumentException("Invalid gender specified");

            var ageData = _heightPercentileData[key]
                .Where(d => d.AgeInMonths == ageInMonths)
                .OrderBy(d => d.Value)
                .ToList();

            return CalculatePercentile(heightInCm, ageData);
        }

        public decimal GetWeightPercentile(decimal weightInKg, int ageInMonths, string gender)
        {
            var key = gender.ToLower();
            if (!_weightPercentileData.ContainsKey(key))
                throw new ArgumentException("Invalid gender specified");

            var ageData = _weightPercentileData[key]
                .Where(d => d.AgeInMonths == ageInMonths)
                .OrderBy(d => d.Value)
                .ToList();

            return CalculatePercentile(weightInKg, ageData);
        }

        public byte[] GenerateGrowthChart(int patientId, string measurementType)
        {
            // Implement growth chart generation logic here
            // This would create a visual representation of the patient's growth over time
            throw new NotImplementedException();
        }

        private decimal CalculatePercentile(decimal value, List<GrowthChartDataPoint> ageData)
        {
            if (!ageData.Any())
                throw new ArgumentException("No data available for the specified age");

            // Find the position of the value in the sorted data
            int position = ageData.FindIndex(d => d.Value > value);
            if (position == -1)
                return 100m; // Value is higher than all data points
            if (position == 0)
                return 0m; // Value is lower than all data points

            // Calculate percentile using linear interpolation
            var lower = ageData[position - 1];
            var upper = ageData[position];
            
            decimal percentile = lower.Percentile +
                (value - lower.Value) / (upper.Value - lower.Value) *
                (upper.Percentile - lower.Percentile);

            return Math.Round(percentile, 1);
        }

        private Dictionary<string, List<GrowthChartDataPoint>> LoadBmiPercentileData()
        {
            // Load BMI percentile data from configuration or database
            // This is placeholder data and should be replaced with actual WHO/CDC growth chart data
            return new Dictionary<string, List<GrowthChartDataPoint>>();
        }

        private Dictionary<string, List<GrowthChartDataPoint>> LoadHeightPercentileData()
        {
            // Load height percentile data from configuration or database
            return new Dictionary<string, List<GrowthChartDataPoint>>();
        }

        private Dictionary<string, List<GrowthChartDataPoint>> LoadWeightPercentileData()
        {
            // Load weight percentile data from configuration or database
            return new Dictionary<string, List<GrowthChartDataPoint>>();
        }
    }

    public class GrowthChartDataPoint
    {
        public int AgeInMonths { get; set; }
        public decimal Value { get; set; }
        public decimal Percentile { get; set; }
    }
}
