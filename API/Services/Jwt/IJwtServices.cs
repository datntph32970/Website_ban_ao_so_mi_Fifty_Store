using API.DbConects.DTOs.Client.TaiKhoan;
using System.Security.Claims;

namespace API.Services.JwtServices
{
    public interface IJwtServices
    {
        string GenerateJwtToken(Guid userId, string username, string role, string mataikhoan);
        Guid? GetUserIdFromToken(string token);
        string GetMaTaiKhoanFromToken(string token);
        Guid? GetIdNhanVienFromToken(string token);
        bool ValidateToken(string token);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        ThongTinNguoiDung LayThonTinNguoiDung(string token);
        Guid? GetIdKhachHangFromToken(string token);

    }
}
