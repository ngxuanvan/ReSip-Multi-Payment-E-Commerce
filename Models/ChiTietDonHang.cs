using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResipWeb.Models
{
    public class ChiTietDonHang
    {
        [Key]
        public int Id { get; set; }

        // FK -> DonHang
        [Required]
        public int DonHangId { get; set; }

        // FK -> SanPham
        [Required]
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; }


        [Required]
        public int SoLuong { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]

        public decimal ThanhTien { get; set; }


        // Navigation properties
        public DonHang DonHang { get; set; } = null!;
        public SanPham SanPham { get; set; } = null!;

        public decimal DonGia { get; set; }
    }
}
