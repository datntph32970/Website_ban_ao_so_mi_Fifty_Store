using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_San_Pham;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers.SanPham_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatLieuController : ControllerBase
    {
        private readonly IBaseService<ChatLieu> _chatLieuServices;
        private readonly IJwtServices _jwtServices;

        public ChatLieuController(IBaseService<ChatLieu> chatLieuServices, IJwtServices jwtServices)
        {
            _chatLieuServices = chatLieuServices;
            _jwtServices = jwtServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _chatLieuServices.GetAllWithIncludeAsync(cl => cl.Include(c => c.SanPhams));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _chatLieuServices.GetByIdWithIncludeAsync(id, cl => cl.Include(c => c.SanPhams));
            if (result == null)
                return NotFound("Không tìm thấy chất liệu");
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Add(ThemChatLieuAdminDTO chatLieuDTO)
        {
            if (chatLieuDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (chatLieuDTO.ten_chat_lieu == null)
                return BadRequest("Yêu cầu nhập tên chất liệu");

            // Kiểm tra trùng tên
            var existingChatLieu = await _chatLieuServices.GetAllAsync();
            if (existingChatLieu.Any(x => x.ten_chat_lieu.ToLower() == chatLieuDTO.ten_chat_lieu.ToLower()))
                return BadRequest("Tên chất liệu đã tồn tại");

            var chatLieu = new ChatLieu
            {
                id_chat_lieu = Guid.NewGuid(),
                ma_chat_lieu = TaoMaChatLieu(),
                ten_chat_lieu = chatLieuDTO.ten_chat_lieu,
                mo_ta = chatLieuDTO.mo_ta,
                trang_thai = "HoatDong",
                id_nguoi_tao = (Guid)GetIdNhanVien(),
                ngay_tao = DateTime.Now
            };
            var result = _chatLieuServices.CreateAsync(chatLieu).Result;
            if (result) return Ok("Thêm chất liệu thành công");
            return BadRequest("Đã có lỗi khi thêm chất liệu");
        }

        [HttpPut]
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Update(Guid id, SuaChatLieuAdminDTO chatLieuDTO)
        {
            if (chatLieuDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (chatLieuDTO.ten_chat_lieu == null)
                return BadRequest("Yêu cầu nhập tên chất liệu");
            if (chatLieuDTO.mo_ta == null)
                return BadRequest("Yêu cầu nhập mô tả");
            if (chatLieuDTO.trang_thai == null)
                return BadRequest("Yêu cầu nhập trạng thái");

            // Kiểm tra trùng tên với chất liệu khác
            var existingChatLieu = _chatLieuServices.GetAllAsync().Result
                .FirstOrDefault(x => x.id_chat_lieu != id && x.ten_chat_lieu.ToLower() == chatLieuDTO.ten_chat_lieu.ToLower());
            if (existingChatLieu != null)
                return BadRequest("Tên chất liệu đã tồn tại");

            var chatLieu = _chatLieuServices.GetByIdAsync(id).Result;
            if (chatLieu == null)
                return NotFound("Không tìm thấy chất liệu");

            chatLieu.ten_chat_lieu = chatLieuDTO.ten_chat_lieu;
            chatLieu.mo_ta = chatLieuDTO.mo_ta;
            chatLieu.trang_thai = chatLieuDTO.trang_thai;
            chatLieu.id_nguoi_sua = (Guid)GetIdNhanVien();
            chatLieu.ngay_sua = DateTime.Now;

            var result = _chatLieuServices.UpdateAsync(chatLieu).Result;
            if (result) return Ok("Cập nhật chất liệu thành công");
            return BadRequest("Đã có lỗi khi cập nhật chất liệu");
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var chatLieu = await _chatLieuServices.GetByIdWithIncludeAsync(id, cl => cl.Include(c => c.SanPhams));
            if (chatLieu == null)
                return NotFound("Không tìm thấy chất liệu");

            if (chatLieu.SanPhams != null && chatLieu.SanPhams.Any())
                return BadRequest("Không thể xóa chất liệu này vì đang có sản phẩm đang sử dụng");

            var result = await _chatLieuServices.DeleteAsync(id);
            if (result) return Ok("Xóa chất liệu thành công");
            return BadRequest("Đã có lỗi khi xóa chất liệu");
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveMaterials()
        {
            var allMaterials = await _chatLieuServices.GetAllWithIncludeAsync(cl => cl.Include(c => c.SanPhams));
            var activeMaterials = allMaterials.Where(c => c.trang_thai == "HoatDong").ToList();
            return Ok(activeMaterials);
        }

        private string TaoMaChatLieu()
        {
            var lastChatLieu = _chatLieuServices.GetAllAsync().Result.OrderByDescending(x => x.ma_chat_lieu).FirstOrDefault();
            if (lastChatLieu == null)
                return "CL0001";
            int startNumber = int.Parse(lastChatLieu.ma_chat_lieu.Substring(2)) + 1;
            return $"CL{startNumber:D4}";
        }
        private Guid? GetIdNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                throw new UnauthorizedAccessException("Token không hợp lệ");
            var idtnhanvien = _jwtServices.GetIdNhanVienFromToken(token);
            return idtnhanvien;
        }
    }
}
