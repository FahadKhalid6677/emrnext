using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Portal;

namespace EMRNext.Core.Interfaces
{
    public interface IGroupAppointmentService
    {
        Task<GroupAppointment> GetAppointmentAsync(Guid appointmentId);
        Task<GroupAppointment> UpdateAppointmentAsync(GroupAppointment appointment);
        Task CancelAppointmentAsync(Guid appointmentId, string reason);
        Task<GroupParticipant> AddParticipantAsync(Guid appointmentId, Guid patientId);
        Task RemoveParticipantAsync(Guid appointmentId, Guid patientId);
        Task<IEnumerable<GroupParticipant>> GetParticipantsAsync(Guid appointmentId);
        Task<bool> IsParticipantEnrolledAsync(Guid appointmentId, Guid patientId);
        Task<GroupParticipant> UpdateParticipantStatusAsync(Guid appointmentId, Guid patientId, string status);
        Task<bool> HasAvailableSpaceAsync(Guid appointmentId);
        Task<int> GetWaitlistPositionAsync(Guid appointmentId, Guid patientId);
        Task<IEnumerable<GroupAppointment>> GetUpcomingAppointmentsAsync(Guid patientId);
        Task<GroupAppointment> UpdateVirtualSettingsAsync(Guid appointmentId, bool isVirtual, string meetingLink);
        Task<bool> ValidateAppointmentTimeAsync(DateTime startTime, DateTime endTime);
    }
}
