using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services.Portal
{
    public interface IGroupAppointmentService
    {
        Task<GroupAppointment> CreateGroupSessionAsync(
            string name,
            string description,
            int providerId,
            int locationId,
            int appointmentTypeId,
            DateTime startTime,
            DateTime endTime,
            int maxParticipants,
            int minParticipants,
            GroupSessionRequirements requirements);

        Task<bool> UpdateGroupSessionAsync(
            int groupAppointmentId,
            string name = null,
            string description = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? maxParticipants = null,
            int? minParticipants = null);

        Task<bool> CancelGroupSessionAsync(
            int groupAppointmentId,
            string reason,
            bool notifyParticipants = true);

        Task<GroupParticipant> AddParticipantAsync(
            int groupAppointmentId,
            int patientId,
            ParticipantRequirements requirements = null);

        Task<bool> RemoveParticipantAsync(
            int groupAppointmentId,
            int patientId,
            string reason);

        Task<bool> UpdateParticipantStatusAsync(
            int groupAppointmentId,
            int patientId,
            string status);

        Task<bool> RecordAttendanceAsync(
            int groupAppointmentId,
            int patientId,
            bool attended,
            string notes = null);

        Task<IEnumerable<GroupAppointment>> GetUpcomingGroupSessionsAsync(
            int? providerId = null,
            int? locationId = null,
            string sessionType = null);

        Task<IEnumerable<GroupParticipant>> GetSessionParticipantsAsync(
            int groupAppointmentId);

        Task<bool> ValidateGroupCompositionAsync(
            int groupAppointmentId);

        Task<GroupSessionCapacity> CheckGroupCapacityAsync(
            int groupAppointmentId);

        Task<bool> AssignBackupProviderAsync(
            int groupAppointmentId,
            int backupProviderId);

        Task<bool> UpdateGroupMaterialsAsync(
            int groupAppointmentId,
            GroupSessionMaterials materials);

        Task<bool> SendGroupNotificationAsync(
            int groupAppointmentId,
            string message,
            bool includeProviders = true);

        Task<GroupSessionReport> GenerateSessionReportAsync(
            int groupAppointmentId);

        Task ProcessWaitlistedParticipantsAsync(
            int groupAppointmentId);
    }

    public class GroupSessionRequirements
    {
        public string ClinicalCriteria { get; set; }
        public string SkillLevel { get; set; }
        public List<string> RequiredEquipment { get; set; }
        public bool RequiresPreAssessment { get; set; }
        public List<string> PrerequisiteSessions { get; set; }
        public List<string> Languages { get; set; }
        public bool AccessibilitySupport { get; set; }
        public Dictionary<string, string> SpecialInstructions { get; set; }
    }

    public class ParticipantRequirements
    {
        public bool NeedsInterpreter { get; set; }
        public string PreferredLanguage { get; set; }
        public bool NeedsAccessibilitySupport { get; set; }
        public string AccessibilityRequirements { get; set; }
        public string ClinicalNotes { get; set; }
        public List<string> SpecialRequirements { get; set; }
    }

    public class GroupSessionCapacity
    {
        public int TotalCapacity { get; set; }
        public int CurrentParticipants { get; set; }
        public int WaitlistedParticipants { get; set; }
        public bool HasAvailableSpots { get; set; }
        public int RemainingSpots { get; set; }
        public bool MeetsMinimumRequirement { get; set; }
    }

    public class GroupSessionMaterials
    {
        public string SessionInstructions { get; set; }
        public List<string> RequiredForms { get; set; }
        public List<string> HandoutMaterials { get; set; }
        public string PreSessionPreparation { get; set; }
        public string PostSessionInstructions { get; set; }
        public Dictionary<string, string> Resources { get; set; }
    }

    public class GroupSessionReport
    {
        public int GroupAppointmentId { get; set; }
        public DateTime SessionDate { get; set; }
        public string SessionName { get; set; }
        public string ProviderName { get; set; }
        public int TotalParticipants { get; set; }
        public int AttendedParticipants { get; set; }
        public List<ParticipantAttendance> Attendance { get; set; }
        public string SessionNotes { get; set; }
        public List<string> Outcomes { get; set; }
        public List<string> FollowUpActions { get; set; }
    }

    public class ParticipantAttendance
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public bool Attended { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
    }
}
