using API.DbConects.DTOs.Client.TaiKhoan;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using System.Text.Json;

namespace API.Controllers.TaiKhoan_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenService _authenService;
        private readonly IJwtServices _jwtServices;

        public AuthController(IAuthenService authenService, IJwtServices jwtServices)
        {
            _authenService = authenService;
            _jwtServices = jwtServices;
        }

        [HttpPost("dang-ky-tai-khoan")]
        [AllowAnonymous]
        public async Task<IActionResult> DangKy([FromBody] DangKyClientDTO dangKyDTO)
        {
            if (dangKyDTO == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (string.IsNullOrEmpty(dangKyDTO.ten_dang_nhap) || string.IsNullOrEmpty(dangKyDTO.mat_khau))
                return BadRequest("Username và password không được để trống");
            if (dangKyDTO.mat_khau != dangKyDTO.xac_nhan_mat_khau)
                return BadRequest("Mật khẩu và xác nhận mật khẩu không khớp");

            var result = await _authenService.DangKyAsync(dangKyDTO);
            if (result.Item1)
                return Ok(result.Item2);
            return BadRequest(result.Item2);
        }

        [HttpPost("dang-nhap-tai-khoan")]
        [AllowAnonymous]
        public async Task<IActionResult> DangNhap([FromBody] DangNhapClientDTO dangNhapDTO)
        {
            if (dangNhapDTO == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (string.IsNullOrEmpty(dangNhapDTO.ten_dang_nhap) || string.IsNullOrEmpty(dangNhapDTO.mat_khau))
                return BadRequest("Username và password không được để trống");

            var result = await _authenService.DangNhapAsync(dangNhapDTO);
            if (result.Item1)
                return Ok(result.Item2);
            return BadRequest(result.Item2);
        }

        // [HttpPost("quen-mat-khau")]
        // [AllowAnonymous]
        // public async Task<IActionResult> QuenMatKhau([FromBody] QuenMatKhauDTO quenMatKhauDTO)
        // {
        //     if (quenMatKhauDTO == null)
        //         return BadRequest("Dữ liệu không hợp lệ");

        //     if (string.IsNullOrEmpty(quenMatKhauDTO.username))
        //         return BadRequest("Username không được để trống");

        //     var result = await _authenService.QuenMatKhau(quenMatKhauDTO);
        //     if (result.Item1)
        //         return Ok(result.Item2);
        //     return BadRequest(result.Item2);
        // }

        [HttpPost("doi-mat-khau")]
        [Authorize]
        public async Task<IActionResult> DoiMatKhau([FromBody] DoiMatKhauClientDTO doiMatKhauDTO)
        {
            if (doiMatKhauDTO == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (string.IsNullOrEmpty(doiMatKhauDTO.mat_khau_cu) ||
                string.IsNullOrEmpty(doiMatKhauDTO.mat_khau_moi) ||
                string.IsNullOrEmpty(doiMatKhauDTO.xac_nhan_mat_khau_moi))
                return BadRequest("Vui lòng điền đầy đủ thông tin");

            if (doiMatKhauDTO.mat_khau_moi != doiMatKhauDTO.xac_nhan_mat_khau_moi)
                return BadRequest("Mật khẩu mới và xác nhận mật khẩu không khớp");

            var maTaiKhoan = _jwtServices.GetMaTaiKhoanFromToken(HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last());
            var result = await _authenService.DoiMatKhauAsync(doiMatKhauDTO);
            if (result.Item1)
                return Ok(result.Item2);
            return BadRequest(result.Item2);
        }
        [HttpGet("me")]
        [Authorize]
        public IActionResult LayThongTinNguuoiDung()
        {

            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !_jwtServices.ValidateToken(token))
                return Unauthorized("Token không hợp lệ");
            var thongtinnguoidung = _jwtServices.LayThonTinNguoiDung(token);
            if (thongtinnguoidung == null)
                return NotFound("Không tìm thấy tài khoản");
            return Ok(thongtinnguoidung);
        }

    }
}
