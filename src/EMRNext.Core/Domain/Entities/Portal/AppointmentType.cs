using System;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Core.Domain.Entities.Portal
{
    public class AppointmentType : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DefaultDuration { get; set; } // Duration in minutes
        public string Color { get; set; }
        public bool RequiresApproval { get; set; }
        public bool IsActive { get; set; }
        public int? MaxParticipants { get; set; }
        public bool IsGroupAppointment { get; set; }
        public bool AllowWaitlist { get; set; }
        public bool RequiresReferral { get; set; }
        public bool RequiresAuthorization { get; set; }
        public string SpecialInstructions { get; set; }
        public decimal? Cost { get; set; }
        public bool IsBillable { get; set; }
    }
}
