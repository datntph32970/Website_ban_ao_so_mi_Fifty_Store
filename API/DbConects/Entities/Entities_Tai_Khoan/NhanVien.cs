using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Khuyen_Mai;
using API.DbConects.Entities.Entities_San_Pham;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Tai_Khoan
{
    public class NhanVien
    {
        [Key]
        public Guid id_nhan_vien { get; set; }
        [ForeignKey("TaiKhoanNhanVien")]
        [Required(ErrorMessage = "Mã tài khoản không được để trống")]
        public Guid id_tai_khoan { get; set; }
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        public string ma_nhan_vien { get; set; }
        [Required(ErrorMessage = "Tên nhân viên không được để trống")]
        [StringLength(100, ErrorMessage = "Tên nhân viên không được vượt quá 100 ký tự")]
        public string ten_nhan_vien { get; set; }
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string email { get; set; }
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string so_dien_thoai { get; set; }
        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        public DateTime ngay_sinh { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string dia_chi { get; set; }
        [Required(ErrorMessage = "CCCD không được để trống")]
        public string cccd { get; set; }
        [Required(ErrorMessage = "Giới tính không được để trống")]
        public string gioi_tinh { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string trang_thai { get; set; }
        [ForeignKey("NguoiTao")]
        [Required(ErrorMessage = "Mã người tạo không được để trống")]
        public Guid id_nguoi_tao { get; set; }
        [Required(ErrorMessage = "Ngày tạo không được để trống")]
        public DateTime ngay_tao { get; set; }
        public DateTime? ngay_sua { get; set; }

        public virtual TaiKhoan? TaiKhoanNhanVien { get; set; }
        public virtual TaiKhoan? NguoiTao { get; set; }
        public virtual ICollection<HoaDon>? TaoHoaDons { get; set; }
        public virtual ICollection<HoaDon>? SuaHoaDons { get; set; }
        public virtual ICollection<HoaDonChiTiet>? SuaHoaDonChiTiets { get; set; }
        public virtual ICollection<XuatXu>? TaoXuatXus { get; set; }
        public virtual ICollection<XuatXu>? SuaXuatXus { get; set; }
        public virtual ICollection<DanhMuc>? TaoDanhMucs { get; set; }
        public virtual ICollection<DanhMuc>? SuaDanhMucs { get; set; }
        public virtual ICollection<SanPhamChiTiet>? TaoSanPhamChiTiets { get; set; }
        public virtual ICollection<SanPhamChiTiet>? SuaSanPhamChiTiets { get; set; }
        public virtual ICollection<SanPham>? TaoSanPhams { get; set; }
        public virtual ICollection<SanPham>? SuaSanPhams { get; set; }
        public virtual ICollection<ThuongHieu>? TaoThuongHieus { get; set; }
        public virtual ICollection<ThuongHieu>? SuaThuongHieus { get; set; }
        public virtual ICollection<MauSac>? TaoMauSacs { get; set; }
        public virtual ICollection<MauSac>? SuaMauSacs { get; set; }
        public virtual ICollection<KieuDang>? TaoKieuDangs { get; set; }
        public virtual ICollection<KieuDang>? SuaKieuDangs { get; set; }
        public virtual ICollection<KichCo>? TaoKichCos { get; set; }
        public virtual ICollection<KichCo>? SuaKichCos { get; set; }
        public virtual ICollection<HinhAnh>? TaoHinhAnhs { get; set; }
        public virtual ICollection<HinhAnh>? SuaHinhAnhs { get; set; }
        public virtual ICollection<ChatLieu>? TaoChatLieus { get; set; }
        public virtual ICollection<ChatLieu>? SuaChatLieus { get; set; }
        public virtual ICollection<GiamGia>? TaoGiamGias { get; set; }
        public virtual ICollection<GiamGia>? SuaGiamGias { get; set; }
        public virtual ICollection<KhuyenMai>? TaoKhuyenMais { get; set; }
        public virtual ICollection<KhuyenMai>? SuaKhuyenMais { get; set; }

    }
}
