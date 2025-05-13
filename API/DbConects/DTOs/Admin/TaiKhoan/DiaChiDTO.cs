namespace API.DbConects.DTOs.Admin.TaiKhoan
{
    public class DiaChiDTO
    {
        public Guid id_dia_chi { get; set; }
        public Guid id_khach_hang { get; set; }
        public string tinh { get; set; }
        public string huyen { get; set; }
        public string xa { get; set; }
        public bool dia_chi_mac_dinh { get; set; }
        public string ngay_tao { get; set; }
        public string ngay_sua { get; set; }

    }
}