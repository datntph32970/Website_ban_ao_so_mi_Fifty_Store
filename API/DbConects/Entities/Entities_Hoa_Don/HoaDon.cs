using API.DbConects.Entities.Entities_Khuyen_Mai;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Hoa_Don
{
    public class HoaDon
    {
        [Key]
        public Guid id_hoa_don { get; set; }
        [Required(ErrorMessage = "Mã hóa đơn không được để trống")]
        public string ma_hoa_don { get; set; }
        [Required(ErrorMessage = "Ngày tạo không được để trống")]
        public DateTime ngay_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public decimal? tong_tien_don_hang { get; set; }
        public decimal? so_tien_khuyen_mai { get; set; }
        [RegularExpression("^(PhanTram|TienMat)$\"", ErrorMessage = "Kiểu khuyến mãi không hợp lệ")]
        public string? ghi_chu { get; set; }
        public string? ly_do_huy_don_hang { get; set; }
        public decimal? tong_tien_phai_thanh_toan { get; set; }
        public decimal? so_tien_khach_tra { get; set; }
        public decimal? so_tien_thua_tra_khach { get; set; }
        public string? ten_khach_hang { get; set; }
        public string? ten_nhan_vien { get; set; }
        public string? sdt_khach_hang { get; set; }
        public decimal? phi_van_chuyen { get; set; }
        public string? dia_chi_nhan_hang { get; set; }

        [RegularExpression("TaiQuay|Online", ErrorMessage = "Kiểu hóa đơn không hợp lệ")]
        public string loai_hoa_don { get; set; }
        //
        [ForeignKey("KhachHang")]
        public Guid? id_khach_hang { get; set; }
        [ForeignKey("NhanVienXuLy")]
        public Guid? id_nhan_vien_xu_ly { get; set; }
        [ForeignKey("CuaHang")]
        public Guid? id_cua_hang { get; set; }

        [ForeignKey("KhuyenMai")]
        public Guid? id_khuyen_mai { get; set; }
        [ForeignKey("PhuongThucThanhToan")]
        public Guid? id_phuong_thuc_thanh_toan { get; set; }
        [Required(ErrorMessage = "Trạng thái hóa đơn không được để trống")]
        [RegularExpression("^(ChoTaiQuay|DangChoXuLy|DaXacNhan|DangChuanBi|DangGiaoHang|HetHang|DaThanhToan|ChuaThanhToan|DaHoanThanh|DaHuy|DaNhanHang|DaHoanTraMotPhan|DaHoanTraToanBo)$", ErrorMessage = "Trạng thái hóa đơn không hợp lệ")]
        public string trang_thai_hoa_don { get; set; }
        //
        public virtual KhachHang? KhachHang { get; set; }
        public virtual NhanVien? NhanVienXuLy { get; set; }
        public virtual KhuyenMai? KhuyenMai { get; set; }
        public virtual CuaHang? CuaHang { get; set; }
        public virtual PhuongThucThanhToan? PhuongThucThanhToan { get; set; }
        public virtual ICollection<HoaDonChiTiet>? HoaDonChiTiets { get; set; }

        public string? ly_do_tra_hang { get; set; }
        public DateTime? ngay_yeu_cau_tra_hang { get; set; }
        public DateTime? ngay_xac_nhan_tra_hang { get; set; }
        public DateTime? ngay_hoan_thanh_tra_hang { get; set; }
        public string? hinh_anh_tra_hang { get; set; }
    }
}
