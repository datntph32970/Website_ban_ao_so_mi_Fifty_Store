using System;
using System.Collections.Generic;

namespace API.DbConects.DTOs.Admin.SanPham
{
    public class XuatXuAdminDTO
    {
        public Guid id_xuat_xu { get; set; }
        public string ma_xuat_xu { get; set; }
        public string ten_xuat_xu { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public string ten_nguoi_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string ten_nguoi_sua { get; set; }
        public List<SanPhamAdminDTO> SanPhams { get; set; }
    }

    public class ThemXuatXuAdminDTO
    {
        public string ten_xuat_xu { get; set; }
        public string mo_ta { get; set; }
    }

    public class SuaXuatXuAdminDTO
    {
        public string ten_xuat_xu { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
    }
}