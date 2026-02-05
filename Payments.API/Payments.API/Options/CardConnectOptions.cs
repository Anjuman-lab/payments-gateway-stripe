namespace Payments.API.Options
{
    public class CardConnectOptions
    {
        public string BaseUrl { get; set; } = default!;  // e.g., https://<sandbox-host>/cardconnect/rest
        public string MerchantId { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
