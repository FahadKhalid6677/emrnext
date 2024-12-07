using System;
using EMRNext.Core.Models;

namespace EMRNext.Core.Models
{
    public class DocumentRequest
    {
        public string PatientId { get; set; }
        public string TemplateId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DocumentStatus? Status { get; set; }
        public string ChangeDescription { get; set; }
    }

    public class SignatureRequest
    {
        public string DocumentId { get; set; }
        public SignatureType SignatureType { get; set; }
        public string Comments { get; set; }
    }

    public class AmendmentRequest
    {
        public string DocumentId { get; set; }
        public string AmendedContent { get; set; }
        public string Reason { get; set; }
    }

    public class CollaborationRequest
    {
        public string DocumentId { get; set; }
        public string CollaboratorUserId { get; set; }
    }
}
