namespace API.DbConects.DTOs.Admin.HoaDon
{
    public class CuaHangDTO
    {
        public Guid id_cua_hang { get; set; }
        public string ten_cua_hang { get; set; }
        public string website { get; set; }
        public string email { get; set; }
        public string sdt { get; set; }
        public string dia_chi { get; set; }
        public string mo_ta { get; set; }
        public string? hinh_anh_url { get; set; }
    }
}