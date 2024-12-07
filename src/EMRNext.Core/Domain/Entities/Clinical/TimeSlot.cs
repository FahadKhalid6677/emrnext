using System;
using EMRNext.Core.Domain.Entities.Common;

namespace EMRNext.Core.Domain.Entities.Clinical
{
    public class TimeSlot : BaseEntity
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string ProviderId { get; set; }
        public string ResourceId { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }
}
