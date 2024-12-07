using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMRNext.Core.Domain.Entities
{
    public class PortalUserSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PortalUserId { get; set; }

        [Required]
        [StringLength(128)]
        public string SessionToken { get; set; }

        [Required]
        public string IpAddress { get; set; }

        public string UserAgent { get; set; }

        public string DeviceInfo { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public DateTime? LastActivityDate { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey("PortalUserId")]
        public virtual PortalUser PortalUser { get; set; }
    }
}
