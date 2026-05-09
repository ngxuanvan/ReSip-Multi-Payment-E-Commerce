using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ResipWeb.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ResipWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string DASHBOARD_CACHE_KEY = "AdminDashboardStats";

        public AdminController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            // ✅ CACHE dashboard stats trong 5 phút để tăng tốc độ load
            var cacheKey = DASHBOARD_CACHE_KEY;
            DashboardStats? cachedStats = _cache.Get<DashboardStats>(cacheKey);

            if (cachedStats == null)
            {
                // ✅ CHẠY QUERIES TUẦN TỰ (Entity Framework không hỗ trợ concurrent operations trên cùng DbContext)
                // Cache sẽ giúp tối ưu hiệu suất cho các lần load tiếp theo
                
                // 1) Đơn gần nhất
                var recentOrders = await _context.DonHangs
                    .AsNoTracking()
                    .OrderByDescending(d => d.NgayTao)
                    .Take(30)
                    .ToListAsync();

                // 2) Thống kê đơn hàng + doanh thu
                var orderStats = await _context.DonHangs
                    .AsNoTracking()
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        TotalOrders = g.Count(),
                        PendingOrders = g.Count(x => x.TrangThai == "Chờ xác nhận"),
                        CompletedOrders = g.Count(x => x.TrangThai == "Hoàn thành"),
                        TotalRevenue = g.Where(x => x.TrangThai == "Hoàn thành")
                                        .Sum(x => x.TongTien ?? 0)
                    })
                    .FirstOrDefaultAsync() ?? new { TotalOrders = 0, PendingOrders = 0, CompletedOrders = 0, TotalRevenue = 0m };

                // 3) Thống kê sản phẩm
                var productStats = await _context.SanPhams
                    .AsNoTracking()
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        TotalProducts = g.Count(),
                        ActiveProducts = g.Count(x => x.IsActive)
                    })
                    .FirstOrDefaultAsync() ?? new { TotalProducts = 0, ActiveProducts = 0 };

                // 4) Thống kê danh mục
                var categoryStats = await _context.Categories
                    .AsNoTracking()
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        TotalCategories = g.Count(),
                        ActiveCategories = g.Count(x => x.IsActive)
                    })
                    .FirstOrDefaultAsync() ?? new { TotalCategories = 0, ActiveCategories = 0 };

                // 5) Tổng số users
                var totalUsers = await _context.Users.AsNoTracking().CountAsync();

                // 6) Top categories
                var topCategories = await _context.SanPhams
                    .AsNoTracking()
                    .GroupBy(p => p.CategoryId)
                    .Select(g => new { CategoryId = g.Key, ProductCount = g.Count() })
                    .Join(_context.Categories.AsNoTracking(),
                        x => x.CategoryId,
                        c => c.Id,
                        (x, c) => new TopCategoryInfo { CategoryName = c.Name ?? "", ProductCount = x.ProductCount })
                    .OrderByDescending(x => x.ProductCount)
                    .Take(5)
                    .ToListAsync();

                // ✅ Convert TopCategoryInfo sang TopCategoryViewModel (public class) để View có thể truy cập
                var topCategoriesViewModel = topCategories.Select(x => new TopCategoryViewModel
                {
                    CategoryName = x.CategoryName,
                    ProductCount = x.ProductCount
                }).ToList();

                // Tính tổng số sản phẩm từ top categories để truyền vào ViewBag
                var totalProductsInCategories = topCategoriesViewModel.Sum(x => x.ProductCount);

                cachedStats = new DashboardStats
                {
                    RecentOrders = recentOrders,
                    OrderCount = orderStats.TotalOrders,
                    PendingOrders = orderStats.PendingOrders,
                    CompletedOrders = orderStats.CompletedOrders,
                    TotalRevenue = orderStats.TotalRevenue,
                    ProductCount = productStats.TotalProducts,
                    ActiveProducts = productStats.ActiveProducts,
                    CategoryCount = categoryStats.TotalCategories,
                    ActiveCategories = categoryStats.ActiveCategories,
                    TotalUsers = totalUsers,
                    TopCategories = topCategoriesViewModel,
                    TotalProductsInCategories = totalProductsInCategories
                };

                // Cache trong 5 phút
                _cache.Set(cacheKey, cachedStats, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                });
            }

            // Set ViewBag từ cached data
            ViewBag.OrderCount = cachedStats.OrderCount;
            ViewBag.PendingOrders = cachedStats.PendingOrders;
            ViewBag.CompletedOrders = cachedStats.CompletedOrders;
            ViewBag.TotalRevenue = cachedStats.TotalRevenue;
            ViewBag.ProductCount = cachedStats.ProductCount;
            ViewBag.ActiveProducts = cachedStats.ActiveProducts;
            ViewBag.InactiveProducts = cachedStats.ProductCount - cachedStats.ActiveProducts;
            ViewBag.CategoryCount = cachedStats.CategoryCount;
            ViewBag.ActiveCategories = cachedStats.ActiveCategories;
            ViewBag.TotalUsers = cachedStats.TotalUsers;
            ViewBag.TopCategories = cachedStats.TopCategories;
            ViewBag.TotalProductsInCategories = cachedStats.TotalProductsInCategories;

            return View(cachedStats.RecentOrders);
        }

        /// <summary>
        /// Xóa cache dashboard khi có thay đổi dữ liệu quan trọng
        /// </summary>
        public static void ClearDashboardCache(IMemoryCache cache)
        {
            cache.Remove(DASHBOARD_CACHE_KEY);
        }

        private class DashboardStats
        {
            public List<DonHang> RecentOrders { get; set; } = new();
            public int OrderCount { get; set; }
            public int PendingOrders { get; set; }
            public int CompletedOrders { get; set; }
            public decimal TotalRevenue { get; set; }
            public int ProductCount { get; set; }
            public int ActiveProducts { get; set; }
            public int CategoryCount { get; set; }
            public int ActiveCategories { get; set; }
            public int TotalUsers { get; set; }
            public List<TopCategoryViewModel> TopCategories { get; set; } = new();
            public int TotalProductsInCategories { get; set; }
        }

        private class TopCategoryInfo
        {
            public string CategoryName { get; set; } = string.Empty;
            public int ProductCount { get; set; }
        }
    }
}
