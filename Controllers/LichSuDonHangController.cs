using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;
using System.Security.Claims;

namespace ResipWeb.Controllers
{
    [Authorize] // Bắt buộc phải đăng nhập mới xem được
    public class LichSuDonHangController : Controller
    {
        private readonly AppDbContext _context;

        public LichSuDonHangController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH (Xem tất cả đơn đã mua)
        // Link chạy: /LichSuDonHang/Index
        public async Task<IActionResult> Index()
        {
            // Lấy ID người dùng (Hệ thống Identity mặc định trả về chuỗi string)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Dòng 26 của bạn sửa lại như sau:
            var donHangs = await _context.DonHangs
                .Where(x => x.UserId == userId) // userId ở đây là string, x.UserId cũng phải là string
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            return View(donHangs);
        }

        // 2. TRANG CHI TIẾT (Xem kỹ 1 đơn gồm những món gì)
        // Link chạy: /LichSuDonHang/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs) // CỰC KỲ QUAN TRỌNG: Để hiện sản phẩm và tiền
        .ThenInclude(ct => ct.SanPham)   // Để hiện tên và ảnh sản phẩm
        .FirstOrDefaultAsync(d => d.Id == id);

            // Kiểm tra: Đơn hàng phải tồn tại VÀ phải đúng của người này mới cho xem
            if (donHang == null || donHang.UserId != userId)
            {
                return RedirectToAction("Index");
            }
            return View(donHang);
        }
    }
}