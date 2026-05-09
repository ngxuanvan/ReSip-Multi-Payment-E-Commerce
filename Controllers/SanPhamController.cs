using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResipWeb.Models;
using ResipWeb.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ResipWeb.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SanPhamController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /SanPham
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var query = _context.SanPhams
                .Include(x => x.Category)
                .AsQueryable();

            // chỉ hiển thị sản phẩm đang hoạt động
            query = query.Where(x => x.IsActive);

            // tìm theo tên hoặc SKU
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x => x.TenSanPham.Contains(search) || x.SKU.Contains(search));
            }

            // lọc theo danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            // đổ dropdown danh mục ra View
            ViewBag.CategoryId = new SelectList(
                _context.Categories.Where(c => c.IsActive).OrderBy(c => c.Name),
                "Id", "Name", categoryId
            );

            var ds = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
            return View(ds);
        }
        //public async Task<IActionResult> Index(string search) 
        //{
        //    var products = _context.SanPhams.AsQueryable();

        //    if (!string.IsNullOrEmpty(search))
        //    {
        //        products = products.Where(p => p.TenSanPham.Contains(search));
        //    }

        //    return View(await products.ToListAsync());
        //}

        // GET: /SanPham/Create
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories.Where(c => c.IsActive).OrderBy(c => c.Name),
                "Id",
                "Name"
            );

            var vm = new SanPhamCreateViewModel
            {
                ThuocTinhs = new List<SanPhamThuocTinhVM>()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham model, IFormFile imgFile)
        {
            if (ModelState.IsValid)
            {
                // 1. Xử lý lưu ảnh nếu có file được chọn
                if (imgFile != null && imgFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imgFile.FileName);

                    // Đường dẫn vật lý đến thư mục wwwroot/images/products
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imgFile.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn vào database (để sau này hiển thị)
                    model.AnhChinh = "/images/products/" + fileName;
                }

                // 2. Lưu vào Database
                model.IsActive = true;
                _context.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi, phải load lại danh mục để hiện lại Form
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }



        // GET: /SanPham/Duplicate/5
        public async Task<IActionResult> Duplicate(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null) return NotFound();

            var copy = new SanPham
            {
                TenSanPham = sp.TenSanPham + " (copy)",
                SKU = sp.SKU + "-clone",
                Slug = (sp.Slug ?? sp.TenSanPham)?.ToLower().Replace(" ", "-") + "-copy-" + Guid.NewGuid().ToString("N").Substring(0, 5),
                MoTaNgan = sp.MoTaNgan,
                MoTaChiTiet = sp.MoTaChiTiet,
                GiaBan = sp.GiaBan,
                GiaCu = sp.GiaCu,
                StockQuantity = sp.StockQuantity,
                IsActive = sp.IsActive,
                IsFeatured = sp.IsFeatured,
                CategoryId = sp.CategoryId,
                AnhChinh = sp.AnhChinh
            };

            _context.SanPhams.Add(copy);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = copy.Id });
        }

        // GET: /SanPham/DanhMuc?slug=binh-nuoc
        public async Task<IActionResult> DanhMuc(string slug, decimal? min_price, decimal? max_price)
        {
            // Tìm danh mục
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
            if (category == null) return NotFound();

            // Truyền dữ liệu ra View
            ViewBag.CategoryName = category.Name;
            ViewBag.CurrentSlug = slug; // Dòng này cực kỳ quan trọng để không bị lỗi 404
            ViewBag.MinPrice = min_price ?? 0;
            ViewBag.MaxPrice = max_price ?? 10000000;

            // Lọc sản phẩm
            var query = _context.SanPhams.Where(p => p.CategoryId == category.Id && p.IsActive);
            if (min_price.HasValue) query = query.Where(p => p.GiaBan >= min_price.Value);
            if (max_price.HasValue) query = query.Where(p => p.GiaBan <= max_price.Value);

            return View(await query.ToListAsync());
        }

        // ================== THÊM PHẦN NÀY ==================

        // GET: /SanPham/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null)
            {
                return NotFound();
            }

            ViewBag.CategoryId = new SelectList(
                _context.Categories
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.Name),
                "Id",
                "Name",
                sp.CategoryId
            );

            var vm = new SanPhamCreateViewModel
            {
                Id = sp.Id,
                TenSanPham = sp.TenSanPham,
                SKU = sp.SKU,
                Slug = sp.Slug,
                MoTaNgan = sp.MoTaNgan,
                MoTaChiTiet = sp.MoTaChiTiet,
                GiaBan = sp.GiaBan,
                GiaCu = sp.GiaCu,
                StockQuantity = sp.StockQuantity,
                IsActive = sp.IsActive,
                IsFeatured = sp.IsFeatured,
                CategoryId = sp.CategoryId,
                ThuocTinhs = new List<SanPhamThuocTinhVM>()  // nếu muốn load thuộc tính cũ thì làm thêm
            };

            // nếu muốn hiện ảnh hiện tại thì truyền qua ViewBag
            ViewBag.AnhChinhHienTai = sp.AnhChinh;

            return View(vm);
        }

        // POST: /SanPham/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    SanPham model,
    IFormFile? AnhChinhFile,
    List<IFormFile>? AnhPhuFiles
)
        {
            var sp = await _context.SanPhams
                .Include(x => x.SanPhamAnhs)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (sp == null) return NotFound();

            // ===== CẬP NHẬT FIELD CƠ BẢN =====
            sp.TenSanPham = model.TenSanPham;
            sp.GiaBan = model.GiaBan;
            sp.GiaCu = model.GiaCu;
            sp.StockQuantity = model.StockQuantity;
            sp.IsActive = model.IsActive;

            // ===== ẢNH CHÍNH =====
            if (AnhChinhFile != null && AnhChinhFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(AnhChinhFile.FileName);
                var path = Path.Combine("wwwroot/uploads/sanpham", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await AnhChinhFile.CopyToAsync(stream);

                sp.AnhChinh = "/uploads/sanpham/" + fileName;
            }
            // ❗ KHÔNG else → giữ ảnh cũ

            // ===== ẢNH PHỤ =====
            if (AnhPhuFiles != null && AnhPhuFiles.Count > 0)
            {
                foreach (var file in AnhPhuFiles)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var path = Path.Combine("wwwroot/uploads/sanpham", fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);

                    _context.SanPhamAnhs.Add(new SanPhamAnh
                    {
                        SanPhamId = sp.Id,
                        Url = "/uploads/sanpham/" + fileName
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        // GET: /SanPham/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            // Xóa ảnh phụ theo SanPhamId (nếu có)
            var anhs = _context.SanPhamAnhs.Where(a => a.SanPhamId == id);
            _context.SanPhamAnhs.RemoveRange(anhs);

            // Xóa thuộc tính theo SanPhamId (nếu có)
            var thuocTinhs = _context.SanPhamThuocTinhs.Where(t => t.SanPhamId == id);
            _context.SanPhamThuocTinhs.RemoveRange(thuocTinhs);

            // Xóa sản phẩm
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null)
            {
                return NotFound();
            }

            _context.SanPhams.Remove(sp);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // GET: /SanPham/ChiTiet/5
        public async Task<IActionResult> ChiTiet(int id)
        {
            var sp = await _context.SanPhams
                .Include(x => x.Category)
                .Include(x => x.SanPhamAnhs)
                .Include(x => x.ThuocTinhs)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

            if (sp == null) return NotFound();

            return View(sp);   // Views/SanPham/ChiTiet.cshtml
        }


    }

}
