using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Core.Domain.Entities.Portal
{
    public class GroupAppointment : AuditableEntity
    {
        public GroupAppointment()
        {
            Participants = new HashSet<GroupParticipant>();
        }

        public Guid Id { get; set; }

        [Required]
        public Guid GroupSeriesId { get; set; }

        [Required]
        public Guid AppointmentTypeId { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }

        public bool AllowWaitlist { get; set; }

        [StringLength(500)]
        public string CancellationReason { get; set; }

        public bool IsVirtual { get; set; }

        [StringLength(500)]
        public string MeetingLink { get; set; }

        public Guid? BackupProviderId { get; set; }

        [StringLength(2000)]
        public string SessionMaterials { get; set; }

        // Navigation properties
        public virtual GroupSeries GroupSeries { get; set; }
        public virtual AppointmentType AppointmentType { get; set; }
        public virtual Provider BackupProvider { get; set; }
        public virtual ICollection<GroupParticipant> Participants { get; set; }
    }
}
