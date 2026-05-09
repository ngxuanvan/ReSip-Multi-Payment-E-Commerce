using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResipWeb.Models
{
    public class SanPhamAnh
    {
        public int Id { get; set; }

        
        public string? Url { get; set; }          // ~/assets/uploads/products/xxx.jpg

        public int ThuTu { get; set; }           // Thứ tự hiển thị (1,2,3...)

        public int SanPhamId { get; set; }

        [ForeignKey(nameof(SanPhamId))]
        public SanPham? SanPham { get; set; }
    }
}
