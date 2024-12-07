using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Models.Growth;

namespace EMRNext.Core.Interfaces
{
    public interface IGrowthDataRepository
    {
        Task<GrowthStandard> GetGrowthStandardAsync(GrowthStandardType type);
        Task SaveGrowthStandardAsync(GrowthStandard standard);
        Task<ChartDefinition> GetChartDefinitionAsync(MeasurementType type, string gender, int minAge, int maxAge);
        Task<List<GrowthMeasurement>> GetMeasurementsAsync(int patientId, MeasurementType type, DateTime startDate, DateTime endDate);
        Task SaveMeasurementAsync(GrowthMeasurement measurement);
        Task<List<GrowthAlert>> GetAlertsAsync(int patientId, DateTime startDate, DateTime endDate);
        Task SaveAlertAsync(GrowthAlert alert);
        Task<GrowthVelocity> CalculateVelocityAsync(int patientId, MeasurementType type, DateTime startDate, DateTime endDate);
    }
}
