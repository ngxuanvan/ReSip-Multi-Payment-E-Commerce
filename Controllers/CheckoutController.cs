using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Areas.Admin.Repository;
using ResipWeb.Models;
using ResipWeb.Models.Payments;
using ResipWeb.Models.ViewModels;
using ResipWeb.Services;
using System.Security.Claims;
using System.Text.Json;


namespace ResipWeb.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly AppDbContext _context;
        private readonly MomoService _momo;
        private readonly IEmailSender _emailSender;
        private readonly VnPayService _vnpay;
        private readonly PayPalService _paypal;
        private readonly ExchangeRateService _exchangeRate;
        private readonly IOrderService _orderService;
        public CheckoutController(AppDbContext context, MomoService momo, IEmailSender emailSender, VnPayService vnpay, PayPalService paypal, ExchangeRateService exchangeRate, IOrderService orderService)
        {
            _context = context;
            _momo = momo;
            _vnpay = vnpay;
            _paypal = paypal;
            _exchangeRate = exchangeRate;
            _emailSender = emailSender;
            _orderService = orderService;
        }

        // GET: /Checkout
        public async Task<IActionResult> Index()
        {
            var cartItems = await GetCartItems();

            if (cartItems.Count == 0)
                return RedirectToAction("Index", "Cart");

            var model = new CheckoutViewModel
            {
                CartItems = cartItems.Select(x => new CartItemViewModel
                {
                    TenSanPham = x.SanPham.TenSanPham,
                    DonGia = x.SanPham.GiaBan,
                    SoLuong = x.SoLuong,
                    HinhAnh = x.SanPham.AnhChinh
                }).ToList(),
                TongTienHang = cartItems.Sum(x => x.SanPham.GiaBan * x.SoLuong),
                PhiVanChuyen = 30000,
                PhuongThucThanhToan = "COD"
            };

            return View(model);
        }

        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var cartItems = await GetCartItems();
            if (cartItems.Count == 0)
            {
                // Trường hợp user bấm submit 2 lần: request #2 vào sau khi request #1 đã finalize và xoá giỏ.
                // Redirect về trang thành công thay vì đưa user về giỏ hàng trống.
                var currentUserId = GetUserId();
                var userIdStr = currentUserId.ToString();

                var recentOrder = await _context.DonHangs
                    .Where(x => x.UserId == userIdStr)
                    .OrderByDescending(x => x.NgayTao)
                    .FirstOrDefaultAsync();

                if (recentOrder?.NgayTao != null)
                {
                    var age = DateTime.Now - recentOrder.NgayTao.Value;
                    if (age.TotalMinutes <= 2)
                        return RedirectToAction("OrderSuccess", new { orderCode = recentOrder.MaDonHang });
                }

                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                ReloadCartToModel(model, cartItems);
                return View("Index", model);
            }

            var userId = GetUserId();
            // tính tiền
            decimal tongTien = cartItems.Sum(i => i.SoLuong * i.SanPham.GiaBan);
            decimal phiShip = 30000;
            decimal tongThanhToan = tongTien + phiShip;

            var donHang = new DonHang
            {
                UserId = userId.ToString(),
                // cải tiến id đơn hàng
                MaDonHang = "DH" + DateTime.Now.ToString("yyMMddHHmmss") + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                HoTen = model.HoTen,
                DienThoai = model.SoDienThoai,
                Email = model.Email,
                DiaChi = $"{model.DiaChiCuThe}, {model.PhuongXa}, {model.TinhThanh}",
                NgayTao = DateTime.Now,
                TongTien = tongThanhToan
            };
            var ptttStr = (model.PhuongThucThanhToan ?? "COD").ToUpper();

            donHang.PhuongThucThanhToan = ptttStr switch
            {
                "MOMO" => PhuongThucThanhToanEnum.MOMO,
                "VNPAY" => PhuongThucThanhToanEnum.VNPAY,
                "SEPAY" => PhuongThucThanhToanEnum.SEPAY,
                "PAYPAL" => PhuongThucThanhToanEnum.PAYPAL,
                _ => PhuongThucThanhToanEnum.COD
            };
            // COD
            if (ptttStr == "COD")
            {
                donHang.TrangThai = "ChoXuLy";
                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();

                // Dùng OrderService để xử lý trừ kho và gửi email
                await _orderService.TryFinalizeOrderByIdAsync(donHang.Id, "ChoXuLy");

                return RedirectToAction("OrderSuccess");
            }

            // MOMO
            if (ptttStr == "MOMO")
            {
                donHang.TrangThai = "ChoThanhToan";
                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();

                var amount = (long)Math.Round(tongThanhToan);
                var orderIdForMomo = donHang.MaDonHang;
                var orderInfo = $"Thanh toán đơn {donHang.MaDonHang} - Resip";

                var momoRes = await _momo.CreatePayWithAtmAsync(amount, orderIdForMomo, orderInfo);

                if (momoRes.payUrl is null)
                {
                    donHang.TrangThai = "ThanhToanLoi";
                    await _context.SaveChangesAsync();
                    return BadRequest($"MoMo lỗi: {momoRes.resultCode} - {momoRes.message}");
                }

                return Redirect(momoRes.payUrl);
            }


            // VNPAY
            // VNPAY
            if (ptttStr == "VNPAY")
            {
                donHang.TrangThai = "ChoThanhToan";
                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();

                var amount = (long)Math.Round(tongThanhToan);

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
                var payUrl = _vnpay.CreatePaymentUrl(donHang.MaDonHang, amount, ipAddress);

                return Redirect(payUrl);
            }
            //sepay
            if (ptttStr == "SEPAY") // hoặc model.PhuongThucThanhToan == SEPAY
            {
                donHang.TrangThai = "ChoThanhToan";
                donHang.NgayTao = DateTime.Now;
                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();

                return Redirect($"/sepay/pay?orderCode={donHang.MaDonHang}");
            }

            // PAYPAL
            if (ptttStr == "PAYPAL")
            {
                donHang.TrangThai = "ChoThanhToan";
                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();

                decimal vndRate = await _exchangeRate.GetUsdToVndAsync();
                decimal amountUsd = Math.Round(tongThanhToan / vndRate, 2);

                try 
                {
                    var approvalUrl = await _paypal.CreateOrderAsync(amountUsd, donHang.MaDonHang);
                    return Redirect(approvalUrl);
                }
                catch (Exception ex)
                {
                    donHang.TrangThai = "ThanhToanLoi";
                    await _context.SaveChangesAsync();
                    return BadRequest($"PayPal lỗi: {ex.Message}");
                }
            }



            return BadRequest("Phương thức thanh toán không hợp lệ");
        }

        // Trang thông báo đặt hàng thành công
        public IActionResult OrderSuccess()
        {
            return View();
        }

        // =========================
        // MoMo Return: user quay về (GET)
        // =========================
        // PayPal Return: user quay về (GET)
        // =========================
        [AllowAnonymous]
        [HttpGet("checkout/paypal-return")]
        public async Task<IActionResult> PayPalReturn(string token, string PayerID)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.IsSuccess = false;
                return View("PayPalReturn");
            }

            var (success, referenceId, paypalOrderId, amountUsd, payerEmail) =
                await _paypal.CaptureOrderAsync(token);

            // ✅ Lưu Log Giao dịch PayPal vào Database
            var trans = new PayPalTransaction
            {
                OrderId = referenceId ?? "", // MaDonHang
                PayPalOrderId = paypalOrderId ?? "", // Capture ID
                Token = token, // Order ID từ PayPal
                PayerID = PayerID ?? "",
                PayerEmail = payerEmail ?? "",
                AmountUsd = amountUsd ?? "",
                Status = success ? "COMPLETED" : "FAILED",
                CreatedAt = DateTime.Now
            };

            _context.PayPalTransactions.Add(trans);
            await _context.SaveChangesAsync();

            ViewBag.IsSuccess     = success;
            ViewBag.OrderId       = referenceId;
            ViewBag.PayPalOrderId = paypalOrderId;
            ViewBag.AmountUsd     = amountUsd;
            ViewBag.PayerEmail    = payerEmail;
            ViewBag.PayTime       = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            // Quy đổi sang VND để hiển thị
            if (decimal.TryParse(amountUsd,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var usd))
            {
                decimal rate = await _exchangeRate.GetUsdToVndAsync();
                decimal vnd  = Math.Round(usd * rate);
                ViewBag.AmountVnd = vnd.ToString("#,##0");
            }

            if (success && !string.IsNullOrEmpty(referenceId) &&
                await CanFinalizePayPalPaymentAsync(referenceId, amountUsd))
            {
                await _orderService.TryFinalizeOrderAsync(referenceId);
            }

            return View("PayPalReturn");
        }

        // =========================
        [AllowAnonymous]
        [HttpGet("checkout/momo-return")]
        public async Task<IActionResult> MomoReturn()
        {
            var ok = _momo.VerifySignatureFromQuery(Request.Query);
            ViewBag.IsValidSignature = ok;

            var orderId = Request.Query["orderId"].ToString();
            int.TryParse(Request.Query["resultCode"], out var resultCode);

            var tx = new ResipWeb.Models.Payments.MomoTransaction
            {
                PartnerCode = Request.Query["partnerCode"].ToString(),
                OrderId = Request.Query["orderId"].ToString(),
                RequestId = Request.Query["requestId"].ToString(),
                Amount = long.TryParse(Request.Query["amount"], out var amt) ? amt : 0,
                OrderInfo = Request.Query["orderInfo"].ToString(),
                OrderType = Request.Query["orderType"].ToString(),
                TransId = long.TryParse(Request.Query["transId"], out var tid) ? tid : 0,
                ResultCode = int.TryParse(Request.Query["resultCode"], out var rc) ? rc : -999,
                Message = Request.Query["message"].ToString(),
                PayType = Request.Query["payType"].ToString(),
                ResponseTime = long.TryParse(Request.Query["responseTime"], out var rt) ? rt : 0,
                ExtraData = Request.Query["extraData"].ToString(),
                Signature = Request.Query["signature"].ToString(),
                Source = "RETURN",
                CreatedAt = DateTime.Now
            };

            var existed = await _context.MomoTransactions
                .AnyAsync(x => x.OrderId == tx.OrderId && x.TransId == tx.TransId && x.Source == "RETURN");

            if (!existed)
            {
                _context.MomoTransactions.Add(tx);
                await _context.SaveChangesAsync();
            }


            if (!ok)
                return BadRequest("Invalid signature (Return)");

            if (!string.IsNullOrWhiteSpace(orderId) && resultCode == 0 &&
                await CanFinalizeVndPaymentAsync(orderId, PhuongThucThanhToanEnum.MOMO, tx.Amount))
            {
                await _orderService.TryFinalizeOrderAsync(orderId);
            }

            return View();
        }

        //vnpay get
        [AllowAnonymous]
        [HttpGet("checkout/vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            // VNPay có thể redirect về với nhiều case (success/cancel/fail). Không được throw để tránh lỗi ảnh hưởng luồng khác.
            string txnRef = "";
            string resp = "";
            string status = "";
            bool ok = false;

            try
            {
                ok = _vnpay.ValidateReturn(Request.Query, out txnRef, out resp, out status);
            }
            catch
            {
                ok = false;
            }

            // Luôn set ViewBag để View hiển thị được kể cả khi lỗi/thiếu params
            ViewBag.IsValidSignature = ok;
            ViewBag.TxnRef = txnRef;
            ViewBag.ResponseCode = resp;
            ViewBag.TransactionStatus = status;

            // ===== LƯU DB (best-effort, không làm văng lỗi UI) =====
            try
            {
                var tx = new VnPayTransaction
                {
                    TxnRef = Request.Query["vnp_TxnRef"].ToString(),
                    Amount = long.TryParse(Request.Query["vnp_Amount"], out var a) ? a : 0,
                    BankCode = Request.Query["vnp_BankCode"].ToString(),
                    BankTranNo = Request.Query["vnp_BankTranNo"].ToString(),
                    CardType = Request.Query["vnp_CardType"].ToString(),
                    OrderInfo = Request.Query["vnp_OrderInfo"].ToString(),
                    PayDate = Request.Query["vnp_PayDate"].ToString(),
                    TransactionNo = Request.Query["vnp_TransactionNo"].ToString(),
                    ResponseCode = resp,
                    TransactionStatus = status,
                    TmnCode = Request.Query["vnp_TmnCode"].ToString(),
                    SecureHash = Request.Query["vnp_SecureHash"].ToString(),
                    SecureHashType = Request.Query["vnp_SecureHashType"].ToString(),
                    Source = "RETURN",
                    IsValidSignature = ok,
                    MaDonHang = txnRef,
                    CreatedAt = DateTime.Now,
                    HasReturn = true
                };

                var existed = await _context.VnPayTransactions
                    .AnyAsync(x => x.MaDonHang == tx.MaDonHang && x.Source == "RETURN" && x.TransactionNo == tx.TransactionNo);

                if (!existed)
                {
                    _context.VnPayTransactions.Add(tx);
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                // ignore - chỉ log/DB, không ảnh hưởng UI
            }
            // ===================================

            // Signature fail => hiển thị thất bại (không trả 400 để user không thấy trang error)
            if (!ok)
                return View("~/Views/VnPayPayment/Return.cshtml");

            var paidAmount = long.TryParse(Request.Query["vnp_Amount"], out var paidAmountRaw)
                ? paidAmountRaw / 100m
                : 0m;

            // Success => finalize
            if (resp == "00" && status == "00" && !string.IsNullOrWhiteSpace(txnRef) &&
                await CanFinalizeVndPaymentAsync(txnRef, PhuongThucThanhToanEnum.VNPAY, paidAmount))
            {
                await _orderService.TryFinalizeOrderAsync(txnRef);
                return View("~/Views/VnPayPayment/Return.cshtml");
            }

            // Cancel/Fail => nếu đơn còn đang chờ thanh toán thì mark thất bại (idempotent)
            if (!string.IsNullOrWhiteSpace(txnRef))
            {
                try
                {
                    var dhFail = await _context.DonHangs.FirstOrDefaultAsync(x => x.MaDonHang == txnRef);
                    if (dhFail != null && dhFail.TrangThai == "ChoThanhToan")
                    {
                        dhFail.TrangThai = "ThanhToanThatBai";
                        await _context.SaveChangesAsync();
                    }
                }
                catch
                {
                    // ignore
                }
            }

            return View("~/Views/VnPayPayment/Return.cshtml");
        }


        [AllowAnonymous]
        [HttpGet("checkout/vnpay-test")]
        public IActionResult VnpayTest(string orderId, long amount)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var payUrl = _vnpay.CreatePaymentUrl(orderId, amount, ip);
            return Redirect(payUrl);
        }
        [AllowAnonymous]
        [HttpGet("checkout/vnpay-ipn")]
        public async Task<IActionResult> VnPayIpn()
        {
            var ok = _vnpay.ValidateReturn(Request.Query, out var txnRef, out var resp, out var status);

            var tx = new VnPayTransaction
            {
                TxnRef = Request.Query["vnp_TxnRef"].ToString(),
                Amount = long.TryParse(Request.Query["vnp_Amount"], out var a) ? a : 0,
                TransactionNo = Request.Query["vnp_TransactionNo"].ToString(),
                ResponseCode = resp,
                TransactionStatus = status,
                OrderInfo = Request.Query["vnp_OrderInfo"].ToString(),
                BankCode = Request.Query["vnp_BankCode"].ToString(),
                SecureHash = Request.Query["vnp_SecureHash"].ToString(),
                Source = "IPN",
                IsValidSignature = ok,
                CreatedAt = DateTime.Now,
                MaDonHang = Request.Query["vnp_TxnRef"].ToString(),
            };

            var existed = await _context.VnPayTransactions.AnyAsync(x =>
                x.Source == "IPN" &&
                x.TxnRef == tx.TxnRef &&
                x.TransactionNo == tx.TransactionNo
            );

            if (!existed)
            {
                _context.VnPayTransactions.Add(tx);
                await _context.SaveChangesAsync();
            }

            var paidAmount = tx.Amount / 100m;

            if (ok && resp == "00" && status == "00" && !string.IsNullOrWhiteSpace(txnRef) &&
                await CanFinalizeVndPaymentAsync(txnRef, PhuongThucThanhToanEnum.VNPAY, paidAmount))
            {
                await _orderService.TryFinalizeOrderAsync(txnRef);
            }

            // VNPAY thường muốn response dạng text/json tùy spec – sandbox có thể không cần
            return Ok("OK");
        }

        [HttpGet("/hooks/sepay-payment")]
        public IActionResult Health() => Ok("SePay webhook is running");


        // =========================
        // MoMo IPN: server callback (POST)
        // =========================
        [AllowAnonymous]
        [HttpPost("checkout/momo-ipn")]
        public async Task<IActionResult> MomoIpn()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine("===== MOMO IPN =====");
            Console.WriteLine(body);
            Console.WriteLine("====================");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // ✅ verify signature
            var ok = _momo.VerifySignatureFromIpnJson(root);
            if (!ok) return BadRequest("Invalid signature (IPN)");

            // ✅ lấy orderId/resultCode 
            var orderId = root.TryGetProperty("orderId", out var oid1) ? oid1.GetString() : null;
            var resultCode = root.TryGetProperty("resultCode", out var rc1) ? rc1.GetInt32() : -999;

            // map transaction 
            var tx = new ResipWeb.Models.Payments.MomoTransaction
            {
                PartnerCode = root.TryGetProperty("partnerCode", out var pc) ? pc.GetString() ?? "" : "",
                OrderId = root.TryGetProperty("orderId", out var oid2) ? oid2.GetString() ?? "" : "",
                RequestId = root.TryGetProperty("requestId", out var rid) ? rid.GetString() ?? "" : "",
                Amount = root.TryGetProperty("amount", out var a) ? a.GetInt64() : 0,
                OrderInfo = root.TryGetProperty("orderInfo", out var oi) ? oi.GetString() ?? "" : "",
                OrderType = root.TryGetProperty("orderType", out var ot) ? ot.GetString() ?? "" : "",
                TransId = root.TryGetProperty("transId", out var t) ? t.GetInt64() : 0,
                ResultCode = root.TryGetProperty("resultCode", out var rc2) ? rc2.GetInt32() : -999,
                Message = root.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "",
                PayType = root.TryGetProperty("payType", out var pt) ? pt.GetString() ?? "" : "",
                ResponseTime = root.TryGetProperty("responseTime", out var rtime) ? rtime.GetInt64() : 0,
                ExtraData = root.TryGetProperty("extraData", out var ed) ? ed.GetString() ?? "" : "",
                Signature = root.TryGetProperty("signature", out var sig) ? sig.GetString() ?? "" : "",
                Source = "IPN",
                CreatedAt = DateTime.Now
            };

            // tránh lưu trùng
            // 🔐 Idempotent: chống IPN retry gây double
            var existing = await _context.MomoTransactions
                .FirstOrDefaultAsync(x => x.TransId == tx.TransId && tx.TransId != 0);

            if (existing == null)
            {
                _context.MomoTransactions.Add(tx);
            }
            else
            {
                // IPN gọi lại → update record cũ
                existing.ResultCode = tx.ResultCode;
                existing.Message = tx.Message;
                existing.ResponseTime = tx.ResponseTime;
                existing.Signature = tx.Signature;
                existing.PayType = tx.PayType;
                existing.ExtraData = tx.ExtraData;
                existing.CreatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();


            // ✅ xử lý đơn hàng
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                if (resultCode == 0)
                {
                    if (await CanFinalizeVndPaymentAsync(orderId, PhuongThucThanhToanEnum.MOMO, tx.Amount))
                    {
                        await _orderService.TryFinalizeOrderAsync(orderId);
                    }
                }
                else
                {
                    var dhFail = await _context.DonHangs.FirstOrDefaultAsync(x => x.MaDonHang == orderId);
                    if (dhFail != null && dhFail.TrangThai == "ChoThanhToan")
                    {
                        dhFail.TrangThai = "ThanhToanThatBai";
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Ok();
        }

       

        // =====================================================
        // Helpers
        // =====================================================

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim) : 0;
        }

        private async Task<List<GioHang>> GetCartItems()
        {
            var userId = GetUserId();
            return await _context.GioHangs
                .Include(x => x.SanPham)
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        private void ReloadCartToModel(CheckoutViewModel model, List<GioHang> cartItems)
        {
            model.CartItems = cartItems.Select(x => new CartItemViewModel
            {
                TenSanPham = x.SanPham.TenSanPham,
                DonGia = x.SanPham.GiaBan,
                SoLuong = x.SoLuong,
                HinhAnh = x.SanPham.AnhChinh
            }).ToList();

            model.TongTienHang = cartItems.Sum(x => x.SanPham.GiaBan * x.SoLuong);
            model.PhiVanChuyen = 30000;
        }

        private async Task<bool> CanFinalizeVndPaymentAsync(
            string orderCode,
            PhuongThucThanhToanEnum expectedMethod,
            decimal paidAmountVnd)
        {
            var order = await _context.DonHangs.FirstOrDefaultAsync(x => x.MaDonHang == orderCode);
            if (order == null)
                return false;

            if (order.TrangThai != OrderService.StatusChoThanhToan)
                return false;

            var expectedAmount = Math.Round(order.TongTien ?? 0m, 0, MidpointRounding.AwayFromZero);
            var paidAmount = Math.Round(paidAmountVnd, 0, MidpointRounding.AwayFromZero);

            if (order.PhuongThucThanhToan == expectedMethod && expectedAmount == paidAmount)
                return true;

            order.TrangThai = "ThanhToanCanDoiSoat";
            await _context.SaveChangesAsync();
            return false;
        }

        private async Task<bool> CanFinalizePayPalPaymentAsync(string orderCode, string? paidAmountUsdText)
        {
            var order = await _context.DonHangs.FirstOrDefaultAsync(x => x.MaDonHang == orderCode);
            if (order == null)
                return false;

            if (order.TrangThai != OrderService.StatusChoThanhToan)
                return false;

            var parsed = decimal.TryParse(
                paidAmountUsdText,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var paidAmountUsd);

            if (!parsed || order.PhuongThucThanhToan != PhuongThucThanhToanEnum.PAYPAL)
            {
                order.TrangThai = "ThanhToanCanDoiSoat";
                await _context.SaveChangesAsync();
                return false;
            }

            var rate = await _exchangeRate.GetUsdToVndAsync();
            var expectedAmountUsd = Math.Round((order.TongTien ?? 0m) / rate, 2);

            if (Math.Abs(expectedAmountUsd - paidAmountUsd) <= 0.01m)
                return true;

            order.TrangThai = "ThanhToanCanDoiSoat";
            await _context.SaveChangesAsync();
            return false;
        }
    }
}
