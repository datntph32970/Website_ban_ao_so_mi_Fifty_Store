using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.DbConects.Entities.Entities_Tai_Khoan;

namespace API.DbConects.Entities.Entities_Hoa_Don
{
    public class PhuongThucThanhToan
    {
        [Key]
        public Guid id_phuong_thuc_thanh_toan { get; set; }
        [Required(ErrorMessage = "Tên phương thức thanh toán không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên phương thức thanh toán không được vượt quá 100 ký tự")]
        public string ten_phuong_thuc_thanh_toan { get; set; }
        [Required(ErrorMessage = "Mã phương thức thanh toán không được để trống")]
        [MaxLength(10, ErrorMessage = "Mã phương thức thanh toán không được vượt quá 10 ký tự")]
        public string ma_phuong_thuc_thanh_toan { get; set; }
        [Required(ErrorMessage = "Mô tả phương thức thanh toán không được để trống")]
        [MaxLength(255, ErrorMessage = "Mô tả phương thức thanh toán không được vượt quá 255 ký tự")]
        public string mo_ta { get; set; }
        [Required(ErrorMessage = "Trạng thái phương thức thanh toán không được để trống")]
        public bool trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public DateTime? ngay_cap_nhat { get; set; }
        [Required(ErrorMessage = "Id người tạo không được để trống")]
        [ForeignKey("NguoiTao")]
        public Guid id_nguoi_tao { get; set; }
        [ForeignKey("NguoiSua")]
        public Guid? id_nguoi_sua { get; set; }
        public virtual ICollection<HoaDon>? HoaDons { get; set; }
        public virtual NhanVien? NguoiTao { get; set; }
        public virtual NhanVien? NguoiSua { get; set; }
    }
}
