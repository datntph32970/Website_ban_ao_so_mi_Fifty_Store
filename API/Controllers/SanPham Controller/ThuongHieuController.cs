using API.DbConects.DTO.SanPham_DTO;
using API.DbConects.Entities.Entities_San_Pham;
using API.Services.JwtServices;
using API.Services.SanPham_Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Controllers.SanPham_Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThuongHieuController : ControllerBase
    {
        private readonly IThuongHieuServices _thuongHieuService;
        private readonly IJwtServices _jwtService;

        public ThuongHieuController(IThuongHieuServices thuongHieuService, IJwtServices jwtService)
        {
            _thuongHieuService = thuongHieuService;
            _jwtService = jwtService;
        }

        [HttpGet("lay-danh-sach-thuong-hieu")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _thuongHieuService.GetThuongHieuAsync();
            return Ok(result);
        }

        [HttpPost("them-thuong-hieu")]
        [Authorize(Roles = "NhanVien")]
        public async Task<IActionResult> ThemThuongHieu(Them_ThuongHieuDTO thuongHieuDTO)
        {
            if (thuongHieuDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (thuongHieuDTO.ten_thuong_hieu == null)
                return BadRequest("Yêu cầu nhập tên thương hiệu");

            // Extract user ID from token

            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            var mataikhoan = _jwtService.GetMaTaiKhoanFromToken(token);
            if (mataikhoan == null)
                return Unauthorized("Không thể xác thực người dùng");


            var result = await _thuongHieuService.Add(thuongHieuDTO, mataikhoan);
            if (!result.Item1)
                return BadRequest(result.Item2);

            return Ok("Thêm Thương hiệu mới thành công");
        }


    }
}
