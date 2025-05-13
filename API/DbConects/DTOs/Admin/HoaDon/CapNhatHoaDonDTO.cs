using System;
using System.ComponentModel.DataAnnotations;

namespace API.DbConects.DTOs.Admin.HoaDon
{
    public class CapNhatHoaDonDTO
    {
        [Required]
        public Guid id_hoa_don { get; set; }
        public string? id_khach_hang { get; set; }
        public string? dia_chi_nhan_hang { get; set; }
        public string? id_khuyen_mai { get; set; }
        public string? ghi_chu { get; set; }
    }
}