using System.ComponentModel.DataAnnotations;

namespace ResipWeb.Models
{
    public class UserProfileViewModel
    {
        public string Username { get; set; } = ""; // Chỉ để xem, không cho sửa
        public string Email { get; set; } = "";    // Chỉ để xem

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = "";

        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Địa chỉ giao hàng")]
        public string? ShippingAddress { get; set; }
    }
}