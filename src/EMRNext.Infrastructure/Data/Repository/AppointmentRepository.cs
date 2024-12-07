using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;

namespace EMRNext.Infrastructure.Data.Repository
{
    public class AppointmentRepository : BaseRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(EMRDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(int patientId)
        {
            return await _dbSet
                .Where(a => a.PatientId == patientId && !a.IsDeleted)
                .OrderByDescending(a => a.StartTime)
                .Include(a => a.Provider)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int patientId)
        {
            var currentDate = DateTime.UtcNow;
            return await _dbSet
                .Where(a => 
                    a.PatientId == patientId && 
                    !a.IsDeleted &&
                    a.StartTime > currentDate &&
                    !a.IsCancelled)
                .OrderBy(a => a.StartTime)
                .Include(a => a.Provider)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetProviderAppointmentsAsync(int providerId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            return await _dbSet
                .Where(a => 
                    a.ProviderId == providerId && 
                    !a.IsDeleted &&
                    !a.IsCancelled &&
                    a.StartTime >= startOfDay &&
                    a.StartTime <= endOfDay)
                .OrderBy(a => a.StartTime)
                .Include(a => a.Patient)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetProviderScheduleAsync(int providerId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(a => 
                    a.ProviderId == providerId && 
                    !a.IsDeleted &&
                    a.StartTime >= startDate &&
                    a.StartTime <= endDate)
                .OrderBy(a => a.StartTime)
                .Include(a => a.Patient)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetCancelledAppointmentsAsync(int patientId)
        {
            return await _dbSet
                .Where(a => 
                    a.PatientId == patientId && 
                    !a.IsDeleted &&
                    a.IsCancelled)
                .OrderByDescending(a => a.CancellationDate)
                .Include(a => a.Provider)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeSpan>> GetAvailableTimeSlotsAsync(int providerId, DateTime date)
        {
            // Get provider's schedule for the day
            var appointments = await GetProviderAppointmentsAsync(providerId, date);
            
            // Define working hours (e.g., 9 AM to 5 PM)
            var workStart = new TimeSpan(9, 0, 0);
            var workEnd = new TimeSpan(17, 0, 0);
            var slotDuration = TimeSpan.FromMinutes(30); // 30-minute slots

            var availableSlots = new List<TimeSpan>();
            var currentSlot = workStart;

            while (currentSlot + slotDuration <= workEnd)
            {
                var slotStartTime = date.Date + currentSlot;
                var slotEndTime = slotStartTime + slotDuration;

                var isSlotAvailable = !appointments.Any(a =>
                    (a.StartTime < slotEndTime && a.EndTime > slotStartTime));

                if (isSlotAvailable)
                {
                    availableSlots.Add(currentSlot);
                }

                currentSlot = currentSlot.Add(slotDuration);
            }

            return availableSlots;
        }

        public async Task<IEnumerable<Appointment>> SearchAppointmentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Where(a => 
                    a.AppointmentReason.Contains(searchTerm) ||
                    a.ChiefComplaint.Contains(searchTerm) ||
                    a.Location.Contains(searchTerm))
                .OrderByDescending(a => a.StartTime)
                .Include(a => a.Provider)
                .Include(a => a.Patient)
                .ToListAsync();
        }
    }
}
