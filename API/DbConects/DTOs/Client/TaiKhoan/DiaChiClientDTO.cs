namespace API.DbConects.DTOs.Client.TaiKhoan
{
    public class DiaChiClientDTO
    {
        public string tinh { get; set; }
        public string huyen { get; set; }
        public string xa { get; set; }
        public bool dia_chi_mac_dinh { get; set; }
        public string ngay_tao { get; set; }
        public string ngay_sua { get; set; }
        public KhachHangClientDTO khach_hang { get; set; }
    }
    public class ThemDiaChiClientDTO
    {
        public string tinh { get; set; }
        public string huyen { get; set; }
        public string xa { get; set; }
        public bool dia_chi_mac_dinh { get; set; }
    }
    public class SuaDiaChiClientDTO
    {
        public string tinh { get; set; }
        public string huyen { get; set; }
        public string xa { get; set; }
        public bool dia_chi_mac_dinh { get; set; }
    }
    public class KhachHangClientDTO
    {
        public string ten_khach_hang { get; set; }
        public string so_dien_thoai { get; set; }
        public List<DiaChiClientDTO> dia_chis { get; set; }
    }
}