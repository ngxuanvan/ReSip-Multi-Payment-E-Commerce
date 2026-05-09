using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ResipWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace ResipWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UsersController(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // 1. DANH SÁCH KHÁCH HÀNG
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

        // 2. TẠO MỚI (Giao diện)
        public IActionResult Create()
        {
            return View();
        }

        // 2. TẠO MỚI (Xử lý)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            // Bỏ qua validate Id vì tự tăng
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                // Check trùng Username/Email
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(user);
                }
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(user);
                }

                // Mã hóa mật khẩu
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    user.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash);
                }

                user.CreatedAt = DateTime.UtcNow;
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // 3. CHỈNH SỬA (Giao diện)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Xóa password hash đi để hiển thị ô trống (nếu không đổi pass)
            user.PasswordHash = "";
            return View(user);
        }

        // 3. CHỈNH SỬA (Xử lý)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id) return NotFound();
            ModelState.Remove("PasswordHash"); // Password có thể để trống

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                    if (existingUser == null) return NotFound();

                    // Logic mật khẩu: Nếu nhập mới thì mã hóa, không nhập thì lấy cái cũ
                    if (!string.IsNullOrEmpty(user.PasswordHash))
                    {
                        user.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash);
                    }
                    else
                    {
                        user.PasswordHash = existingUser.PasswordHash;
                    }

                    user.CreatedAt = existingUser.CreatedAt; // Giữ nguyên ngày tạo

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // 4. XÓA KHÁCH HÀNG (Giao diện xác nhận - GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // 4. XÓA KHÁCH HÀNG (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}