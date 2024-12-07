using System;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Core.Domain.Entities.Portal
{
    public class PortalUserDevice : AuditableEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string OperatingSystem { get; set; }
        public string Browser { get; set; }
        public DateTime LastUsed { get; set; }
        public bool IsTrusted { get; set; }
        public string PushToken { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public string DeactivatedReason { get; set; }
        public string IpAddress { get; set; }
        public string Location { get; set; }
    }
}
