using System;
using System.Collections.Generic;
using EMRNext.Core.Entities;
using EMRNext.Core.Models;

namespace EMRNext.Core.Services.Clinical
{
    public interface IVitalsCalculationService
    {
        BmiResult CalculateBmi(decimal heightInCm, decimal weightInKg);
        BmiPercentileResult CalculateBmiPercentile(decimal bmi, int ageInMonths, string gender);
        VitalTrend AnalyzeVitalTrend(IEnumerable<Vital> vitals, string vitalType);
        bool IsVitalInNormalRange(string vitalType, decimal value, int ageInMonths, string gender);
    }

    public class VitalsCalculationService : IVitalsCalculationService
    {
        private readonly IGrowthChartService _growthChartService;

        public VitalsCalculationService(IGrowthChartService growthChartService)
        {
            _growthChartService = growthChartService;
        }

        public BmiResult CalculateBmi(decimal heightInCm, decimal weightInKg)
        {
            // Convert height to meters
            decimal heightInM = heightInCm / 100;
            
            // Calculate BMI using the formula: weight (kg) / (height (m))²
            decimal bmi = weightInKg / (heightInM * heightInM);
            
            // Round to 1 decimal place
            bmi = Math.Round(bmi, 1);

            var category = GetBmiCategory(bmi);

            return new BmiResult
            {
                Bmi = bmi,
                Category = category,
                RecommendedRange = GetRecommendedBmiRange(category)
            };
        }

        public BmiPercentileResult CalculateBmiPercentile(decimal bmi, int ageInMonths, string gender)
        {
            var percentile = _growthChartService.GetBmiPercentile(bmi, ageInMonths, gender);
            
            return new BmiPercentileResult
            {
                Bmi = bmi,
                Percentile = percentile,
                Category = GetBmiPercentileCategory(percentile)
            };
        }

        public VitalTrend AnalyzeVitalTrend(IEnumerable<Vital> vitals, string vitalType)
        {
            // Implement trend analysis logic here
            // This would analyze the pattern of vitals over time
            // and determine if they're increasing, decreasing, or stable
            throw new NotImplementedException();
        }

        public bool IsVitalInNormalRange(string vitalType, decimal value, int ageInMonths, string gender)
        {
            // Implement normal range checking logic here
            // This would use standard vital ranges based on age and gender
            throw new NotImplementedException();
        }

        private string GetBmiCategory(decimal bmi)
        {
            return bmi switch
            {
                < 18.5m => "Underweight",
                < 25.0m => "Normal weight",
                < 30.0m => "Overweight",
                _ => "Obese"
            };
        }

        private string GetRecommendedBmiRange(string category)
        {
            return category switch
            {
                "Underweight" => "18.5 - 24.9",
                "Normal weight" => "18.5 - 24.9",
                "Overweight" => "18.5 - 24.9",
                "Obese" => "18.5 - 24.9",
                _ => "18.5 - 24.9"
            };
        }

        private string GetBmiPercentileCategory(decimal percentile)
        {
            return percentile switch
            {
                < 5 => "Underweight",
                < 85 => "Normal weight",
                < 95 => "Overweight",
                _ => "Obese"
            };
        }
    }
}
