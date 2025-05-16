using API.DbConects.DTOs.Client.TaiKhoan;
using API.DbConects.Entities.Entities_Tai_Khoan;

namespace API.Services.Interfaces
{
    public interface IDiaChiService : IBaseService<DiaChi>
    {
        Task<IEnumerable<DiaChiDTO>> GetDiaChiByKhachHangAsync(Guid idKhachHang);
        Task<DiaChiDTO> GetDiaChiMacDinhAsync(Guid idKhachHang);
        Task<(bool success, string message)> CreateDiaChiAsync(Guid idKhachHang, CreateDiaChiDTO createDto);
        Task<(bool success, string message)> UpdateDiaChiAsync(Guid idDiaChi, Guid idKhachHang, UpdateDiaChiDTO updateDto);
        Task<(bool success, string message)> DeleteDiaChiAsync(Guid idDiaChi, Guid idKhachHang);
        Task<(bool success, string message)> SetDiaChiMacDinhAsync(Guid idDiaChi, Guid idKhachHang);
    }
}