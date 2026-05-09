using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ResipWeb.Models.Payments;


namespace ResipWeb.Services;


public class MomoOptions
{
    public string Endpoint { get; set; } = default!;
    public string PartnerCode { get; set; } = default!;
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string RedirectUrl { get; set; } = default!;
    public string IpnUrl { get; set; } = default!;
}

public class MomoService
{
    private readonly HttpClient _http;
    private readonly MomoOptions _opt;

    public MomoService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _opt = config.GetSection("Momo").Get<MomoOptions>()!;
    }

    public async Task<MomoCreateResponse> CreatePayWithAtmAsync(long amount, string orderId, string orderInfo)
    {
        var requestId = $"{orderId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var extraData = "";
        var requestType = "payWithATM";

        // IMPORTANT: rawHash phải đúng thứ tự 
        var rawHash =
            "accessKey=" + _opt.AccessKey +
            "&amount=" + amount +
            "&extraData=" + extraData +
            "&ipnUrl=" + _opt.IpnUrl +
            "&orderId=" + orderId +
            "&orderInfo=" + orderInfo +
            "&partnerCode=" + _opt.PartnerCode +
            "&redirectUrl=" + _opt.RedirectUrl +
            "&requestId=" + requestId +
            "&requestType=" + requestType;

        var signature = HmacSha256(rawHash, _opt.SecretKey);

        var req = new MomoCreateRequest
        {
            partnerCode = _opt.PartnerCode,
            requestId = requestId,
            amount = amount,
            orderId = orderId,
            orderInfo = orderInfo,
            redirectUrl = _opt.RedirectUrl,
            ipnUrl = _opt.IpnUrl,
            extraData = extraData,
            requestType = requestType,
            signature = signature
        };

        var json = JsonSerializer.Serialize(req);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = await _http.PostAsync(_opt.Endpoint, content);
        var body = await resp.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<MomoCreateResponse>(
            body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return result ?? new MomoCreateResponse { resultCode = -1, message = "Empty response from MoMo" };
    }

    public bool VerifySignatureFromQuery(IQueryCollection q)
    {
        // MoMo Return Query có các field này:
        // partnerCode, orderId, requestId, amount, orderInfo, orderType, transId,
        // resultCode, message, payType, responseTime, extraData, signature

        var rawHash =
            "accessKey=" + _opt.AccessKey +
            "&amount=" + q["amount"] +
            "&extraData=" + q["extraData"] +
            "&message=" + q["message"] +
            "&orderId=" + q["orderId"] +
            "&orderInfo=" + q["orderInfo"] +
            "&orderType=" + q["orderType"] +
            "&partnerCode=" + q["partnerCode"] +
            "&payType=" + q["payType"] +
            "&requestId=" + q["requestId"] +
            "&responseTime=" + q["responseTime"] +
            "&resultCode=" + q["resultCode"] +
            "&transId=" + q["transId"];

        var expected = HmacSha256(rawHash, _opt.SecretKey);
        var provided = q["signature"].ToString();

        return SlowEquals(expected, provided);
    }


    public bool VerifySignatureFromIpnJson(JsonElement root)
    {
        // Lấy field từ JSON IPN
        string GetStr(string name) =>
            root.TryGetProperty(name, out var p) ? p.GetString() ?? "" : "";

        // các field phổ biến của IPN MoMo v2
        var rawHash =
            "accessKey=" + _opt.AccessKey +
            "&amount=" + GetStr("amount") +
            "&extraData=" + GetStr("extraData") +
            "&message=" + GetStr("message") +
            "&orderId=" + GetStr("orderId") +
            "&orderInfo=" + GetStr("orderInfo") +
            "&orderType=" + GetStr("orderType") +
            "&partnerCode=" + GetStr("partnerCode") +
            "&payType=" + GetStr("payType") +
            "&requestId=" + GetStr("requestId") +
            "&responseTime=" + GetStr("responseTime") +
            "&resultCode=" + GetStr("resultCode") +
            "&transId=" + GetStr("transId");

        var expected = HmacSha256(rawHash, _opt.SecretKey);
        var provided = GetStr("signature");

        return SlowEquals(expected, provided);
    }
    private static bool SlowEquals(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
        if (a.Length != b.Length) return false;

        int diff = 0;
        for (int i = 0; i < a.Length; i++)
            diff |= a[i] ^ b[i];

        return diff == 0;
    }



    private static string HmacSha256(string input, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(input);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(inputBytes);

        var sb = new StringBuilder(hashBytes.Length * 2);
        foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
