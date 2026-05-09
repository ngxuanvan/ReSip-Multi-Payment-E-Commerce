using System.ComponentModel.DataAnnotations;

namespace ResipWeb.Models
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Tiêu đề bài viết")]
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài viết")]
        [MaxLength(250)]
        public string Title { get; set; } = null!;

        [Display(Name = "Alias (Đường dẫn SEO)")]
        [MaxLength(250)]
        public string? Alias { get; set; }

        [Display(Name = "Hình đại diện")]
        [MaxLength(500)]
        public string? Image { get; set; }

        [Display(Name = "Mô tả ngắn")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Nội dung chi tiết")]
        public string? Detail { get; set; }

        // --- CẤU HÌNH SEO ---
        [Display(Name = "Tiêu đề SEO (Meta Title)")]
        [MaxLength(250)]
        public string? MetaTitle { get; set; }

        [Display(Name = "Từ khóa SEO (Meta Keys)")]
        [MaxLength(250)]
        public string? MetaKeyword { get; set; }

        [Display(Name = "Mô tả SEO (Meta Description)")]
        [MaxLength(500)]
        public string? MetaDescription { get; set; }

        // --- Cấu hình khác ---
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Hiển thị")]
        public bool IsActive { get; set; } = true;
    }
}