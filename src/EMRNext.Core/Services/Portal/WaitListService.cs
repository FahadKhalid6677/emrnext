using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Infrastructure;

namespace EMRNext.Core.Services.Portal
{
    public class WaitListService : IWaitListService
    {
        private readonly EMRNextDbContext _context;
        private readonly IAppointmentSchedulingService _appointmentService;
        private readonly INotificationService _notificationService;
        private readonly IClinicalService _clinicalService;
        private readonly IAuditService _auditService;

        public WaitListService(
            EMRNextDbContext context,
            IAppointmentSchedulingService appointmentService,
            INotificationService notificationService,
            IClinicalService clinicalService,
            IAuditService auditService)
        {
            _context = context;
            _appointmentService = appointmentService;
            _notificationService = notificationService;
            _clinicalService = clinicalService;
            _auditService = auditService;
        }

        public async Task<WaitListEntry> AddToWaitListAsync(
            int patientId,
            int appointmentTypeId,
            DateTime earliestDate,
            DateTime? latestDate = null,
            int? preferredProviderId = null,
            string notes = null)
        {
            // Validate patient and appointment type
            var patient = await _context.Patients.FindAsync(patientId);
            var appointmentType = await _context.AppointmentTypes.FindAsync(appointmentTypeId);

            if (patient == null || appointmentType == null)
            {
                throw new ArgumentException("Invalid patient or appointment type");
            }

            // Calculate priority based on clinical factors
            var priority = await CalculatePriorityAsync(patientId, appointmentTypeId, DateTime.UtcNow);

            var entry = new WaitListEntry
            {
                PatientId = patientId,
                AppointmentTypeId = appointmentTypeId,
                PreferredProviderId = preferredProviderId,
                EarliestDate = earliestDate,
                LatestDate = latestDate,
                Priority = priority.Priority,
                Status = "Active",
                Notes = notes,
                CreatedDate = DateTime.UtcNow
            };

            _context.WaitListEntries.Add(entry);
            await _context.SaveChangesAsync();

            // Log the activity
            await _auditService.LogActivityAsync(
                patientId,
                "WaitListAdded",
                $"Added to wait list for {appointmentType.Name}",
                $"Priority: {priority.Priority}, Factors: {string.Join(", ", priority.Factors)}"
            );

            // Notify patient
            await _notificationService.SendWaitListConfirmationAsync(entry);

            // Process wait list to check for immediate matches
            await ProcessWaitListAsync();

            return entry;
        }

        public async Task<bool> UpdatePriorityAsync(int waitListEntryId, int newPriority)
        {
            var entry = await _context.WaitListEntries.FindAsync(waitListEntryId);
            if (entry == null)
            {
                return false;
            }

            entry.Priority = newPriority;
            entry.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log priority change
            await _auditService.LogActivityAsync(
                entry.PatientId,
                "WaitListPriorityUpdated",
                $"Wait list priority updated for entry {waitListEntryId}",
                $"New Priority: {newPriority}"
            );

            return true;
        }

        public async Task<WaitListPriority> CalculatePriorityAsync(
            int patientId,
            int appointmentTypeId,
            DateTime requestDate)
        {
            var factors = new Dictionary<string, int>();
            var totalPriority = 0;

            // Get clinical factors
            var clinicalPriority = await _clinicalService.GetPatientPriorityFactorsAsync(patientId);
            factors.Add("Clinical", clinicalPriority.Score);
            totalPriority += clinicalPriority.Score;

            // Check wait time
            var existingEntry = await _context.WaitListEntries
                .FirstOrDefaultAsync(w => w.PatientId == patientId && w.Status == "Active");
            
            if (existingEntry != null)
            {
                var waitDays = (DateTime.UtcNow - existingEntry.CreatedDate).Days;
                var waitScore = Math.Min(waitDays * 2, 50); // Cap at 50
                factors.Add("WaitTime", waitScore);
                totalPriority += waitScore;
            }

            // Check appointment urgency
            var appointmentType = await _context.AppointmentTypes.FindAsync(appointmentTypeId);
            if (appointmentType.RequiresPreAuth)
            {
                factors.Add("Urgency", 30);
                totalPriority += 30;
            }

            // Consider cancellations
            var recentCancellations = await _context.Appointments
                .CountAsync(a => 
                    a.PatientId == patientId && 
                    a.Status == "Cancelled" &&
                    a.CancellationDate >= DateTime.UtcNow.AddMonths(-3));
            
            if (recentCancellations > 0)
            {
                var cancellationPenalty = -10 * recentCancellations;
                factors.Add("Cancellations", cancellationPenalty);
                totalPriority += cancellationPenalty;
            }

            return new WaitListPriority
            {
                Priority = Math.Max(totalPriority, 1), // Ensure minimum priority of 1
                Factors = factors,
                Reason = $"Based on clinical factors ({clinicalPriority.Score}), " +
                        $"wait time, urgency, and history"
            };
        }

        public async Task ProcessWaitListAsync()
        {
            var activeEntries = await _context.WaitListEntries
                .Where(w => w.Status == "Active")
                .OrderByDescending(w => w.Priority)
                .ThenBy(w => w.CreatedDate)
                .ToListAsync();

            foreach (var entry in activeEntries)
            {
                var availableSlots = await FindMatchingSlotsAsync(entry.Id);
                if (!availableSlots.Any())
                {
                    continue;
                }

                foreach (var slot in availableSlots)
                {
                    if (await OfferSlotToPatientAsync(entry.Id, slot))
                    {
                        break; // Move to next patient once a slot is offered
                    }
                }
            }
        }

        public async Task<IEnumerable<AppointmentSlot>> FindMatchingSlotsAsync(int waitListEntryId)
        {
            var entry = await _context.WaitListEntries
                .Include(w => w.AppointmentType)
                .FirstOrDefaultAsync(w => w.Id == waitListEntryId);

            if (entry == null)
            {
                return Enumerable.Empty<AppointmentSlot>();
            }

            var slots = await _appointmentService.GetAvailableSlotsAsync(
                entry.PreferredProviderId ?? 0,
                entry.EarliestDate,
                entry.LatestDate ?? entry.EarliestDate.AddMonths(3),
                entry.AppointmentType.Name);

            return slots.Where(s => 
                (entry.PreferredProviderId == null || s.ProviderId == entry.PreferredProviderId) &&
                s.StartTime >= entry.EarliestDate &&
                (!entry.LatestDate.HasValue || s.StartTime <= entry.LatestDate.Value));
        }

        public async Task<bool> OfferSlotToPatientAsync(int waitListEntryId, AppointmentSlot slot)
        {
            var entry = await _context.WaitListEntries
                .Include(w => w.Patient)
                .FirstOrDefaultAsync(w => w.Id == waitListEntryId);

            if (entry == null)
            {
                return false;
            }

            // Create notification
            var notification = new WaitListNotification
            {
                WaitListEntryId = waitListEntryId,
                NotificationType = "SlotOffer",
                Message = $"Appointment slot available on {slot.StartTime:g} with Dr. {slot.ProviderName}",
                ExpiryDate = DateTime.UtcNow.AddHours(24),
                CreatedDate = DateTime.UtcNow
            };

            _context.WaitListNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send notification
            await _notificationService.SendWaitListSlotOfferAsync(
                entry.Patient.Email,
                slot,
                notification.ExpiryDate);

            return true;
        }

        public async Task<bool> AcceptOfferedSlotAsync(int waitListEntryId, int appointmentSlotId)
        {
            var entry = await _context.WaitListEntries.FindAsync(waitListEntryId);
            if (entry == null)
            {
                return false;
            }

            // Schedule the appointment
            var slot = await _context.AppointmentSlots.FindAsync(appointmentSlotId);
            if (slot == null)
            {
                return false;
            }

            await _appointmentService.ScheduleAppointmentAsync(
                entry.PatientId,
                slot.ProviderId,
                slot.StartTime,
                entry.AppointmentType.Name,
                entry.Notes);

            // Update wait list entry
            entry.Status = "Completed";
            entry.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log the acceptance
            await _auditService.LogActivityAsync(
                entry.PatientId,
                "WaitListSlotAccepted",
                $"Accepted wait list slot for {slot.StartTime:g}",
                null
            );

            return true;
        }

        public async Task<bool> DeclineOfferedSlotAsync(
            int waitListEntryId,
            int appointmentSlotId,
            string reason)
        {
            var entry = await _context.WaitListEntries.FindAsync(waitListEntryId);
            if (entry == null)
            {
                return false;
            }

            // Log the decline
            await _auditService.LogActivityAsync(
                entry.PatientId,
                "WaitListSlotDeclined",
                $"Declined wait list slot",
                reason
            );

            // Update priority based on declines
            var newPriority = await CalculatePriorityAsync(
                entry.PatientId,
                entry.AppointmentTypeId,
                DateTime.UtcNow);

            entry.Priority = newPriority.Priority;
            entry.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // Implementation of other interface methods...
    }
}
