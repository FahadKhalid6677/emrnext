using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Billing.Types;
using EMRNext.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMRNext.Core.Services.Billing
{
    public interface IBillingService
    {
        Task<Invoice> CreateInvoiceAsync(Invoice invoice);
        Task<Invoice> UpdateInvoiceAsync(Invoice invoice);
        Task<Invoice> GetInvoiceByIdAsync(Guid invoiceId);
        Task<List<Invoice>> GetPatientInvoicesAsync(Guid patientId);
        Task<InsuranceClaim> SubmitInsuranceClaimAsync(InsuranceClaim claim);
        Task<InsuranceClaim> UpdateInsuranceClaimAsync(InsuranceClaim claim);
        Task<List<InsuranceClaim>> GetPatientClaimsAsync(Guid patientId);
        Task<Payment> RecordPaymentAsync(Payment payment);
        Task<decimal> CalculateOutstandingBalanceAsync(Guid patientId);
    }

    public class BillingService : IBillingService
    {
        private readonly ApplicationDbContext _context;

        public BillingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
        {
            // Validate invoice
            if (invoice.LineItems == null || !invoice.LineItems.Any())
            {
                throw new ArgumentException("Invoice must have at least one line item.");
            }

            invoice.Id = Guid.NewGuid();
            invoice.InvoiceDate = DateTime.UtcNow;
            invoice.Status = BillingStatus.Draft;
            invoice.TotalAmount = invoice.LineItems.Sum(li => li.TotalPrice);

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }

        public async Task<Invoice> UpdateInvoiceAsync(Invoice invoice)
        {
            var existingInvoice = await _context.Invoices
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id);

            if (existingInvoice == null)
            {
                throw new ArgumentException("Invoice not found.");
            }

            // Update invoice details
            existingInvoice.Status = invoice.Status;
            existingInvoice.PaidAmount = invoice.PaidAmount;
            existingInvoice.DueDate = invoice.DueDate;

            // Update line items
            _context.InvoiceLineItems.RemoveRange(existingInvoice.LineItems);
            existingInvoice.LineItems = invoice.LineItems;
            existingInvoice.TotalAmount = invoice.LineItems.Sum(li => li.TotalPrice);

            await _context.SaveChangesAsync();
            return existingInvoice;
        }

        public async Task<Invoice> GetInvoiceByIdAsync(Guid invoiceId)
        {
            return await _context.Invoices
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);
        }

        public async Task<List<Invoice>> GetPatientInvoicesAsync(Guid patientId)
        {
            return await _context.Invoices
                .Where(i => i.PatientId == patientId)
                .Include(i => i.LineItems)
                .ToListAsync();
        }

        public async Task<InsuranceClaim> SubmitInsuranceClaimAsync(InsuranceClaim claim)
        {
            claim.Id = Guid.NewGuid();
            claim.ClaimDate = DateTime.UtcNow;
            claim.Status = BillingStatus.Pending;

            _context.InsuranceClaims.Add(claim);
            await _context.SaveChangesAsync();

            return claim;
        }

        public async Task<InsuranceClaim> UpdateInsuranceClaimAsync(InsuranceClaim claim)
        {
            var existingClaim = await _context.InsuranceClaims
                .Include(c => c.LineItems)
                .FirstOrDefaultAsync(c => c.Id == claim.Id);

            if (existingClaim == null)
            {
                throw new ArgumentException("Insurance claim not found.");
            }

            existingClaim.Status = claim.Status;
            existingClaim.ApprovedAmount = claim.ApprovedAmount;
            existingClaim.Notes = claim.Notes;

            // Update line items
            _context.ClaimLineItems.RemoveRange(existingClaim.LineItems);
            existingClaim.LineItems = claim.LineItems;

            await _context.SaveChangesAsync();
            return existingClaim;
        }

        public async Task<List<InsuranceClaim>> GetPatientClaimsAsync(Guid patientId)
        {
            return await _context.InsuranceClaims
                .Where(c => c.PatientId == patientId)
                .Include(c => c.LineItems)
                .ToListAsync();
        }

        public async Task<Payment> RecordPaymentAsync(Payment payment)
        {
            var invoice = await _context.Invoices.FindAsync(payment.InvoiceId);
            if (invoice == null)
            {
                throw new ArgumentException("Invoice not found.");
            }

            payment.Id = Guid.NewGuid();
            payment.PaymentDate = DateTime.UtcNow;

            // Update invoice paid amount
            invoice.PaidAmount += payment.Amount;
            invoice.Status = invoice.PaidAmount >= invoice.TotalAmount 
                ? BillingStatus.Paid 
                : BillingStatus.Partially_Paid;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<decimal> CalculateOutstandingBalanceAsync(Guid patientId)
        {
            var invoices = await GetPatientInvoicesAsync(patientId);
            return invoices
                .Where(i => i.Status != BillingStatus.Paid)
                .Sum(i => i.TotalAmount - i.PaidAmount);
        }
    }
}
