using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities.Common;
using EMRNext.Core.Domain.Entities.Identity;

namespace EMRNext.Core.Domain.Entities.Clinical
{
    public class AlertEntity : BaseIntEntity
    {
        public string AlertType { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool RequiresAcknowledgement { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedDate { get; set; }
        public string AcknowledgedBy { get; set; }
        public string AcknowledgementComment { get; set; }
        public string Status { get; set; }
        public int? PatientId { get; set; }
        public int? EncounterId { get; set; }
        public int? OrderId { get; set; }
        public string Category { get; set; }
        public string Source { get; set; }
        public string Context { get; set; }
        public string ActionRequired { get; set; }
        public string Resolution { get; set; }
        public virtual ICollection<User> Recipients { get; set; }
        public virtual ICollection<AlertNotification> Notifications { get; set; }
    }

    public class AlertNotification : BaseIntEntity
    {
        public int AlertId { get; set; }
        public virtual AlertEntity Alert { get; set; }
        public int RecipientId { get; set; }
        public virtual User Recipient { get; set; }
        public DateTime NotificationDate { get; set; }
        public string NotificationType { get; set; }
        public string Status { get; set; }
        public DateTime? ReadDate { get; set; }
        public string DeliveryStatus { get; set; }
        public string DeliveryError { get; set; }
    }
}
