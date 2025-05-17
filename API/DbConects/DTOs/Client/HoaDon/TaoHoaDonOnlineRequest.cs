using System;
using System.Collections.Generic;

namespace API.DbConects.DTOs.Client.HoaDon
{
    public class TaoHoaDonOnlineRequest
    {
        public string dia_chi_nhan_hang { get; set; }
        public string ten_khach_hang { get; set; }
        public string sdt_khach_hang { get; set; }
        public string? ghi_chu { get; set; }
        public string id_phuong_thuc_thanh_toan { get; set; }
        public string? id_khuyen_mai { get; set; }
        public decimal phi_van_chuyen { get; set; }

    }
}
