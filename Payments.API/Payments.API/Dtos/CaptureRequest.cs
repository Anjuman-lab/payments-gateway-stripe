namespace Payments.API.Dtos
{
    public class CaptureRequest
    {
        public long? Amount { get; set; }                // optional partial capture in cents
    }
}
