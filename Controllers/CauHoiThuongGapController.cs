using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;

namespace ResipWeb.Controllers
{
    public class CauHoiThuongGapController : Controller
    {
        private readonly AppDbContext _context;

        // Hàm khởi tạo để kết nối Database
        public CauHoiThuongGapController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("/cau-hoi-thuong-gap")]
        public async Task<IActionResult> CauHoiThuongGap()
        {
            // Lấy danh sách câu hỏi từ DB sắp xếp theo thứ tự
            var data = await _context.CauHoiThuongGaps
                                     .OrderBy(x => x.ThuTu)
                                     .ToListAsync();

            // Truyền dữ liệu sang View để hiển thị
            return View(data);
        }
    }
}