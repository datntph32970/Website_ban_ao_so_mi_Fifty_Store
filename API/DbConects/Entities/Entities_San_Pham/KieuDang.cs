using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_San_Pham
{
    public class KieuDang
    {
        [Key]
        public Guid id_kieu_dang { get; set; }
        [Required(ErrorMessage = "Mã kiểu dáng không được để trống")]
        public string ma_kieu_dang { get; set; }
        [Required(ErrorMessage = "Tên kiểu dáng không được để trống")]
        public string ten_kieu_dang { get; set; }
        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string mo_ta { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string trang_thai { get; set; }
        [Required(ErrorMessage = "Mã người tạo không được để trống")]
        public Guid id_nguoi_tao { get; set; }
        [Required(ErrorMessage = "Ngày tạo không được để trống")]
        public DateTime ngay_tao { get; set; }
        public Guid? id_nguoi_sua { get; set; }
        public DateTime? ngay_sua { get; set; }
        public virtual ICollection<SanPham>? SanPhams { get; set; }
        [ForeignKey("id_nguoi_tao")]
        public virtual NhanVien? NguoiTao { get; set; }
        [ForeignKey("id_nguoi_sua")]
        public virtual NhanVien? NguoiSua { get; set; }
    }
}
