using Microsoft.Extensions.Options;
using ResipWeb.Models.Payments.SePay;

namespace ResipWeb.Services.SePay
{
    public class SePayService : ISePayService
    {
        private readonly SePayOptions _opt;

        public SePayService(IOptions<SePayOptions> opt)
        {
            _opt = opt.Value;
        }

        public string BuildQrImageUrl(string orderCode, decimal amount)
        {
            // QR động nhúng web/app:
            // https://qr.sepay.vn/img?acc=...&bank=...&amount=...&des=...

            var baseUrl = _opt.QrBaseUrl?.TrimEnd('/') ?? "https://qr.sepay.vn/img";

            // des nên là mã đơn
            var url =
                $"{baseUrl}?acc={Uri.EscapeDataString(_opt.Acc)}" +
                $"&bank={Uri.EscapeDataString(_opt.Bank)}" +
                $"&amount={amount:0}" +
                $"&des={Uri.EscapeDataString(orderCode)}";

            return url;
        }
    }
}
