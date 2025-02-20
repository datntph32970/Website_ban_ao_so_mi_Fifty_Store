using System.ComponentModel.DataAnnotations;
using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations.Schema;


namespace API.DbConects.DTO.SanPham_DTO
{
    public class ThemXuatXuDTO
    {
        [Required(ErrorMessage = "Tên xuất xứ không được để trống.")]
        public string TenXuatXu { get; set; }
    }

    public class SuaXuatXuDTO
    {
        [Required(ErrorMessage = "Tên xuất xứ không được để trống.")]
        public string TenXuatXu { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        public TrangThaiXuatXuDTO TrangThai { get; set; }
    }

    public enum TrangThaiXuatXuDTO
    {
        HoatDong = 1,
        TamNgung = 0
    }
}
