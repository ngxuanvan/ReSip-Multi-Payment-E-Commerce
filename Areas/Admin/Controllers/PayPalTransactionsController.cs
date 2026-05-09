using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;
using ResipWeb.Models.Payments;

namespace ResipWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PayPalTransactionsController : Controller
    {
        private readonly AppDbContext _context;
        public PayPalTransactionsController(AppDbContext context) => _context = context;

        // /Admin/PayPalTransactions
        public async Task<IActionResult> Index(string? q, string? status, int page = 1, int pageSize = 30)
        {
            q = (q ?? "").Trim();
            status = (status ?? "").Trim().ToUpper();   // COMPLETED / FAILED

            var query = _context.PayPalTransactions.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(x =>
                    x.OrderId.Contains(q) ||
                    x.PayPalOrderId.Contains(q) ||
                    x.PayerEmail.Contains(q) ||
                    x.Token.Contains(q));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status == status);

            query = query.OrderByDescending(x => x.CreatedAt);

            // ✅ Tổng theo bộ lọc hiện tại
            var total = await query.CountAsync();

            // ✅ THỐNG KÊ (theo bộ lọc hiện tại)
            var successCount = await query.CountAsync(x => x.Status == "COMPLETED");
            var failCount = total - successCount;

            ViewBag.SuccessCount = successCount;
            ViewBag.FailCount = failCount;

            // ✅ phân trang
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Q = q;
            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;

            return View(items);
        }

        // /Admin/PayPalTransactions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tx = await _context.PayPalTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (tx == null) return NotFound();
            return View(tx);
        }
    }
}
