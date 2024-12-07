using System;
using EMRNext.Core.Domain.Entities.Common;

namespace EMRNext.Core.Domain.Entities.Clinical
{
    public class WaitlistEntry : BaseEntity
    {
        public string PatientId { get; set; }
        public string ProviderId { get; set; }
        public string AppointmentType { get; set; }
        public DateTime RequestedDate { get; set; }
        public string PreferredTimeOfDay { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Notes { get; set; }
        public DateTime? NotifiedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
