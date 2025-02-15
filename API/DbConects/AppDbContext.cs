using API.DbConects.Entities.Hoa_Don;
using API.DbConects.Entities.Khuyen_Mai;
using API.DbConects.Entities.San_Pham;
using API.DbConects.Entities.Tai_Khoan;
using Microsoft.EntityFrameworkCore;

namespace API.DbConects
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        #region DbSet
        public DbSet<GioHangChiTiet> GioHangChiTiets { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<HoaDonChiTiet> HoaDonChiTiets { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<PhuongThucThanhToan> PhuongThucThanhToans { get; set; }
        public DbSet<SanPhamChiTiet> SanPhamChiTiets { get; set; }
        public DbSet<TrangThaiHoaDon> TrangThaiHoaDons { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<DiaChi> DiaChis { get; set; }
        public DbSet<XuatXu> XuatXus { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<ThuongHieu> ThuongHieus { get; set; }
        public DbSet<MauSac> MauSacs { get; set; }
        public DbSet<KieuDang> KieuDangs { get; set; }
        public DbSet<KichCo> KichCos { get; set; }
        public DbSet<HinhAnh> HinhAnhs { get; set; }
        public DbSet<ChatLieu> ChatLieus { get; set; }
        public DbSet<GiamGia> GiamGias { get; set; }
        public DbSet<GiamGiaSanPhamChiTiet> GiamGiaSanPhamChiTiets { get; set; }
        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GiamGia>()
                .HasOne(g => g.NguoiTao)
                .WithMany()
                .HasForeignKey(g => g.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GiamGia>()
                .HasOne(g => g.NguoiSua)
                .WithMany()
                .HasForeignKey(g => g.id_nguoi_cap_nhat)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GiamGiaSanPhamChiTiet>()
                .HasOne(g => g.NguoiTao)
                .WithMany()
                .HasForeignKey(g => g.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GiamGiaSanPhamChiTiet>()
                .HasOne(a => a.NguoiSua)
                .WithMany()
                .HasForeignKey(a => a.id_nguoi_cap_nhat)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<XuatXu>()
                .HasOne(x => x.NguoiTao)
                .WithMany()
                .HasForeignKey(x => x.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<XuatXu>()
                .HasOne(x => x.NguoiSua)
                .WithMany()
                .HasForeignKey(x => x.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ThuongHieu>()
                .HasOne(t => t.NguoiTao)
                .WithMany()
                .HasForeignKey(t => t.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ThuongHieu>()
                .HasOne(t => t.NguoiSua)
                .WithMany()
                .HasForeignKey(t => t.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.NguoiTao)
                .WithMany()
                .HasForeignKey(s => s.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.NguoiSua)
                .WithMany()
                .HasForeignKey(s => s.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SanPhamChiTiet>()
                .HasOne(s => s.NguoiTao)
                .WithMany()
                .HasForeignKey(s => s.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SanPhamChiTiet>()
                .HasOne(s => s.NguoiSua)
                .WithMany()
                .HasForeignKey(s => s.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MauSac>()
                .HasOne(m => m.NguoiTao)
                .WithMany()
                .HasForeignKey(m => m.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MauSac>()
                .HasOne(m => m.NguoiSua)
                .WithMany()
                .HasForeignKey(m => m.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KieuDang>()
                .HasOne(k => k.NguoiTao)
                .WithMany()
                .HasForeignKey(k => k.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KieuDang>()
                .HasOne(k => k.NguoiSua)
                .WithMany()
                .HasForeignKey(k => k.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KichCo>()
                .HasOne(k => k.NguoiTao)
                .WithMany()
                .HasForeignKey(k => k.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KichCo>()
                .HasOne(k => k.NguoiSua)
                .WithMany()
                .HasForeignKey(k => k.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatLieu>()
                .HasOne(c => c.NguoiTao)
                .WithMany()
                .HasForeignKey(c => c.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatLieu>()
                .HasOne(c => c.NguoiSua)
                .WithMany()
                .HasForeignKey(c => c.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HinhAnh>()
                .HasOne(h => h.NguoiTao)
                .WithMany()
                .HasForeignKey(h => h.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HinhAnh>()
                .HasOne(h => h.NguoiSua)
                .WithMany()
                .HasForeignKey(h => h.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KhuyenMai>()
                .HasOne(k => k.NguoiTao)
                .WithMany()
                .HasForeignKey(k => k.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KhuyenMai>()
                .HasOne(k => k.NguoiSua)
                .WithMany()
                .HasForeignKey(k => k.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NhanVien>()
                .HasOne(nv => nv.TaiKhoanNhanVien)
                .WithOne(tk => tk.NhanVien)
                .HasForeignKey<NhanVien>(nv => nv.id_tai_khoan)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KhachHang>()
                .HasOne(kh => kh.TaiKhoan)
                .WithOne(tk => tk.KhachHang)
                .HasForeignKey<KhachHang>(kh => kh.id_tai_khoan)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
