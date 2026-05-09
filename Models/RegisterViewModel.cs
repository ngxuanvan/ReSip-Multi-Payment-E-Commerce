using System.ComponentModel.DataAnnotations;

namespace ResipWeb.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "H? và tên là b?t bu?c"), StringLength(100)]
        [Display(Name = "FullName")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Email là b?t bu?c"), EmailAddress(ErrorMessage = "Email không h?p l?"), StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Tên ??ng nh?p là b?t bu?c"), StringLength(50)]
        [Display(Name = "Username")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "M?t kh?u là b?t bu?c"), DataType(DataType.Password), StringLength(100, MinimumLength = 6, ErrorMessage = "M?t kh?u ph?i có ít nh?t {2} ký t?")]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Xác nh?n m?t kh?u là b?t bu?c"), DataType(DataType.Password), Compare("Password", ErrorMessage = "M?t kh?u xác nh?n không kh?p")]
        [Display(Name = "ConfirmPassword")]
        public string ConfirmPassword { get; set; } = "";

        [Phone(ErrorMessage = "S? ?i?n tho?i không h?p l?"), StringLength(20)]
        [Display(Name = "PhoneNumber")]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        [Display(Name = "ShippingAddress")]
        public string? ShippingAddress { get; set; }
    }
}
