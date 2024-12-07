using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services.Portal
{
    public interface IAppointmentSchedulingService
    {
        Task<IEnumerable<AppointmentSlot>> GetAvailableSlotsAsync(
            int providerId, 
            DateTime startDate, 
            DateTime endDate, 
            string appointmentType);

        Task<Appointment> ScheduleAppointmentAsync(
            int patientId,
            int providerId,
            DateTime appointmentTime,
            string appointmentType,
            string reason);

        Task<bool> CancelAppointmentAsync(
            int appointmentId,
            int patientId,
            string cancellationReason);

        Task<bool> RescheduleAppointmentAsync(
            int appointmentId,
            int patientId,
            DateTime newAppointmentTime);

        Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(
            int patientId,
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<IEnumerable<AppointmentType>> GetAvailableAppointmentTypesAsync(int providerId);

        Task<bool> ConfirmAppointmentAsync(int appointmentId, int patientId);

        Task<bool> SendAppointmentReminderAsync(int appointmentId);

        Task<AppointmentDetails> GetAppointmentDetailsAsync(int appointmentId, int patientId);

        Task<IEnumerable<Provider>> GetAvailableProvidersAsync(
            string specialty,
            DateTime appointmentDate);

        Task<bool> UpdateAppointmentNotesAsync(
            int appointmentId,
            int patientId,
            string notes);

        Task<bool> SetAppointmentPreferencesAsync(
            int patientId,
            AppointmentPreferences preferences);
    }

    public class AppointmentSlot
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public bool IsAvailable { get; set; }
        public string AppointmentType { get; set; }
        public int Duration { get; set; }
    }

    public class AppointmentDetails
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string AppointmentType { get; set; }
        public string Status { get; set; }
        public Provider Provider { get; set; }
        public string Location { get; set; }
        public string PreAppointmentInstructions { get; set; }
        public bool RequiresPreVisitDocuments { get; set; }
        public IEnumerable<string> RequiredDocuments { get; set; }
        public bool HasInsuranceVerification { get; set; }
        public bool IsTelehealth { get; set; }
        public string TelehealthPlatform { get; set; }
        public string TelehealthLink { get; set; }
    }

    public class AppointmentPreferences
    {
        public bool PreferTelehealth { get; set; }
        public List<string> PreferredDays { get; set; }
        public TimeSpan EarliestTime { get; set; }
        public TimeSpan LatestTime { get; set; }
        public List<int> PreferredProviders { get; set; }
        public List<string> PreferredLocations { get; set; }
        public string PreferredLanguage { get; set; }
        public bool NeedsInterpreter { get; set; }
        public bool NeedsTransportation { get; set; }
        public string SpecialAccommodations { get; set; }
    }
}
