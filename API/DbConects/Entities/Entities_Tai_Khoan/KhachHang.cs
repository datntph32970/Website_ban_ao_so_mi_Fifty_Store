using API.DbConects.Entities.Entities_Hoa_Don;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Tai_Khoan
{
    public class KhachHang
    {
        [Key]
        public Guid id_khach_hang { get; set; }
        [ForeignKey("TaiKhoan")]
        public Guid? id_tai_khoan { get; set; }
        [Required(ErrorMessage = "Mã khách hàng không được để trống")]
        public string ma_khach_hang { get; set; }
        [StringLength(100, ErrorMessage = "Tên khách hàng không được vượt quá 100 ký tự")]
        public string? ten_khach_hang { get; set; }
        public DateOnly? ngay_sinh { get; set; }
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? so_dien_thoai { get; set; }
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? email { get; set; }
        public string? gioi_tinh { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public virtual TaiKhoan? TaiKhoan { get; set; }
        public virtual ICollection<GioHangChiTiet>? GioHangChiTiets { get; set; }
        public virtual ICollection<HoaDon>? HoaDons { get; set; }
        public virtual ICollection<DiaChi>? DiaChis { get; set; }
    }
}
