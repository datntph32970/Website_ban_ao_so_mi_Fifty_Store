using API.DbConects.Entities.Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.San_Pham
{
    public class KichCo
    {
        [Key]
        public Guid id_kich_co { get; set; }
        public string ma_kich_co { get; set; }
        public string ten_kich_co { get; set; }
        public string trang_thai { get; set; }
        [ForeignKey("NguoiTao")]
        public Guid id_nguoi_tao { get; set; }
        public DateTime ngay_tao { get; set; }
        [ForeignKey("NguoiSua")]
        public Guid? id_nguoi_sua { get; set; }
        public DateTime? ngay_sua { get; set; }
        public virtual ICollection<SanPhamChiTiet>? SanPhamChiTiets { get; set; }
        public virtual NhanVien? NguoiTao { get; set; }
        public virtual NhanVien? NguoiSua { get; set; }
    }
}
