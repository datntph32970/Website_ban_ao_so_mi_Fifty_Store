using System.Security.Claims;

namespace API.Services.JwtServices
{
    public interface IJwtServices
    {
        string GenerateJwtToken(Guid userId, string username, string role, string mataikhoan);
        Guid? GetUserIdFromToken(string token);
        string GetMaTaiKhoanFromToken(string token);
        bool ValidateToken(string token);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    }
}
