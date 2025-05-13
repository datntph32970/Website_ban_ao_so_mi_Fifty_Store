using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_San_Pham;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Controllers.SanPham_Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThuongHieuController : ControllerBase
    {
        private readonly IBaseService<ThuongHieu> _thuongHieuService;
        private readonly IJwtServices _jwtService;

        public ThuongHieuController(IBaseService<ThuongHieu> thuongHieuService, IJwtServices jwtService)
        {
            _thuongHieuService = thuongHieuService;
            _jwtService = jwtService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _thuongHieuService.GetAllWithIncludeAsync(cl => cl.Include(c => c.SanPhams));
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> ThemThuongHieu(ThemThuongHieuAdminDTO thuongHieuDTO)
        {
            if (thuongHieuDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (thuongHieuDTO.ten_thuong_hieu == null)
                return BadRequest("Yêu cầu nhập tên thương hiệu");
            if (thuongHieuDTO.mo_ta == null)
                return BadRequest("Yêu cầu nhập mô tả");

            // Kiểm tra trùng tên
            var existingThuongHieu = await _thuongHieuService.GetAllAsync();
            if (existingThuongHieu.Any(x => x.ten_thuong_hieu.ToLower() == thuongHieuDTO.ten_thuong_hieu.ToLower()))
                return BadRequest("Tên thương hiệu đã tồn tại");

            var thuongHieu = new ThuongHieu
            {
                id_thuong_hieu = Guid.NewGuid(),
                ma_thuong_hieu = await TaoMaThuongHieu(),
                ten_thuong_hieu = thuongHieuDTO.ten_thuong_hieu,
                mo_ta = thuongHieuDTO.mo_ta,
                trang_thai = "HoatDong",
                id_nguoi_tao = (Guid)GetIdNhanVien(),
                ngay_tao = DateTime.Now
            };

            var result = await _thuongHieuService.CreateAsync(thuongHieu);
            if (result) return Ok("Thêm thương hiệu thành công");
            return BadRequest("Đã có lỗi khi thêm thương hiệu");
        }
        [HttpPut]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> CapNhatThuongHieu(Guid id, SuaThuongHieuAdminDTO thuongHieuDTO)
        {
            if (thuongHieuDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (thuongHieuDTO.ten_thuong_hieu == null)
                return BadRequest("Yêu cầu nhập tên thương hiệu");
            if (thuongHieuDTO.mo_ta == null)
                return BadRequest("Yêu cầu nhập mô tả");

            // Kiểm tra trùng tên với thương hiệu khác
            var existingThuongHieu = await _thuongHieuService.GetAllAsync();
            if (existingThuongHieu.Any(x => x.id_thuong_hieu != id && x.ten_thuong_hieu.ToLower() == thuongHieuDTO.ten_thuong_hieu.ToLower()))
                return BadRequest("Tên thương hiệu đã tồn tại");

            var thuongHieu = await _thuongHieuService.GetByIdAsync(id);
            if (thuongHieu == null)
                return NotFound("Không tìm thấy thương hiệu");

            thuongHieu.ten_thuong_hieu = thuongHieuDTO.ten_thuong_hieu;
            thuongHieu.mo_ta = thuongHieuDTO.mo_ta;
            thuongHieu.trang_thai = thuongHieuDTO.trang_thai;
            thuongHieu.id_nguoi_sua = (Guid)GetIdNhanVien();
            thuongHieu.ngay_sua = DateTime.Now;

            var result = await _thuongHieuService.UpdateAsync(thuongHieu);
            if (result) return Ok("Cập nhật thương hiệu thành công");
            return BadRequest("Đã có lỗi khi cập nhật thương hiệu");
        }
        [HttpDelete]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> XoaThuongHieu(Guid id)
        {
            var result = await _thuongHieuService.DeleteAsync(id);
            if (!result)
                return BadRequest("Đã có lỗi khi xóa Thương hiệu");
            return Ok("Xóa Thương hiệu thành công");
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> LayThuongHieuTheoId(Guid id)
        {
            var result = await _thuongHieuService.GetByIdWithIncludeAsync(id, cl => cl.Include(c => c.SanPhams));
            return Ok(result);
        }
        private async Task<string> TaoMaThuongHieu()
        {
            var lastThuongHieu = await _thuongHieuService.GetAllAsync();
            if (lastThuongHieu == null || !lastThuongHieu.Any())
                return "TH0001";
            int startNumber = int.Parse(lastThuongHieu.OrderByDescending(x => x.ma_thuong_hieu).First().ma_thuong_hieu.Substring(2)) + 1;
            return $"TH{startNumber:D4}";
        }
        private Guid? GetIdNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                throw new UnauthorizedAccessException("Token không hợp lệ");
            var idtnhanvien = _jwtService.GetIdNhanVienFromToken(token);
            return idtnhanvien;
        }
    }
}
