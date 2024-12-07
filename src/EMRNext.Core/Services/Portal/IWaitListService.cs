using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services.Portal
{
    public interface IWaitListService
    {
        Task<WaitListEntry> AddToWaitListAsync(
            int patientId,
            int appointmentTypeId,
            DateTime earliestDate,
            DateTime? latestDate = null,
            int? preferredProviderId = null,
            string notes = null);

        Task<bool> RemoveFromWaitListAsync(int waitListEntryId);

        Task<bool> UpdateWaitListEntryAsync(
            int waitListEntryId,
            DateTime? earliestDate = null,
            DateTime? latestDate = null,
            int? preferredProviderId = null,
            string notes = null);

        Task<bool> UpdatePriorityAsync(int waitListEntryId, int newPriority);

        Task<IEnumerable<WaitListEntry>> GetPatientWaitListEntriesAsync(int patientId);

        Task<IEnumerable<WaitListEntry>> GetWaitListByAppointmentTypeAsync(
            int appointmentTypeId,
            string status = null);

        Task<IEnumerable<WaitListEntry>> GetWaitListByProviderAsync(
            int providerId,
            string status = null);

        Task<IEnumerable<AppointmentSlot>> FindMatchingSlotsAsync(int waitListEntryId);

        Task<bool> OfferSlotToPatientAsync(int waitListEntryId, AppointmentSlot slot);

        Task<bool> AcceptOfferedSlotAsync(int waitListEntryId, int appointmentSlotId);

        Task<bool> DeclineOfferedSlotAsync(int waitListEntryId, int appointmentSlotId, string reason);

        Task<WaitListPriority> CalculatePriorityAsync(
            int patientId,
            int appointmentTypeId,
            DateTime requestDate);

        Task ProcessWaitListAsync();

        Task<IEnumerable<WaitListNotification>> GetPendingNotificationsAsync(int patientId);

        Task<bool> MarkNotificationAsReadAsync(int notificationId);
    }

    public class WaitListEntry
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int AppointmentTypeId { get; set; }
        public int? PreferredProviderId { get; set; }
        public DateTime EarliestDate { get; set; }
        public DateTime? LatestDate { get; set; }
        public int Priority { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual AppointmentType AppointmentType { get; set; }
        public virtual Provider PreferredProvider { get; set; }
    }

    public class WaitListPriority
    {
        public int Priority { get; set; }
        public string Reason { get; set; }
        public Dictionary<string, int> Factors { get; set; }
    }

    public class WaitListNotification
    {
        public int Id { get; set; }
        public int WaitListEntryId { get; set; }
        public string NotificationType { get; set; }
        public string Message { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
