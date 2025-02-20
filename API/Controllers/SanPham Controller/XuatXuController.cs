using API.DbConects.DTO.SanPham_DTO;
using API.Services.JwtServices;
using API.Services.SanPham_Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.SanPham_Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class XuatXuController : ControllerBase
    {
        private readonly IXuatXuService _xuatXuService;
        private readonly IJwtServices _jwtService;

        public XuatXuController(IXuatXuService xuatXuService, IJwtServices jwtService)
        {
            _xuatXuService = xuatXuService;
            _jwtService = jwtService;
        }

        [HttpGet("lay-danh-sach-xuat-xu")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _xuatXuService.GetXuatXuAsync();
            return Ok(result);
        }

        [HttpPost("them-xuat-xu")]
        [Authorize(Roles = "NhanVien")]
        public async Task<IActionResult> ThemXuatXu(ThemXuatXuDTO xuatXuDTO)
        {
            if (xuatXuDTO == null || string.IsNullOrEmpty(xuatXuDTO.TenXuatXu))
                return BadRequest("Tên xuất xứ không được để trống");

            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var mataikhoan = _jwtService.GetMaTaiKhoanFromToken(token);
            if (mataikhoan == null)
                return Unauthorized("Không thể xác thực người dùng");

            var result = await _xuatXuService.Add(xuatXuDTO, mataikhoan);
            return result.Item1 ? Ok("Thêm xuất xứ thành công") : BadRequest(result.Item2);
        }
    }
}
