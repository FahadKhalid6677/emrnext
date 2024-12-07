using System;
using System.Threading.Tasks;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace EMRNext.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly EMRNextDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IEmailService _emailService;
        private readonly ISMSService _smsService;

        public NotificationService(
            EMRNextDbContext context,
            ILogger<NotificationService> logger,
            IEmailService emailService,
            ISMSService smsService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _smsService = smsService;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            try
            {
                await _emailService.SendEmailAsync(recipientEmail, subject, body);
                await LogNotificationAsync("Email", recipientEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {recipientEmail}");
                throw;
            }
        }

        public async Task SendSMSAsync(string phoneNumber, string message)
        {
            try
            {
                await _smsService.SendSMSAsync(phoneNumber, message);
                await LogNotificationAsync("SMS", phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending SMS to {phoneNumber}");
                throw;
            }
        }

        public async Task SendInAppNotificationAsync(string userId, string title, string message)
        {
            try
            {
                var notification = new InAppNotification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.InAppNotifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending in-app notification to user {userId}");
                throw;
            }
        }

        public async Task SendAppointmentReminderAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null) throw new ArgumentException("Appointment not found");

                var patient = await _context.Patients.FindAsync(appointment.PatientId);
                if (patient == null) throw new ArgumentException("Patient not found");

                // Send email reminder
                if (!string.IsNullOrEmpty(patient.Email))
                {
                    var emailSubject = "Appointment Reminder";
                    var emailBody = $"This is a reminder for your appointment on {appointment.StartTime:g}";
                    await SendEmailAsync(patient.Email, emailSubject, emailBody);
                }

                // Send SMS reminder
                if (!string.IsNullOrEmpty(patient.PhoneNumber))
                {
                    var smsMessage = $"Reminder: You have an appointment on {appointment.StartTime:g}";
                    await SendSMSAsync(patient.PhoneNumber, smsMessage);
                }

                // Send in-app notification
                var notificationMessage = $"You have an upcoming appointment on {appointment.StartTime:g}";
                await SendInAppNotificationAsync(patient.UserId, "Appointment Reminder", notificationMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending appointment reminder for appointment {appointmentId}");
                throw;
            }
        }

        private async Task LogNotificationAsync(string type, string recipient, string content)
        {
            var log = new NotificationLog
            {
                Type = type,
                Recipient = recipient,
                Content = content,
                SentAt = DateTime.UtcNow,
                Status = "Sent"
            };

            _context.NotificationLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }

    public class NotificationLog
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Recipient { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public string Status { get; set; }
    }

    public class InAppNotification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
