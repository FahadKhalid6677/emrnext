using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Core.Domain.Entities.Portal
{
    public class GroupSeries : AuditableEntity
    {
        public GroupSeries()
        {
            Appointments = new HashSet<GroupAppointment>();
        }

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

        public virtual ICollection<GroupAppointment> Appointments { get; set; }
    }
}
