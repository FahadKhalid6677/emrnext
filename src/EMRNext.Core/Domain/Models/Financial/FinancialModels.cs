using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Models.Financial
{
    public class ClaimPayment
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class AnalyticsRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class AnalyticsReport
    {
        public int TotalClaims { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TrendReport
    {
        public int Month { get; set; }
        public int TotalClaims { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PerformanceMetrics
    {
        public int TotalClaims { get; set; }
        public double ProcessedClaimsPercentage { get; set; }
        public double DeniedClaimsPercentage { get; set; }
    }

    public enum ClaimStatus
    {
        Pending,
        Processed,
        Denied,
        Paid
    }
}
