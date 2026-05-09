using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using ResipWeb.Models; // Cần thiết cho các tham chiếu Entity Model
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;



namespace ResipWeb.Models.ViewModels
{
    

    // =======================================================================
    // 1. ĐỊNH NGHĨA VIEWMODEL CHO THUỘC TÍNH (Sửa lỗi CS0246 nếu thiếu)
    // =======================================================================
    public class SanPhamThuocTinhVM
    {
        public string TenThuocTinh { get; set; }    // "Màu sắc"
        public string GiaTri { get; set; }          // "Xanh lá"
        public decimal? GiaChenhLech { get; set; }
        public int? SoLuongTon { get; set; }
    }

    // =======================================================================
    // 2. VIEWMODEL CHÍNH (Đã sửa lỗi NullReference & IsFeatured)
    // =======================================================================
    public class SanPhamCreateViewModel
    {
        public string? AnhChinh { get; set; }

        [ValidateNever]
        public IFormFile? AnhChinhFile { get; set; }

        [ValidateNever]
        public List<IFormFile>? AnhPhuFiles { get; set; }
        // Thông tin sản phẩm
        public int Id { get; set; }
        public string TenSanPham { get; set; }
        public string SKU { get; set; }
        public string Slug { get; set; }
        public string MoTaNgan { get; set; }
        public string MoTaChiTiet { get; set; }

        public decimal GiaBan { get; set; }
        public decimal? GiaCu { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }

        public int? CategoryId { get; set; }

      

        // Thuộc tính
        public List<SanPhamThuocTinhVM> ThuocTinhs { get; set; } = new List<SanPhamThuocTinhVM>();
        // Khởi tạo List ngay tại đây để TRÁNH LỖI NullReferenceException trong View
    }
}