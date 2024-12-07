using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Entities.Portal;
using EMRNext.Core.Infrastructure;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Data;

namespace EMRNext.Core.Services.Portal
{
    public class GroupAppointmentService : IGroupAppointmentService
    {
        private readonly EMRNextDbContext _context;
        private readonly IResourceManagementService _resourceService;
        private readonly IClinicalService _clinicalService;
        private readonly INotificationService _notificationService;
        private readonly IDocumentService _documentService;
        private readonly IAuditService _auditService;

        public GroupAppointmentService(
            EMRNextDbContext context,
            IResourceManagementService resourceService,
            IClinicalService clinicalService,
            INotificationService notificationService,
            IDocumentService documentService,
            IAuditService auditService)
        {
            _context = context;
            _resourceService = resourceService;
            _clinicalService = clinicalService;
            _notificationService = notificationService;
            _documentService = documentService;
            _auditService = auditService;
        }

        public async Task<GroupAppointment> CreateGroupSessionAsync(
            string name,
            string description,
            int providerId,
            int locationId,
            int appointmentTypeId,
            DateTime startTime,
            DateTime endTime,
            int maxParticipants,
            int minParticipants,
            GroupSessionRequirements requirements)
        {
            // Validate provider availability
            var providerAvailable = await ValidateProviderAvailabilityAsync(providerId, startTime, endTime);
            if (!providerAvailable)
            {
                throw new InvalidOperationException("Provider is not available for the specified time slot");
            }

            // Validate location and resource availability
            var resourcesAvailable = await _resourceService.ValidateResourceAvailabilityAsync(
                locationId,
                requirements.RequiredEquipment,
                startTime,
                endTime);

            if (!resourcesAvailable)
            {
                throw new InvalidOperationException("Required resources are not available");
            }

            var groupSession = new GroupAppointment
            {
                Name = name,
                Description = description,
                ProviderId = providerId,
                LocationId = locationId,
                AppointmentTypeId = appointmentTypeId,
                StartTime = startTime,
                EndTime = endTime,
                MaxParticipants = maxParticipants,
                MinParticipants = minParticipants,
                Status = "Scheduled",
                CreatedDate = DateTime.UtcNow,
                Requirements = requirements
            };

            _context.GroupAppointments.Add(groupSession);
            await _context.SaveChangesAsync();

            // Reserve resources
            await _resourceService.ReserveResourcesAsync(
                groupSession.Id,
                locationId,
                requirements.RequiredEquipment,
                startTime,
                endTime);

            // Create session materials
            await _documentService.CreateSessionMaterialsAsync(
                groupSession.Id,
                requirements);

            // Log creation
            await _auditService.LogActivityAsync(
                providerId,
                "GroupSessionCreated",
                $"Created group session: {name}",
                $"SessionId: {groupSession.Id}"
            );

            return groupSession;
        }

        public async Task<GroupParticipant> AddParticipantAsync(
            int groupAppointmentId,
            int patientId,
            ParticipantRequirements requirements = null)
        {
            var groupSession = await _context.GroupAppointments
                .Include(g => g.Participants)
                .FirstOrDefaultAsync(g => g.Id == groupAppointmentId);

            if (groupSession == null)
            {
                throw new ArgumentException("Group session not found");
            }

            // Check capacity
            if (groupSession.Participants.Count >= groupSession.MaxParticipants)
            {
                throw new InvalidOperationException("Group session is at maximum capacity");
            }

            // Validate clinical appropriateness
            var clinicallyAppropriate = await _clinicalService.ValidateGroupParticipationAsync(
                patientId,
                groupSession.AppointmentTypeId);

            if (!clinicallyAppropriate)
            {
                throw new InvalidOperationException("Patient does not meet clinical criteria for this group");
            }

            // Check for existing participation
            if (groupSession.Participants.Any(p => p.PatientId == patientId))
            {
                throw new InvalidOperationException("Patient is already registered for this group");
            }

            var participant = new GroupParticipant
            {
                GroupAppointmentId = groupAppointmentId,
                PatientId = patientId,
                Status = "Registered",
                JoinedDate = DateTime.UtcNow,
                Requirements = requirements
            };

            _context.GroupParticipants.Add(participant);
            await _context.SaveChangesAsync();

            // Send confirmation
            await _notificationService.SendGroupRegistrationConfirmationAsync(
                patientId,
                groupSession);

            // Update group materials if needed
            if (requirements?.NeedsAccessibilitySupport == true)
            {
                await UpdateGroupAccessibilityRequirementsAsync(
                    groupAppointmentId,
                    requirements.AccessibilityRequirements);
            }

            return participant;
        }

        public async Task<bool> UpdateParticipantStatusAsync(
            int groupAppointmentId,
            int patientId,
            string status)
        {
            var participant = await _context.GroupParticipants
                .FirstOrDefaultAsync(p => 
                    p.GroupAppointmentId == groupAppointmentId && 
                    p.PatientId == patientId);

            if (participant == null)
            {
                return false;
            }

            participant.Status = status;
            await _context.SaveChangesAsync();

            // Handle status-specific actions
            switch (status.ToLower())
            {
                case "confirmed":
                    await _notificationService.SendGroupConfirmationAsync(patientId, groupAppointmentId);
                    break;
                case "cancelled":
                    await ProcessWaitlistedParticipantsAsync(groupAppointmentId);
                    break;
                case "completed":
                    await _clinicalService.UpdateGroupParticipationRecordAsync(patientId, groupAppointmentId);
                    break;
            }

            return true;
        }

        public async Task<bool> RecordAttendanceAsync(
            int groupAppointmentId,
            int patientId,
            bool attended,
            string notes = null)
        {
            var participant = await _context.GroupParticipants
                .FirstOrDefaultAsync(p => 
                    p.GroupAppointmentId == groupAppointmentId && 
                    p.PatientId == patientId);

            if (participant == null)
            {
                return false;
            }

            participant.Attended = attended;
            participant.AttendanceNotes = notes;
            participant.CheckInTime = attended ? DateTime.UtcNow : (DateTime?)null;

            await _context.SaveChangesAsync();

            // Update clinical record
            await _clinicalService.UpdateGroupAttendanceRecordAsync(
                patientId,
                groupAppointmentId,
                attended,
                notes);

            return true;
        }

        public async Task<GroupSessionReport> GenerateSessionReportAsync(int groupAppointmentId)
        {
            var session = await _context.GroupAppointments
                .Include(g => g.Provider)
                .Include(g => g.Participants)
                .ThenInclude(p => p.Patient)
                .FirstOrDefaultAsync(g => g.Id == groupAppointmentId);

            if (session == null)
            {
                throw new ArgumentException("Group session not found");
            }

            var attendance = session.Participants.Select(p => new ParticipantAttendance
            {
                PatientId = p.PatientId,
                PatientName = $"{p.Patient.FirstName} {p.Patient.LastName}",
                Attended = p.Attended,
                Status = p.Status,
                Notes = p.AttendanceNotes,
                CheckInTime = p.CheckInTime,
                CheckOutTime = p.CheckOutTime
            }).ToList();

            var report = new GroupSessionReport
            {
                GroupAppointmentId = groupAppointmentId,
                SessionDate = session.StartTime,
                SessionName = session.Name,
                ProviderName = $"{session.Provider.FirstName} {session.Provider.LastName}",
                TotalParticipants = session.Participants.Count,
                AttendedParticipants = session.Participants.Count(p => p.Attended),
                Attendance = attendance,
                SessionNotes = session.Notes,
                Outcomes = await _clinicalService.GetGroupSessionOutcomesAsync(groupAppointmentId),
                FollowUpActions = await _clinicalService.GetGroupFollowUpActionsAsync(groupAppointmentId)
            };

            // Store report in document service
            await _documentService.StoreGroupSessionReportAsync(report);

            return report;
        }

        private async Task<bool> ValidateProviderAvailabilityAsync(
            int providerId,
            DateTime startTime,
            DateTime endTime)
        {
            var provider = await _context.Providers
                .Include(p => p.Schedule)
                .FirstOrDefaultAsync(p => p.Id == providerId);

            if (provider == null)
            {
                return false;
            }

            // Check regular schedule
            var daySchedule = provider.Schedule
                .FirstOrDefault(s => s.DayOfWeek == startTime.DayOfWeek);

            if (daySchedule == null || !daySchedule.IsAvailable)
            {
                return false;
            }

            // Check for conflicts
            var hasConflicts = await _context.Appointments
                .AnyAsync(a =>
                    a.ProviderId == providerId &&
                    a.Status != "Cancelled" &&
                    ((a.StartTime <= startTime && a.EndTime > startTime) ||
                     (a.StartTime < endTime && a.EndTime >= endTime)));

            return !hasConflicts;
        }

        private async Task UpdateGroupAccessibilityRequirementsAsync(
            int groupAppointmentId,
            string requirements)
        {
            var session = await _context.GroupAppointments.FindAsync(groupAppointmentId);
            if (session == null)
            {
                return;
            }

            // Update accessibility requirements
            session.AccessibilityRequirements = requirements;
            await _context.SaveChangesAsync();

            // Notify facility management if needed
            await _notificationService.NotifyAccessibilityRequirementsAsync(
                groupAppointmentId,
                requirements);
        }

        public async Task<GroupAppointment> UpdateGroupSessionAsync(
            int sessionId, 
            string title = null, 
            string description = null, 
            DateTime? startTime = null, 
            DateTime? endTime = null, 
            int? roomId = null, 
            int? providerId = null)
        {
            var session = await _context.GroupAppointments
                .Include(g => g.Provider)
                .Include(g => g.Room)
                .FirstOrDefaultAsync(g => g.Id == sessionId);

            if (session == null)
                throw new ArgumentException("Session not found");

            if (title != null)
                session.Title = title;
            if (description != null)
                session.Description = description;
            if (startTime.HasValue)
                session.StartTime = startTime.Value;
            if (endTime.HasValue)
                session.EndTime = endTime.Value;
            if (roomId.HasValue)
            {
                var room = await _context.Rooms.FindAsync(roomId.Value);
                if (room == null)
                    throw new ArgumentException("Room not found");
                session.RoomId = roomId.Value;
            }
            if (providerId.HasValue)
            {
                var provider = await _context.Providers.FindAsync(providerId.Value);
                if (provider == null)
                    throw new ArgumentException("Provider not found");
                session.ProviderId = providerId.Value;
            }

            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Updated group session {sessionId}");
            return session;
        }

        public async Task CancelGroupSessionAsync(int sessionId, string reason, bool notifyParticipants = true)
        {
            var session = await _context.GroupAppointments
                .Include(g => g.Participants)
                .FirstOrDefaultAsync(g => g.Id == sessionId);

            if (session == null)
                throw new ArgumentException("Session not found");

            session.Status = "Cancelled";
            session.CancellationReason = reason;
            session.CancelledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Cancelled group session {sessionId}: {reason}");

            if (notifyParticipants)
            {
                foreach (var participant in session.Participants)
                {
                    await _notificationService.SendEmailAsync(
                        participant.Email,
                        "Group Session Cancelled",
                        $"The group session scheduled for {session.StartTime:g} has been cancelled. Reason: {reason}");
                }
            }
        }

        public async Task<bool> RemoveParticipantAsync(int sessionId, int participantId, string reason)
        {
            var participant = await _context.GroupParticipants
                .FirstOrDefaultAsync(p => p.GroupAppointmentId == sessionId && p.Id == participantId);

            if (participant == null)
                return false;

            participant.Status = "Removed";
            participant.RemovalReason = reason;
            participant.RemovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Removed participant {participantId} from session {sessionId}: {reason}");

            await _notificationService.SendEmailAsync(
                participant.Email,
                "Removed from Group Session",
                $"You have been removed from the group session scheduled for {participant.GroupAppointment.StartTime:g}. Reason: {reason}");

            return true;
        }

        public async Task<IEnumerable<GroupAppointment>> GetUpcomingGroupSessionsAsync(
            int? providerId = null,
            int? roomId = null,
            string status = null)
        {
            var query = _context.GroupAppointments
                .Include(g => g.Provider)
                .Include(g => g.Room)
                .Include(g => g.Participants)
                .Where(g => g.StartTime > DateTime.UtcNow);

            if (providerId.HasValue)
                query = query.Where(g => g.ProviderId == providerId.Value);
            if (roomId.HasValue)
                query = query.Where(g => g.RoomId == roomId.Value);
            if (!string.IsNullOrEmpty(status))
                query = query.Where(g => g.Status == status);

            return await query
                .OrderBy(g => g.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<GroupParticipant>> GetSessionParticipantsAsync(int sessionId)
        {
            return await _context.GroupParticipants
                .Include(p => p.Patient)
                .Where(p => p.GroupAppointmentId == sessionId && p.Status != "Removed")
                .OrderBy(p => p.Patient.LastName)
                .ThenBy(p => p.Patient.FirstName)
                .ToListAsync();
        }

        public async Task<bool> ValidateGroupCompositionAsync(int sessionId)
        {
            var session = await _context.GroupAppointments
                .Include(g => g.Participants)
                .Include(g => g.Type)
                .FirstOrDefaultAsync(g => g.Id == sessionId);

            if (session == null)
                return false;

            // Check minimum participants
            if (session.Type.MinParticipants > 0 && 
                session.Participants.Count(p => p.Status == "Confirmed") < session.Type.MinParticipants)
                return false;

            // Check maximum participants
            if (session.Type.MaxParticipants > 0 && 
                session.Participants.Count(p => p.Status == "Confirmed") > session.Type.MaxParticipants)
                return false;

            return true;
        }

        public async Task<int> CheckGroupCapacityAsync(int sessionId)
        {
            var session = await _context.GroupAppointments
                .Include(g => g.Participants)
                .Include(g => g.Type)
                .FirstOrDefaultAsync(g => g.Id == sessionId);

            if (session == null || session.Type.MaxParticipants <= 0)
                return 0;

            var currentParticipants = session.Participants.Count(p => p.Status == "Confirmed");
            return session.Type.MaxParticipants - currentParticipants;
        }

        public async Task<bool> AssignBackupProviderAsync(int sessionId, int backupProviderId)
        {
            var session = await _context.GroupAppointments.FindAsync(sessionId);
            if (session == null)
                return false;

            var provider = await _context.Providers.FindAsync(backupProviderId);
            if (provider == null)
                return false;

            session.BackupProviderId = backupProviderId;
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Assigned backup provider {backupProviderId} to session {sessionId}");

            return true;
        }

        public async Task<bool> UpdateGroupMaterialsAsync(int sessionId, GroupSessionMaterials materials)
        {
            var session = await _context.GroupAppointments.FindAsync(sessionId);
            if (session == null)
                return false;

            materials.GroupAppointmentId = sessionId;
            materials.UploadDate = DateTime.UtcNow;

            _context.GroupSessionMaterials.Add(materials);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Updated materials for session {sessionId}");

            return true;
        }

        public async Task SendGroupNotificationAsync(int sessionId, string message, bool emailOnly = false)
        {
            var session = await _context.GroupAppointments
                .Include(g => g.Participants)
                .FirstOrDefaultAsync(g => g.Id == sessionId);

            if (session == null)
                throw new ArgumentException("Session not found");

            foreach (var participant in session.Participants.Where(p => p.Status == "Confirmed"))
            {
                await _notificationService.SendEmailAsync(
                    participant.Email,
                    "Group Session Update",
                    message);

                if (!emailOnly && !string.IsNullOrEmpty(participant.PhoneNumber))
                {
                    await _notificationService.SendSMSAsync(
                        participant.PhoneNumber,
                        message);
                }
            }

            await _auditService.LogActivityAsync($"Sent group notification to session {sessionId} participants");
        }

        public async Task ProcessWaitlistedParticipantsAsync(int sessionId)
        {
            var session = await _context.GroupAppointments
                .Include(g => g.Participants)
                .Include(g => g.Type)
                .FirstOrDefaultAsync(g => g.Id == sessionId);

            if (session == null)
                throw new ArgumentException("Session not found");

            var capacity = await CheckGroupCapacityAsync(sessionId);
            if (capacity <= 0)
                return;

            var waitlistedParticipants = session.Participants
                .Where(p => p.Status == "Waitlisted")
                .OrderBy(p => p.CreatedAt)
                .Take(capacity);

            foreach (var participant in waitlistedParticipants)
            {
                participant.Status = "Confirmed";
                participant.ConfirmedAt = DateTime.UtcNow;

                await _notificationService.SendEmailAsync(
                    participant.Email,
                    "Moved from Waitlist to Confirmed",
                    $"You have been moved from the waitlist to confirmed status for the group session on {session.StartTime:g}");
            }

            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Processed waitlisted participants for session {sessionId}");
        }
    }
}
