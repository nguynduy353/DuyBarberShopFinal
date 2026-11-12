
using DuyBarberShop.Models;
using Microsoft.EntityFrameworkCore;

namespace DuyBarberShop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<DichVu> DichVus { get; set; }
        public DbSet<LichDat> LichDats { get; set; }
        public DbSet<ChiTietLichDat> ChiTietLichDats { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. TAIKHOAN
            modelBuilder.Entity<TaiKhoan>(entity =>
            {
                entity.ToTable("TAIKHOAN");

                entity.HasIndex(t => t.Email)
                      .IsUnique();

                entity.HasOne(t => t.NhanVien)
                      .WithOne(n => n.TaiKhoan)
                      .HasForeignKey<NhanVien>(
                          n => n.MaTaiKhoan)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 2. NHANVIEN
            modelBuilder.Entity<NhanVien>(entity =>
            {
                entity.ToTable("NHANVIEN");

                entity.HasIndex(n => n.MaTaiKhoan)
                      .IsUnique();
            });

            // 3. DICHVU
            modelBuilder.Entity<DichVu>(entity =>
            {
                entity.ToTable("DICHVU");
            });

            // 4. LICHDAT
            modelBuilder.Entity<LichDat>(entity =>
            {
                entity.ToTable("LICHDAT");

                entity.HasOne(l => l.KhachHang)
                      .WithMany(t => t.LichDats)
                      .HasForeignKey(l => l.MaKhachHang)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.NhanVien)
                      .WithMany(n => n.LichDats)
                      .HasForeignKey(l => l.MaNhanVien)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 5. CHITIETLICHDAT
            modelBuilder.Entity<ChiTietLichDat>(entity =>
            {
                entity.ToTable("CHITIETLICHDAT");

                entity.HasOne(c => c.LichDat)
                      .WithMany(l => l.ChiTietLichDats)
                      .HasForeignKey(c => c.MaLichDat)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.DichVu)
                      .WithMany(d => d.ChiTietLichDats)
                      .HasForeignKey(c => c.MaDichVu)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
