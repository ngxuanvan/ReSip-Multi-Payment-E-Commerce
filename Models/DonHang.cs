using ResipWeb.Models.Payments;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResipWeb.Models
{
    public class DonHang
    {
        public int Id { get; set; }
        public string MaDonHang { get; set; }
        public string HoTen { get; set; }
        public string? DienThoai { get; set; }
        public string DiaChi { get; set; }
        public decimal? TongTien { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; } // email nhận thông báo của đơn

        // LƯU DB
        public PhuongThucThanhToanEnum PhuongThucThanhToan { get; set; }
            = PhuongThucThanhToanEnum.COD;

        // DÙNG HIỂN THỊ (không map DB)
        [NotMapped]
        public string TenPhuongThucThanhToan => PhuongThucThanhToan switch
        {
            PhuongThucThanhToanEnum.COD => "COD",
            PhuongThucThanhToanEnum.MOMO => "MoMo",
            PhuongThucThanhToanEnum.VNPAY => "VNPay",
            PhuongThucThanhToanEnum.SEPAY => "SEPAY",
            PhuongThucThanhToanEnum.PAYPAL => "PayPal",
            _ => "Không xác định"
        };

        // Danh sách các món trong đơn hàng
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    = new List<ChiTietDonHang>();

    }
}