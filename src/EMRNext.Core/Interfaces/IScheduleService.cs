using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Portal;

namespace EMRNext.Core.Interfaces
{
    public interface IScheduleService
    {
        Task<bool> IsProviderAvailableAsync(int providerId, DateTime startTime, DateTime endTime);
        Task<bool> IsPatientAvailableAsync(int patientId, DateTime startTime, DateTime endTime);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime);
        Task<bool> IsEquipmentAvailableAsync(int equipmentId, DateTime startTime, DateTime endTime);
        Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int providerId, DateTime date, int duration);
        Task<IEnumerable<DateTime>> GetGroupSessionSlotsAsync(int groupId, DateTime startDate, DateTime endDate);
        Task<bool> ValidateScheduleConflictsAsync(int appointmentId, DateTime startTime, DateTime endTime);
        Task UpdateProviderScheduleAsync(int providerId, IEnumerable<ScheduleBlock> schedule);
        Task UpdateRoomScheduleAsync(int roomId, IEnumerable<ScheduleBlock> schedule);
        Task BlockTimeSlotAsync(int resourceId, DateTime startTime, DateTime endTime, string reason);
        Task UnblockTimeSlotAsync(int resourceId, DateTime startTime, DateTime endTime);
    }

    public class ScheduleBlock
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string Notes { get; set; }
    }
}
