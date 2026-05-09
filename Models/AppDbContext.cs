using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using ResipWeb.Models;
using ResipWeb.Models.Payments;
using ResipWeb.Models.Payments.SePay;


namespace ResipWeb.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<HomeService> HomeServices { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public virtual DbSet<User> Users { get; set; }

    public DbSet<SanPham> SanPhams { get; set; }
    public DbSet<SanPhamAnh> SanPhamAnhs { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }
    public virtual DbSet<WebsiteSetting> WebsiteSettings { get; set; }

    public DbSet<SanPhamThuocTinh> SanPhamThuocTinhs { get; set; }

    public DbSet<DonHang> DonHangs { get; set; }
    public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
    public DbSet<CauHoiThuongGap> CauHoiThuongGaps { get; set; }

    public DbSet<MomoTransaction> MomoTransactions { get; set; }
    public DbSet<VnPayTransaction> VnPayTransactions { get; set; }

    public DbSet<SePayTransaction> SePayTransactions { get; set; }
    public DbSet<PayPalTransaction> PayPalTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC07CE31E569");
            entity.ToTable("Category");

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Slug).HasMaxLength(255);
        });
        modelBuilder.Entity<SanPhamThuocTinh>()
    .Property(x => x.GiaChenhLech)
    .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("SanPhams");

            entity.Property(e => e.TenSanPham).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SKU).HasMaxLength(150);
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.MoTaNgan).HasMaxLength(500);

            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaCu).HasColumnType("decimal(18, 2)");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.SanPhams)
                .HasForeignKey(e => e.CategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();

            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ShippingAddress).HasMaxLength(255);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasIndex(e => e.MaDonHang)
                  .IsUnique()
                  .HasDatabaseName("IX_DonHangs_MaDonHang_Unique");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
