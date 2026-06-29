using Microsoft.EntityFrameworkCore;
using ResipWeb.Areas.Admin.Repository;
using ResipWeb.Models;

namespace ResipWeb.Services
{
    public interface IOrderService
    {
        Task TryFinalizeOrderAsync(string maDonHang, string targetStatus = "DaThanhToan");
        Task TryFinalizeOrderByIdAsync(int id, string targetStatus = "DaThanhToan");
        Task TryFinalizeOrderByIdAsync(long id, string targetStatus = "DaThanhToan");
    }

    public class OrderService : IOrderService
    {
        public const string StatusChoThanhToan = "ChoThanhToan";
        public const string StatusDaThanhToan = "DaThanhToan";
        public const string StatusChoXuLy = "ChoXuLy";
        // Trạng thái thanh toán thủ công (SEPAY): user đã xác nhận chuyển khoản, chờ admin xác nhận
        public const string StatusDaThanhToanChoXacNhan = "DaThanhToanChoXacNhan";
        // Backward-compat: đơn cũ đã lưu theo tên cũ
        public const string StatusDaThanhToanChoAdminXacNhan = "DaThanhToanChoAdminXacNhan";
        public const string StatusDaXacNhanThanhToan = "DaXacNhanThanhToan";

        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<OrderService> _logger;

        public OrderService(AppDbContext context, IEmailSender emailSender, ILogger<OrderService> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task TryFinalizeOrderAsync(string maDonHang, string targetStatus = "DaThanhToan")
        {
            var dh = await _context.DonHangs.FirstOrDefaultAsync(x => x.MaDonHang == maDonHang);
            if (dh == null) return;

            await ProcessFinalize(dh, targetStatus);
        }

        public async Task TryFinalizeOrderByIdAsync(int id, string targetStatus = "DaThanhToan")
        {
            var dh = await _context.DonHangs.FindAsync(id);
            if (dh == null) return;

            await ProcessFinalize(dh, targetStatus);
        }

        public Task TryFinalizeOrderByIdAsync(long id, string targetStatus = "DaThanhToan")
        {
            if (id < int.MinValue || id > int.MaxValue)
            {
                _logger.LogWarning("Order id {OrderId} is out of int range, skipping finalize.", id);
                return Task.CompletedTask;
            }

            return TryFinalizeOrderByIdAsync((int)id, targetStatus);
        }

        private async Task ProcessFinalize(DonHang dh, string targetStatus)
        {
            // 1. Kiểm tra trạng thái: Nếu thanh toán online thì phải đang ở 'ChoThanhToan'
            // Nếu là COD (targetStatus = ChoXuLy) thì bỏ qua bước check này hoặc check khác
            var isPaymentFinalize =
                targetStatus == StatusDaThanhToan ||
                targetStatus == StatusDaThanhToanChoXacNhan ||
                targetStatus == StatusDaThanhToanChoAdminXacNhan;

            if (isPaymentFinalize && dh.TrangThai != StatusChoThanhToan)
            {
                _logger.LogInformation("Order {MaDonHang} status is {Status}, skipping online finalize.", dh.MaDonHang, dh.TrangThai);
                return;
            }

            // 2. Lấy giỏ hàng của khách
            if (!int.TryParse(dh.UserId, out var userId)) return;
            var cartItems = await _context.GioHangs
                .Include(x => x.SanPham)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                _logger.LogWarning("Order {MaDonHang} has empty cart, marking status.", dh.MaDonHang);
                dh.TrangThai = "ThanhToanOK_NhungGioRong";
                await _context.SaveChangesAsync();
                return;
            }

            // 3. Tạo chi tiết đơn hàng & Trừ kho
            var chiTietList = new List<ChiTietDonHang>();
            foreach (var item in cartItems)
            {
                var product = await _context.SanPhams.FindAsync(item.SanPhamId);
                if (product != null)
                {
                    product.StockQuantity -= item.SoLuong;
                    _context.SanPhams.Update(product);
                }

                chiTietList.Add(new ChiTietDonHang
                {
                    DonHangId = dh.Id,
                    SanPhamId = item.SanPhamId,
                    SoLuong = item.SoLuong,
                    DonGia = item.SanPham.GiaBan,
                    TenSanPham = item.SanPham.TenSanPham,
                });
            }

            _context.ChiTietDonHangs.AddRange(chiTietList);
            _context.GioHangs.RemoveRange(cartItems);

            // 4. Cập nhật trạng thái đơn hàng
            dh.TrangThai = targetStatus;
            await _context.SaveChangesAsync();

            // 5. Gửi Email
            try
            {
                var ptttLabel = dh.TenPhuongThucThanhToan; // lay ten phuong thuc thanh toan
                var tongTien = (decimal)(dh.TongTien ?? 0);

                var customerSubject =
                    targetStatus == StatusChoXuLy
                        ? $"Xác nhận đặt hàng {dh.MaDonHang}"
                        : (targetStatus == StatusDaThanhToanChoXacNhan || targetStatus == StatusDaThanhToanChoAdminXacNhan)
                            ? $"Đã nhận thông tin thanh toán {dh.MaDonHang} (chờ xác nhận)"
                            : $"Xác nhận thanh toán đơn {dh.MaDonHang}";

                var adminSubject =
                    targetStatus == StatusChoXuLy
                        ? $"[Đơn mới] {dh.MaDonHang} - {dh.HoTen}"
                        : (targetStatus == StatusDaThanhToanChoXacNhan || targetStatus == StatusDaThanhToanChoAdminXacNhan)
                            ? $"[Chờ xác nhận thanh toán] {dh.MaDonHang} - {dh.HoTen}"
                            : $"[Thanh toán OK] {dh.MaDonHang} - {dh.HoTen}";

                // Gửi cho khách
                await _emailSender.SendAsync(
                    dh.Email!,
                    customerSubject,
                    OrderEmailBuilder.BuildOrderEmailHtml(dh, cartItems, tongTien, 30000, ptttLabel)
                );

                // Gửi cho admin
                await _emailSender.SendAsync(
                    "hotro.resip@gmail.com",
                    adminSubject,
                    OrderEmailBuilder.BuildAdminNotifyEmailHtml(dh, cartItems, tongTien, 30000, ptttLabel)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending confirmation email for order {MaDonHang}", dh.MaDonHang);
            }
        }
    }
}
