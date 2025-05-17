using API.DbConects.DTOs.Client.TaiKhoan;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services.JwtServices
{
    public class JwtServices : IJwtServices
    {
        private readonly string _secret;
        private readonly IBaseRepository<TaiKhoan> _taiKhoanRepository;
        private readonly IBaseRepository<NhanVien> _nhanVienRepository;
        private readonly IBaseRepository<KhachHang> _khachHangRepository;
        public JwtServices(IConfiguration configuration, IBaseRepository<TaiKhoan> taiKhoanRepository, IBaseRepository<NhanVien> nhanVienRepository, IBaseRepository<KhachHang> khachHangRepository)
        {
            _secret = configuration["Jwt:Key"];
            _taiKhoanRepository = taiKhoanRepository;
            _nhanVienRepository = nhanVienRepository;
            _khachHangRepository = khachHangRepository;
        }

        public string GenerateJwtToken(Guid userId, string username, string role, string mataikhoan)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id_tai_khoan", userId.ToString()),
                    new Claim("ten_dang_nhap", username),
                    new Claim(ClaimTypes.Role, role),
                    new Claim("ma_tai_khoan", mataikhoan)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secret)),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }
        public ThongTinNguoiDung LayThonTinNguoiDung(string token)
        {
            if (token == null)
                return null;
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            var userIdClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "id_tai_khoan");
            var usernameClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "ten_dang_nhap");
            var roleClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "role");
            var maTaiKhoanClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "ma_tai_khoan");

            var isDoiMatKhau = _taiKhoanRepository.GetByIdAsync(Guid.Parse(userIdClaim?.Value)).Result.da_doi_mat_khau;
            return new ThongTinNguoiDung
            {
                id_tai_khoan = userIdClaim?.Value,
                ma_tai_khoan = maTaiKhoanClaim?.Value,
                chuc_vu = roleClaim?.Value,
                da_doi_mat_khau = isDoiMatKhau,
                ten_dang_nhap = usernameClaim?.Value
            };
        }
        public Guid? GetUserIdFromToken(string token)
        {
            if (token == null)
                return null;

            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            var userIdClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "id_tai_khoan");

            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : (Guid?)null;
        }

        public string GetMaTaiKhoanFromToken(string token)
        {
            if (token == null)
                return null;

            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            var maTaiKhoanClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "ma_tai_khoan");

            return maTaiKhoanClaim?.Value;
        }
        public Guid? GetIdNhanVienFromToken(string token)
        {
            if (token == null)
                return null;

            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            var maTaiKhoanClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "ma_tai_khoan");

            if (maTaiKhoanClaim == null)
                return null;

            var nhanVien = _nhanVienRepository.GetFirstOrDefaultAsync(x => x.TaiKhoanNhanVien.ma_tai_khoan == maTaiKhoanClaim.Value).Result;

            return nhanVien?.id_nhan_vien;
        }
        public Guid? GetIdKhachHangFromToken(string token)
        {
            if (token == null)
                return null;

            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            var idTaiKhoanClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "id_tai_khoan");

            if (idTaiKhoanClaim == null)
                return null;

            var khachHang = _khachHangRepository.GetByConditionAsync(x => x.id_tai_khoan == Guid.Parse(idTaiKhoanClaim.Value)).Result;

            return khachHang?.FirstOrDefault()?.id_khach_hang;
        }
    }
}
