using System.ComponentModel.DataAnnotations;

namespace API.DbConects.Entities.Entities_Tai_Khoan
{
    public class TaiKhoan
    {
        [Key]
        public Guid id_tai_khoan { get; set; }

        public string ma_tai_khoan { get; set; }
        public string ten_dang_nhap { get; set; }
        public string mat_khau { get; set; }
        public string trang_thai { get; set; }
        public string chuc_vu { get; set; }
        public virtual KhachHang? KhachHang { get; set; }
        public virtual NhanVien? NhanVien { get; set; }
    }
}
