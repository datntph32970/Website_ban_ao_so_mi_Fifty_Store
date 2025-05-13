using API.DbConects.DTOs.Client.TaiKhoan;
using API.DbConects.Entities.Entities_Tai_Khoan;

namespace API.Services.Interfaces
{
    public interface IAuthenService : IBaseService<TaiKhoan>
    {
        Task<(bool success, object message)> DangNhapAsync(DangNhapClientDTO dto);
        Task<(bool success, string message)> DangKyAsync(DangKyClientDTO dto);
        Task<(bool success, string message)> DoiMatKhauAsync(DoiMatKhauClientDTO dto);
    }
}