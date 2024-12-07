namespace EMRNext.Core.Stubs
{
    // Stub types to resolve compilation issues
    public class TimeSlot 
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class WaitlistEntry
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime RequestedTime { get; set; }
    }

    public class ScheduleMetrics
    {
        public int TotalAppointments { get; set; }
        public double UtilizationRate { get; set; }
    }

    public class ResourceUsage 
    {
        public int ResourceId { get; set; }
        public double UtilizationPercentage { get; set; }
    }

    public class FinancialMetrics 
    {
        public decimal TotalRevenue { get; set; }
        public decimal Expenses { get; set; }
    }

    public class SchedulingEfficiency 
    {
        public double WaitTime { get; set; }
        public double CancellationRate { get; set; }
    }

    public class WaitlistStatus 
    {
        public int QueueLength { get; set; }
        public TimeSpan AverageWaitTime { get; set; }
    }

    public class PatientProgress 
    {
        public int PatientId { get; set; }
        public string ProgressStatus { get; set; }
    }

    public class OutcomeTracking 
    {
        public int EncounterId { get; set; }
        public string Outcome { get; set; }
    }

    public class ProtocolCompliance 
    {
        public int PatientId { get; set; }
        public double CompliancePercentage { get; set; }
    }

    public class QualityMeasures 
    {
        public string MeasureName { get; set; }
        public double Score { get; set; }
    }

    public class PreAuthRequest 
    {
        public int PatientId { get; set; }
        public string ServiceCode { get; set; }
    }

    public class PreAuthorizationResult 
    {
        public bool Approved { get; set; }
        public string AuthorizationCode { get; set; }
    }

    public class InsuranceEligibility 
    {
        public bool IsEligible { get; set; }
        public decimal CoverageLimit { get; set; }
    }

    public class Statement 
    {
        public int PatientId { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class Account 
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }
    }

    public class AccountBalance 
    {
        public int AccountId { get; set; }
        public decimal CurrentBalance { get; set; }
    }

    public class FeeSchedule 
    {
        public string ServiceCode { get; set; }
        public decimal Fee { get; set; }
    }

    public class Adjustment 
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; }
    }

    public class CreditCheck 
    {
        public bool Approved { get; set; }
        public decimal CreditLimit { get; set; }
    }

    public class PaymentPlan 
    {
        public int PatientId { get; set; }
        public decimal MonthlyPayment { get; set; }
    }

    public class DocumentRequest 
    {
        public int PatientId { get; set; }
        public string DocumentType { get; set; }
    }

    public class AlertRequest 
    {
        public int PatientId { get; set; }
        public string AlertType { get; set; }
    }

    public class LabOrderReport 
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
    }

    public class LabResultReport 
    {
        public int OrderId { get; set; }
        public string Result { get; set; }
    }

    public class LabOrderSummary 
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class InterfaceError 
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class QualityReport 
    {
        public string ReportType { get; set; }
        public double Score { get; set; }
    }

    public class QualityIssue 
    {
        public string IssueType { get; set; }
        public string Severity { get; set; }
    }

    public class QualityMetric 
    {
        public string MetricName { get; set; }
        public double Value { get; set; }
    }
}
