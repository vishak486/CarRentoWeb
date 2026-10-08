using System.Net.Http.Headers;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CarRentoWeb.Services
{
    public class RazorpayService : IRazorpayService
    {
        private readonly HttpClient _http;
        private readonly RazorpayOptions _opts;

        public RazorpayService(HttpClient http,IOptions<RazorpayOptions> opts)
        {
            _http = http;
            _opts = opts.Value;
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_opts.KeyId}:{_opts.KeySecret}"));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            _http.BaseAddress = new Uri("https://api.razorpay.com/v1/");
        }

        public async Task<string> CreateProviderOrderAsync(long amountInPaise, string receipt)
        {
            var payload = new
            {
                amount = amountInPaise,
                currency = "INR",
                receipt = receipt
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("orders", content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("id", out var idProp))
            {
                var id = idProp.GetString();
                if (!string.IsNullOrEmpty(id))
                    return id;
            }

            throw new InvalidOperationException("Failed to create provider order: response did not contain an order id.");
        }

        public bool VerifySignature(string providerOrderId, string providerPaymentId, string signature)
        {
            if (string.IsNullOrEmpty(providerOrderId) || string.IsNullOrEmpty(providerPaymentId) || string.IsNullOrEmpty(signature))
                return false;

            var payload = $"{providerOrderId}|{providerPaymentId}";
            var keyBytes = Encoding.UTF8.GetBytes(_opts.KeySecret ?? string.Empty);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expected = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            return FixedTimeEquals(expected, signature);
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];

            return diff == 0;
        }
    }
}
