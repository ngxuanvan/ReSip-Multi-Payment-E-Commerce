// Areas/Admin/Models/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

// Namespace phải có tên Area của bạn
namespace ResipWeb.Areas.Admin.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Tên đăng nhập.")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty; // Sửa dòng 11

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ tôi")]
        public bool RememberMe { get; set; }
    }
}