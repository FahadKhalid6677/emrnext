using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMRNext.Core.Domain.Entities
{
    public class PortalUserDevice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PortalUserId { get; set; }

        [Required]
        [StringLength(100)]
        public string DeviceName { get; set; }

        [Required]
        [StringLength(100)]
        public string DeviceType { get; set; }

        [Required]
        public string DeviceIdentifier { get; set; }

        public string PushToken { get; set; }

        public bool IsTrusted { get; set; }

        public DateTime LastUsedDate { get; set; }

        public DateTime RegisteredDate { get; set; }

        [ForeignKey("PortalUserId")]
        public virtual PortalUser PortalUser { get; set; }
    }
}
