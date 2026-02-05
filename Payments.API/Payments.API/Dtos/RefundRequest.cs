namespace Payments.API.Dtos
{
    public class RefundRequest
    {
        public long? Amount { get; set; }                // optional partial refund in cents
    }
}
