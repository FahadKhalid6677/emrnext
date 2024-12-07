using EMRNext.Core.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace EMRNext.Core.Validation.Rules
{
    public class VitalValidationRules : IValidationRule<Vital>
    {
        private readonly IPatientService _patientService;
        private readonly IEncounterService _encounterService;

        public VitalValidationRules(
            IPatientService patientService,
            IEncounterService encounterService)
        {
            _patientService = patientService;
            _encounterService = encounterService;
        }

        public async Task<ValidationResult> ValidateAsync(Vital entity, ValidationContext context)
        {
            var result = new ValidationResult();

            // Required fields validation
            if (entity.PatientId <= 0)
            {
                result.AddError(nameof(entity.PatientId), "Patient ID is required");
            }

            if (entity.EncounterId <= 0)
            {
                result.AddError(nameof(entity.EncounterId), "Encounter ID is required");
            }

            if (entity.Date == default)
            {
                result.AddError(nameof(entity.Date), "Date is required");
            }
            else if (entity.Date > DateTime.UtcNow)
            {
                result.AddError(nameof(entity.Date), "Date cannot be in the future");
            }

            // Temperature validation
            if (entity.Temperature.HasValue)
            {
                if (string.IsNullOrWhiteSpace(entity.TemperatureUnit))
                {
                    result.AddError(nameof(entity.TemperatureUnit), "Temperature unit is required when temperature is provided");
                }
                else if (entity.TemperatureUnit != "F" && entity.TemperatureUnit != "C")
                {
                    result.AddError(nameof(entity.TemperatureUnit), "Temperature unit must be either F or C");
                }

                // Validate temperature ranges
                if (entity.TemperatureUnit == "C")
                {
                    if (entity.Temperature < 30 || entity.Temperature > 45)
                    {
                        result.AddError(nameof(entity.Temperature), 
                            "Temperature is outside normal range (30-45°C)",
                            ValidationSeverity.Warning);
                    }
                }
                else if (entity.TemperatureUnit == "F")
                {
                    if (entity.Temperature < 86 || entity.Temperature > 113)
                    {
                        result.AddError(nameof(entity.Temperature), 
                            "Temperature is outside normal range (86-113°F)",
                            ValidationSeverity.Warning);
                    }
                }
            }

            // Pulse validation
            if (entity.Pulse.HasValue)
            {
                if (entity.Pulse < 0 || entity.Pulse > 300)
                {
                    result.AddError(nameof(entity.Pulse), 
                        "Pulse rate is outside normal range (0-300 bpm)",
                        ValidationSeverity.Warning);
                }
            }

            // Respiratory rate validation
            if (entity.RespiratoryRate.HasValue)
            {
                if (entity.RespiratoryRate < 0 || entity.RespiratoryRate > 100)
                {
                    result.AddError(nameof(entity.RespiratoryRate), 
                        "Respiratory rate is outside normal range (0-100 breaths/min)",
                        ValidationSeverity.Warning);
                }
            }

            // Blood pressure validation
            if (entity.BloodPressureSystolic.HasValue || entity.BloodPressureDiastolic.HasValue)
            {
                if (!entity.BloodPressureSystolic.HasValue || !entity.BloodPressureDiastolic.HasValue)
                {
                    result.AddError("BloodPressure", "Both systolic and diastolic values are required");
                }
                else
                {
                    if (entity.BloodPressureSystolic < 0 || entity.BloodPressureSystolic > 300)
                    {
                        result.AddError(nameof(entity.BloodPressureSystolic), 
                            "Systolic pressure is outside normal range (0-300 mmHg)",
                            ValidationSeverity.Warning);
                    }

                    if (entity.BloodPressureDiastolic < 0 || entity.BloodPressureDiastolic > 200)
                    {
                        result.AddError(nameof(entity.BloodPressureDiastolic), 
                            "Diastolic pressure is outside normal range (0-200 mmHg)",
                            ValidationSeverity.Warning);
                    }

                    if (entity.BloodPressureDiastolic >= entity.BloodPressureSystolic)
                    {
                        result.AddError("BloodPressure", "Diastolic pressure must be lower than systolic pressure");
                    }
                }
            }

            // Oxygen saturation validation
            if (entity.OxygenSaturation.HasValue)
            {
                if (entity.OxygenSaturation < 0 || entity.OxygenSaturation > 100)
                {
                    result.AddError(nameof(entity.OxygenSaturation), 
                        "Oxygen saturation must be between 0 and 100%");
                }

                if (entity.OxygenSaturation < 90)
                {
                    result.AddError(nameof(entity.OxygenSaturation), 
                        "Critical: Oxygen saturation is below 90%",
                        ValidationSeverity.Warning);
                }
            }

            // Height validation
            if (entity.Height.HasValue)
            {
                if (string.IsNullOrWhiteSpace(entity.HeightUnit))
                {
                    result.AddError(nameof(entity.HeightUnit), "Height unit is required when height is provided");
                }
                else if (entity.HeightUnit != "in" && entity.HeightUnit != "cm")
                {
                    result.AddError(nameof(entity.HeightUnit), "Height unit must be either in or cm");
                }

                if (entity.HeightUnit == "cm" && (entity.Height < 0 || entity.Height > 300))
                {
                    result.AddError(nameof(entity.Height), 
                        "Height is outside normal range (0-300 cm)",
                        ValidationSeverity.Warning);
                }
                else if (entity.HeightUnit == "in" && (entity.Height < 0 || entity.Height > 118))
                {
                    result.AddError(nameof(entity.Height), 
                        "Height is outside normal range (0-118 in)",
                        ValidationSeverity.Warning);
                }
            }

            // Weight validation
            if (entity.Weight.HasValue)
            {
                if (string.IsNullOrWhiteSpace(entity.WeightUnit))
                {
                    result.AddError(nameof(entity.WeightUnit), "Weight unit is required when weight is provided");
                }
                else if (entity.WeightUnit != "lbs" && entity.WeightUnit != "kg")
                {
                    result.AddError(nameof(entity.WeightUnit), "Weight unit must be either lbs or kg");
                }

                if (entity.WeightUnit == "kg" && (entity.Weight < 0 || entity.Weight > 500))
                {
                    result.AddError(nameof(entity.Weight), 
                        "Weight is outside normal range (0-500 kg)",
                        ValidationSeverity.Warning);
                }
                else if (entity.WeightUnit == "lbs" && (entity.Weight < 0 || entity.Weight > 1102))
                {
                    result.AddError(nameof(entity.Weight), 
                        "Weight is outside normal range (0-1102 lbs)",
                        ValidationSeverity.Warning);
                }
            }

            // BMI validation
            if (entity.BMI.HasValue)
            {
                if (entity.BMI < 0 || entity.BMI > 100)
                {
                    result.AddError(nameof(entity.BMI), 
                        "BMI is outside normal range (0-100)",
                        ValidationSeverity.Warning);
                }
            }

            // Percentile validations
            var percentileFields = new[] 
            { 
                (entity.HeadCircumferencePercentile, nameof(entity.HeadCircumferencePercentile)),
                (entity.HeightPercentile, nameof(entity.HeightPercentile)),
                (entity.WeightPercentile, nameof(entity.WeightPercentile)),
                (entity.BMIPercentile, nameof(entity.BMIPercentile))
            };

            foreach (var (percentile, fieldName) in percentileFields)
            {
                if (percentile.HasValue)
                {
                    if (percentile < 0 || percentile > 100)
                    {
                        result.AddError(fieldName, "Percentile must be between 0 and 100");
                    }
                }
            }

            // Growth chart type validation
            if (!string.IsNullOrWhiteSpace(entity.GrowthChartType))
            {
                if (entity.GrowthChartType != "WHO" && entity.GrowthChartType != "CDC")
                {
                    result.AddError(nameof(entity.GrowthChartType), "Growth chart type must be either WHO or CDC");
                }
            }

            // Validate patient exists
            if (entity.PatientId > 0)
            {
                var patient = await _patientService.GetByIdAsync(entity.PatientId);
                if (patient == null)
                {
                    result.AddError(nameof(entity.PatientId), "Patient does not exist");
                }
            }

            // Validate encounter exists and belongs to the patient
            if (entity.EncounterId > 0)
            {
                var encounter = await _encounterService.GetByIdAsync(entity.EncounterId);
                if (encounter == null)
                {
                    result.AddError(nameof(entity.EncounterId), "Encounter does not exist");
                }
                else if (encounter.PatientId != entity.PatientId)
                {
                    result.AddError(nameof(entity.EncounterId), "Encounter does not belong to the specified patient");
                }
            }

            return result;
        }
    }
}
