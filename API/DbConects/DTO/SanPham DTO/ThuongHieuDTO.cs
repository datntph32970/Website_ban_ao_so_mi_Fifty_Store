using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.DTO.SanPham_DTO
{
    public class Them_ThuongHieuDTO
    {
        public string ten_thuong_hieu { get; set; }
    }
    public class Sua_ThuongHieuDTO
    {
        public string ten_thuong_hieu { get; set; }
        public TrangThaiThuongHieuDTO TrangThaiThuongHieuDTO { get; set; }
    }
    public enum TrangThaiThuongHieuDTO
    {
        HoatDong,
        TamNgung
    }
}
