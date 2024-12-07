using System;
using EMRNext.Core.Domain.Entities.Common;

namespace EMRNext.Core.Domain.Entities.Clinical
{
    public class DocumentEntity : BaseIntEntity
    {
        public string DocumentType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }
        public string Description { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadDate { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public int? PatientId { get; set; }
        public int? EncounterId { get; set; }
        public int? OrderId { get; set; }
        public string Metadata { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? ArchivedDate { get; set; }
        public string ArchivedBy { get; set; }
        public string SecurityLevel { get; set; }
        public string AccessControlList { get; set; }
        public string VersionNumber { get; set; }
        public string Checksum { get; set; }
    }
}
