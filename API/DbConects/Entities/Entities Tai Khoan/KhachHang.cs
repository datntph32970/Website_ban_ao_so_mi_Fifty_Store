using API.DbConects.Entities.Entities_Hoa_Don;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Tai_Khoan
{
    public class KhachHang
    {
        [Key]
        public Guid id_khach_hang { get; set; }
        [ForeignKey("KhachHang")]
        public Guid id_tai_khoan { get; set; }
        public string ma_khach_hang { get; set; }
        public string ten_khach_hang { get; set; }
        public DateTime ngay_sinh { get; set; }
        public string so_dien_thoai { get; set; }
        public string email { get; set; }
        public string gioi_tinh { get; set; }
        public string trang_thai { get; set; }
        public virtual TaiKhoan? TaiKhoan { get; set; }
        public virtual ICollection< GioHangChiTiet>? GioHangChiTiets { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
        public virtual ICollection<DiaChi> DiaChis { get; set; }
    }
}
