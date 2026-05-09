using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ResipWeb.Models;

namespace ResipWeb.ViewComponents
{
    /// <summary>
    /// ViewComponent để cache WebsiteSettings, tránh query database trên mỗi request
    /// </summary>
    public class WebsiteSettingViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CACHE_KEY = "WebsiteSetting";

        public WebsiteSettingViewComponent(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public IViewComponentResult Invoke()
        {
            // Cache trong 30 phút - WebsiteSettings ít khi thay đổi
            var setting = _cache.GetOrCreate(CACHE_KEY, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                
                return _context.WebsiteSettings.FirstOrDefault() ?? new WebsiteSetting();
            });

            return View(setting);
        }

        /// <summary>
        /// Xóa cache khi WebsiteSettings được cập nhật
        /// </summary>
        public static void ClearCache(IMemoryCache cache)
        {
            cache.Remove(CACHE_KEY);
        }
    }
}
