using System;
using System.Collections.Generic;

namespace API.DbConects.DTOs.Client.KhuyenMai
{
    public class KhuyenMaiClientDTO
    {
        public Guid id_khuyen_mai { get; set; }
        public string ma_khuyen_mai { get; set; }
        public string ten_khuyen_mai { get; set; }
        public string mo_ta { get; set; }
        public decimal phan_tram_giam { get; set; }
        public DateTime ngay_bat_dau { get; set; }
        public DateTime ngay_ket_thuc { get; set; }
        public List<SanPhamKhuyenMaiClientDTO> SanPhamKhuyenMais { get; set; }
    }

    public class SanPhamKhuyenMaiClientDTO
    {
        public Guid id_san_pham_khuyen_mai { get; set; }
        public Guid id_san_pham_chi_tiet { get; set; }
        public string ten_san_pham { get; set; }
        public string ten_mau_sac { get; set; }
        public string ten_kich_co { get; set; }
        public decimal gia_goc { get; set; }
        public decimal gia_sau_giam { get; set; }
        public List<string> hinh_anh_urls { get; set; }
    }
}