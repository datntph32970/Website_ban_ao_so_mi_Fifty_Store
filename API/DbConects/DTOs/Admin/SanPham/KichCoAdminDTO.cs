using System;
using System.Collections.Generic;

namespace API.DbConects.DTOs.Admin.SanPham
{
    public class KichCoAdminDTO
    {
        public Guid id_kich_co { get; set; }
        public string ma_kich_co { get; set; }
        public string ten_kich_co { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
        public DateTime? ngay_tao { get; set; }
        public string ma_nguoi_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string ma_nguoi_sua { get; set; }
        public List<SanPhamChiTietAdminDTO> SanPhamChiTiets { get; set; }
    }

    public class ThemKichCoAdminDTO
    {
        public string ten_kich_co { get; set; }
        public string mo_ta { get; set; }
    }

    public class SuaKichCoAdminDTO
    {
        public string ten_kich_co { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
    }
}