using API.DbConects.DTOs.Admin.TaiKhoan;

namespace API.DbConects.DTOs.Admin.KhachHang
{
    public class ThamSoPhanTrangKhachHangDTO
    {
        public int trang_hien_tai { get; set; }
        public int so_phan_tu_tren_trang { get; set; }
        public int tong_so_trang { get; set; }
        public int tong_so_phan_tu { get; set; }
        public string? tim_kiem { get; set; }
    }
}