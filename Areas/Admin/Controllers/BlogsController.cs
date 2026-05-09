using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;
// using ResipWeb.Data; // Đã bỏ dòng này vì gây lỗi

namespace ResipWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BlogsController : Controller
    {
        private readonly AppDbContext _context; // Đã sửa thành AppDbContext
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BlogsController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Blogs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Blogs.ToListAsync());
        }

        // GET: Admin/Blogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // GET: Admin/Blogs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Blogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog, IFormFile ImageUpload)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (ImageUpload != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "media/blogs");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageUpload.CopyToAsync(fileStream);
                    }

                    blog.Image = "/media/blogs/" + uniqueFileName;
                }

                blog.CreatedDate = DateTime.Now; // Tự động set ngày tạo
                if (string.IsNullOrEmpty(blog.Alias))
                {
                    blog.Alias = blog.Title.ToLower().Replace(" ", "-"); // Tạo alias đơn giản nếu chưa có
                }

                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // GET: Admin/Blogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }

        // POST: Admin/Blogs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog, IFormFile ImageUpload)
        {
            if (id != blog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy bài viết cũ để giữ lại ảnh nếu không upload ảnh mới
                    var existingBlog = await _context.Blogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

                    if (ImageUpload != null)
                    {
                        // Upload ảnh mới
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "media/blogs");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageUpload.CopyToAsync(fileStream);
                        }

                        blog.Image = "/media/blogs/" + uniqueFileName;
                    }
                    else
                    {
                        // Giữ nguyên ảnh cũ
                        blog.Image = existingBlog.Image;
                    }

                    // Giữ nguyên ngày tạo cũ nếu cần
                    blog.CreatedDate = existingBlog.CreatedDate;

                    _context.Update(blog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
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
            return View(blog);
        }

        // GET: Admin/Blogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Admin/Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                // Có thể thêm logic xóa file ảnh trong folder media tại đây nếu muốn
                _context.Blogs.Remove(blog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
    }
}