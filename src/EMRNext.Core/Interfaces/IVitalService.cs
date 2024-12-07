using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Interfaces
{
    public interface IVitalService
    {
        Task<Vital> GetVitalByIdAsync(int vitalId);
        Task<IEnumerable<Vital>> GetPatientVitalsAsync(int patientId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Vital>> GetEncounterVitalsAsync(int encounterId);
        Task<Vital> AddVitalAsync(Vital vital);
        Task<Vital> UpdateVitalAsync(Vital vital);
        Task<bool> DeleteVitalAsync(int vitalId);
        Task<decimal?> CalculateBMIAsync(decimal height, decimal weight, string heightUnit = "in", string weightUnit = "lbs");
        Task<bool> ValidateVitalRangesAsync(Vital vital);
        Task<Dictionary<string, (decimal Min, decimal Max)>> GetVitalRangesForPatientAsync(int patientId);
        Task<IEnumerable<Vital>> GetAbnormalVitalsAsync(int patientId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
