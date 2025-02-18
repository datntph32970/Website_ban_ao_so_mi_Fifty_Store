using API.Services.JwtServices;
using API.Services.TaiKhoan_Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenController : ControllerBase
    {
        private readonly ITaiKhoanServices _taiKhoanServices;

        public AuthenController(ITaiKhoanServices taiKhoanServices)
        {
            _taiKhoanServices = taiKhoanServices;
        }
        [HttpPost("dang-ky-tai-khoan")]
        [AllowAnonymous]
        public async Task<IActionResult> DangKy(string username, string password)
        {
            var result = _taiKhoanServices.DangKyTaiKhoan(username, password);
            if (result.Item1)
            {
                return Ok(result.Item2);
            }
            return BadRequest(result.Item2);
        }
        [HttpGet("dang-nhap-tai-khoan")]
        [AllowAnonymous]
        public async Task<IActionResult> DangNhap(string username, string password)
        {
            var result = _taiKhoanServices.DangNhapTaiKhoan(username, password);
            if (result.Item1)
            {
                return Ok(result.Item2);
            }
            return BadRequest(result.Item2);
        }
    }
}
