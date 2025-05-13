using API.DbConects.Entities.Entities_Tai_Khoan;

namespace API.DbConects.DTOs.Admin.SanPham
{
    public class DanhMucAdminDTO
    {
        public Guid id_danh_muc { get; set; }
        public string ten_danh_muc { get; set; }
        public string ma_danh_muc { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public NhanVien? NguoiTao { get; set; }
        public NhanVien? NguoiSua { get; set; }
    }
    public class ThemDanhMucAdminDTO
    {
        public string ten_danh_muc { get; set; }
        public string mo_ta { get; set; }
    }

    public class SuaDanhMucAdminDTO
    {
        public string ten_danh_muc { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
    }
}
