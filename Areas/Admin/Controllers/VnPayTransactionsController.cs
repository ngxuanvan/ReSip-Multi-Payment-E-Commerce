using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;
using ResipWeb.Models.Payments;

namespace ResipWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class VnPayTransactionsController : Controller
    {
        private readonly AppDbContext _context;
        public VnPayTransactionsController(AppDbContext context) => _context = context;

        // /Admin/VnPayTransactions
        public async Task<IActionResult> Index(string? q, string? source, string? status, int page = 1, int pageSize = 30)
        {
            q = (q ?? "").Trim();
            source = (source ?? "").ToUpper().Trim();   // IPN / RETURN
            status = (status ?? "").Trim().ToUpper();   // SUCCESS / FAIL

            var query = _context.VnPayTransactions.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(x =>
                    x.MaDonHang.Contains(q) ||
                    x.TransactionNo.Contains(q) ||
                    x.TmnCode.Contains(q));

            if (!string.IsNullOrWhiteSpace(source))
                query = query.Where(x => x.Source == source);

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "SUCCESS")
                    query = query.Where(x => x.ResponseCode == "00" && x.TransactionStatus == "00");
                else if (status == "FAIL")
                    query = query.Where(x => !(x.ResponseCode == "00" && x.TransactionStatus == "00"));
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            // ✅ Tổng theo bộ lọc hiện tại
            var total = await query.CountAsync();

            // ✅ THÊM THỐNG KÊ CHO CHART (theo bộ lọc hiện tại)
            var successCount = await query.CountAsync(x => x.ResponseCode == "00" && x.TransactionStatus == "00");
            var failCount = total - successCount;

            ViewBag.SuccessCount = successCount;
            ViewBag.FailCount = failCount;

            // ✅ phân trang
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Q = q;
            ViewBag.Source = source;
            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;

            return View(items);
        }

        // /Admin/VnPayTransactions/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var tx = await _context.VnPayTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (tx == null) return NotFound();
            return View(tx);
        }
    }
}
