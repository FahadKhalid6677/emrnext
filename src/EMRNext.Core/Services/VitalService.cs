using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Data;
using EMRNext.Core.Validation;

namespace EMRNext.Core.Services
{
    public class VitalService : IVitalService
    {
        private readonly EMRNextDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IValidationService _validationService;

        public VitalService(
            EMRNextDbContext context, 
            IAuditService auditService,
            IValidationService validationService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        public async Task<Vital> CreateAsync(Vital vital)
        {
            if (vital == null)
                throw new ArgumentNullException(nameof(vital));

            // Validate using the new validation framework
            var validationResult = await _validationService.ValidateAsync(vital);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }

            if (vital.Height.HasValue && vital.Weight.HasValue)
            {
                vital.BMI = await CalculateBMIAsync(
                    vital.Height.Value,
                    vital.Weight.Value,
                    vital.HeightUnit,
                    vital.WeightUnit);
            }

            _context.Vitals.Add(vital);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Added vital signs for patient {vital.PatientId}");

            return vital;
        }

        public async Task<Vital> GetByIdAsync(int id)
        {
            return await _context.Vitals
                .Include(v => v.Patient)
                .Include(v => v.Encounter)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task UpdateAsync(Vital vital)
        {
            if (vital == null)
                throw new ArgumentNullException(nameof(vital));

            var existingVital = await _context.Vitals.FindAsync(vital.Id);
            if (existingVital == null)
                throw new KeyNotFoundException($"Vital with ID {vital.Id} not found");

            // Validate using the new validation framework
            var context = ValidationContext.Create(isNew: false);
            var validationResult = await _validationService.ValidateAsync(vital, context);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }

            if (vital.Height.HasValue && vital.Weight.HasValue)
            {
                vital.BMI = await CalculateBMIAsync(
                    vital.Height.Value,
                    vital.Weight.Value,
                    vital.HeightUnit,
                    vital.WeightUnit);
            }

            _context.Entry(existingVital).CurrentValues.SetValues(vital);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Updated vital signs for patient {vital.PatientId}");
        }

        public async Task<IEnumerable<Vital>> GetPatientVitalsAsync(int patientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Vitals
                .Include(v => v.Encounter)
                .Where(v => v.PatientId == patientId);

            if (startDate.HasValue)
                query = query.Where(v => v.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(v => v.Date <= endDate.Value);

            return await query.OrderByDescending(v => v.Date).ToListAsync();
        }

        public async Task<IEnumerable<Vital>> GetEncounterVitalsAsync(int encounterId)
        {
            return await _context.Vitals
                .Where(v => v.EncounterId == encounterId)
                .OrderByDescending(v => v.Date)
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(int vitalId)
        {
            var vital = await _context.Vitals.FindAsync(vitalId);
            if (vital == null)
                return false;

            _context.Vitals.Remove(vital);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Deleted vital signs with ID {vitalId}");

            return true;
        }

        public async Task<decimal?> CalculateBMIAsync(decimal height, decimal weight, string heightUnit = "in", string weightUnit = "lbs")
        {
            // Convert to metric if necessary
            if (heightUnit.ToLower() == "in")
                height *= 2.54M; // Convert inches to cm
            if (weightUnit.ToLower() == "lbs")
                weight *= 0.453592M; // Convert lbs to kg

            // Convert cm to meters
            var heightInMeters = height / 100M;

            // Calculate BMI: weight (kg) / height² (m²)
            if (heightInMeters <= 0)
                return null;

            return Math.Round(weight / (heightInMeters * heightInMeters), 1);
        }

        public async Task<IEnumerable<Vital>> GetAbnormalVitalsAsync(int patientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var vitals = await GetPatientVitalsAsync(patientId, startDate, endDate);
            var ranges = await GetVitalRangesForPatientAsync(patientId);
            
            return vitals.Where(v =>
                (v.Temperature.HasValue && (v.Temperature < ranges["Temperature"].Min || v.Temperature > ranges["Temperature"].Max)) ||
                (v.Pulse.HasValue && (v.Pulse < ranges["Pulse"].Min || v.Pulse > ranges["Pulse"].Max)) ||
                (v.RespiratoryRate.HasValue && (v.RespiratoryRate < ranges["RespiratoryRate"].Min || v.RespiratoryRate > ranges["RespiratoryRate"].Max)) ||
                (v.BloodPressureSystolic.HasValue && (v.BloodPressureSystolic < ranges["BloodPressureSystolic"].Min || v.BloodPressureSystolic > ranges["BloodPressureSystolic"].Max)) ||
                (v.BloodPressureDiastolic.HasValue && (v.BloodPressureDiastolic < ranges["BloodPressureDiastolic"].Min || v.BloodPressureDiastolic > ranges["BloodPressureDiastolic"].Max)) ||
                (v.OxygenSaturation.HasValue && (v.OxygenSaturation < ranges["OxygenSaturation"].Min || v.OxygenSaturation > ranges["OxygenSaturation"].Max))
            );
        }

        public async Task<Dictionary<string, (decimal Min, decimal Max)>> GetVitalRangesForPatientAsync(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
                throw new KeyNotFoundException($"Patient with ID {patientId} not found");

            // Calculate age-appropriate ranges
            var age = DateTime.Today.Year - patient.DateOfBirth.Year;
            
            // Default ranges for adults
            var ranges = new Dictionary<string, (decimal Min, decimal Max)>
            {
                ["Temperature"] = (97.0M, 99.5M),
                ["Pulse"] = (60M, 100M),
                ["RespiratoryRate"] = (12M, 20M),
                ["BloodPressureSystolic"] = (90M, 120M),
                ["BloodPressureDiastolic"] = (60M, 80M),
                ["OxygenSaturation"] = (95M, 100M)
            };

            // Adjust ranges based on age, gender, and other factors
            if (age < 18)
            {
                // Adjust ranges for pediatric patients
                // Implementation depends on specific pediatric vital sign guidelines
            }

            return ranges;
        }
    }
}
