using System.ComponentModel.DataAnnotations.Schema;

namespace ResipWeb.Models
{
    public class SanPhamThuocTinh
    {
        public int Id { get; set; }

        public string TenThuocTinh { get; set; }     // Ví dụ: "Màu sắc", "Dung tích"
        public string GiaTri { get; set; }           // Ví dụ: "Xanh lá", "500ml"

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaChenhLech { get; set; }
        public int? SoLuongTon { get; set; }         // Tồn kho theo biến thể (có thể null)

        public int SanPhamId { get; set; }

        [ForeignKey(nameof(SanPhamId))]
        public SanPham SanPham { get; set; }
    }
}
