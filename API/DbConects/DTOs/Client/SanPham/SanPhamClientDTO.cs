using System;
using System.Collections.Generic;

namespace API.DbConects.DTOs.Client.SanPham
{
    public class SanPhamClientDTO
    {
        public Guid id_san_pham { get; set; }
        public string ma_san_pham { get; set; }
        public string ten_san_pham { get; set; }
        public string mo_ta { get; set; }
        public string ten_thuong_hieu { get; set; }
        public string ten_kieu_dang { get; set; }
        public string ten_chat_lieu { get; set; }
        public string ten_xuat_xu { get; set; }
        public List<SanPhamChiTietClientDTO> SanPhamChiTiets { get; set; }
        public List<string> hinh_anh_urls { get; set; }
        public decimal gia_thap_nhat { get; set; }
        public decimal gia_cao_nhat { get; set; }
        public bool co_giam_gia { get; set; }
        public int so_luong_con_lai { get; set; }
        public double danh_gia_trung_binh { get; set; }
        public int so_luong_danh_gia { get; set; }
    }

    public class SanPhamChiTietClientDTO
    {
        public Guid id_san_pham_chi_tiet { get; set; }
        public string ma_san_pham_chi_tiet { get; set; }
        public string ten_mau_sac { get; set; }
        public string ma_mau { get; set; }
        public string ten_kich_co { get; set; }
        public int so_luong { get; set; }
        public decimal gia_ban { get; set; }
        public decimal gia_goc { get; set; }
        public decimal phan_tram_giam { get; set; }
        public List<string> hinh_anh_urls { get; set; }
        public bool co_hang { get; set; }
    }
}