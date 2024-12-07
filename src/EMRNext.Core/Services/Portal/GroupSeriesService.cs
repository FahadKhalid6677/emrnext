using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities.Portal;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Data;

namespace EMRNext.Core.Services.Portal
{
    public class GroupSeriesService : IGroupSeriesService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;

        public GroupSeriesService(
            ApplicationDbContext context,
            INotificationService notificationService,
            IAuditService auditService)
        {
            _context = context;
            _notificationService = notificationService;
            _auditService = auditService;
        }

        public async Task<GroupSeries> GetSeriesAsync(Guid seriesId)
        {
            return await _context.GroupSeries
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(s => s.Id == seriesId);
        }

        public async Task<IEnumerable<GroupSeries>> GetAllSeriesAsync()
        {
            return await _context.GroupSeries
                .Include(s => s.Appointments)
                .ToListAsync();
        }

        public async Task<GroupSeries> CreateSeriesAsync(GroupSeries series)
        {
            _context.GroupSeries.Add(series);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("GroupSeries", series.Id, "Created");
            return series;
        }

        public async Task<GroupSeries> UpdateSeriesAsync(GroupSeries series)
        {
            var existingSeries = await GetSeriesAsync(series.Id);
            if (existingSeries == null)
                throw new KeyNotFoundException($"Series with ID {series.Id} not found");

            _context.Entry(existingSeries).CurrentValues.SetValues(series);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("GroupSeries", series.Id, "Updated");
            return existingSeries;
        }

        public async Task DeleteSeriesAsync(Guid seriesId)
        {
            var series = await GetSeriesAsync(seriesId);
            if (series == null)
                throw new KeyNotFoundException($"Series with ID {seriesId} not found");

            series.Status = "Deleted";
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("GroupSeries", seriesId, "Deleted");
        }

        public async Task<IEnumerable<GroupAppointment>> GetUpcomingSessionsAsync(Guid seriesId)
        {
            return await _context.GroupAppointments
                .Where(s => s.GroupSeriesId == seriesId && s.StartTime > DateTime.UtcNow)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<GroupParticipant>> GetSessionParticipantsAsync(Guid sessionId)
        {
            return await _context.GroupParticipants
                .Where(p => p.GroupAppointmentId == sessionId)
                .ToListAsync();
        }

        public async Task<GroupParticipant> EnrollParticipantAsync(Guid seriesId, Guid patientId)
        {
            var series = await GetSeriesAsync(seriesId);
            if (series == null)
                throw new KeyNotFoundException($"Series with ID {seriesId} not found");

            // Check if the participant is already enrolled
            var existingParticipant = await _context.GroupParticipants
                .FirstOrDefaultAsync(p => p.GroupAppointment.GroupSeriesId == seriesId && p.PatientId == patientId);

            if (existingParticipant != null)
                throw new InvalidOperationException($"Patient {patientId} is already enrolled in series {seriesId}");

            // Get the first session of the series
            var firstSession = await _context.GroupAppointments
                .Where(s => s.GroupSeriesId == seriesId)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

            if (firstSession == null)
                throw new InvalidOperationException($"No sessions found for series {seriesId}");

            var participant = new GroupParticipant
            {
                GroupAppointmentId = firstSession.Id,
                PatientId = patientId,
                EnrollmentDate = DateTime.UtcNow,
                EnrollmentStatus = "Enrolled",
                Status = "Active",
                IsWaitlisted = false
            };

            _context.GroupParticipants.Add(participant);
            await _context.SaveChangesAsync();
            await _notificationService.SendEnrollmentConfirmationAsync(patientId, seriesId);
            await _auditService.LogActivityAsync("GroupParticipant", participant.Id, "Enrolled");
            return participant;
        }

        public async Task WithdrawParticipantAsync(Guid seriesId, Guid patientId)
        {
            var participant = await _context.GroupParticipants
                .FirstOrDefaultAsync(p => p.GroupAppointment.GroupSeriesId == seriesId && p.PatientId == patientId);

            if (participant == null)
                throw new KeyNotFoundException($"Participant not found in series {seriesId}");

            participant.Status = "Withdrawn";
            participant.EnrollmentStatus = "Withdrawn";
            await _context.SaveChangesAsync();
            await _notificationService.SendWithdrawalConfirmationAsync(patientId, seriesId);
            await _auditService.LogActivityAsync("GroupParticipant", participant.Id, "Withdrawn");
        }

        public async Task<IEnumerable<GroupAppointment>> GenerateSessionsAsync(Guid seriesId, DateTime startDate, int numberOfSessions)
        {
            var series = await GetSeriesAsync(seriesId);
            if (series == null)
                throw new KeyNotFoundException($"Series with ID {seriesId} not found");

            var sessions = new List<GroupAppointment>();
            var currentDate = startDate;

            for (int i = 0; i < numberOfSessions; i++)
            {
                while (IsHoliday(currentDate))
                {
                    currentDate = currentDate.AddDays(1);
                }

                var session = new GroupAppointment
                {
                    GroupSeriesId = seriesId,
                    AppointmentTypeId = series.AppointmentTypeId,
                    Location = series.Location,
                    StartTime = currentDate,
                    EndTime = currentDate.AddHours(1),
                    Status = "Scheduled",
                    IsVirtual = series.IsVirtual,
                    MeetingLink = series.MeetingLink,
                    AllowWaitlist = series.AllowWaitlist
                };

                sessions.Add(session);
                currentDate = currentDate.AddDays(7);
            }

            _context.GroupAppointments.AddRange(sessions);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("GroupSeries", seriesId, $"Generated {numberOfSessions} sessions");
            return sessions;
        }

        public async Task<IEnumerable<GroupAppointment>> RegenerateSessionsAsync(Guid seriesId)
        {
            var series = await GetSeriesAsync(seriesId);
            if (series == null)
                throw new KeyNotFoundException($"Series with ID {seriesId} not found");

            // Delete existing future sessions
            var futureSessions = await _context.GroupAppointments
                .Where(s => s.GroupSeriesId == seriesId && s.StartTime > DateTime.UtcNow)
                .ToListAsync();

            _context.GroupAppointments.RemoveRange(futureSessions);

            // Generate new sessions
            var lastSession = await _context.GroupAppointments
                .Where(s => s.GroupSeriesId == seriesId)
                .OrderByDescending(s => s.StartTime)
                .FirstOrDefaultAsync();

            var startDate = lastSession?.StartTime.AddDays(7) ?? DateTime.UtcNow;
            var remainingSessions = (int)((series.EndDate - startDate).TotalDays / 7) + 1;
            return await GenerateSessionsAsync(seriesId, startDate, remainingSessions);
        }

        public async Task<GroupParticipant> UpdateParticipantStatusAsync(Guid sessionId, Guid patientId, string status)
        {
            var participant = await _context.GroupParticipants
                .FirstOrDefaultAsync(p => p.GroupAppointmentId == sessionId && p.PatientId == patientId);

            if (participant == null)
                throw new KeyNotFoundException($"Participant not found in session {sessionId}");

            participant.Status = status;
            participant.HasAttendanceConfirmed = true;
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("GroupParticipant", participant.Id, $"Status updated to {status}");
            return participant;
        }

        public async Task<SeriesOutcome> RecordSeriesOutcomeAsync(Guid seriesId, Guid patientId, SeriesOutcome outcome)
        {
            var participant = await _context.GroupParticipants
                .FirstOrDefaultAsync(p => p.GroupAppointment.GroupSeriesId == seriesId && p.PatientId == patientId);

            if (participant == null)
                throw new KeyNotFoundException($"Participant not found in series {seriesId}");

            outcome.GroupSeriesId = seriesId;
            outcome.PatientId = patientId;
            _context.SeriesOutcomes.Add(outcome);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("SeriesOutcome", outcome.Id, "Recorded");
            return outcome;
        }

        public async Task<IEnumerable<SeriesOutcome>> GetParticipantOutcomesAsync(Guid seriesId, Guid patientId)
        {
            return await _context.SeriesOutcomes
                .Where(o => o.GroupSeriesId == seriesId && o.PatientId == patientId)
                .ToListAsync();
        }

        public async Task<ParticipantReport> GenerateParticipantReportAsync(Guid seriesId, Guid patientId)
        {
            var participant = await _context.GroupParticipants
                .Include(p => p.GroupAppointment)
                .FirstOrDefaultAsync(p => p.GroupAppointment.GroupSeriesId == seriesId && p.PatientId == patientId);

            if (participant == null)
                throw new KeyNotFoundException($"Participant not found in series {seriesId}");

            var series = await GetSeriesAsync(seriesId);
            var outcomes = await GetParticipantOutcomesAsync(seriesId, patientId);
            var attendanceRate = await CalculateAttendanceRateAsync(seriesId, patientId);

            var seriesReport = await _context.SeriesReports
                .FirstOrDefaultAsync(r => r.GroupSeriesId == seriesId) ?? new SeriesReport
                {
                    GroupSeriesId = seriesId,
                    GeneratedDate = DateTime.UtcNow,
                    ReportType = "Progress",
                    Status = "Draft"
                };

            if (!_context.SeriesReports.Local.Contains(seriesReport))
            {
                _context.SeriesReports.Add(seriesReport);
                await _context.SaveChangesAsync();
            }

            var report = new ParticipantReport
            {
                SeriesReportId = seriesReport.Id,
                ParticipantId = participant.Id,
                SessionsAttended = (int)(attendanceRate * series.Appointments.Count),
                AttendanceRate = attendanceRate,
                ParticipationLevel = participant.ParticipationNotes != null ? "Active" : "Unknown",
                Progress = participant.ProgressNotes,
                Goals = participant.Goals,
                InterventionsApplied = participant.Interventions
            };

            _context.ParticipantReports.Add(report);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("ParticipantReport", report.Id, "Generated");
            return report;
        }

        public async Task<IEnumerable<GroupAppointment>> AdjustForHolidaysAsync(Guid seriesId)
        {
            var sessions = await _context.GroupAppointments
                .Where(s => s.GroupSeriesId == seriesId && s.StartTime > DateTime.UtcNow)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            foreach (var session in sessions)
            {
                while (IsHoliday(session.StartTime))
                {
                    session.StartTime = session.StartTime.AddDays(1);
                    session.EndTime = session.EndTime.AddDays(1);
                }
            }

            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("GroupSeries", seriesId, "Adjusted for holidays");
            return sessions;
        }

        private async Task<double> CalculateAttendanceRateAsync(Guid seriesId, Guid patientId)
        {
            var sessions = await _context.GroupAppointments
                .Where(s => s.GroupSeriesId == seriesId && s.StartTime <= DateTime.UtcNow)
                .ToListAsync();

            var attendedSessions = await _context.GroupParticipants
                .CountAsync(p => p.GroupAppointment.GroupSeriesId == seriesId && 
                               p.PatientId == patientId && 
                               p.Status == "Attended");

            return sessions.Count > 0 ? (double)attendedSessions / sessions.Count : 0;
        }

        private bool IsHoliday(DateTime date)
        {
            // TODO: Implement holiday checking logic
            // This could be expanded to check against a holiday calendar service or database
            return false;
        }
    }
}
