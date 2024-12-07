using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities
{
    public class GroupSeries
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int GroupSessionTemplateId { get; set; }
        public virtual GroupSessionTemplate Template { get; set; }
        
        // Recurrence Pattern
        public string RecurrencePattern { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberOfSessions { get; set; }
        
        // Series Configuration
        public bool AllowDropIns { get; set; }
        public bool RequiresContinuity { get; set; }
        public int MinimumAttendance { get; set; }
        public string CompletionCriteria { get; set; }
        
        // Holiday Handling
        public bool AutoAdjustForHolidays { get; set; }
        public string HolidayHandlingStrategy { get; set; }
        
        // Conflict Resolution
        public string ConflictResolutionStrategy { get; set; }
        public bool AutoRescheduleEnabled { get; set; }
        
        // Series Progress
        public string Status { get; set; }
        public int CompletedSessions { get; set; }
        public DateTime? LastSessionDate { get; set; }
        public DateTime? NextSessionDate { get; set; }
        
        // Tracking
        public virtual ICollection<GroupAppointment> Sessions { get; set; }
        public virtual ICollection<SeriesParticipant> Participants { get; set; }
        public virtual ICollection<SeriesOutcome> Outcomes { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
    }

    public class SeriesParticipant
    {
        public int Id { get; set; }
        public int GroupSeriesId { get; set; }
        public int PatientId { get; set; }
        public virtual GroupSeries Series { get; set; }
        public virtual Patient Patient { get; set; }
        
        // Participation Tracking
        public DateTime EnrollmentDate { get; set; }
        public string EnrollmentStatus { get; set; }
        public int SessionsAttended { get; set; }
        public int SessionsMissed { get; set; }
        public DateTime? LastAttendanceDate { get; set; }
        
        // Progress Tracking
        public string ProgressStatus { get; set; }
        public string ClinicalNotes { get; set; }
        public bool CompletedSeries { get; set; }
        public DateTime? CompletionDate { get; set; }
        
        // Outcomes
        public virtual ICollection<ParticipantOutcome> Outcomes { get; set; }
    }

    public class SeriesOutcome
    {
        public int Id { get; set; }
        public int GroupSeriesId { get; set; }
        public virtual GroupSeries Series { get; set; }
        
        public string OutcomeType { get; set; }
        public string Measure { get; set; }
        public string Value { get; set; }
        public DateTime MeasurementDate { get; set; }
        public string Notes { get; set; }
    }

    public class ParticipantOutcome
    {
        public int Id { get; set; }
        public int SeriesParticipantId { get; set; }
        public virtual SeriesParticipant Participant { get; set; }
        
        public string OutcomeType { get; set; }
        public string Measure { get; set; }
        public string Value { get; set; }
        public DateTime MeasurementDate { get; set; }
        public string Notes { get; set; }
    }
}
