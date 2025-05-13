namespace API.DbConects.DTOs.Admin.TaiKhoan
{
    public class NhanVienAdiminDTO
    {
        public Guid id_tai_khoan { get; set; }
        public string ma_tai_khoan { get; set; }
        public string ten_dang_nhap { get; set; }
        public string ho_ten { get; set; }
        public string so_dien_thoai { get; set; }
        public string email { get; set; }
        public string gioi_tinh { get; set; }
        public string cccd { get; set; }
        public DateTime ngay_sinh { get; set; }
        public string dia_chi { get; set; }
        public string chuc_vu { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public string ten_nguoi_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string ten_nguoi_sua { get; set; }
    }
    public class ThemNhanVienAdminDTO
    {
        public string ten_dang_nhap { get; set; }
        public string ho_ten { get; set; }
        public string so_dien_thoai { get; set; }
        public string email { get; set; }
        public string gioi_tinh { get; set; }
        public string cccd { get; set; }
        public DateTime ngay_sinh { get; set; }
        public string dia_chi { get; set; }
        public string chuc_vu { get; set; }
        public string trang_thai { get; set; }
    }
    public class SuaNhanVienAdminDTO
    {
        public string ho_ten { get; set; }
        public string so_dien_thoai { get; set; }
        public string email { get; set; }
        public string gioi_tinh { get; set; }
        public string cccd { get; set; }
        public DateTime ngay_sinh { get; set; }
        public string dia_chi { get; set; }
        public string chuc_vu { get; set; }
        public string trang_thai { get; set; }
    }
    public class XoaNhanVienAdminDTO
    {
        public string id_nhan_vien { get; set; }
    }
}