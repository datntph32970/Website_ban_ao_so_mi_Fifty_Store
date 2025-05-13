using System;
using System.Collections.Generic;

namespace API.DbConects.DTOs.Admin.SanPham
{
    public class KieuDangAdminDTO
    {
        public Guid id_kieu_dang { get; set; }
        public string ma_kieu_dang { get; set; }
        public string ten_kieu_dang { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public string ten_nguoi_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string ten_nguoi_sua { get; set; }
        public List<SanPhamAdminDTO> SanPhams { get; set; }
    }

    public class ThemKieuDangAdminDTO
    {
        public string ten_kieu_dang { get; set; }
        public string mo_ta { get; set; }
    }

    public class SuaKieuDangAdminDTO
    {
        public string ten_kieu_dang { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
    }
}