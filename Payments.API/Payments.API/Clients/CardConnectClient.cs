using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Payments.API.Options;

namespace Payments.API.Clients
{
    public class CardConnectClient : ICardConnectClient
    {
        private readonly HttpClient _http;
        private readonly CardConnectOptions _opts;
        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNamingPolicy = null };

        public CardConnectClient(HttpClient http, IOptions<CardConnectOptions> opts)
        {
            _http = http;
            _opts = opts.Value;

            var creds = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_opts.Username}:{_opts.Password}"));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", creds);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<JsonObject> PostAsync(string path, JsonObject payload, CancellationToken ct)
        {
            var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOpts), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync(path, content, ct);
            res.EnsureSuccessStatusCode();
            var text = await res.Content.ReadAsStringAsync(ct);
            return JsonNode.Parse(text)!.AsObject();
        }

        public Task<JsonObject> AuthorizeAsync(JsonObject payload, CancellationToken ct = default)
            => PostAsync("auth", payload, ct);

        public Task<JsonObject> CaptureAsync(JsonObject payload, CancellationToken ct = default)
            => PostAsync("capture", payload, ct);

        public Task<JsonObject> RefundAsync(JsonObject payload, CancellationToken ct = default)
            => PostAsync("refund", payload, ct);

        public Task<JsonObject> VoidAsync(JsonObject payload, CancellationToken ct = default)
            => PostAsync("void", payload, ct);

        public async Task<JsonObject> InquireAsync(string retref, CancellationToken ct = default)
        {
            var res = await _http.GetAsync($"inquire/{retref}/{_opts.MerchantId}", ct);
            res.EnsureSuccessStatusCode();
            var text = await res.Content.ReadAsStringAsync(ct);
            return JsonNode.Parse(text)!.AsObject();
        }
    }
}
