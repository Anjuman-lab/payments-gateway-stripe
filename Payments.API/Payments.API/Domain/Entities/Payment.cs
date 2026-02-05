using Payments.API.Domain.Enums;

namespace Payments.API.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string OrderId { get; set; } = default!;
        public long Amount { get; set; }              // stored in cents
        public string Currency { get; set; } = "USD";
        public PaymentStatus Status { get; set; } = PaymentStatus.Created;

        // 🔹 Stripe fields
        public string Provider { get; set; } = "stripe";           // e.g., "stripe"
        public string ProviderPaymentId { get; set; } = default!;  // Stripe PaymentIntent ID (pi_...)

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedUtc { get; set; }

        public ICollection<TransactionEvent> Events { get; set; } = new List<TransactionEvent>();
    }
}
