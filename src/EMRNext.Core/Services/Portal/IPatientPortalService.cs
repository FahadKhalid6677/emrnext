using EMRNext.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMRNext.Core.Services.Portal
{
    public interface IPatientPortalService
    {
        // User Management
        Task<PortalUser> RegisterPatientAsync(Patient patient, string email, string password);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<bool> UpdateProfileAsync(int userId, Dictionary<string, string> profileData);
        
        // Health Records
        Task<IEnumerable<Document>> GetPatientDocumentsAsync(int patientId);
        Task<IEnumerable<LabResult>> GetLabResultsAsync(int patientId);
        Task<IEnumerable<Vital>> GetVitalsAsync(int patientId);
        Task<IEnumerable<Medication>> GetMedicationsAsync(int patientId);
        Task<IEnumerable<Problem>> GetProblemsAsync(int patientId);
        Task<IEnumerable<Allergy>> GetAllergiesAsync(int patientId);
        
        // Appointments
        Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int patientId);
        Task<Appointment> RequestAppointmentAsync(int patientId, DateTime preferredDate, string reason);
        Task<bool> CancelAppointmentAsync(int appointmentId, string reason);
        
        // Messaging
        Task<SecureMessage> SendMessageAsync(int fromUserId, int toUserId, string subject, string body);
        Task<IEnumerable<SecureMessage>> GetMessagesAsync(int userId);
        Task<bool> MarkMessageAsReadAsync(int messageId);
        
        // Forms and Questionnaires
        Task<Document> SubmitFormAsync(int patientId, string formType, Dictionary<string, string> formData);
        Task<IEnumerable<Document>> GetPendingFormsAsync(int patientId);
        
        // Billing and Statements
        Task<IEnumerable<Document>> GetBillingStatementsAsync(int patientId);
        Task<bool> MakePaymentAsync(int patientId, decimal amount, string paymentMethod);
        
        // Preferences
        Task<Dictionary<string, string>> GetNotificationPreferencesAsync(int userId);
        Task<bool> UpdateNotificationPreferencesAsync(int userId, Dictionary<string, string> preferences);
        
        // Activity Tracking
        Task LogActivityAsync(int userId, string activityType, string details);
        Task<IEnumerable<PortalUserActivity>> GetUserActivityHistoryAsync(int userId);
    }
}
