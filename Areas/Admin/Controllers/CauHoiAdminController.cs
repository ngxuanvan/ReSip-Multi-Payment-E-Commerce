using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResipWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CauHoiAdminController : Controller
    {
        private readonly AppDbContext _context;

        public CauHoiAdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/CauHoiAdmin
        public async Task<IActionResult> Index()
        {
            return View(await _context.CauHoiThuongGaps.ToListAsync());
        }

        // GET: Admin/CauHoiAdmin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cauHoiThuongGap = await _context.CauHoiThuongGaps
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cauHoiThuongGap == null)
            {
                return NotFound();
            }

            return View(cauHoiThuongGap);
        }

        // GET: Admin/CauHoiAdmin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/CauHoiAdmin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CauHoi,CauTraLoi,ThuTu")] CauHoiThuongGap cauHoiThuongGap)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cauHoiThuongGap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cauHoiThuongGap);
        }

        // GET: Admin/CauHoiAdmin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cauHoiThuongGap = await _context.CauHoiThuongGaps.FindAsync(id);
            if (cauHoiThuongGap == null)
            {
                return NotFound();
            }
            return View(cauHoiThuongGap);
        }

        // POST: Admin/CauHoiAdmin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CauHoi,CauTraLoi,ThuTu")] CauHoiThuongGap cauHoiThuongGap)
        {
            if (id != cauHoiThuongGap.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cauHoiThuongGap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CauHoiThuongGapExists(cauHoiThuongGap.Id))
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
            return View(cauHoiThuongGap);
        }

        // GET: Admin/CauHoiAdmin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cauHoiThuongGap = await _context.CauHoiThuongGaps
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cauHoiThuongGap == null)
            {
                return NotFound();
            }

            return View(cauHoiThuongGap);
        }

        // POST: Admin/CauHoiAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cauHoiThuongGap = await _context.CauHoiThuongGaps.FindAsync(id);
            if (cauHoiThuongGap != null)
            {
                _context.CauHoiThuongGaps.Remove(cauHoiThuongGap);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CauHoiThuongGapExists(int id)
        {
            return _context.CauHoiThuongGaps.Any(e => e.Id == id);
        }
    }
}
