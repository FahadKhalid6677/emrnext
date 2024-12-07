using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Portal;

namespace EMRNext.Core.Interfaces
{
    public interface IGroupSeriesService
    {
        Task<GroupSeries> GetSeriesAsync(Guid seriesId);
        Task<IEnumerable<GroupSeries>> GetAllSeriesAsync();
        Task<GroupSeries> CreateSeriesAsync(GroupSeries series);
        Task<GroupSeries> UpdateSeriesAsync(GroupSeries series);
        Task DeleteSeriesAsync(Guid seriesId);
        Task<IEnumerable<GroupAppointment>> GetUpcomingSessionsAsync(Guid seriesId);
        Task<IEnumerable<GroupParticipant>> GetSessionParticipantsAsync(Guid sessionId);
        Task<GroupParticipant> EnrollParticipantAsync(Guid seriesId, Guid patientId);
        Task WithdrawParticipantAsync(Guid seriesId, Guid patientId);
        Task<IEnumerable<GroupAppointment>> GenerateSessionsAsync(Guid seriesId, DateTime startDate, int numberOfSessions);
        Task<IEnumerable<GroupAppointment>> RegenerateSessionsAsync(Guid seriesId);
        Task<GroupParticipant> UpdateParticipantStatusAsync(Guid sessionId, Guid patientId, string status);
        Task<SeriesOutcome> RecordSeriesOutcomeAsync(Guid seriesId, Guid patientId, SeriesOutcome outcome);
        Task<IEnumerable<SeriesOutcome>> GetParticipantOutcomesAsync(Guid seriesId, Guid patientId);
        Task<ParticipantReport> GenerateParticipantReportAsync(Guid seriesId, Guid patientId);
        Task<IEnumerable<GroupAppointment>> AdjustForHolidaysAsync(Guid seriesId);
    }
}
