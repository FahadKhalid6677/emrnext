using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Core.Domain.Entities.Portal
{
    public class GroupSessionMaterials : AuditableEntity
    {
        public int Id { get; set; }
        public int GroupAppointmentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }  // e.g., Handout, Presentation, Video, etc.
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsRequired { get; set; }
        public string Status { get; set; }
        public string Version { get; set; }
        public string Tags { get; set; }
        public bool IsPublic { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public virtual GroupAppointment GroupAppointment { get; set; }
        public virtual ICollection<MaterialAccessLog> AccessLogs { get; set; }
    }

    public class MaterialAccessLog : AuditableEntity
    {
        public int Id { get; set; }
        public int MaterialId { get; set; }
        public string UserId { get; set; }
        public DateTime AccessTime { get; set; }
        public string AccessType { get; set; }  // e.g., View, Download
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }

        public virtual GroupSessionMaterials Material { get; set; }
    }
}
