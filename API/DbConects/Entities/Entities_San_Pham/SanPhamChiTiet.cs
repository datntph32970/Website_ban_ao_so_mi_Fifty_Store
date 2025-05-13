using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Khuyen_Mai;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_San_Pham
{
    public class SanPhamChiTiet
    {
        [Key]
        public Guid id_san_pham_chi_tiet { get; set; }
        [Required(ErrorMessage = "Mã sản phẩm chi tiết không được để trống")]
        public string ma_san_pham_chi_tiet { get; set; }
        [Required(ErrorMessage = "Số lượng không được để trống")]
        public int so_luong { get; set; }
        [Required(ErrorMessage = "Giá bán không được để trống")]
        public decimal gia_ban { get; set; }
        [Required(ErrorMessage = "Giá nhập không được để trống")]
        public decimal gia_nhap { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string trang_thai { get; set; }
        [Required(ErrorMessage = "Mã người tạo không được để trống")]
        public DateTime ngay_tao { get; set; }
        public Guid id_nguoi_tao { get; set; }
        public Guid? id_nguoi_sua { get; set; }
        public DateTime? ngay_sua { get; set; }
        [ForeignKey("SanPham")]
        public Guid id_san_pham { get; set; }
        [ForeignKey("KichCo")]
        public Guid id_kich_co { get; set; }
        [ForeignKey("MauSac")]
        public Guid id_mau_sac { get; set; }
        [ForeignKey("GiamGia")]
        public Guid? id_giam_gia { get; set; }
        public virtual SanPham? SanPham { get; set; }
        public virtual KichCo? KichCo { get; set; }
        public virtual MauSac? MauSac { get; set; }
        public virtual ICollection<HinhAnhSanPhamChiTiet>? HinhAnhSanPhamChiTiets { get; set; }
        public virtual ICollection<HoaDonChiTiet>? HoaDonChiTiets { get; set; }
        public virtual ICollection<GioHangChiTiet>? GioHangChiTiets { get; set; }
        [ForeignKey("id_nguoi_tao")]
        public virtual NhanVien? NguoiTao { get; set; }
        [ForeignKey("id_nguoi_sua")]
        public virtual NhanVien? NguoiSua { get; set; }
        public virtual GiamGia? GiamGia { get; set; }
    }
}
