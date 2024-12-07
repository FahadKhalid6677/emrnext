using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Validation;
using EMRNext.Core.Exceptions;

namespace EMRNext.Core.Services
{
    public class SchedulingService : ISchedulingService
    {
        private readonly ISchedulingRepository _schedulingRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly ILoggingService _loggingService;
        private readonly AppointmentValidator _appointmentValidator;
        private readonly TimeSlotValidator _timeSlotValidator;
        private readonly ResourceValidator _resourceValidator;
        private readonly WaitlistEntryValidator _waitlistValidator;

        public SchedulingService(
            ISchedulingRepository schedulingRepository,
            IProviderRepository providerRepository,
            ILoggingService loggingService)
        {
            _schedulingRepository = schedulingRepository;
            _providerRepository = providerRepository;
            _loggingService = loggingService;
            _appointmentValidator = new AppointmentValidator();
            _timeSlotValidator = new TimeSlotValidator();
            _resourceValidator = new ResourceValidator();
            _waitlistValidator = new WaitlistEntryValidator();
        }

        public async Task<Appointment> ScheduleAppointmentAsync(Appointment appointment)
        {
            try
            {
                var validationResult = await _appointmentValidator.ValidateAsync(appointment);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

                // Check provider availability
                var isProviderAvailable = await CheckProviderAvailabilityAsync(
                    appointment.ProviderId,
                    appointment.StartTime,
                    appointment.EndTime
                );
                if (!isProviderAvailable)
                    throw new BusinessRuleException("Provider is not available for the selected time slot");

                // Check resource availability
                if (appointment.RequiredResources?.Any() == true)
                {
                    var areResourcesAvailable = await CheckResourceAvailabilityAsync(
                        appointment.RequiredResources,
                        appointment.StartTime,
                        appointment.EndTime
                    );
                    if (!areResourcesAvailable)
                        throw new BusinessRuleException("Required resources are not available");
                }

                // Check for scheduling conflicts
                var conflicts = await CheckForConflictsAsync(appointment);
                if (conflicts.Any())
                    throw new BusinessRuleException($"Scheduling conflicts detected: {string.Join(", ", conflicts)}");

                appointment.Status = AppointmentStatus.Scheduled;
                appointment.CreatedDate = DateTime.UtcNow;
                appointment.LastModified = DateTime.UtcNow;

                var result = await _schedulingRepository.CreateAppointmentAsync(appointment);

                // Reserve resources
                if (appointment.RequiredResources?.Any() == true)
                {
                    await ReserveResourcesAsync(
                        appointment.RequiredResources,
                        appointment.StartTime,
                        appointment.EndTime
                    );
                }

                // Create reminder
                await SetAppointmentReminderAsync(result.Id, result.StartTime.AddDays(-1));

                await _loggingService.LogAuditAsync(
                    "ScheduleAppointment",
                    "Appointment",
                    result.Id.ToString(),
                    $"Scheduled appointment for patient {appointment.PatientId}"
                );

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error scheduling appointment for patient {PatientId}", appointment.PatientId);
                throw;
            }
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, string reason)
        {
            try
            {
                var appointment = await _schedulingRepository.GetAppointmentAsync(appointmentId);
                if (appointment == null)
                    throw new NotFoundException($"Appointment with ID {appointmentId} not found");

                // Check cancellation policy
                var canCancel = await CheckCancellationPolicyAsync(appointment);
                if (!canCancel)
                    throw new BusinessRuleException("Appointment cannot be cancelled due to policy restrictions");

                // Release resources
                if (appointment.RequiredResources?.Any() == true)
                {
                    await ReleaseResourcesAsync(
                        appointment.RequiredResources,
                        appointment.StartTime,
                        appointment.EndTime
                    );
                }

                // Cancel reminder
                await CancelReminderAsync(appointmentId);

                // Update appointment status
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.CancellationReason = reason;
                appointment.CancellationDate = DateTime.UtcNow;
                appointment.LastModified = DateTime.UtcNow;

                await _schedulingRepository.UpdateAppointmentAsync(appointmentId, appointment);

                // Check waitlist
                await ProcessWaitlistAsync(appointment.ProviderId, appointment.StartTime, appointment.EndTime);

                await _loggingService.LogAuditAsync(
                    "CancelAppointment",
                    "Appointment",
                    appointmentId.ToString(),
                    $"Cancelled appointment {appointmentId}: {reason}"
                );

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error cancelling appointment {AppointmentId}", appointmentId);
                throw;
            }
        }

        public async Task<IEnumerable<TimeSlot>> GetProviderAvailabilityAsync(int providerId, DateTime date)
        {
            try
            {
                var provider = await _providerRepository.GetByIdAsync(providerId);
                if (provider == null)
                    throw new NotFoundException($"Provider with ID {providerId} not found");

                // Get provider schedule
                var schedule = await _schedulingRepository.GetProviderScheduleAsync(providerId, date);

                // Get provider working hours
                var workingHours = await GetProviderWorkingHoursAsync(providerId, date);

                // Calculate available time slots
                var availableSlots = CalculateAvailableTimeSlots(workingHours, schedule);

                return availableSlots;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error getting availability for provider {ProviderId}", providerId);
                throw;
            }
        }

        public async Task<ScheduleMetrics> GetScheduleMetricsAsync(DateTime start, DateTime end)
        {
            try
            {
                var appointments = await _schedulingRepository.GetAppointmentsInRangeAsync(start, end);
                
                var metrics = new ScheduleMetrics
                {
                    TotalAppointments = appointments.Count(),
                    CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                    CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                    NoShowAppointments = appointments.Count(a => a.Status == AppointmentStatus.NoShow),
                    UtilizationRate = CalculateUtilizationRate(appointments),
                    AverageAppointmentDuration = CalculateAverageAppointmentDuration(appointments),
                    Period = new DateRange { Start = start, End = end }
                };

                return metrics;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error getting schedule metrics for period {Start} to {End}", start, end);
                throw;
            }
        }

        // Private helper methods
        private async Task<bool> CheckProviderAvailabilityAsync(int providerId, DateTime start, DateTime end)
        {
            var schedule = await _schedulingRepository.GetProviderScheduleAsync(providerId, start.Date);
            return !schedule.Any(a => 
                (a.StartTime <= start && a.EndTime > start) ||
                (a.StartTime < end && a.EndTime >= end) ||
                (a.StartTime >= start && a.EndTime <= end));
        }

        private async Task<bool> CheckResourceAvailabilityAsync(
            IEnumerable<int> resourceIds,
            DateTime start,
            DateTime end)
        {
            foreach (var resourceId in resourceIds)
            {
                var isAvailable = await _schedulingRepository.IsResourceAvailableAsync(resourceId, start, end);
                if (!isAvailable)
                    return false;
            }
            return true;
        }

        private async Task<IEnumerable<string>> CheckForConflictsAsync(Appointment appointment)
        {
            var conflicts = new List<string>();

            // Check provider double-booking
            var providerConflicts = await _schedulingRepository.GetProviderConflictsAsync(
                appointment.ProviderId,
                appointment.StartTime,
                appointment.EndTime
            );
            if (providerConflicts.Any())
                conflicts.Add("Provider double-booking");

            // Check resource conflicts
            if (appointment.RequiredResources?.Any() == true)
            {
                var resourceConflicts = await _schedulingRepository.GetResourceConflictsAsync(
                    appointment.RequiredResources,
                    appointment.StartTime,
                    appointment.EndTime
                );
                if (resourceConflicts.Any())
                    conflicts.Add("Resource conflict");
            }

            return conflicts;
        }

        private async Task<bool> CheckCancellationPolicyAsync(Appointment appointment)
        {
            // Implementation of cancellation policy rules
            var hoursUntilAppointment = (appointment.StartTime - DateTime.UtcNow).TotalHours;
            return hoursUntilAppointment >= 24; // Simple 24-hour cancellation policy
        }

        private async Task ProcessWaitlistAsync(int providerId, DateTime start, DateTime end)
        {
            var waitlistEntries = await _schedulingRepository.GetWaitlistEntriesAsync(providerId);
            foreach (var entry in waitlistEntries)
            {
                if (IsTimeSlotCompatible(entry, start, end))
                {
                    await NotifyWaitlistedPatientAsync(entry, start, end);
                    break;
                }
            }
        }

        private bool IsTimeSlotCompatible(WaitlistEntry entry, DateTime start, DateTime end)
        {
            return entry.PreferredTimeRanges.Any(r => 
                r.Start.TimeOfDay <= start.TimeOfDay && 
                r.End.TimeOfDay >= end.TimeOfDay);
        }

        private async Task NotifyWaitlistedPatientAsync(WaitlistEntry entry, DateTime start, DateTime end)
        {
            // Implementation of patient notification logic
        }

        private decimal CalculateUtilizationRate(IEnumerable<Appointment> appointments)
        {
            if (!appointments.Any())
                return 0;

            var completedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed);
            return (decimal)completedAppointments / appointments.Count() * 100;
        }

        private TimeSpan CalculateAverageAppointmentDuration(IEnumerable<Appointment> appointments)
        {
            if (!appointments.Any())
                return TimeSpan.Zero;

            var totalMinutes = appointments.Average(a => (a.EndTime - a.StartTime).TotalMinutes);
            return TimeSpan.FromMinutes(totalMinutes);
        }
    }
}
