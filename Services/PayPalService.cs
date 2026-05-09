using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ResipWeb.Services
{
    public class PayPalService
    {
        private readonly HttpClient _http;
        private readonly string _clientId;
        private readonly string _secret;
        private readonly string _baseUrl;
        private readonly string _returnUrl;
        private readonly string _cancelUrl;

        public PayPalService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _clientId = config["PayPal:ClientId"] ?? "";
            _secret = config["PayPal:Secret"] ?? "";
            string mode = config["PayPal:Mode"] ?? "sandbox";
            _baseUrl = mode == "live" ? "https://api-m.paypal.com" : "https://api-m.sandbox.paypal.com";
            _returnUrl = config["PayPal:ReturnUrl"] ?? "";
            _cancelUrl = config["PayPal:CancelUrl"] ?? "";
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_secret}"));
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString() ?? "";
        }

        public async Task<string> CreateOrderAsync(decimal amountUsd, string referenceId)
        {
            var accessToken = await GetAccessTokenAsync();

            var orderRequest = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = referenceId,
                        amount = new
                        {
                            currency_code = "USD",
                            value = amountUsd.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
                },
                application_context = new
                {
                    return_url = _returnUrl,
                    cancel_url = _cancelUrl,
                    user_action = "PAY_NOW"
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v2/checkout/orders");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(JsonSerializer.Serialize(orderRequest), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"PayPal API Error: {content}");
            }

            using var doc = JsonDocument.Parse(content);
            var links = doc.RootElement.GetProperty("links").EnumerateArray();
            foreach (var link in links)
            {
                if (link.GetProperty("rel").GetString() == "approve")
                {
                    return link.GetProperty("href").GetString() ?? "";
                }
            }

            throw new Exception("Approval URL not found");
        }

        public async Task<(bool success, string referenceId, string paypalOrderId, string amountUsd, string payerEmail)> CaptureOrderAsync(string token)
        {
            var accessToken = await GetAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v2/checkout/orders/{token}/capture");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent("", Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Capture Error: {errContent}");
                return (false, "", token, "", "");
            }

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            var status = doc.RootElement.GetProperty("status").GetString();
            if (status != "COMPLETED") return (false, "", token, "", "");

            // Lấy PayPal Order ID
            var paypalOrderId = doc.RootElement.TryGetProperty("id", out var idProp)
                ? idProp.GetString() ?? token : token;

            // Lấy email người mua
            var payerEmail = "";
            if (doc.RootElement.TryGetProperty("payer", out var payer) &&
                payer.TryGetProperty("email_address", out var emailProp))
                payerEmail = emailProp.GetString() ?? "";

            // Lấy reference_id và số tiền từ purchase_units
            var purchaseUnits = doc.RootElement.GetProperty("purchase_units").EnumerateArray();
            foreach (var punit in purchaseUnits)
            {
                var refId = punit.TryGetProperty("reference_id", out var r) ? r.GetString() ?? "" : "";

                var amountUsd = "";
                if (punit.TryGetProperty("payments", out var payments) &&
                    payments.TryGetProperty("captures", out var captures))
                {
                    foreach (var cap in captures.EnumerateArray())
                    {
                        if (cap.TryGetProperty("amount", out var amt) &&
                            amt.TryGetProperty("value", out var val))
                        {
                            amountUsd = val.GetString() ?? "";
                            break;
                        }
                    }
                }

                return (true, refId, paypalOrderId, amountUsd, payerEmail);
            }

            return (true, "", paypalOrderId, "", payerEmail);
        }
    }
}
