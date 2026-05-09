using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models; // AppDbContext
using ResipWeb.Models.Payments;

namespace ResipWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] 
    public class MomoTransactionsController : Controller
    {
        private readonly AppDbContext _context;

        public MomoTransactionsController(AppDbContext context)
        {
            _context = context;
        }

        // /Admin/MomoTransactions
        public async Task<IActionResult> Index(
            string? keyword,
            int? resultCode,
            string? source,
            int page = 1,
            int pageSize = 20)
        {
            var q = _context.MomoTransactions.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                q = q.Where(x =>
                    x.OrderId.Contains(keyword) ||
                    x.RequestId.Contains(keyword) ||
                    x.PartnerCode.Contains(keyword) ||
                    x.Message.Contains(keyword));
            }

            if (resultCode.HasValue)
                q = q.Where(x => x.ResultCode == resultCode.Value);

            if (!string.IsNullOrWhiteSpace(source))
                q = q.Where(x => x.Source == source);

            var total = await q.CountAsync();

            var data = await q
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.ResultCode = resultCode;
            ViewBag.Source = source;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;

            return View(data);
        }

        // /Admin/MomoTransactions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tx = await _context.MomoTransactions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (tx == null) return NotFound();
            return View(tx);
        }
    }
}
