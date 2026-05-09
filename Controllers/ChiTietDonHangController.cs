using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;

namespace ResipWeb.Controllers
{
    public class ChiTietDonHangController : Controller
    {
        private readonly AppDbContext _context;

        public ChiTietDonHangController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /ChiTietDonHang/Details/5
        public IActionResult Details(int id)
        {
            var donHang = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .FirstOrDefault(d => d.Id == id);

            if (donHang == null)
            {
                return NotFound();
            }

            return View("ChiTietDonHang", donHang);
        }
    }
}
