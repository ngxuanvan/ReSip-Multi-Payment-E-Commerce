using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ResipWeb.Areas.Admin.Models;
using ResipWeb.Models;

namespace ResipWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingController : Controller
    {
        private readonly AppDbContext _context; // Thay ApplicationDbContext bằng tên DbContext của bạn
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache _cache;

        public SettingController(AppDbContext context, IWebHostEnvironment webHostEnvironment, IMemoryCache cache)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Lấy dòng cài đặt đầu tiên, nếu chưa có thì tạo mới (chưa lưu db) để hiển thị form
            // Tìm trong bảng SiteConfigurations của bạn
            var setting = _context.WebsiteSettings.FirstOrDefault();

            // Nếu bảng trống, nó sẽ tự động tạo dòng mới (Insert) khi bạn bấm Lưu
            if (setting == null)
            {
                setting = new WebsiteSetting();
                _context.WebsiteSettings.Add(setting);
            }
            var model = new WebsiteSettingViewModel();

            if (setting != null)
            {
                model.Id = setting.Id;
                model.CurrentLogo = setting.LogoUrl;
                model.CurrentFavicon = setting.FaviconUrl;
                model.FooterContent = setting.FooterContent;
                model.HeadCode = setting.HeadCode;
                model.BodyCode = setting.BodyCode;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(WebsiteSettingViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm setting trong DB
                // Tìm trong bảng SiteConfigurations
                var setting = _context.WebsiteSettings.FirstOrDefault();

                // Nếu bảng trống, tự động chuẩn bị lệnh INSERT dòng mới
                if (setting == null)
                {
                    setting = new WebsiteSetting();
                    _context.WebsiteSettings.Add(setting);
                }
                // --- Xử lý Upload Logo ---
                if (model.LogoUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/settings");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    string fileName = "logo_" + Guid.NewGuid().ToString() + Path.GetExtension(model.LogoUpload.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.LogoUpload.CopyToAsync(fileStream);
                    }
                    setting.LogoUrl = "/images/settings/" + fileName;
                }

                // --- Xử lý Upload Favicon ---
                if (model.FaviconUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/settings");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    string fileName = "favicon_" + Guid.NewGuid().ToString() + Path.GetExtension(model.FaviconUpload.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.FaviconUpload.CopyToAsync(fileStream);
                    }
                    setting.FaviconUrl = "/images/settings/" + fileName;
                }

                // --- Lưu các thông tin text ---
                setting.FooterContent = model.FooterContent;
                setting.HeadCode = model.HeadCode;
                setting.BodyCode = model.BodyCode;

                await _context.SaveChangesAsync();
                // ✅ Xóa cache WebsiteSettings khi có thay đổi
                _cache.Remove("WebsiteSetting");
                TempData["Success"] = "Cập nhật cài đặt thành công!";
                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}