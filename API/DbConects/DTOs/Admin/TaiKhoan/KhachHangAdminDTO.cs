using System.ComponentModel.DataAnnotations;
using API.DbConects.DTOs.Admin.HoaDon;
using API.DbConects.DTOs.Client.HoaDon;

namespace API.DbConects.DTOs.Admin.TaiKhoan
{
    public class KhachHangAdminDTO
    {
        public Guid id_khach_hang { get; set; }
        public string ma_khach_hang { get; set; }
        public string? ten_khach_hang { get; set; }
        public DateOnly? ngay_sinh { get; set; }
        public string? so_dien_thoai { get; set; }
        public string? email { get; set; }
        public string? gioi_tinh { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public int so_dia_chi { get; set; }
        public int so_don_hang { get; set; }

        public virtual ICollection<GioHangItemClientDTO>? gioHangItemsDTOs { get; set; }
        public virtual ICollection<HoaDonAdminDTO>? hoaDonDTOs { get; set; }
        public virtual ICollection<DiaChiDTO>? diaChiDTOs { get; set; }
    }
    public class ThemKhachHangMuaTaiQuayAdminDTO
    {
        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khách hàng không được vượt quá 100 ký tự")]
        public string ten_khach_hang { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng số 0 và có 10 chữ số")]
        public string so_dien_thoai { get; set; }
    }
    public class SuaKhachHangAdminDTO
    {
        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khách hàng không được vượt quá 100 ký tự")]
        public string? ten_khach_hang { get; set; }

        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Ngày sinh phải có định dạng yyyy-MM-dd")]
        public DateOnly? ngay_sinh { get; set; }

        [RegularExpression("^(Nam|Nu)$", ErrorMessage = "Giới tính phải là 'Nam' hoặc 'Nu'")]
        public string? gioi_tinh { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string? email { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng số 0 và có 10 chữ số")]
        public string? so_dien_thoai { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(HoatDong|KhongHoatDong)$", ErrorMessage = "Trạng thái phải là 'HoatDong' hoặc 'KhongHoatDong'")]
        public string? trang_thai { get; set; }
    }
    public class CapNhatTrangThaiKhachHangDTO
    {
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(HoatDong|KhongHoatDong)$", ErrorMessage = "Trạng thái phải là 'HoatDong' hoặc 'KhongHoatDong'")]
        public string trang_thai { get; set; }
    }
}