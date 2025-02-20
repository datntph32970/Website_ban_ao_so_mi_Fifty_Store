using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.DbConects.DTO.SanPham_DTO
{
    public class ThemChatLieuDTO
    {
        [Required(ErrorMessage = "Tên chất liệu không được để trống.")]
        public string TenChatLieu { get; set; }
    }

    public class SuaChatLieuDTO
    {
        [Required(ErrorMessage = "Tên chất liệu không được để trống.")]
        public string TenChatLieu { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        public TrangThaiChatLieuDTO TrangThai { get; set; }
    }

    public enum TrangThaiChatLieuDTO
    {
        HoatDong = 1,
        TamNgung = 0
    }
}
