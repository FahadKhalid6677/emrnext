using System;
using System.Threading.Tasks;

namespace EMRNext.Infrastructure.Services.External
{
    public interface IPaymentGatewayService
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount);
        Task<PaymentMethod> SavePaymentMethodAsync(PaymentMethodRequest request);
        Task<PaymentMethod> GetPaymentMethodAsync(string paymentMethodId);
        Task<bool> DeletePaymentMethodAsync(string paymentMethodId);
        Task<PaymentTransaction> GetTransactionDetailsAsync(string transactionId);
    }

    public class PaymentRequest
    {
        public string PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Description { get; set; }
        public string CustomerId { get; set; }
        public bool SavePaymentMethod { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }

    public class PaymentResult
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethodId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }

    public class PaymentMethodRequest
    {
        public string CustomerId { get; set; }
        public string Type { get; set; } // "card", "ach", etc.
        public CardInfo Card { get; set; }
        public BankAccountInfo BankAccount { get; set; }
        public BillingInfo BillingInfo { get; set; }
    }

    public class PaymentMethod
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string Type { get; set; }
        public string Last4 { get; set; }
        public string Brand { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public BillingInfo BillingInfo { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CardInfo
    {
        public string Number { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string CVV { get; set; }
    }

    public class BankAccountInfo
    {
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string AccountHolderName { get; set; }
    }

    public class BillingInfo
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class PaymentTransaction
    {
        public string TransactionId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime TransactionDate { get; set; }
        public string PaymentMethodId { get; set; }
        public string CustomerId { get; set; }
        public string Description { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }
}
