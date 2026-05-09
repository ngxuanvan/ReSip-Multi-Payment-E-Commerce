using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần thiết cho SelectList (ViewBag)
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;
using ResipWeb.Models.ViewModels;

namespace ResipWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class SanPhamController : Controller
    {
        private readonly AppDbContext _context;

        public SanPhamController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/SanPham/Index
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var query = _context.SanPhams
                .Include(x => x.Category)
                .Where(x => x.IsActive)
                .AsQueryable();

            // Tìm theo tên hoặc SKU
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.TenSanPham.Contains(search) ||
                    x.SKU.Contains(search)
                );
            }

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId);
            }

            ViewBag.CategoryId = new SelectList(
                _context.Categories.Where(c => c.IsActive),
                "Id",
                "Name"
            );

            var ds = await query.ToListAsync();
            return View(ds);
        }

        // =======================================================================
        // 1. CREATE (Tạo mới) - Đã sửa lỗi logic khởi tạo Model
        // =======================================================================

        // GET: /Admin/SanPham/Create
        public IActionResult Create()
        {
            // LƯU Ý: Vấn đề cũ nằm ở đây, bạn đang cố gắng gán List<SanPhamThuocTinh> 
            // cho một thuộc tính trong ViewModel vốn mong đợi List<SanPhamThuocTinhVM>.

            // Khởi tạo ViewModel.
            var model = new SanPhamCreateViewModel
            {
                
                ThuocTinhs = new List<SanPhamThuocTinhVM>(),
            };

            // Load dữ liệu Danh mục
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            ViewData["Title"] = "Thêm sản phẩm mới";

            return View(model);
        }

        // POST: /Admin/SanPham/Create
    
        // LƯU Ý: Phải nhận SanPhamCreateViewModel để xử lý ảnh và Thuộc tính

        // =======================================================================
        // 2. EDIT (Sửa) - Giữ nguyên logic cũ
        // =======================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPhamCreateViewModel model)
        {
            // Nếu bạn muốn Slug và ảnh phụ không bắt buộc:
            ModelState.Remove("Slug");
            ModelState.Remove("AnhPhuFiles");

            // Ảnh chính: nếu DB đang NOT NULL thì vẫn phải có giá trị AnhChinh
            if (string.IsNullOrWhiteSpace(model.Slug) && !string.IsNullOrWhiteSpace(model.TenSanPham))
            {
                model.Slug = model.TenSanPham.Trim().ToLower().Replace(" ", "-");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            // 1) Upload ảnh chính (BẮT BUỘC vì DB AnhChinh NOT NULL)
            string anhChinhPath;

            if (model.AnhChinhFile != null && model.AnhChinhFile.Length > 0)
            {
                var ext = Path.GetExtension(model.AnhChinhFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";

                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "sanpham");
                Directory.CreateDirectory(folder);

                var savePath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await model.AnhChinhFile.CopyToAsync(stream);
                }

                anhChinhPath = $"/uploads/sanpham/{fileName}";
            }
            else
            {
                // Nếu bạn chưa chọn ảnh, vẫn phải có giá trị để DB không lỗi
                // (hãy tạo file no-image.png trong wwwroot/uploads/sanpham/)
                anhChinhPath = "/uploads/sanpham/no-image.png";
            }

            // 2) Map ViewModel -> Entity
            var sp = new SanPham
            {
                TenSanPham = model.TenSanPham,
                SKU = model.SKU,
                Slug = model.Slug,
                MoTaNgan = model.MoTaNgan,
                MoTaChiTiet = model.MoTaChiTiet,

                GiaBan = model.GiaBan,
                GiaCu = model.GiaCu,
                StockQuantity = model.StockQuantity,

                IsActive = model.IsActive,
                IsFeatured = model.IsFeatured,

                CategoryId = model.CategoryId,

                AnhChinh = anhChinhPath,
                CreatedAt = DateTime.Now
            };

            _context.SanPhams.Add(sp);
            await _context.SaveChangesAsync(); // sp.Id có sau dòng này

            // 3) (Tuỳ chọn) Lưu ảnh phụ
            if (model.AnhPhuFiles != null && model.AnhPhuFiles.Count > 0)
            {
                int thuTu = 1;
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "sanpham");
                Directory.CreateDirectory(folder);

                foreach (var f in model.AnhPhuFiles.Where(x => x != null && x.Length > 0))
                {
                    var ext = Path.GetExtension(f.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var savePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await f.CopyToAsync(stream);
                    }

                    _context.SanPhamAnhs.Add(new SanPhamAnh
                    {
                        SanPhamId = sp.Id,
                        Url = $"/uploads/sanpham/{fileName}",
                        ThuTu = thuTu++
                    });
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/SanPham/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null) return NotFound();

            // MAP ENTITY → VIEWMODEL
            var model = new SanPhamCreateViewModel
            {
                Id = sp.Id,
                TenSanPham = sp.TenSanPham,
                SKU = sp.SKU,
                Slug = sp.Slug,
                GiaBan = sp.GiaBan,
                GiaCu = sp.GiaCu,
                StockQuantity = sp.StockQuantity,
                IsActive = sp.IsActive,
                IsFeatured = sp.IsFeatured,
                CategoryId = sp.CategoryId,
                MoTaNgan = sp.MoTaNgan,
                MoTaChiTiet = sp.MoTaChiTiet,
                ThuocTinhs = new List<SanPhamThuocTinhVM>()
            };

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);

            // 👉 DÙNG LẠI VIEW CREATE
            return View("Create", model);
        }


        // POST: /Admin/SanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SanPhamCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
                return View("Create", model);
            }

            var sp = await _context.SanPhams.FindAsync(model.Id);
            if (sp == null) return NotFound();

            // MAP VIEWMODEL → ENTITY
            sp.TenSanPham = model.TenSanPham;
            sp.SKU = model.SKU;
            sp.Slug = model.Slug;
            sp.GiaBan = model.GiaBan;
            sp.GiaCu = model.GiaCu;
            sp.StockQuantity = model.StockQuantity;
            sp.IsActive = model.IsActive;
            sp.IsFeatured = model.IsFeatured;
            sp.CategoryId = model.CategoryId;
            sp.MoTaNgan = model.MoTaNgan;
            sp.MoTaChiTiet = model.MoTaChiTiet;
            sp.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =======================================================================
        // 3. DUPLICATE (Nhân bản) - Bổ sung chức năng (Sửa lỗi 404)
        // =======================================================================

        // GET: /Admin/SanPham/Duplicate/5
        public async Task<IActionResult> Duplicate(int id)
        {
            var spGoc = await _context.SanPhams
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(sp => sp.Id == id);

            if (spGoc == null)
                return NotFound();

            // Map Entity sang ViewModel để hiển thị trong form Create/Duplicate
            var model = new SanPhamCreateViewModel
            {
                TenSanPham = spGoc.TenSanPham + " (Bản sao)",
                SKU = spGoc.SKU,
                GiaBan = spGoc.GiaBan,
                GiaCu = spGoc.GiaCu,
                StockQuantity = spGoc.StockQuantity,
                IsActive = false, // Nên đặt lại trạng thái ẩn cho bản sao
                IsFeatured = false,
                CategoryId = spGoc.CategoryId,
                MoTaNgan = spGoc.MoTaNgan,
                MoTaChiTiet = spGoc.MoTaChiTiet,
                // Khởi tạo List thuộc tính con
                ThuocTinhs = new List<SanPhamThuocTinhVM>()
            };

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            ViewData["Title"] = "Nhân bản sản phẩm";

            // Trả về View Create để người dùng lưu sản phẩm mới
            return View("Create", model);
        }

        // =======================================================================
        // 4. DELETE (Xóa) - Bổ sung chức năng (Sửa lỗi 404)
        // =======================================================================

        // GET: /Admin/SanPham/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);

            if (sp == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            _context.SanPhams.Remove(sp);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa thành công sản phẩm: {sp.TenSanPham}";
            return RedirectToAction(nameof(Index));
        }
    }
}
