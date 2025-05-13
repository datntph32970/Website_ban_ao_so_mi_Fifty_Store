using API.DbConects.Entities.Entities_Hoa_Don;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GioHangController : ControllerBase
    {
        private readonly IGioHangService _gioHangService;
        private readonly IJwtServices _jwtServices;

        public GioHangController(IGioHangService gioHangService, IJwtServices jwtServices)
        {
            _gioHangService = gioHangService;
            _jwtServices = jwtServices;
        }

        [HttpGet("get-gio-hang")]
        public async Task<IActionResult> GetGioHang(Guid idKhachHang)
        {
            var gioHang = await _gioHangService.GetGioHangByKhachHangAsync(idKhachHang);
            return Ok(gioHang);
        }

        [HttpPost("them-san-pham")]
        public async Task<IActionResult> ThemSanPham(Guid idKhachHang, Guid idSanPhamChiTiet, int soLuong)
        {
            var result = await _gioHangService.ThemSanPhamVaoGioHangAsync(idKhachHang, idSanPhamChiTiet, soLuong);
            if (result)
            {
                return Ok("Thêm sản phẩm vào giỏ hàng thành công");
            }
            return BadRequest("Không thể thêm sản phẩm vào giỏ hàng");
        }

        [HttpPut("cap-nhat-so-luong")]
        public async Task<IActionResult> CapNhatSoLuong(Guid idGioHangChiTiet, int soLuong)
        {
            var result = await _gioHangService.CapNhatSoLuongAsync(idGioHangChiTiet, soLuong);
            if (result)
            {
                return Ok("Cập nhật số lượng thành công");
            }
            return BadRequest("Không thể cập nhật số lượng");
        }

        [HttpDelete("xoa-san-pham")]
        public async Task<IActionResult> XoaSanPham(Guid idGioHangChiTiet)
        {
            var result = await _gioHangService.XoaSanPhamKhoiGioHangAsync(idGioHangChiTiet);
            if (result)
            {
                return Ok("Xóa sản phẩm khỏi giỏ hàng thành công");
            }
            return BadRequest("Không thể xóa sản phẩm khỏi giỏ hàng");
        }

        [HttpDelete("xoa-gio-hang")]
        public async Task<IActionResult> XoaGioHang(Guid idKhachHang)
        {
            var result = await _gioHangService.XoaGioHangAsync(idKhachHang);
            if (result)
            {
                return Ok("Xóa giỏ hàng thành công");
            }
            return BadRequest("Không thể xóa giỏ hàng");
        }
    }
}