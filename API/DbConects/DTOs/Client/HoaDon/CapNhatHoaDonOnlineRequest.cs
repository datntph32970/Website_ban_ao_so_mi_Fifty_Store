using System.ComponentModel.DataAnnotations;

namespace API.DbConects.DTOs.Client.HoaDon
{
    public class CapNhatHoaDonOnlineRequest
    {
        public string? id_dia_chi_nhan_hang { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Phí vận chuyển không được âm")]
        public decimal phi_van_chuyen { get; set; }

        [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? ghi_chu { get; set; }

        public string? id_khuyen_mai { get; set; }

        public string? id_phuong_thuc_thanh_toan { get; set; }
    }
}