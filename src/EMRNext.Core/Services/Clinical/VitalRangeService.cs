using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Models;

namespace EMRNext.Core.Services.Clinical
{
    public interface IVitalRangeService
    {
        Task<VitalRange> GetVitalRangeAsync(string vitalType, int ageInMonths, string gender);
        Task<bool> IsVitalInRangeAsync(string vitalType, decimal value, int ageInMonths, string gender);
        Task<string> GetVitalInterpretationAsync(string vitalType, decimal value, int ageInMonths, string gender);
    }

    public class VitalRangeService : IVitalRangeService
    {
        private readonly Dictionary<string, Func<int, string, VitalRange>> _rangeCalculators;

        public VitalRangeService()
        {
            _rangeCalculators = new Dictionary<string, Func<int, string, VitalRange>>
            {
                { "HeartRate", CalculateHeartRateRange },
                { "RespiratoryRate", CalculateRespiratoryRateRange },
                { "BloodPressureSystolic", CalculateBloodPressureSystolicRange },
                { "BloodPressureDiastolic", CalculateBloodPressureDiastolicRange },
                { "Temperature", CalculateTemperatureRange },
                { "OxygenSaturation", CalculateOxygenSaturationRange }
            };
        }

        public async Task<VitalRange> GetVitalRangeAsync(string vitalType, int ageInMonths, string gender)
        {
            if (!_rangeCalculators.ContainsKey(vitalType))
                throw new ArgumentException($"Unsupported vital type: {vitalType}");

            return await Task.FromResult(_rangeCalculators[vitalType](ageInMonths, gender));
        }

        public async Task<bool> IsVitalInRangeAsync(string vitalType, decimal value, int ageInMonths, string gender)
        {
            var range = await GetVitalRangeAsync(vitalType, ageInMonths, gender);
            return value >= range.MinValue && value <= range.MaxValue;
        }

        public async Task<string> GetVitalInterpretationAsync(string vitalType, decimal value, int ageInMonths, string gender)
        {
            var range = await GetVitalRangeAsync(vitalType, ageInMonths, gender);
            
            if (value < range.MinValue)
                return "Below normal range";
            if (value > range.MaxValue)
                return "Above normal range";
            return "Within normal range";
        }

        private VitalRange CalculateHeartRateRange(int ageInMonths, string gender)
        {
            // Heart rate ranges based on age
            if (ageInMonths <= 1)
                return new VitalRange { MinValue = 100, MaxValue = 160, Unit = "bpm", AgeGroup = "Newborn" };
            if (ageInMonths <= 12)
                return new VitalRange { MinValue = 80, MaxValue = 140, Unit = "bpm", AgeGroup = "Infant" };
            if (ageInMonths <= 36)
                return new VitalRange { MinValue = 80, MaxValue = 130, Unit = "bpm", AgeGroup = "Toddler" };
            if (ageInMonths <= 144)
                return new VitalRange { MinValue = 70, MaxValue = 120, Unit = "bpm", AgeGroup = "Child" };
            return new VitalRange { MinValue = 60, MaxValue = 100, Unit = "bpm", AgeGroup = "Adult" };
        }

        private VitalRange CalculateRespiratoryRateRange(int ageInMonths, string gender)
        {
            if (ageInMonths <= 1)
                return new VitalRange { MinValue = 30, MaxValue = 60, Unit = "breaths/min", AgeGroup = "Newborn" };
            if (ageInMonths <= 12)
                return new VitalRange { MinValue = 20, MaxValue = 40, Unit = "breaths/min", AgeGroup = "Infant" };
            if (ageInMonths <= 36)
                return new VitalRange { MinValue = 20, MaxValue = 30, Unit = "breaths/min", AgeGroup = "Toddler" };
            if (ageInMonths <= 144)
                return new VitalRange { MinValue = 15, MaxValue = 25, Unit = "breaths/min", AgeGroup = "Child" };
            return new VitalRange { MinValue = 12, MaxValue = 20, Unit = "breaths/min", AgeGroup = "Adult" };
        }

        private VitalRange CalculateBloodPressureSystolicRange(int ageInMonths, string gender)
        {
            if (ageInMonths <= 1)
                return new VitalRange { MinValue = 60, MaxValue = 90, Unit = "mmHg", AgeGroup = "Newborn" };
            if (ageInMonths <= 12)
                return new VitalRange { MinValue = 70, MaxValue = 100, Unit = "mmHg", AgeGroup = "Infant" };
            if (ageInMonths <= 36)
                return new VitalRange { MinValue = 80, MaxValue = 110, Unit = "mmHg", AgeGroup = "Toddler" };
            if (ageInMonths <= 144)
                return new VitalRange { MinValue = 90, MaxValue = 120, Unit = "mmHg", AgeGroup = "Child" };
            return new VitalRange { MinValue = 90, MaxValue = 140, Unit = "mmHg", AgeGroup = "Adult" };
        }

        private VitalRange CalculateBloodPressureDiastolicRange(int ageInMonths, string gender)
        {
            if (ageInMonths <= 1)
                return new VitalRange { MinValue = 30, MaxValue = 60, Unit = "mmHg", AgeGroup = "Newborn" };
            if (ageInMonths <= 12)
                return new VitalRange { MinValue = 40, MaxValue = 70, Unit = "mmHg", AgeGroup = "Infant" };
            if (ageInMonths <= 36)
                return new VitalRange { MinValue = 50, MaxValue = 80, Unit = "mmHg", AgeGroup = "Toddler" };
            if (ageInMonths <= 144)
                return new VitalRange { MinValue = 60, MaxValue = 80, Unit = "mmHg", AgeGroup = "Child" };
            return new VitalRange { MinValue = 60, MaxValue = 90, Unit = "mmHg", AgeGroup = "Adult" };
        }

        private VitalRange CalculateTemperatureRange(int ageInMonths, string gender)
        {
            // Temperature ranges are generally the same across age groups (in Celsius)
            return new VitalRange
            {
                MinValue = 36.5m,
                MaxValue = 37.5m,
                Unit = "°C",
                AgeGroup = "All"
            };
        }

        private VitalRange CalculateOxygenSaturationRange(int ageInMonths, string gender)
        {
            // Oxygen saturation ranges are generally the same across age groups
            return new VitalRange
            {
                MinValue = 95,
                MaxValue = 100,
                Unit = "%",
                AgeGroup = "All"
            };
        }
    }
}
