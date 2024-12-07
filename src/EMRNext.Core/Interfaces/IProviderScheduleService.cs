using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Portal;

namespace EMRNext.Core.Interfaces
{
    public interface IProviderScheduleService
    {
        Task<bool> IsProviderAvailableAsync(int providerId, DateTime startTime, DateTime endTime);
        Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int providerId, DateTime date);
        Task<IEnumerable<DateTime>> GetAvailableSlotsForGroupAsync(int providerId, DateTime date, int duration);
        Task UpdateProviderScheduleAsync(int providerId, IEnumerable<ProviderScheduleBlock> schedule);
        Task BlockTimeSlotAsync(int providerId, DateTime startTime, DateTime endTime, string reason);
        Task UnblockTimeSlotAsync(int providerId, DateTime startTime, DateTime endTime);
        Task<IEnumerable<ProviderScheduleBlock>> GetProviderScheduleAsync(int providerId, DateTime startDate, DateTime endDate);
        Task<bool> ValidateProviderAvailabilityAsync(int providerId, DateTime startTime, DateTime endTime);
        Task<IEnumerable<int>> FindAvailableProvidersAsync(DateTime startTime, DateTime endTime, string specialty = null);
        Task SetProviderWorkingHoursAsync(int providerId, IEnumerable<WorkingHours> workingHours);
    }

    public class ProviderScheduleBlock
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string BlockReason { get; set; }
        public string Notes { get; set; }
    }

    public class WorkingHours
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan? BreakStart { get; set; }
        public TimeSpan? BreakEnd { get; set; }
        public bool IsWorkingDay { get; set; }
    }
}
