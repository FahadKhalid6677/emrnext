using System;
using System.ComponentModel.DataAnnotations;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Core.Domain.Entities.Portal
{
    public class GroupParticipant : AuditableEntity
    {
        public Guid Id { get; set; }

        [Required]
        public Guid GroupAppointmentId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }

        public bool IsWaitlisted { get; set; }

        public int? WaitlistPosition { get; set; }

        [Required]
        public DateTime EnrollmentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string EnrollmentStatus { get; set; }

        [StringLength(500)]
        public string CancellationReason { get; set; }

        public bool HasAttendanceConfirmed { get; set; }

        [StringLength(2000)]
        public string ParticipationNotes { get; set; }

        [StringLength(2000)]
        public string ProgressNotes { get; set; }

        [StringLength(2000)]
        public string Goals { get; set; }

        [StringLength(2000)]
        public string Interventions { get; set; }

        // Navigation properties
        public virtual GroupAppointment GroupAppointment { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual ParticipantReport ParticipantReport { get; set; }
        public virtual SeriesOutcome SeriesOutcome { get; set; }

        public GroupParticipant()
        {
            // Removed the initialization of Reports and Outcomes as they are no longer part of the entity
        }
    }
}
