using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMRNext.Core.Domain.Billing.Types
{
    public enum BillingStatus
    {
        Draft,
        Pending,
        Submitted,
        Paid,
        Partially_Paid,
        Overdue,
        Cancelled
    }

    public enum PaymentMethod
    {
        Cash,
        Credit_Card,
        Debit_Card,
        Insurance,
        Bank_Transfer,
        Online_Payment
    }

    public class InsuranceProvider
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(50)]
        public string ProviderCode { get; set; }
        
        [StringLength(200)]
        public string ContactInformation { get; set; }
        
        public bool IsActive { get; set; }
    }

    public class Invoice
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid PatientId { get; set; }
        
        [Required]
        public DateTime InvoiceDate { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public BillingStatus Status { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        public decimal PaidAmount { get; set; }
        
        public Guid? InsuranceClaimId { get; set; }
        
        public List<InvoiceLineItem> LineItems { get; set; }
    }

    public class InvoiceLineItem
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid InvoiceId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Description { get; set; }
        
        [Required]
        public decimal Quantity { get; set; }
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public class InsuranceClaim
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid PatientId { get; set; }
        
        [Required]
        public Guid InsuranceProviderId { get; set; }
        
        [Required]
        public DateTime ClaimDate { get; set; }
        
        public BillingStatus Status { get; set; }
        
        [Required]
        public decimal ClaimAmount { get; set; }
        
        public decimal ApprovedAmount { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        public List<ClaimLineItem> LineItems { get; set; }
    }

    public class ClaimLineItem
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid ClaimId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string ServiceDescription { get; set; }
        
        [Required]
        public decimal ServiceCost { get; set; }
        
        public bool IsApproved { get; set; }
    }

    public class Payment
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid InvoiceId { get; set; }
        
        [Required]
        public DateTime PaymentDate { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        public PaymentMethod PaymentMethod { get; set; }
        
        [StringLength(100)]
        public string TransactionReference { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
    }
}
