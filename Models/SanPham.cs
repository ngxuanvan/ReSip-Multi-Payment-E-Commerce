using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace ResipWeb.Models
{
    [Table("SanPhams")]
    public class SanPham
    {
       
            public int Id { get; set; }

            // Thông tin cơ bản
            [Required, StringLength(200)]
            public string TenSanPham { get; set; }

            [StringLength(150)]
            public string SKU { get; set; }              // Mã sản phẩm

            [StringLength(300)]
            public string Slug { get; set; }             // URL thân thiện (sau này dùng)

            [Column(TypeName = "nvarchar(MAX)")]
            public string MoTaNgan { get; set; }         // Mô tả ngắn

            [Column(TypeName = "nvarchar(MAX)")]
            public string MoTaChiTiet { get; set; }      // Mô tả chi tiết (HTML)

            // Giá & tồn kho
            [Required]
            public decimal GiaBan { get; set; }          // Giá đang bán

            public decimal? GiaCu { get; set; }          // Giá gốc (hiển thị giảm giá)

            public int StockQuantity { get; set; }       // Tồn kho tổng

            public bool IsActive { get; set; } = true;   // Có hiển thị trên web không
            public bool IsFeatured { get; set; }         // Sản phẩm nổi bật trên trang chủ

            // Ảnh
            public string? AnhChinh { get; set; }         // Ảnh chính (đường dẫn)

            public List<SanPhamAnh> SanPhamAnhs { get; set; }
            public List<SanPhamThuocTinh> ThuocTinhs { get; set; }

            // Danh mục (tạm thời để null, sau tính sau)
            public int? CategoryId { get; set; }
            public Category? Category { get; set; }

            // Tracking
            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public DateTime? UpdatedAt { get; set; }
        }
    }

