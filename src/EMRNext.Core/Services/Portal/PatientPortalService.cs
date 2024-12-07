using EMRNext.Core.Domain.Entities;
using EMRNext.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMRNext.Core.Services.Portal
{
    public class PatientPortalService : IPatientPortalService
    {
        private readonly EMRNextDbContext _context;
        private readonly ILogger<PatientPortalService> _logger;
        private readonly ISecureMessagingService _messagingService;
        private readonly IPortalAuthenticationService _authService;
        private readonly IClinicalService _clinicalService;

        public PatientPortalService(
            EMRNextDbContext context,
            ILogger<PatientPortalService> logger,
            ISecureMessagingService messagingService,
            IPortalAuthenticationService authService,
            IClinicalService clinicalService)
        {
            _context = context;
            _logger = logger;
            _messagingService = messagingService;
            _authService = authService;
            _clinicalService = clinicalService;
        }

        public async Task<PortalUser> RegisterPatientAsync(Patient patient, string email, string password)
        {
            try
            {
                // Create portal user
                var portalUser = new PortalUser
                {
                    PatientId = patient.Id,
                    Email = email,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                // Set up authentication
                await _authService.CreateUserAsync(portalUser, password);

                // Send verification email
                await _authService.SendVerificationEmailAsync(portalUser.Email);

                _context.PortalUsers.Add(portalUser);
                await _context.SaveChangesAsync();

                await LogActivityAsync(portalUser.Id, "Registration", "Patient portal account created");

                return portalUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering patient portal user");
                throw;
            }
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            return await _authService.VerifyEmailAsync(token);
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            return await _authService.ResetPasswordAsync(token, newPassword);
        }

        public async Task<bool> UpdateProfileAsync(int userId, Dictionary<string, string> profileData)
        {
            var user = await _context.PortalUsers.FindAsync(userId);
            if (user == null) return false;

            // Update profile fields
            foreach (var field in profileData)
            {
                // Update appropriate fields based on the dictionary
                // Implementation depends on specific profile fields
            }

            await _context.SaveChangesAsync();
            await LogActivityAsync(userId, "ProfileUpdate", "Profile information updated");

            return true;
        }

        public async Task<IEnumerable<Document>> GetPatientDocumentsAsync(int patientId)
        {
            return await _context.Documents
                .Where(d => d.PatientId == patientId)
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabResult>> GetLabResultsAsync(int patientId)
        {
            return await _context.LabResults
                .Where(l => l.PatientId == patientId)
                .OrderByDescending(l => l.ResultDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vital>> GetVitalsAsync(int patientId)
        {
            return await _context.Vitals
                .Where(v => v.PatientId == patientId)
                .OrderByDescending(v => v.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medication>> GetMedicationsAsync(int patientId)
        {
            return await _context.Medications
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.PrescribedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Problem>> GetProblemsAsync(int patientId)
        {
            return await _context.Problems
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.OnsetDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Allergy>> GetAllergiesAsync(int patientId)
        {
            return await _context.Allergies
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.IdentifiedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int patientId)
        {
            var currentDate = DateTime.UtcNow.Date;
            return await _context.Appointments
                .Where(a => a.PatientId == patientId && a.StartTime >= currentDate)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<Appointment> RequestAppointmentAsync(int patientId, DateTime preferredDate, string reason)
        {
            var appointment = new Appointment
            {
                PatientId = patientId,
                StartTime = preferredDate,
                Status = "Requested",
                Reason = reason,
                CreatedDate = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            await LogActivityAsync(patientId, "AppointmentRequest", $"Appointment requested for {preferredDate}");

            return appointment;
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, string reason)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return false;

            appointment.Status = "Cancelled";
            appointment.CancellationReason = reason;
            appointment.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await LogActivityAsync(appointment.PatientId, "AppointmentCancellation", $"Appointment {appointmentId} cancelled");

            return true;
        }

        public async Task<SecureMessage> SendMessageAsync(int fromUserId, int toUserId, string subject, string body)
        {
            return await _messagingService.SendMessageAsync(fromUserId, toUserId, subject, body);
        }

        public async Task<IEnumerable<SecureMessage>> GetMessagesAsync(int userId)
        {
            return await _messagingService.GetUserMessagesAsync(userId);
        }

        public async Task<bool> MarkMessageAsReadAsync(int messageId)
        {
            return await _messagingService.MarkMessageAsReadAsync(messageId);
        }

        public async Task<Document> SubmitFormAsync(int patientId, string formType, Dictionary<string, string> formData)
        {
            var document = new Document
            {
                PatientId = patientId,
                Type = formType,
                Content = System.Text.Json.JsonSerializer.Serialize(formData),
                CreatedDate = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            await LogActivityAsync(patientId, "FormSubmission", $"Submitted form: {formType}");

            return document;
        }

        public async Task<IEnumerable<Document>> GetPendingFormsAsync(int patientId)
        {
            return await _context.Documents
                .Where(d => d.PatientId == patientId && d.Status == "Pending")
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetBillingStatementsAsync(int patientId)
        {
            return await _context.Documents
                .Where(d => d.PatientId == patientId && d.Type == "BillingStatement")
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> MakePaymentAsync(int patientId, decimal amount, string paymentMethod)
        {
            // Implementation depends on payment processing integration
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, string>> GetNotificationPreferencesAsync(int userId)
        {
            var user = await _context.PortalUsers
                .Include(u => u.NotificationPreferences)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.NotificationPreferences?.ToDictionary(p => p.Key, p => p.Value) 
                ?? new Dictionary<string, string>();
        }

        public async Task<bool> UpdateNotificationPreferencesAsync(int userId, Dictionary<string, string> preferences)
        {
            var user = await _context.PortalUsers.FindAsync(userId);
            if (user == null) return false;

            // Update notification preferences
            user.NotificationPreferences = preferences.Select(p => new PortalUserPreference 
            { 
                UserId = userId,
                Key = p.Key,
                Value = p.Value
            }).ToList();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task LogActivityAsync(int userId, string activityType, string details)
        {
            var activity = new PortalUserActivity
            {
                UserId = userId,
                ActivityType = activityType,
                Details = details,
                ActivityDate = DateTime.UtcNow
            };

            _context.PortalUserActivities.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PortalUserActivity>> GetUserActivityHistoryAsync(int userId)
        {
            return await _context.PortalUserActivities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }
    }
}
