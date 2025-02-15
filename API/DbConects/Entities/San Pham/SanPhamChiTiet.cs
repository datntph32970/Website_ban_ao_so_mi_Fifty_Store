using API.DbConects.Entities.Hoa_Don;
using API.DbConects.Entities.Khuyen_Mai;
using API.DbConects.Entities.Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.San_Pham
{
    public class SanPhamChiTiet
    {
        [Key]
        public Guid id_san_pham_chi_tiet { get; set; }
        public string ma_san_pham_chi_tiet { get; set; }

        public int so_luong { get; set; }
        public decimal gia_ban { get; set; }
        public decimal gia_nhap { get; set; }
        public decimal so_tien_giam_gia_theo_chuong_trinh { get; set; }
        public string trang_thai { get; set; }
        [ForeignKey("NguoiTao")]
        public Guid id_nguoi_tao { get; set; }
        public DateTime ngay_tao { get; set; }
        [ForeignKey("NguoiSua")]
        public Guid? id_nguoi_sua { get; set; }
        public DateTime? ngay_sua { get; set; }
        [ForeignKey("SanPham")]
        public Guid id_san_pham { get; set; }
        [ForeignKey("KichCo")]
        public Guid id_kich_co { get; set; }
        [ForeignKey("MauSac")]
        public Guid id_mau_sac { get; set; }
        public virtual SanPham? SanPham { get; set; }
        public virtual KichCo? KichCo { get; set; }
        public virtual MauSac? MauSac { get; set; }
        public virtual ICollection<HinhAnh>? HinhAnhs { get; set; }
        public virtual ICollection<HoaDonChiTiet>? HoaDonChiTiets { get; set; }
        public virtual ICollection<GioHangChiTiet>? GioHangChiTiets { get; set; }
        public virtual NhanVien? NguoiTao { get; set; }
        public virtual NhanVien? NguoiSua { get; set; }
        public virtual GiamGiaSanPhamChiTiet? GiamGiaSanPhamChiTiets { get; set; }
    }
}
