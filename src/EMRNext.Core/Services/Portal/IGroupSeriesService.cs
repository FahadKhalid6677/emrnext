using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services.Portal
{
    public interface IGroupSeriesService
    {
        // Series Management
        Task<GroupSeries> CreateSeriesAsync(GroupSeries series);
        Task<GroupSeries> GetSeriesAsync(int seriesId);
        Task<GroupSeries> UpdateSeriesAsync(GroupSeries series);
        Task<bool> DeleteSeriesAsync(int seriesId);
        
        // Session Generation
        Task<IEnumerable<GroupAppointment>> GenerateSessionsAsync(int seriesId);
        Task<bool> RegenerateSessionsAsync(int seriesId, DateTime fromDate);
        Task<bool> AdjustForHolidaysAsync(int seriesId);
        
        // Participant Management
        Task<SeriesParticipant> EnrollParticipantAsync(int seriesId, int patientId);
        Task<bool> UpdateParticipantStatusAsync(int seriesId, int patientId, string status);
        Task<bool> WithdrawParticipantAsync(int seriesId, int patientId);
        
        // Progress Tracking
        Task<SeriesParticipant> UpdateParticipantProgressAsync(int seriesId, int patientId, string progress);
        Task<IEnumerable<ParticipantOutcome>> GetParticipantOutcomesAsync(int seriesId, int patientId);
        Task<bool> RecordParticipantOutcomeAsync(ParticipantOutcome outcome);
        
        // Series Outcomes
        Task<bool> RecordSeriesOutcomeAsync(SeriesOutcome outcome);
        Task<IEnumerable<SeriesOutcome>> GetSeriesOutcomesAsync(int seriesId);
        
        // Scheduling
        Task<bool> HandleScheduleConflictAsync(int sessionId, string resolution);
        Task<bool> OptimizeSeriesScheduleAsync(int seriesId);
        
        // Reporting
        Task<SeriesReport> GenerateSeriesReportAsync(int seriesId);
        Task<ParticipantReport> GenerateParticipantReportAsync(int seriesId, int patientId);
    }
}
