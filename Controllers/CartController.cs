using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;
using System.Security.Claims;

[Authorize] // Bắt buộc đăng nhập
public class CartController : Controller
{
    private readonly AppDbContext _context;
    public CartController(AppDbContext context) { _context = context; }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int SanPhamId, int SoLuong)
    {
        // Lấy Id của User đang đăng nhập
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
        var item = await _context.GioHangs
            .FirstOrDefaultAsync(x => x.UserId == userId && x.SanPhamId == SanPhamId);

        if (item == null)
        {
            _context.GioHangs.Add(new GioHang
            {
                UserId = userId,
                SanPhamId = SanPhamId,
                SoLuong = SoLuong
            });
        }
        else
        {
            item.SoLuong += SoLuong;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    // Xem giỏ hàng
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var cart = await _context.GioHangs
            .Include(x => x.SanPham)
            .Where(x => x.UserId == userId)
            .ToListAsync();
        return View(cart);
    }

    // Xóa sản phẩm khỏi giỏ
    public async Task<IActionResult> Remove(int id)
    {
        var item = await _context.GioHangs.FindAsync(id);
        if (item != null)
        {
            _context.GioHangs.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    // Cập nhật số lượng (Dùng Ajax hoặc Form)
    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int id, int quantity)
    {
        var item = await _context.GioHangs.FindAsync(id);
        if (item != null && quantity > 0)
        {
            item.SoLuong = quantity;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}