using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;
using System.Security.Claims;

namespace ResipWeb.Controllers
{
    public class DonHangController : Controller
    {
        private readonly AppDbContext _context;

        public DonHangController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Xem Lịch sử đơn hàng
        public async Task<IActionResult> Index()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Account");

            // Thêm .ToString() vào sau user.Id
            var donHangs = await _context.DonHangs
                .Where(d => d.UserId == user.Id.ToString())
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            return View(donHangs);
        }

        // 2. Xem Chi tiết đơn hàng
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(ct => ct.SanPham) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (donHang == null) return NotFound();

            return View(donHang);
        }
        // 1. Giao diện sửa thông tin đơn hàng
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var donHang = await _context.DonHangs.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (donHang == null) return NotFound();

            // Chỉ cho sửa khi đơn hàng đang "Chờ xử lý"
            if (donHang.TrangThai != "ChoXuLy")
            {
                TempData["Error"] = "Đơn hàng đã được xử lý, không thể sửa thông tin!";
                return RedirectToAction("Details", new { id = id });
            }

            return View(donHang);
        }

        // 2. Xử lý lưu thông tin đơn hàng sau khi sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DonHang model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var donHang = await _context.DonHangs.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (donHang == null) return NotFound();

            // Cập nhật các trường thông tin đơn hàng (Không phải thông tin Profile)
            donHang.HoTen = model.HoTen;
            donHang.DienThoai = model.DienThoai;
            donHang.DiaChi = model.DiaChi;

            _context.Update(donHang);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thông tin nhận hàng thành công!";
            return RedirectToAction("Details", new { id = id });
        }
    }
}