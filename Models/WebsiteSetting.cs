using System.ComponentModel.DataAnnotations;

namespace ResipWeb.Models
{
    public class WebsiteSetting
    {
        [Key]
        public int Id { get; set; } // Thường sẽ luôn là ID = 1

        public string? LogoUrl { get; set; }    // Đường dẫn ảnh Logo
        public string? FaviconUrl { get; set; } // Đường dẫn ảnh Favicon

        public string? FooterContent { get; set; } // Nội dung Footer (HTML)

        // Phần chèn code
        public string? HeadCode { get; set; } // Chèn code vào thẻ <head> (VD: Google Analytics)
        public string? BodyCode { get; set; } // Chèn code vào thẻ <body> (VD: Chat script)
    }
}