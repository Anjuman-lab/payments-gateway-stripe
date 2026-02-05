namespace Payments.API.Dtos
{
    public record CreatePaymentRequest(
        long Amount,                     // in the smallest currency unit (e.g., cents)
        string Currency = "usd",
        string? Description = null,
        string? OrderId = null,          // optional: good for your DB linkage
        bool Capture = true              // true = auth+capture now; false = auth only (manual capture)
    );

}
