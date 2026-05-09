using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ResipWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResipWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DonHangsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public DonHangsController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: Admin/DonHangs
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách và sắp xếp giảm dần theo Id (hoặc NgayTao)
            var query = await _context.DonHangs
                                .OrderByDescending(d => d.Id)
                                .ToListAsync();

            // Trả về View với dữ liệu đã được sắp xếp
            return View(query);
        }

        // GET: Admin/DonHangs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // GET: Admin/DonHangs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/DonHangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MaDonHang,HoTen,DienThoai,DiaChi,TongTien,TrangThai,NgayTao,UserId")] DonHang donHang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(donHang);
                await _context.SaveChangesAsync();
                // ✅ Xóa cache dashboard khi có đơn hàng mới
                AdminController.ClearDashboardCache(_cache);
                return RedirectToAction(nameof(Index));
            }
            return View(donHang);

        }

        // GET: Admin/DonHangs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null)
            {
                return NotFound();
            }
            return View(donHang);
        }

        // POST: Admin/DonHangs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaDonHang,HoTen,DienThoai,DiaChi,TongTien,TrangThai,NgayTao,UserId")] DonHang donHang)
        {
            if (id != donHang.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(donHang);
                    await _context.SaveChangesAsync();
                    // ✅ Xóa cache dashboard khi đơn hàng được cập nhật
                    AdminController.ClearDashboardCache(_cache);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonHangExists(donHang.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(donHang);
        }

        // GET: Admin/DonHangs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // POST: Admin/DonHangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang != null)
            {
                _context.DonHangs.Remove(donHang);
            }

            await _context.SaveChangesAsync();
            // ✅ Xóa cache dashboard khi đơn hàng bị xóa
            AdminController.ClearDashboardCache(_cache);
            return RedirectToAction(nameof(Index));
        }

        private bool DonHangExists(int id)
        {
            return _context.DonHangs.Any(e => e.Id == id);
        }
       
    }
}
