using Payments.API.Domain.Enums;

namespace Payments.API.Dtos
{
    public class PaymentResponseDto
    {
        public Guid Id { get; set; }
        public string? OrderId { get; set; }
        public long Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public PaymentStatus Status { get; set; }
        public string Provider { get; set; } = "stripe";
        public string ProviderPaymentId { get; set; } = default!;
        public string? ClientSecret { get; set; } = default!;
        public DateTime CreatedUtc { get; set; }
        public DateTime? UpdatedUtc { get; set; }
    }
}

