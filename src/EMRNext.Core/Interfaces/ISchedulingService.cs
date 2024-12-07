using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Interfaces
{
    public interface ISchedulingService
    {
        // Appointment Management
        Task<Appointment> ScheduleAppointmentAsync(Appointment appointment);
        Task<Appointment> UpdateAppointmentAsync(int appointmentId, Appointment appointment);
        Task<bool> CancelAppointmentAsync(int appointmentId, string reason);
        Task<Appointment> GetAppointmentAsync(int appointmentId);
        Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(int patientId);
        
        // Provider Schedule Management
        Task<IEnumerable<TimeSlot>> GetProviderAvailabilityAsync(int providerId, DateTime date);
        Task<bool> BlockProviderTimeAsync(int providerId, DateTime start, DateTime end, string reason);
        Task<bool> UnblockProviderTimeAsync(int providerId, DateTime start, DateTime end);
        Task<IEnumerable<Appointment>> GetProviderScheduleAsync(int providerId, DateTime date);
        
        // Resource Management
        Task<bool> ReserveResourceAsync(int resourceId, DateTime start, DateTime end);
        Task<bool> ReleaseResourceAsync(int resourceId, DateTime start, DateTime end);
        Task<IEnumerable<Resource>> GetAvailableResourcesAsync(DateTime start, DateTime end);
        
        // Calendar Operations
        Task<IEnumerable<Appointment>> GetDailyScheduleAsync(DateTime date);
        Task<IEnumerable<Appointment>> GetWeeklyScheduleAsync(DateTime weekStart);
        Task<IEnumerable<Appointment>> GetMonthlyScheduleAsync(DateTime monthStart);
        
        // Reminder System
        Task<bool> SetAppointmentReminderAsync(int appointmentId, DateTime reminderTime);
        Task<bool> CancelReminderAsync(int appointmentId);
        Task<IEnumerable<Reminder>> GetPendingRemindersAsync();
        
        // Schedule Optimization
        Task<IEnumerable<TimeSlot>> GetNextAvailableSlotAsync(string appointmentType, int duration);
        Task<bool> OptimizeScheduleAsync(DateTime date);
        
        // Waitlist Management
        Task<WaitlistEntry> AddToWaitlistAsync(WaitlistEntry entry);
        Task<bool> RemoveFromWaitlistAsync(int waitlistEntryId);
        Task<IEnumerable<WaitlistEntry>> GetWaitlistAsync();
        
        // Conflict Management
        Task<bool> CheckForConflictsAsync(Appointment appointment);
        Task<IEnumerable<Conflict>> GetScheduleConflictsAsync(DateTime date);
        Task<bool> ResolveConflictAsync(int conflictId, string resolution);
        
        // Schedule Statistics
        Task<ScheduleMetrics> GetScheduleMetricsAsync(DateTime start, DateTime end);
        Task<UtilizationReport> GetResourceUtilizationAsync(int resourceId, DateTime start, DateTime end);
    }
}
