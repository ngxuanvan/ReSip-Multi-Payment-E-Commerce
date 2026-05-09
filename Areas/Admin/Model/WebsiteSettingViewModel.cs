using System.ComponentModel.DataAnnotations;

namespace ResipWeb.Areas.Admin.Models
{
    public class WebsiteSettingViewModel
    {
        public int Id { get; set; }

        // Dùng để hiển thị ảnh hiện tại
        public string? CurrentLogo { get; set; }
        public string? CurrentFavicon { get; set; }

        // Dùng để nhận file mới upload
        public IFormFile? LogoUpload { get; set; }
        public IFormFile? FaviconUpload { get; set; }

        public string? FooterContent { get; set; }
        public string? HeadCode { get; set; }
        public string? BodyCode { get; set; }
    }
}