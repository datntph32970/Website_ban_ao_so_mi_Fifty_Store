using System;
using System.Collections.Generic;

namespace API.DbConects.DTOs.Admin.SanPham
{
    public class ThuongHieuAdminDTO
    {
        public Guid id_thuong_hieu { get; set; }
        public string ma_thuong_hieu { get; set; }
        public string ten_thuong_hieu { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public string ten_nguoi_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string ten_nguoi_sua { get; set; }
        public List<SanPhamAdminDTO> SanPhams { get; set; }
    }

    public class ThemThuongHieuAdminDTO
    {
        public string ten_thuong_hieu { get; set; }
        public string mo_ta { get; set; }
    }

    public class SuaThuongHieuAdminDTO
    {
        public string ten_thuong_hieu { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
    }
}