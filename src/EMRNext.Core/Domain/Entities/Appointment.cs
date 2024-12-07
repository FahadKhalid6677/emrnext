using System;

namespace EMRNext.Core.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        
        // Appointment Details
        public string Type { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; }
        public string Location { get; set; }
        public string Room { get; set; }
        
        // Clinical Information
        public string AppointmentReason { get; set; }
        public string ChiefComplaint { get; set; }
        public string VisitType { get; set; }
        public string SpecialtyType { get; set; }
        public string Instructions { get; set; }
        public bool PreOpAppointment { get; set; }
        public bool IsFollowUp { get; set; }
        
        // Patient Information
        public bool IsNewPatient { get; set; }
        public string PatientPreferences { get; set; }
        public string SpecialNeeds { get; set; }
        public string PreferredLanguage { get; set; }
        public bool NeedsTranslator { get; set; }
        
        // Scheduling Information
        public DateTime RequestedDate { get; set; }
        public string RequestMethod { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public string ConfirmationMethod { get; set; }
        public string ScheduledBy { get; set; }
        
        // Reminders
        public bool ReminderSent { get; set; }
        public DateTime? LastReminderDate { get; set; }
        public string ReminderPreference { get; set; }
        public string ReminderStatus { get; set; }
        
        // Cancellation/Rescheduling
        public bool IsCancelled { get; set; }
        public DateTime? CancellationDate { get; set; }
        public string CancellationReason { get; set; }
        public string CancelledBy { get; set; }
        public bool IsRescheduled { get; set; }
        public int? RescheduledFromId { get; set; }
        public int? RescheduledToId { get; set; }
        
        // Check-in/Check-out
        public DateTime? CheckInTime { get; set; }
        public string CheckInBy { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string CheckOutBy { get; set; }
        public int? WaitTime { get; set; }
        
        // Insurance/Billing
        public string InsuranceVerification { get; set; }
        public string CopayAmount { get; set; }
        public bool CopayCollected { get; set; }
        public string PaymentMethod { get; set; }
        public string BillingNotes { get; set; }
        
        // Resources
        public string RequiredEquipment { get; set; }
        public string RequiredStaff { get; set; }
        public string RoomPreference { get; set; }
        public int? EstimatedDuration { get; set; }
        
        // Recurring Appointment
        public bool IsRecurring { get; set; }
        public string RecurrencePattern { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public int? RecurrenceGroupId { get; set; }
        
        // Documentation
        public string Notes { get; set; }
        public string AttachmentPaths { get; set; }
        public int? ResultingEncounterId { get; set; }
        
        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation Properties
        public virtual Patient Patient { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual Encounter ResultingEncounter { get; set; }
    }
}
