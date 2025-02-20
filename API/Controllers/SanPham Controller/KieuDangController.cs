using API.DbConects.DTO.SanPham_DTO;
using API.Services.JwtServices;
using API.Services.SanPham_Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.SanPham_Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class KieuDangController : ControllerBase
    {
        private readonly IKieuDangService _kieuDangService;
        private readonly IJwtServices _jwtService;

        public KieuDangController(IKieuDangService kieuDangService, IJwtServices jwtService)
        {
            _kieuDangService = kieuDangService;
            _jwtService = jwtService;
        }

        [HttpGet("lay-danh-sach-kieu-dang")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _kieuDangService.GetKieuDangAsync();
            return Ok(result);
        }

        [HttpPost("them-kieu-dang")]
        [Authorize(Roles = "NhanVien")]
        public async Task<IActionResult> ThemKieuDang(ThemKieuDangDTO kieuDangDTO)
        {
            if (kieuDangDTO == null || string.IsNullOrEmpty(kieuDangDTO.TenKieuDang))
                return BadRequest("Tên kiểu dáng không được để trống");

            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var mataikhoan = _jwtService.GetMaTaiKhoanFromToken(token);
            if (mataikhoan == null)
                return Unauthorized("Không thể xác thực người dùng");

            var result = await _kieuDangService.Add(kieuDangDTO, mataikhoan);
            return result.Item1 ? Ok("Thêm kiểu dáng thành công") : BadRequest(result.Item2);
        }
    }
}
