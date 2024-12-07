using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Infrastructure;
using EMRNext.Core.Services.Scheduling;

namespace EMRNext.Core.Services.Portal
{
    public class AppointmentSchedulingService : IAppointmentSchedulingService
    {
        private readonly EMRNextDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IProviderScheduleService _providerScheduleService;
        private readonly IInsuranceVerificationService _insuranceService;

        public AppointmentSchedulingService(
            EMRNextDbContext context,
            INotificationService notificationService,
            IAuditService auditService,
            IProviderScheduleService providerScheduleService,
            IInsuranceVerificationService insuranceService)
        {
            _context = context;
            _notificationService = notificationService;
            _auditService = auditService;
            _providerScheduleService = providerScheduleService;
            _insuranceService = insuranceService;
        }

        public async Task<IEnumerable<AppointmentSlot>> GetAvailableSlotsAsync(
            int providerId,
            DateTime startDate,
            DateTime endDate,
            string appointmentType)
        {
            var provider = await _context.Providers
                .Include(p => p.Schedule)
                .FirstOrDefaultAsync(p => p.Id == providerId);

            if (provider == null)
            {
                throw new ArgumentException("Provider not found");
            }

            var appointmentTypeInfo = await _context.AppointmentTypes
                .FirstOrDefaultAsync(at => at.Name == appointmentType);

            if (appointmentTypeInfo == null)
            {
                throw new ArgumentException("Invalid appointment type");
            }

            var existingAppointments = await _context.Appointments
                .Where(a => 
                    a.ProviderId == providerId &&
                    a.AppointmentTime >= startDate &&
                    a.AppointmentTime <= endDate &&
                    a.Status != "Cancelled")
                .ToListAsync();

            var availableSlots = await _providerScheduleService.GetAvailableTimeSlotsAsync(
                providerId,
                startDate,
                endDate,
                appointmentTypeInfo.Duration);

            return availableSlots.Select(slot => new AppointmentSlot
            {
                StartTime = slot.StartTime,
                EndTime = slot.StartTime.AddMinutes(appointmentTypeInfo.Duration),
                ProviderId = providerId,
                ProviderName = $"{provider.FirstName} {provider.LastName}",
                IsAvailable = true,
                AppointmentType = appointmentType,
                Duration = appointmentTypeInfo.Duration
            });
        }

        public async Task<Appointment> ScheduleAppointmentAsync(
            int patientId,
            int providerId,
            DateTime appointmentTime,
            string appointmentType,
            string reason)
        {
            // Verify the slot is still available
            var slots = await GetAvailableSlotsAsync(
                providerId,
                appointmentTime.Date,
                appointmentTime.Date.AddDays(1),
                appointmentType);

            if (!slots.Any(s => s.StartTime == appointmentTime))
            {
                throw new InvalidOperationException("Selected time slot is no longer available");
            }

            // Verify insurance if needed
            var insuranceVerification = await _insuranceService.VerifyInsuranceAsync(patientId);
            
            var appointment = new Appointment
            {
                PatientId = patientId,
                ProviderId = providerId,
                AppointmentTime = appointmentTime,
                AppointmentType = appointmentType,
                Reason = reason,
                Status = "Scheduled",
                CreatedDate = DateTime.UtcNow,
                InsuranceVerified = insuranceVerification.IsVerified
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Send notifications
            await _notificationService.SendAppointmentConfirmationAsync(appointment);
            
            // Log the activity
            await _auditService.LogActivityAsync(
                patientId,
                "AppointmentScheduled",
                $"Appointment scheduled with Dr. {appointment.Provider.LastName}",
                $"AppointmentId: {appointment.Id}"
            );

            return appointment;
        }

        public async Task<bool> CancelAppointmentAsync(
            int appointmentId,
            int patientId,
            string cancellationReason)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => 
                    a.Id == appointmentId && 
                    a.PatientId == patientId);

            if (appointment == null)
            {
                return false;
            }

            // Check cancellation policy
            var hoursUntilAppointment = (appointment.AppointmentTime - DateTime.UtcNow).TotalHours;
            if (hoursUntilAppointment < 24)
            {
                throw new InvalidOperationException("Appointments must be cancelled at least 24 hours in advance");
            }

            appointment.Status = "Cancelled";
            appointment.CancellationReason = cancellationReason;
            appointment.CancellationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify provider
            await _notificationService.SendAppointmentCancellationAsync(appointment);

            // Log the cancellation
            await _auditService.LogActivityAsync(
                patientId,
                "AppointmentCancelled",
                $"Appointment {appointmentId} cancelled",
                cancellationReason
            );

            return true;
        }

        public async Task<bool> RescheduleAppointmentAsync(
            int appointmentId,
            int patientId,
            DateTime newAppointmentTime)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => 
                    a.Id == appointmentId && 
                    a.PatientId == patientId);

            if (appointment == null)
            {
                return false;
            }

            // Verify new slot is available
            var slots = await GetAvailableSlotsAsync(
                appointment.ProviderId,
                newAppointmentTime.Date,
                newAppointmentTime.Date.AddDays(1),
                appointment.AppointmentType);

            if (!slots.Any(s => s.StartTime == newAppointmentTime))
            {
                throw new InvalidOperationException("Selected time slot is not available");
            }

            var oldTime = appointment.AppointmentTime;
            appointment.AppointmentTime = newAppointmentTime;
            appointment.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send notifications
            await _notificationService.SendAppointmentRescheduleAsync(appointment, oldTime);

            // Log the change
            await _auditService.LogActivityAsync(
                patientId,
                "AppointmentRescheduled",
                $"Appointment {appointmentId} rescheduled",
                $"From: {oldTime}, To: {newAppointmentTime}"
            );

            return true;
        }

        public async Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(
            int patientId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.Appointments
                .Include(a => a.Provider)
                .Where(a => a.PatientId == patientId);

            if (startDate.HasValue)
            {
                query = query.Where(a => a.AppointmentTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.AppointmentTime <= endDate.Value);
            }

            return await query
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentType>> GetAvailableAppointmentTypesAsync(int providerId)
        {
            return await _context.AppointmentTypes
                .Where(at => at.ProviderId == providerId || at.ProviderId == null)
                .OrderBy(at => at.Name)
                .ToListAsync();
        }

        public async Task<bool> ConfirmAppointmentAsync(int appointmentId, int patientId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => 
                    a.Id == appointmentId && 
                    a.PatientId == patientId);

            if (appointment == null)
            {
                return false;
            }

            appointment.Status = "Confirmed";
            appointment.ConfirmationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send confirmation notification
            await _notificationService.SendAppointmentConfirmedAsync(appointment);

            return true;
        }

        public async Task<bool> SendAppointmentReminderAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Provider)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return false;
            }

            await _notificationService.SendAppointmentReminderAsync(appointment);
            return true;
        }

        public async Task<AppointmentDetails> GetAppointmentDetailsAsync(int appointmentId, int patientId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Provider)
                .Include(a => a.Location)
                .FirstOrDefaultAsync(a => 
                    a.Id == appointmentId && 
                    a.PatientId == patientId);

            if (appointment == null)
            {
                return null;
            }

            var requiredDocuments = await _context.AppointmentDocuments
                .Where(ad => ad.AppointmentTypeId == appointment.AppointmentTypeId)
                .Select(ad => ad.DocumentName)
                .ToListAsync();

            return new AppointmentDetails
            {
                AppointmentId = appointment.Id,
                AppointmentTime = appointment.AppointmentTime,
                AppointmentType = appointment.AppointmentType,
                Status = appointment.Status,
                Provider = appointment.Provider,
                Location = appointment.Location?.Name,
                PreAppointmentInstructions = appointment.PreAppointmentInstructions,
                RequiresPreVisitDocuments = requiredDocuments.Any(),
                RequiredDocuments = requiredDocuments,
                HasInsuranceVerification = appointment.InsuranceVerified,
                IsTelehealth = appointment.IsTelehealth,
                TelehealthPlatform = appointment.TelehealthPlatform,
                TelehealthLink = appointment.TelehealthLink
            };
        }

        public async Task<IEnumerable<Provider>> GetAvailableProvidersAsync(
            string specialty,
            DateTime appointmentDate)
        {
            return await _context.Providers
                .Include(p => p.Schedule)
                .Where(p => 
                    p.Specialty == specialty &&
                    p.Schedule.Any(s => 
                        s.DayOfWeek == appointmentDate.DayOfWeek &&
                        s.IsAvailable))
                .OrderBy(p => p.LastName)
                .ToListAsync();
        }

        public async Task<bool> UpdateAppointmentNotesAsync(
            int appointmentId,
            int patientId,
            string notes)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => 
                    a.Id == appointmentId && 
                    a.PatientId == patientId);

            if (appointment == null)
            {
                return false;
            }

            appointment.PatientNotes = notes;
            appointment.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetAppointmentPreferencesAsync(
            int patientId,
            AppointmentPreferences preferences)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
            {
                return false;
            }

            // Update patient preferences
            patient.AppointmentPreferences = preferences;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
