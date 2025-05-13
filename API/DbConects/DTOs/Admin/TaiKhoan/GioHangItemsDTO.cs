namespace API.DbConects.DTOs.Admin.TaiKhoan
{
    public class GioHangItemsDTO
    {
        public Guid id_gio_hang_chi_tiet { get; set; }
        public Guid id_khach_hang { get; set; }
        public Guid id_san_pham_chi_tiet { get; set; }
        public int so_luong { get; set; }
        public string trang_thai { get; set; }
    }

}