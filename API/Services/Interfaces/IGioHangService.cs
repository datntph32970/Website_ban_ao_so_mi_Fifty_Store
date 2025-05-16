using API.DbConects.DTOs.Client.HoaDon;
using API.DbConects.Entities.Entities_Hoa_Don;

namespace API.Services.Interfaces
{
    public interface IGioHangService : IBaseService<GioHangChiTiet>
    {
        Task<IEnumerable<GioHangItemClientDTO>> GetGioHangByKhachHangAsync(Guid idKhachHang);
        Task<IEnumerable<GioHangItemClientDTO>> GetGioHangDaChonAsync(Guid idKhachHang);
        Task<(bool success, string message)> ThemSanPhamVaoGioHangAsync(Guid idKhachHang, Guid idSanPhamChiTiet, int soLuong);
        Task<(bool success, string message)> CapNhatSoLuongAsync(Guid idGioHangChiTiet, int soLuong);
        Task<(bool success, string message)> XoaSanPhamKhoiGioHangAsync(Guid idGioHangChiTiet);
        Task<(bool success, string message)> XoaGioHangAsync(Guid idKhachHang);
        Task<(bool success, string message)> CapNhatTrangThaiGioHangAsync(Guid idGioHangChiTiet, bool trangThai);
        Task<(bool success, string message, int? soLuong)> KiemTraSoLuongTonAsync(Guid idSanPhamChiTiet);
    }
}