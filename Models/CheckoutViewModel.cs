using System.ComponentModel.DataAnnotations;

namespace ResipWeb.Models.ViewModels // Khuyên dùng namespace riêng cho ViewModel
{
    public class CheckoutViewModel
    {
        // --- 1. Thông tin người mua ---
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận.")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự.")]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(15, ErrorMessage = "Số điện thoại không quá 15 số.")]
        public string SoDienThoai { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = null!;// Cần email để gửi hóa đơn/xác nhận

        // --- 2. Thông tin giao hàng chi tiết (Tách ra để tính phí ship sau này) ---
        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố.")]
        public string TinhThanh { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn Phường/Xã.")] 
        public string PhuongXa { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số nhà, tên đường.")]
        public string DiaChiCuThe { get; set; } = null!;

        public string? GhiChu { get; set; }

        // --- 3. Phương thức thanh toán ---
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public string PhuongThucThanhToan { get; set; } = "COD";  // COD | MOMO

        // --- 4. Dữ liệu hiển thị (Read-Only) ---
        // Các field này chỉ để hiển thị bên phải màn hình checkout, không cần bind từ form
        public List<CartItemViewModel>? CartItems { get; set; }
        public decimal TongTienHang { get; set; }
        public decimal PhiVanChuyen { get; set; } = 0; // Mặc định 0 hoặc tính toán sau
        public decimal TongThanhToan => TongTienHang + PhiVanChuyen;
    }

    // Class phụ để hiển thị danh sách hàng trong trang Checkout
    public class CartItemViewModel
    {
        public string TenSanPham { get; set; } = null!;
        public string HinhAnh { get; set; } = null!;
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }

}

