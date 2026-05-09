using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Libs;
using ResipWeb.Models;
using ResipWeb.Models.Payments;
using ResipWeb.Services;

namespace ResipWeb.Controllers
{
    public class VnPayPaymentController : Controller
    {
        private readonly VnPayService _vnPayService;
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;
        private readonly ILogger<VnPayPaymentController> _logger;

        public VnPayPaymentController(VnPayService vnPayService, IConfiguration config, AppDbContext db, ILogger<VnPayPaymentController> logger)
        {
            _vnPayService = vnPayService;
            _config = config;
            _db = db;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Create(string orderCode, long amount)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
            {
                _logger.LogWarning("Create payment called with empty orderCode");
                return BadRequest("Order code is required");
            }

            if (amount <= 0)
            {
                _logger.LogWarning("Create payment called with invalid amount: {Amount}", amount);
                return BadRequest("Amount must be greater than 0");
            }

            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
                var paymentUrl = _vnPayService.CreatePaymentUrl(orderCode, amount, ipAddress);
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment URL for orderCode: {OrderCode}, amount: {Amount}", orderCode, amount);
                return StatusCode(500, "Error creating payment URL");
            }
        }

        // =========================
        // RETURN (client redirect)
        // =========================
        [HttpGet]
        public async Task<IActionResult> Return()
        {
            try
            {
                var hashSecret = (_config["VnPayConfig:HashSecret"] ?? "").Trim();
                var vnpay = new VnPayLibrary();

                // Parse query parameters safely
                foreach (var (k, v) in Request.Query)
                {
                    if (!string.IsNullOrEmpty(k) && k.StartsWith("vnp_"))
                        vnpay.AddResponseData(k, v.ToString());
                }

                // Safe access to query parameters
                var secureHash = Request.Query["vnp_SecureHash"].FirstOrDefault() ?? "";
                if (string.IsNullOrEmpty(secureHash))
                {
                    _logger.LogWarning("VNPay Return: Missing vnp_SecureHash");
                    return Content("INVALID SIGNATURE");
                }

                var ok = vnpay.ValidateSignature(secureHash, hashSecret);

                // Parse dữ liệu cơ bản với safe access
                var txnRef = Request.Query["vnp_TxnRef"].FirstOrDefault() ?? "";
                var transactionNo = Request.Query["vnp_TransactionNo"].FirstOrDefault() ?? "";
                long.TryParse(Request.Query["vnp_Amount"].FirstOrDefault(), out long amount);

                var responseCode = Request.Query["vnp_ResponseCode"].FirstOrDefault() ?? "";
                var txnStatus = Request.Query["vnp_TransactionStatus"].FirstOrDefault() ?? "";
                var tmnCode = Request.Query["vnp_TmnCode"].FirstOrDefault() ?? "";

                // Nếu chữ ký sai vẫn có thể lưu log, nhưng đang return luôn 
                if (!ok)
                {
                    _logger.LogWarning("VNPay Return: Invalid signature for MaDonHang: {MaDonHang}", txnRef);
                    return Content("INVALID SIGNATURE");
                }

                await UpsertVnPayAsync(new VnPayTransaction
                {
                    MaDonHang = txnRef,
                    TransactionNo = transactionNo,
                    Amount = amount,
                    ResponseCode = responseCode,
                    TransactionStatus = txnStatus,
                    TmnCode = tmnCode,
                    IsValidSignature = ok,
                    Source = "RETURN",
                    CreatedAt = DateTime.UtcNow,
                    HasReturn = true
                });

                return Content("OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay Return");
                return Content("ERROR");
            }
        }

        // =========================
        // IPN (server webhook)
        // =========================
        [HttpGet]
        public async Task<IActionResult> Ipn()
        {
            try
            {
                var hashSecret = (_config["VnPayConfig:HashSecret"] ?? "").Trim();
                var vnpay = new VnPayLibrary();

                foreach (var (k, v) in Request.Query)
                {
                    if (!string.IsNullOrEmpty(k) && k.StartsWith("vnp_"))
                        vnpay.AddResponseData(k, v.ToString());
                }

                // Safe access to query parameters
                var secureHash = Request.Query["vnp_SecureHash"].FirstOrDefault() ?? "";
                if (string.IsNullOrEmpty(secureHash))
                {
                    _logger.LogWarning("VNPay IPN: Missing vnp_SecureHash");
                    return Json(new { RspCode = "97", Message = "Invalid signature" });
                }

                var ok = vnpay.ValidateSignature(secureHash, hashSecret);

                var txnRef = Request.Query["vnp_TxnRef"].FirstOrDefault() ?? "";
                var transactionNo = Request.Query["vnp_TransactionNo"].FirstOrDefault() ?? "";
                long.TryParse(Request.Query["vnp_Amount"].FirstOrDefault(), out long amount);

                var responseCode = Request.Query["vnp_ResponseCode"].FirstOrDefault() ?? "";
                var txnStatus = Request.Query["vnp_TransactionStatus"].FirstOrDefault() ?? "";
                var tmnCode = Request.Query["vnp_TmnCode"].FirstOrDefault() ?? "";

                if (!ok)
                {
                    _logger.LogWarning("VNPay IPN: Invalid signature for MaDonHang: {MaDonHang}", txnRef);
                    return Json(new { RspCode = "97", Message = "Invalid signature" });
                }

                await UpsertVnPayAsync(new VnPayTransaction
                {
                    MaDonHang = txnRef,
                    TransactionNo = transactionNo,
                    Amount = amount,
                    ResponseCode = responseCode,
                    TransactionStatus = txnStatus,
                    TmnCode = tmnCode,
                    IsValidSignature = ok,
                    Source = "IPN",
                    CreatedAt = DateTime.UtcNow,
                    HasIpn = true
                });

                return Json(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay IPN");
                return Json(new { RspCode = "99", Message = "Internal error" });
            }
        }

        // ==================================================
        // UPSERT: gộp RETURN + IPN thành 1 record trong DB
        // Key ưu tiên: MaDonHang (txnRef). TransactionNo có thể rỗng ở RETURN.
        // ==================================================
        private async Task UpsertVnPayAsync(VnPayTransaction dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MaDonHang))
            {
                _logger.LogWarning("UpsertVnPayAsync called with empty MaDonHang");
                return;
            }

            try
            {
                // ✅ Gộp theo MaDonHang (TxnRef) để RETURN/IPN luôn match
                var existing = await _db.VnPayTransactions
                    .FirstOrDefaultAsync(x => x.MaDonHang == dto.MaDonHang);

                if (existing == null)
                {
                    // Insert mới
                    dto.HasReturn = dto.Source == "RETURN";
                    dto.HasIpn = dto.Source == "IPN";
                    _db.VnPayTransactions.Add(dto);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Created new VNPay transaction. MaDonHang: {MaDonHang}, Source: {Source}", dto.MaDonHang, dto.Source);
                    return;
                }

                // ✅ Update existing (IPN retry hoặc IPN cập nhật)
                existing.TransactionNo = dto.TransactionNo;
                existing.Amount = dto.Amount;
                existing.ResponseCode = dto.ResponseCode;
                existing.TransactionStatus = dto.TransactionStatus;
                existing.TmnCode = dto.TmnCode;
                existing.BankCode = dto.BankCode;
                existing.BankTranNo = dto.BankTranNo;
                existing.CardType = dto.CardType;
                existing.OrderInfo = dto.OrderInfo;
                existing.PayDate = dto.PayDate;
                existing.SecureHash = dto.SecureHash;
                existing.SecureHashType = dto.SecureHashType;
                existing.IsValidSignature = dto.IsValidSignature;
                existing.CreatedAt = dto.CreatedAt;
                existing.Source = "IPN";

                await _db.SaveChangesAsync();
                _logger.LogInformation("Updated VNPay IPN record. MaDonHang: {MaDonHang}", dto.MaDonHang);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while saving VNPay IPN. MaDonHang: {MaDonHang}", dto.MaDonHang);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save VNPay IPN. MaDonHang: {MaDonHang}", dto.MaDonHang);
                throw;
            }
        }

        // =============================
        // XÓA RECORDS (nếu cần)
        // =============================
        private void CleanupVnPayTransactions()
        {
            try
            {
                // Xóa toàn bộ RETURN records (giữ lại IPN)
                var deleteAllReturn = _db.VnPayTransactions
                    .Where(x => x.Source == "RETURN" || (x.HasReturn && !x.HasIpn));

                if (deleteAllReturn.Any())
                {
                    _db.VnPayTransactions.RemoveRange(deleteAllReturn);
                    _db.SaveChanges();
                    _logger.LogInformation("Deleted all RETURN records from VnPayTransactions table");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up VnPayTransactions table");
            }
        }
    }
}
