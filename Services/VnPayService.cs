using Microsoft.Extensions.Logging;
using ResipWeb.Libs;
using Microsoft.AspNetCore.Http;

public class VnPayService
{
    private readonly IConfiguration _config;
    private readonly ILogger<VnPayService> _logger;
    public bool ValidateReturn(IQueryCollection query, out string txnRef, out string resp, out string status)
    {
        txnRef = "";
        resp = "";
        status = "";

        var hashSecret = (_config["VnPayConfig:HashSecret"] ?? "").Trim();

        var vnp = new VnPayLibrary();

        // add toàn bộ vnp_ từ query vào responseData (trừ null)
        foreach (var (k, v) in query)
        {
            if (!string.IsNullOrWhiteSpace(k) && k.StartsWith("vnp_"))
            {
                vnp.AddResponseData(k, v.ToString());
            }
        }

        // Safe access to query parameter
        var secureHash = query["vnp_SecureHash"].FirstOrDefault() ?? "";
        var ok = !string.IsNullOrEmpty(secureHash) && vnp.ValidateSignature(secureHash, hashSecret);

        txnRef = vnp.GetResponseData("vnp_TxnRef");
        resp = vnp.GetResponseData("vnp_ResponseCode");
        status = vnp.GetResponseData("vnp_TransactionStatus");

        return ok;
    }


    public VnPayService(IConfiguration config, ILogger<VnPayService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public string CreatePaymentUrl(string orderId, long amountVnd, string ipAddress)
    {
        var tmnCode = (_config["VnPayConfig:TmnCode"] ?? "").Trim();
        var hashSecret = (_config["VnPayConfig:HashSecret"] ?? "").Trim();
        var baseUrl = (_config["VnPayConfig:Url"] ?? "").Trim();
        var returnUrl = (_config["VnPayConfig:ReturnUrl"] ?? "").Trim();

        _logger.LogWarning("[VNPAY] baseUrl={baseUrl} returnUrl={returnUrl} tmn={tmn}",
            baseUrl, returnUrl, tmnCode);

        var vnp = new VnPayLibrary();
        vnp.AddRequestData("vnp_Version", "2.1.0");
        vnp.AddRequestData("vnp_Command", "pay");
        vnp.AddRequestData("vnp_TmnCode", tmnCode);
        vnp.AddRequestData("vnp_Amount", (amountVnd * 100).ToString());
        vnp.AddRequestData("vnp_CurrCode", "VND");
        vnp.AddRequestData("vnp_TxnRef", orderId);
        vnp.AddRequestData("vnp_OrderInfo", $"Thanh toan don {orderId}");
        vnp.AddRequestData("vnp_OrderType", "other");
        vnp.AddRequestData("vnp_ReturnUrl", returnUrl);
        vnp.AddRequestData("vnp_IpAddr", ipAddress);
        vnp.AddRequestData("vnp_Locale", "vn");
        vnp.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));

        // ✅ log 2 thứ quan trọng nhất (không log secret value)
        // SECURITY: Chỉ log hashData và secret length, không log secret value
        _logger.LogWarning("[VNPAY] hashData={hashData}", vnp.DebugHashData());
        _logger.LogWarning("[VNPAY] secretLen={len}", hashSecret.Length);

        var url = vnp.CreateRequestUrl(baseUrl, hashSecret);

        _logger.LogWarning("[VNPAY] payUrl={url}", url);

        return url;
    }
}