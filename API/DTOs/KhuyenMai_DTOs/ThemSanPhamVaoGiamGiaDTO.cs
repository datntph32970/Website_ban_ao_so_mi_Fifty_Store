using System;
using System.Collections.Generic;

namespace API.DTOs.KhuyenMai_DTOs
{
    public class ThemGiamGiaVaoSanPhamChiTietDTO
    {
        public string id_giam_gia { get; set; }
        public List<string> san_pham_chi_tiet_ids { get; set; }
    }
}