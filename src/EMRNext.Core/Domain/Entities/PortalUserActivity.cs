using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMRNext.Core.Domain.Entities
{
    public class PortalUserActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PortalUserId { get; set; }

        [Required]
        public string ActivityType { get; set; }

        [Required]
        public string Description { get; set; }

        public string Details { get; set; }

        [Required]
        public string IpAddress { get; set; }

        public string UserAgent { get; set; }

        public int? SessionId { get; set; }

        public DateTime Timestamp { get; set; }

        [ForeignKey("PortalUserId")]
        public virtual PortalUser PortalUser { get; set; }

        [ForeignKey("SessionId")]
        public virtual PortalUserSession Session { get; set; }
    }
}
