using System.ComponentModel.DataAnnotations;

namespace API.DbConects.Entities.Entities_Tai_Khoan
{
    public class TaiKhoan
    {
        [Key]
        public Guid id_tai_khoan { get; set; }
        [Required(ErrorMessage = "Mã tài khoản không được để trống")]
        public string ma_tai_khoan { get; set; }
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string ten_dang_nhap { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string mat_khau { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string trang_thai { get; set; }
        public bool da_doi_mat_khau { get; set; }   
        [Required(ErrorMessage = "Chức vụ không được để trống")]
        public string chuc_vu { get; set; }
        public virtual KhachHang? KhachHang { get; set; }
        public virtual NhanVien? NhanVien { get; set; }
    }
}
