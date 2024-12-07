using System;
using System.Collections.Generic;

namespace EMRNext.Core.Models.Laboratory
{
    public class LabOrderRequest
    {
        public int PatientId { get; set; }
        public int OrderingProviderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Priority { get; set; }
        public string SpecialInstructions { get; set; }
        public string ClinicalInformation { get; set; }
        public bool IsFasting { get; set; }
        public List<int> TestIds { get; set; }
        public string CollectionLocation { get; set; }
        public DateTime? PreferredCollectionTime { get; set; }
        public string DiagnosisCode { get; set; }
        public string DiagnosisDescription { get; set; }
        public int? ExternalLabId { get; set; }
        public bool RequiresAuthorization { get; set; }
        public string InsuranceInformation { get; set; }
    }

    public class LabResultRequest
    {
        public int LabTestOrderId { get; set; }
        public DateTime ResultDate { get; set; }
        public string Status { get; set; }
        public List<LabTestResultValue> ResultValues { get; set; }
        public string Comments { get; set; }
        public string PerformedBy { get; set; }
        public string ReviewedBy { get; set; }
        public bool IsAbnormal { get; set; }
        public string AbnormalityDescription { get; set; }
        public List<string> Flags { get; set; }
    }

    public class LabTestResultValue
    {
        public string Component { get; set; }
        public decimal Value { get; set; }
        public string Unit { get; set; }
        public string ReferenceRange { get; set; }
        public string Flag { get; set; }
        public string Method { get; set; }
    }

    public class DocumentRequest
    {
        public string DocumentType { get; set; }
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string MimeType { get; set; }
        public string Description { get; set; }
        public string UploadedBy { get; set; }
    }

    public class AlertRequest
    {
        public string AlertType { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; }
        public List<int> RecipientIds { get; set; }
        public bool RequiresAcknowledgement { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class InterfaceError
    {
        public int OrderId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string InterfaceType { get; set; }
        public DateTime ErrorTime { get; set; }
        public string StackTrace { get; set; }
    }

    public class QualityIssue
    {
        public string IssueType { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public string ReportedBy { get; set; }
        public DateTime ReportedDate { get; set; }
        public string Resolution { get; set; }
    }

    public class LabOrderSummary
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int AbnormalResults { get; set; }
        public Dictionary<string, int> OrdersByPriority { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; }
        public Dictionary<string, int> OrdersByTest { get; set; }
        public Dictionary<string, int> OrdersByProvider { get; set; }
        public decimal AverageTurnaroundTime { get; set; }
    }

    public class QualityMetrics
    {
        public decimal SampleRejectionRate { get; set; }
        public decimal ResultAccuracy { get; set; }
        public decimal TurnaroundTimeCompliance { get; set; }
        public int CriticalValueNotifications { get; set; }
        public int IncidentReports { get; set; }
        public Dictionary<string, int> IssuesByType { get; set; }
        public Dictionary<string, decimal> PerformanceByTest { get; set; }
    }
}
