using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Clinical;

namespace EMRNext.Core.Interfaces
{
    public interface IVitalSignRepository
    {
        Task<VitalSign> GetByIdAsync(int id);
        Task<IEnumerable<VitalSign>> GetByPatientIdAsync(int patientId);
        Task<IEnumerable<VitalSign>> GetByPatientIdAndDateRangeAsync(int patientId, DateTime startDate, DateTime endDate);
        Task<VitalSign> AddAsync(VitalSign vitalSign);
        Task<VitalSign> UpdateAsync(VitalSign vitalSign);
        Task DeleteAsync(int id);
        Task<IEnumerable<VitalSign>> GetAbnormalReadingsAsync(int patientId);
        Task<Dictionary<string, double>> GetVitalStatisticsAsync(int patientId, DateTime startDate, DateTime endDate);
    }
}
