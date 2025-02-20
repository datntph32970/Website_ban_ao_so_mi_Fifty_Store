using API.DbConects.DTO.Tai_Khoan_DTO;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Khuyen_Mai;
using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Services.TaiKhoan_Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
        public DbSet<ThongKe> GiamGias { get; set; }
        public DbSet<GiamGiaSanPhamChiTiet> GiamGiaSanPhamChiTiets { get; set; }
        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            #region seed data admin va nhan vien

            Guid idAdmin = new Guid("11111111-1111-1111-1111-111111111111");
            Guid idNhanVien = new Guid("22222222-2222-2222-2222-222222222222");

            modelBuilder.Entity<TaiKhoan>().HasData(
                new TaiKhoan
                {
                    id_tai_khoan = new Guid("11111111-1111-1111-1111-111111111111"),
                    chuc_vu = ChucVuTaiKhoan.Admin.ToString(),
                    mat_khau = BamMatKhau("admin"),
                    ma_tai_khoan = "TK00000000",
                    ten_dang_nhap = "admin",
                    trang_thai = TrangThaiTaiKhoan.HoatDong.ToString()
                },
                new TaiKhoan
                {
                    id_tai_khoan = new Guid("22222222-2222-2222-2222-222222222222"),
                    chuc_vu = ChucVuTaiKhoan.NhanVien.ToString(),
                    mat_khau = BamMatKhau("nhanvien"),
                    ma_tai_khoan = "TK00000001",
                    ten_dang_nhap = "nhanvien",
                    trang_thai = TrangThaiTaiKhoan.HoatDong.ToString()
                }
            );
            modelBuilder.Entity<NhanVien>().HasData(
                new NhanVien
                {
                    id_nhan_vien = new Guid("33333333-3333-3333-3333-333333333333"),
                    id_tai_khoan = new Guid("22222222-2222-2222-2222-222222222222"),
                    id_nguoi_tao = new Guid("11111111-1111-1111-1111-111111111111"),
                    cccd = "000000000000",
                    email = "nthanhdat7112004@gmail.com",
                    gioi_tinh = GioiTinhTaiKhoan.Nam.ToString(),
                    ma_nhan_vien = "TK00000001",
                    ngay_sinh = DateTime.Parse("2004-01-01"),
                    ngay_sua = null,
                    ngay_tao = DateTime.Parse("2025-02-01"),
                    so_dien_thoai = "0111111111",
                    ten_nhan_vien = "họ và tên nhân viên 1",
                    trang_thai = TrangThaiTaiKhoan.HoatDong.ToString()
                }
            );
            #endregion
            #region config
            modelBuilder.Entity<GiamGia>()
                .HasOne(g => g.NguoiTao)
                .WithMany(nv => nv.TaoGiamGias)
                .HasForeignKey(g => g.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GiamGia>()
                .HasOne(g => g.NguoiSua)
                .WithMany(nv => nv.SuaGiamGias)
                .HasForeignKey(g => g.id_nguoi_cap_nhat)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GiamGiaSanPhamChiTiet>()
                .HasOne(g => g.NguoiTao)
                .WithMany(nv => nv.TaoGiamGiaSanPhamChiTiets)
                .HasForeignKey(g => g.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GiamGiaSanPhamChiTiet>()
                .HasOne(a => a.NguoiSua)
                .WithMany(nv => nv.SuaGiamGiaSanPhamChiTiets)
                .HasForeignKey(a => a.id_nguoi_cap_nhat)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<XuatXu>()
                .HasOne(x => x.NguoiTao)
                .WithMany(nv => nv.TaoXuatXus)
                .HasForeignKey(x => x.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<XuatXu>()
                .HasOne(x => x.NguoiSua)
                .WithMany(nv => nv.SuaXuatXus)
                .HasForeignKey(x => x.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ThuongHieu>()
                .HasOne(t => t.NguoiTao)
                .WithMany(nv => nv.TaoThuongHieus)
                .HasForeignKey(t => t.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ThuongHieu>()
                .HasOne(t => t.NguoiSua)
                .WithMany(nv => nv.SuaThuongHieus)
                .HasForeignKey(t => t.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.NguoiTao)
                .WithMany(nv => nv.TaoSanPhams)
                .HasForeignKey(s => s.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.NguoiSua)
                .WithMany(nv => nv.SuaSanPhams)
                .HasForeignKey(s => s.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SanPhamChiTiet>()
                .HasOne(s => s.NguoiTao)
                .WithMany(nv => nv.TaoSanPhamChiTiets)
                .HasForeignKey(s => s.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SanPhamChiTiet>()
                .HasOne(s => s.NguoiSua)
                .WithMany(nv => nv.SuaSanPhamChiTiets)
                .HasForeignKey(s => s.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MauSac>()
                .HasOne(m => m.NguoiTao)
                .WithMany(nv => nv.TaoMauSacs)
                .HasForeignKey(m => m.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MauSac>()
                .HasOne(m => m.NguoiSua)
                .WithMany(nv => nv.SuaMauSacs)
                .HasForeignKey(m => m.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KieuDang>()
                .HasOne(k => k.NguoiTao)
                .WithMany(nv => nv.TaoKieuDangs)
                .HasForeignKey(k => k.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KieuDang>()
                .HasOne(k => k.NguoiSua)
                .WithMany(nv => nv.SuaKieuDangs)
                .HasForeignKey(k => k.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KichCo>()
                .HasOne(k => k.NguoiTao)
                .WithMany(nv => nv.TaoKichCos)
                .HasForeignKey(k => k.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KichCo>()
                .HasOne(k => k.NguoiSua)
                .WithMany(nv => nv.SuaKichCos)
                .HasForeignKey(k => k.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatLieu>()
                .HasOne(c => c.NguoiTao)
                .WithMany(nv => nv.TaoChatLieus)
                .HasForeignKey(c => c.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatLieu>()
                .HasOne(c => c.NguoiSua)
                .WithMany(nv => nv.SuaChatLieus)
                .HasForeignKey(c => c.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HinhAnh>()
                .HasOne(h => h.NguoiTao)
                .WithMany(nv => nv.TaoHinhAnhs)
                .HasForeignKey(h => h.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HinhAnh>()
                .HasOne(h => h.NguoiSua)
                .WithMany(nv => nv.SuaHinhAnhs)
                .HasForeignKey(h => h.id_nguoi_sua)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KhuyenMai>()
                .HasOne(k => k.NguoiTao)
                .WithMany(nv => nv.TaoKhuyenMais)
                .HasForeignKey(k => k.id_nguoi_tao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KhuyenMai>()
                .HasOne(k => k.NguoiSua)
                .WithMany(nv => nv.SuaKhuyenMais)
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
            #endregion
        }
        private string BamMatKhau(string matKhau)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(matKhau));
                var sb = new StringBuilder();
                foreach (var b in hashedBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
