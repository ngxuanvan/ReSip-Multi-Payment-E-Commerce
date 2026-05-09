using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResipWeb.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;

namespace ResipWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<ResipWeb.Models.User> _passwordHasher;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDbContext db, IPasswordHasher<ResipWeb.Models.User> passwordHasher, ILogger<AccountController> logger)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        // ==========================================
        // 1. ĐĂNG KÝ (REGISTER)
        // ==========================================
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                if (await _db.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng.");
                    return View(model);
                }

                if (await _db.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                var newUser = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Username = model.Username,
                    PhoneNumber = model.PhoneNumber,
                    ShippingAddress = model.ShippingAddress,
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    Role = "User"
                };


                newUser.PasswordHash = _passwordHasher.HashPassword(newUser, model.Password);

                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, newUser.Username),
                    new Claim(ClaimTypes.Email, newUser.Email),
                    new Claim(ClaimTypes.NameIdentifier, newUser.Id.ToString()),
                    new Claim("UserId", newUser.Id.ToString()),
                    new Claim("FullName", newUser.FullName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.InnerException?.Message ?? ex.Message
                );
                return View(model);
            }

        }

        // ==========================================
        // 2. ĐĂNG NHẬP (LOGIN)
        // ==========================================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            LoginViewModel model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }
        [Authorize] // Bắt buộc đăng nhập mới vào được
        public IActionResult Dashboard()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản không tồn tại.");
                return View(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu không đúng.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("FullName", user.FullName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 3. ĐĂNG XUẤT (LOGOUT)
        // ==========================================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 4. ACCESS DENIED
        // ==========================================
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ==========================================
        // 5. CẬP NHẬT THÔNG TIN CÁ NHÂN (PROFILE)
        // ==========================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = HttpContext.User.FindFirst("UserId")?.Value;
            var username = HttpContext.User.Identity?.Name;

            ResipWeb.Models.User? user = null;

            if (userIdStr != null)
            {
                user = await _db.Users.FindAsync(int.Parse(userIdStr));
            }
            else if (username != null)
            {
                user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            }

            if (user == null) return RedirectToAction("Login");

            var model = new UserProfileViewModel
            {
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                ShippingAddress = user.ShippingAddress
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            var userIdStr = HttpContext.User.FindFirst("UserId")?.Value;
            var username = HttpContext.User.Identity?.Name;

            ResipWeb.Models.User? user = null;

            if (userIdStr != null)
            {
                user = await _db.Users.FindAsync(int.Parse(userIdStr));
            }
            else if (username != null)
            {
                user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            }

            if (user == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                model.Username = user.Username;
                model.Email = user.Email;
                return View(model);
            }

            try
            {
                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;
                user.ShippingAddress = model.ShippingAddress;

                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("FullName", user.FullName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                ViewBag.SuccessMessage = "Cập nhật thông tin thành công!";
                model.Username = user.Username;
                model.Email = user.Email;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi lưu thông tin.");
                model.Username = user.Username;
                model.Email = user.Email;
                return View(model);
            }
        }

        // ==========================================
        // 6. QUÊN MẬT KHẨU (FORGOT PASSWORD) - THÊM MỚI
        // ==========================================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Legacy 3-field verification: Username + Email + PhoneNumber must match to set new password
            var userVerify = await _db.Users.FirstOrDefaultAsync(u =>
                u.Username == model.Username &&
                u.Email == model.Email &&
                u.PhoneNumber == model.PhoneNumber);

            if (userVerify == null)
            {
                ModelState.AddModelError(string.Empty, "Thông tin xác minh không chính xác. Vui lòng kiểm tra lại Tên đăng nhập, Email và Số điện thoại.");
                return View(model);
            }

            try
            {
                userVerify.PasswordHash = _passwordHasher.HashPassword(userVerify, model.NewPassword);
                // clear any existing reset tokens if present (removed earlier)
                // userVerify.ResetToken = null;
                // userVerify.ResetTokenExpires = null;
                _db.Users.Update(userVerify);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đổi mật khẩu");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi hệ thống.");
                return View(model);
            }
        }

        // Remove ResetPassword endpoints (email-token flow) - not used in legacy mode

    }
}