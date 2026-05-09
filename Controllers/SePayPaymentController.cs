using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;
using ResipWeb.Services;
using ResipWeb.Services.SePay;

namespace ResipWeb.Controllers
{
    [Route("sepay")]
    public class SePayPaymentController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ISePayService _sePay;
        private readonly IOrderService _orderService;

        public SePayPaymentController(AppDbContext db, ISePayService sePay, IOrderService orderService)
        {
            _db = db;
            _sePay = sePay;
            _orderService = orderService;
        }

        [HttpGet("ping")]
        public IActionResult Ping() => Content("SEPAY PAY CONTROLLER OK");

        [HttpGet("pay")]
        public async Task<IActionResult> Pay([FromQuery] string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode)) return NotFound("missing orderCode");

            var order = await _db.DonHangs.FirstOrDefaultAsync(x => x.MaDonHang == orderCode);
            if (order == null) return NotFound("order not found");

            var amount = order.TongTien ?? 0m;
            var qrUrl = _sePay.BuildQrImageUrl(order.MaDonHang, amount);


            ViewBag.OrderCode = order.MaDonHang;
            ViewBag.Amount = amount;
            ViewBag.QrUrl = qrUrl;

            return View("~/Views/SePayPayment/Pay.cshtml");
        }

        [HttpPost("confirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
                return BadRequest("Missing orderCode");

            var order = await _db.DonHangs.FirstOrDefaultAsync(x => x.MaDonHang == orderCode);
            if (order == null) return NotFound("order not found");

            // Thủ công: user tự xác nhận đã chuyển khoản -> cập nhật trạng thái: đã thanh toán (chờ admin xác nhận)
            // Đồng thời finalize (chốt giỏ, trừ kho, tạo chi tiết đơn, gửi email) để tránh giỏ thay đổi về sau.
            if (order.TrangThai == OrderService.StatusChoThanhToan)
                await _orderService.TryFinalizeOrderAsync(orderCode, OrderService.StatusDaThanhToanChoXacNhan);

            return RedirectToAction("OrderSuccess", "Checkout", new { orderCode });
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
                return Json(new { found = false });

            var order = await _db.DonHangs
                .FirstOrDefaultAsync(x => x.MaDonHang == orderCode);

            if (order == null)
                return Json(new { found = false });

            var expired = false;

            if (order.TrangThai == "ChoThanhToan" && order.NgayTao.HasValue)
            {
                var diff = DateTime.Now - order.NgayTao.Value; // .Value vì nullable
                if (diff.TotalMinutes > 15)
                {
                    expired = true;
                    order.TrangThai = "HetHan";
                    await _db.SaveChangesAsync();
                }
            }


            var paid = (order.TrangThai == "DaThanhToan");

            return Json(new
            {
                found = true,
                paid,
                expired
            });
        }

        [HttpPost("cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
                return BadRequest("Missing orderCode");

            var order = await _db.Set<DonHang>()
                .FirstOrDefaultAsync(x => x.MaDonHang == orderCode);

            if (order == null)
                return NotFound("Order not found");

            // Đã thanh toán thì không cho huỷ
            if (order.TrangThai == "DaThanhToan")
                return BadRequest("Order already paid");

            // Chỉ cho huỷ khi đang chờ (tuỳ bạn)
            if (order.TrangThai == "ChoThanhToan")
            {
                order.TrangThai = "DaHuy";
                await _db.SaveChangesAsync();
            }

            // Redirect sang trang huỷ
            return RedirectToAction("CancelSuccess", "SePayPayment", new { orderCode });
        }

        [HttpGet("cancel-success")]
        public IActionResult CancelSuccess(string orderCode)
        {
            ViewBag.OrderCode = orderCode;
            return View("~/Views/Checkout/CancelSuccess.cshtml");
        }


    }
}
