namespace API.DbConects.DTOs.Client.HoaDon
{
    public class GioHangItemClientDTO
    {
        public Guid id_gio_hang_chi_tiet { get; set; }
        public Guid id_san_pham_chi_tiet { get; set; }
        public string ma_san_pham_chi_tiet { get; set; }
        public string ten_san_pham { get; set; }
        public string ten_mau_sac { get; set; }
        public string ten_kich_co { get; set; }
        public int so_luong { get; set; }
        public decimal gia_ban { get; set; }
        public decimal? gia_sau_giam { get; set; }
        public string url_anh { get; set; }
        public bool trang_thai { get; set; }
        public int so_luong_ton { get; set; }

        // Thông tin giảm giá nếu có
        public Guid? id_giam_gia { get; set; }
        public string? ten_giam_gia { get; set; }
        public string? kieu_giam_gia { get; set; }  // "PhanTram" hoặc "SoTien"
        public decimal? gia_tri_giam { get; set; }
        public DateTime? thoi_gian_bat_dau { get; set; }
        public DateTime? thoi_gian_ket_thuc { get; set; }
    }
}
