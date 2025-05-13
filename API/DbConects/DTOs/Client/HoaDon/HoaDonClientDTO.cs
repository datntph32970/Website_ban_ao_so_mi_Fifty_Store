using System;
using System.Collections.Generic;

namespace API.DbConects.DTOs.Client.HoaDon
{
    public class HoaDonClientDTO
    {
        public Guid id_hoa_don { get; set; }
        public string ma_hoa_don { get; set; }
        public string dia_chi { get; set; }
        public string ghi_chu { get; set; }
        public decimal tong_tien { get; set; }
        public decimal tien_giam { get; set; }
        public decimal thanh_tien { get; set; }
        public string trang_thai { get; set; }
        public string phuong_thuc_thanh_toan { get; set; }
        public DateTime ngay_tao { get; set; }
        public List<HoaDonChiTietClientDTO> HoaDonChiTiets { get; set; }
    }

    public class HoaDonChiTietClientDTO
    {
        public Guid id_hoa_don_chi_tiet { get; set; }
        public string ten_san_pham { get; set; }
        public string ten_mau_sac { get; set; }
        public string ten_kich_co { get; set; }
        public int so_luong { get; set; }
        public decimal don_gia { get; set; }
        public decimal thanh_tien { get; set; }
        public List<string> hinh_anh_urls { get; set; }
    }

    public class ThemHoaDonClientDTO
    {
        public string dia_chi { get; set; }
        public string ghi_chu { get; set; }
        public string phuong_thuc_thanh_toan { get; set; }
        public List<ThemHoaDonChiTietClientDTO> HoaDonChiTiets { get; set; }
    }

    public class ThemHoaDonChiTietClientDTO
    {
        public Guid id_san_pham_chi_tiet { get; set; }
        public int so_luong { get; set; }
    }
}