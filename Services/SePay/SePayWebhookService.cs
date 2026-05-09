using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ResipWeb.Models;
using ResipWeb.Models.Payments.SePay;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ResipWeb.Services.SePay
{
    public class SePayWebhookService : ISePayWebhookService
    {
        private readonly AppDbContext _db;
        private readonly SePayOptions _opt;
        private readonly IOrderService _orderService;

        public SePayWebhookService(AppDbContext db, IOptions<SePayOptions> opt, IOrderService orderService)
        {
            _db = db;
            _opt = opt.Value;
            _orderService = orderService;
        }

        public async Task HandleAsync(SePayWebhookDto payload, string? authorizationHeader)
        {
            // 0) Validate input
            if (payload == null) return;

            // 1) Verify Auth (API Key)
            EnsureAuthorized(authorizationHeader);

            // 2) Chỉ xử lý tiền vào
            if (!IsIncoming(payload.transferType))
                return;

            // 3) Idempotency chống bắn trùng theo payload.id
            var exists = await _db.Set<SePayTransaction>().AnyAsync(x => x.SepayTxnId == payload.id); if (exists) return;

            // 4) Lấy mã đơn để match: ưu tiên code, fallback content
            var orderCode = GetOrderCode(payload);

            // Nếu không có mã đơn => vẫn log để đối soát
            if (string.IsNullOrWhiteSpace(orderCode))
            {
                await SaveTxnOnly(payload, null, "NO_ORDER_CODE");
                return;
            }

            // 5) Tìm đơn hàng theo MaDonHang
            var order = await _db.Set<DonHang>()
                .FirstOrDefaultAsync(x => x.MaDonHang == orderCode);

            if (order == null)
            {
                await SaveTxnOnly(payload, orderCode, "ORDER_NOT_FOUND");
                return;
            }

            // 6) Không xử lý nếu đơn đã huỷ/hết hạn/đã thanh toán
            if (IsFinalStatus(order.TrangThai))
            {
                await SaveTxnOnly(payload, orderCode, $"IGNORE_STATUS:{order.TrangThai}");
                return;
            }

            // 7) Check amount
            // TongTien của bạn là decimal? (theo ảnh) => xử lý nullable
            var orderAmount = order.TongTien ?? 0m;
            var paidAmount = NormalizeAmount(payload.transferAmount);

            // So sánh an toàn (nếu muốn strict tuyệt đối)
            if (orderAmount != paidAmount)
            {
                await SaveTxnOnly(payload, orderCode, $"AMOUNT_MISMATCH:ORDER={orderAmount};PAYLOAD={paidAmount}");
                return;
            }

            // 8) Update trạng thái thanh toán & Hoàn tất đơn (Email, Trừ kho)
            await _orderService.TryFinalizeOrderAsync(orderCode);

            // 9) Lưu log transaction
            var txn = new SePayTransaction
            {
                SepayTxnId = payload.id,
                OrderCode = orderCode,
                Amount = paidAmount,
                Content = payload.content,
                TransferType = payload.transferType,
                TransactionDate = payload.transactionDate,
                RawGateway = payload.gateway,
                CreatedAt = DateTime.UtcNow,
                Note = "PAID"
            };

            _db.Add(txn);
            await _db.SaveChangesAsync();
        }

        // ===================== Helpers =====================

        private void EnsureAuthorized(string? authorizationHeader)
        {
            // Nếu bạn không bật ApiKey thì bỏ qua
            if (!string.Equals(_opt.WebhookAuthType, "ApiKey", StringComparison.OrdinalIgnoreCase))
                return;

            var key = _opt.WebhookApiKey?.Trim();
            if (string.IsNullOrWhiteSpace(key))
                throw new UnauthorizedAccessException("Missing WebhookApiKey in appsettings");

            // SePay gửi: Authorization: "Apikey <key>"
            if (string.IsNullOrWhiteSpace(authorizationHeader))
                throw new UnauthorizedAccessException("Missing Authorization header");

            var auth = authorizationHeader.Trim();

            // Chấp nhận:
            // - "Apikey key"
            // - "ApiKey key"
            // - "apikey key"
            // - hoặc test thủ công chỉ gửi "key"
            var ok =
                auth.Equals(key, StringComparison.OrdinalIgnoreCase) ||
                auth.Equals($"Apikey {key}", StringComparison.OrdinalIgnoreCase) ||
                auth.Equals($"ApiKey {key}", StringComparison.OrdinalIgnoreCase);

            if (!ok)
                throw new UnauthorizedAccessException("Invalid webhook api key");
        }

        private static bool IsIncoming(string? transferType)
        {
            // SePay thường dùng "in" / "IN"
            return string.Equals(transferType, "in", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsFinalStatus(string? trangThai)
        {
            // Bạn đang dùng string trạng thái
            // Final/ignore: DaThanhToan / DaHuy / HetHan
            if (string.IsNullOrWhiteSpace(trangThai)) return false;

            return trangThai.Equals("DaThanhToan", StringComparison.OrdinalIgnoreCase)
                || trangThai.Equals("DaHuy", StringComparison.OrdinalIgnoreCase)
                || trangThai.Equals("HetHan", StringComparison.OrdinalIgnoreCase);
        }

        private static string? GetOrderCode(SePayWebhookDto payload)
        {
            // Ưu tiên payload.code
            var code = payload.code?.Trim();
            if (!string.IsNullOrWhiteSpace(code))
                return code;

            // Fallback từ content
            return ExtractOrderCode(payload.content);
        }

        private static string? ExtractOrderCode(string? content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            // Match DH + số (DH102969)
            var m = Regex.Match(content, @"\bDH\d+\b", RegexOptions.IgnoreCase);
            return m.Success ? m.Value.ToUpperInvariant() : null;
        }

        private static decimal NormalizeAmount(object? amount)
        {
            // Tuỳ SePay DTO của bạn: transferAmount có thể là decimal/int/long/string
            if (amount == null) return 0m;

            if (amount is decimal d) return d;
            if (amount is int i) return i;
            if (amount is long l) return l;
            if (amount is double db) return (decimal)db;
            if (amount is float f) return (decimal)f;

            if (amount is string s)
            {
                s = s.Trim();
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dd))
                    return dd;

                // trường hợp "50,000" theo culture VN
                if (decimal.TryParse(s, NumberStyles.Any, new CultureInfo("vi-VN"), out var dd2))
                    return dd2;
            }

            // fallback cuối
            try { return Convert.ToDecimal(amount); } catch { return 0m; }
        }

        private async Task SaveTxnOnly(SePayWebhookDto payload, string? orderCode, string note)
        {
            var txn = new SePayTransaction
            {
                SepayTxnId = payload.id,
                OrderCode = orderCode,
                Amount = NormalizeAmount(payload.transferAmount),
                Content = payload.content,
                TransferType = payload.transferType,
                TransactionDate = payload.transactionDate,
                RawGateway = payload.gateway,
                CreatedAt = DateTime.UtcNow,
                Note = note
            };

            _db.Add(txn);
            await _db.SaveChangesAsync();
        }
    }
}
