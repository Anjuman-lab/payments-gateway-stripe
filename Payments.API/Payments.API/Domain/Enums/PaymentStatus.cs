namespace Payments.API.Domain.Enums
{
    public enum PaymentStatus
    {
        Created = 0,
        Authorized = 1,
        Captured = 2,
        Voided = 3,
        Refunded = 4,
        Failed = 5
    }
}

