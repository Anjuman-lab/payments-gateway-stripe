using System.Text.Json.Nodes;

namespace Payments.API.Clients
{
    public interface ICardConnectClient
    {
        Task<JsonObject> AuthorizeAsync(JsonObject payload, CancellationToken ct = default);
        Task<JsonObject> CaptureAsync(JsonObject payload, CancellationToken ct = default);
        Task<JsonObject> RefundAsync(JsonObject payload, CancellationToken ct = default);
        Task<JsonObject> VoidAsync(JsonObject payload, CancellationToken ct = default);
        Task<JsonObject> InquireAsync(string retref, CancellationToken ct = default);
    }
}
