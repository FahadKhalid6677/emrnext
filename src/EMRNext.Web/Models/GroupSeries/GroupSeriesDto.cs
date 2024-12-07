using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMRNext.Web.Models.GroupSeries
{
    public class GroupSeriesDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(100)]
        public string RecurrencePattern { get; set; }

        [Required]
        public int MaxParticipants { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public bool AllowWaitlist { get; set; }

        [Required]
        public Guid ProviderId { get; set; }

        [Required]
        public Guid AppointmentTypeId { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; }

        public bool IsVirtual { get; set; }

        [StringLength(500)]
        public string MeetingLink { get; set; }
    }

    public class GroupSessionDto
    {
        public Guid Id { get; set; }
        public Guid GroupSeriesId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public bool IsVirtual { get; set; }
        public string MeetingLink { get; set; }
        public List<ParticipantDto> Participants { get; set; }
    }

    public class ParticipantDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; }
        public bool IsWaitlisted { get; set; }
        public int? WaitlistPosition { get; set; }
    }

    public class SeriesOutcomeDto
    {
        public Guid Id { get; set; }
        public Guid GroupSeriesId { get; set; }
        public Guid PatientId { get; set; }
        public string OutcomeType { get; set; }
        public string MeasurementTool { get; set; }
        public DateTime MeasurementDate { get; set; }
        public decimal Score { get; set; }
        public string ScoreInterpretation { get; set; }
        public bool IsBaselineMeasurement { get; set; }
        public bool IsFinalMeasurement { get; set; }
        public decimal? PercentageImprovement { get; set; }
    }

    public class ParticipantReportDto
    {
        public Guid Id { get; set; }
        public Guid SeriesReportId { get; set; }
        public Guid ParticipantId { get; set; }
        public int SessionsAttended { get; set; }
        public double AttendanceRate { get; set; }
        public string ParticipationLevel { get; set; }
        public string Progress { get; set; }
        public string Goals { get; set; }
        public List<SeriesOutcomeDto> Outcomes { get; set; }
    }
}
