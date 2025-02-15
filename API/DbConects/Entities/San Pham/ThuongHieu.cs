using API.DbConects.Entities.Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.San_Pham
{
    public class ThuongHieu
    {
        [Key]
        public Guid id_thuong_hieu { get; set; }
        public string ma_thuong_hieu { get; set; }
        public string ten_thuong_hieu { get; set; }
        public string trang_thai { get; set; }
        public Guid id_nguoi_tao { get; set; }
        public DateTime ngay_tao { get; set; }
        [ForeignKey("NguoiTao")]
        public Guid? id_nguoi_sua { get; set; }
        [ForeignKey("NguoiSua")]
        public DateTime? ngay_sua { get; set; }
        public virtual ICollection<SanPham>? SanPhams { get; set; }
        public virtual NhanVien? NguoiTao { get; set; }
        public virtual NhanVien? NguoiSua { get; set; }
    }
}
