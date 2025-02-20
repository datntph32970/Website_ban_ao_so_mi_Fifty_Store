using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_San_Pham
{
    public class ThuongHieu
    {
        [Key]
        public Guid id_thuong_hieu { get; set; }
        [Required]
        public string ma_thuong_hieu { get; set; }
        [Required]
        public string ten_thuong_hieu { get; set; }
        [Required]
        public string trang_thai { get; set; }
        [Required]
        public Guid id_nguoi_tao { get; set; }
        [Required]
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
