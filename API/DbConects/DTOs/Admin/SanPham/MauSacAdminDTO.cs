using System;
using System.Collections.Generic;

namespace API.DbConects.DTOs.Admin.SanPham
{
    public class MauSacAdminDTO
    {
        public Guid id_mau_sac { get; set; }
        public string ma_mau_sac { get; set; }
        public string ten_mau_sac { get; set; }
        public string ma_mau { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
        public DateTime? ngay_tao { get; set; }
        public string ten_nguoi_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string ten_nguoi_sua { get; set; }
        public List<SanPhamChiTietAdminDTO> SanPhamChiTiets { get; set; }
    }

    public class ThemMauSacAdminDTO
    {
        public string ten_mau_sac { get; set; }
        public string mo_ta { get; set; }
    }

    public class SuaMauSacAdminDTO
    {
        public string ten_mau_sac { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
    }
}