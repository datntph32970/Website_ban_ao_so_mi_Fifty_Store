
using API.DbConects.Entities.Entities_Hoa_Don;
using API.Services.JwtServices;
using API.Services.HoaDon_Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using API.DbConects.DTO.HoaDonDTO;

namespace API.Controllers.HoaDon_Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class HoaDonController : ControllerBase
    {
        private readonly IHoaDonServices _hoaDonService;
        private readonly IJwtServices _jwtService;

        public HoaDonController(IHoaDonServices hoaDonService, IJwtServices jwtService)
        {
            _hoaDonService = hoaDonService;
            _jwtService = jwtService;
        }

        [HttpGet("danh-sach-hoa-don")]
        public async Task<IActionResult> DanhSachHoaDon()
        {
            var hoaDons = await _hoaDonService.GetHoaDonAsync();
            return Ok(hoaDons);
        }


        [HttpGet("chi-tiet-hoa-don/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _hoaDonService.GetHoaDonByIdAsync(id);
            if (result == null)
                return NotFound("Hóa đơn không tồn tại");
            return Ok(result);
        }

        [HttpPost("them-hoa-don")]
        [Authorize(Roles = "NhanVien")]
        public async Task<IActionResult> ThemHoaDon(Them_HoaDonDTO hoaDonDTO)
        {
            if (hoaDonDTO == null)
                return BadRequest("Dữ liệu hóa đơn không hợp lệ");

            var result = await _hoaDonService.Add(hoaDonDTO, User.Identity.Name);
            if (!result.Item1)
                return BadRequest(result.Item2);

            return Ok("Hóa đơn đã được thêm thành công");
        }

        [HttpPut("cap-nhat-hoa-don/{id}")]
        [Authorize(Roles = "NhanVien")]
        public async Task<IActionResult> SuaHoaDon(int id, Sua_HoaDonDTO hoaDonDTO)
        {
            var result = await _hoaDonService.Update(hoaDonDTO, User.Identity.Name);
            if (!result.Item1)
                return BadRequest(result.Item2);

            return Ok("Hóa đơn đã được cập nhật thành công");
        }


        [HttpDelete("xoa-hoa-don/{id}")]
        [Authorize(Roles = "QuanLy")]
        public async Task<IActionResult> XoaHoaDon(Guid id)
        {
            var result = await _hoaDonService.Delete(id);
            if (!result)
                return BadRequest("Lỗi khi xóa hóa đơn hoặc hóa đơn không tồn tại");

            return Ok("Hóa đơn đã được xóa thành công");
        }

    }
}
