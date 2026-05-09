namespace ResipWeb.Models
{
    public class GioHang
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SanPhamId { get; set; }
        public int SoLuong { get; set; }
        public string? MauSac { get; set; }

        // Navigation properties
        public User User { get; set; }
        public SanPham SanPham { get; set; }
    }
}