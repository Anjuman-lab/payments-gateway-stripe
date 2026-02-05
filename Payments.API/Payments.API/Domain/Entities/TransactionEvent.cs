namespace Payments.API.Domain.Entities
{
    public class TransactionEvent
    {
        public int Id { get; set; }
        public Guid PaymentId { get; set; }
        public Payment Payment { get; set; } = default!;

        public string Type { get; set; } = default!;     // "auth", "capture", "refund", "void", "inquire"
        public string Status { get; set; } = default!;   // "approved", "declined", "error"

        public string RequestJson { get; set; } = "{}";
        public string ResponseJson { get; set; } = "{}";

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
