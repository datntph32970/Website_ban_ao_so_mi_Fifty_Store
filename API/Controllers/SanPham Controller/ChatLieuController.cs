using API.DbConects.DTO.SanPham_DTO;
using API.Services.JwtServices;
using API.Services.SanPham_Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.SanPham_Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatLieuController : ControllerBase
    {
        private readonly IChatLieuService _chatLieuService;
        private readonly IJwtServices _jwtService;

        public ChatLieuController(IChatLieuService chatLieuService, IJwtServices jwtService)
        {
            _chatLieuService = chatLieuService;
            _jwtService = jwtService;
        }

        [HttpGet("lay-danh-sach-chat-lieu")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _chatLieuService.GetChatLieuAsync();
            return Ok(result);
        }

        [HttpPost("them-chat-lieu")]
        [Authorize(Roles = "NhanVien")]
        public async Task<IActionResult> ThemChatLieu(ThemChatLieuDTO chatLieuDTO)
        {
            if (chatLieuDTO == null || string.IsNullOrEmpty(chatLieuDTO.TenChatLieu))
                return BadRequest("Tên chất liệu không được để trống");

            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var mataikhoan = _jwtService.GetMaTaiKhoanFromToken(token);
            if (mataikhoan == null)
                return Unauthorized("Không thể xác thực người dùng");

            var result = await _chatLieuService.Add(chatLieuDTO, mataikhoan);
            return result.Item1 ? Ok("Thêm chất liệu thành công") : BadRequest(result.Item2);
        }
    }
}
