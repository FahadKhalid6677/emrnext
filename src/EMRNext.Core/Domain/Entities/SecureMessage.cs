using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMRNext.Core.Domain.Entities
{
    public class SecureMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int RecipientId { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }

        public string EncryptedKey { get; set; }

        public string InitializationVector { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadDate { get; set; }

        public bool IsUrgent { get; set; }

        public string Category { get; set; }

        public int? ParentMessageId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [ForeignKey("SenderId")]
        public virtual PortalUser Sender { get; set; }

        [ForeignKey("RecipientId")]
        public virtual PortalUser Recipient { get; set; }

        [ForeignKey("ParentMessageId")]
        public virtual SecureMessage ParentMessage { get; set; }

        public virtual ICollection<SecureMessage> Replies { get; set; }

        public virtual ICollection<SecureMessageAttachment> Attachments { get; set; }
    }

    public class SecureMessageAttachment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MessageId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        public string ContentType { get; set; }

        [Required]
        public string EncryptedContent { get; set; }

        public string EncryptedKey { get; set; }

        public string InitializationVector { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadDate { get; set; }

        [ForeignKey("MessageId")]
        public virtual SecureMessage Message { get; set; }
    }
}
