using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Hoa_Don
{
    public class HoaDonChiTiet
    {
        [Key]
        public Guid id_hoa_don_chi_tiet { get; set; }
        [Required(ErrorMessage = "Mã hóa đơn chi tiết không được để trống")]
        public string ma_hoa_don_chi_tiet { get; set; }
        [ForeignKey("HoaDon")]
        public Guid id_hoa_don { get; set; }
        [ForeignKey("SanPhamChiTiet")]
        public Guid id_san_pham_chi_tiet { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string ten_san_pham { get; set; }
        [Required(ErrorMessage = "Tên màu sắc không được để trống")]
        public string ten_mau_sac { get; set; }
        [Required(ErrorMessage = "Tên kích cỡ không được để trống")]
        public string ten_kich_co { get; set; }
        [Required(ErrorMessage = "Số lượng không được để trống")]
        public int so_luong { get; set; }
        [Required(ErrorMessage = "Đơn giá không được để trống")]
        public decimal don_gia { get; set; }
        public decimal gia_sau_giam_gia { get; set; }
        public decimal gia_tri_khuyen_mai_cua_hoa_don_cho_hdct { get; set; }
        [Required(ErrorMessage = "Thành tiền không được để trống")]
        public decimal thanh_tien { get; set; }
        public string? ghi_chu { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(ChoTaiQuay|DangChoXuLy|DangXuLy|DangGiaoHang|HetHang|DaThanhToan|ChuaThanhToan|DaHoanThanh|DaHuy|DaNhanHang|DaHoanTraMotPhan|DaHoanTraToanBo)$", ErrorMessage = "Trạng thái hóa đơn không hợp lệ")]
        public string trang_thai { get; set; }
        [ForeignKey("NhanVienXuLy")]
        public Guid? id_nhan_vien_xu_ly { get; set; }
        public DateTime? ngay_sua { get; set; }
        public virtual NhanVien? NhanVienXuLy { get; set; }
        public virtual HoaDon? HoaDon { get; set; }
        public virtual SanPhamChiTiet? SanPhamChiTiet { get; set; }
    }
}
