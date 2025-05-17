using System.ComponentModel.DataAnnotations;

namespace API.DbConects.DTOs.Admin.TaiKhoan
{
    public class CapNhatThongTinNhanVienDTO
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string ho_ten { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Số điện thoại phải có 10 chữ số")]
        public string so_dien_thoai { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string email { get; set; }

        [Required(ErrorMessage = "Giới tính không được để trống")]
        public string gioi_tinh { get; set; }

        [Required(ErrorMessage = "CCCD không được để trống")]
        [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "CCCD phải có 12 chữ số")]
        public string cccd { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string dia_chi { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        public DateTime ngay_sinh { get; set; }
    }
}