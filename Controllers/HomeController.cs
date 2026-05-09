using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ResipWeb.Models;

namespace ResipWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;  // lưu lại để dùng
        }

        // ✅ SỬA Index: lấy list HomeService từ DB
        public IActionResult Index()
        {
            var services = _context.HomeServices.ToList();   // SELECT * FROM HomeServices
                                                             // thêm sản phẩm nổi bật
            ViewBag.FeaturedProducts = _context.SanPhams
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToList();

            ViewBag.SanPhamMoiNhat = _context.SanPhams.Take(8).ToList();

            return View(services);                           // truyền ra view
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
