using System.Security.Claims;

namespace API.Services.JwtServices
{
    public interface IJwtServices
    {
        string GenerateJwtToken(string username, string role);
        bool ValidateToken(string token);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    }
}
