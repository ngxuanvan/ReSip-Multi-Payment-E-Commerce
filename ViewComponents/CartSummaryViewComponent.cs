using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;
using System.Security.Claims;

namespace ResipWeb.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public CartSummaryViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            decimal totalMoney = 0;

            // 1. Kiểm tra người dùng đã đăng nhập chưa
            if (User.Identity.IsAuthenticated)
            {
                var userIdStr = ((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdStr))
                {
                    if (int.TryParse(userIdStr, out var userId))
                    {
                        // 2. Tính tổng tiền từ bảng GioHangs trong Database
                        // Dùng join để tránh lỗi nếu dữ liệu giỏ có SanPhamId "mồ côi" (không có record SanPham).
                        totalMoney = await _context.GioHangs
                            .Where(g => g.UserId == userId)
                            .Join(
                                _context.SanPhams,
                                g => g.SanPhamId,
                                s => s.Id,
                                (g, s) => g.SoLuong * s.GiaBan
                            )
                            .SumAsync();
                    }
                }
            }

            // Trả về số tiền định dạng chuỗi (VD: 1.500.000)
            return View("Default", totalMoney.ToString("N0"));
        }
    }
}