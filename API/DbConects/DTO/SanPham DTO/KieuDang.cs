using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.DbConects.DTO.SanPham_DTO
{
    public class ThemKieuDangDTO
    {
        [Required(ErrorMessage = "Tên kiểu dáng không được để trống.")]
        public string TenKieuDang { get; set; }
    }

    public class SuaKieuDangDTO
    {
        [Required(ErrorMessage = "Tên kiểu dáng không được để trống.")]
        public string TenKieuDang { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        public TrangThaiKieuDangDTO TrangThai { get; set; }
    }

    public enum TrangThaiKieuDangDTO
    {
        HoatDong = 1,
        TamNgung = 0
    }

}
