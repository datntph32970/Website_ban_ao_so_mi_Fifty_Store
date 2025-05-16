using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Services.Interfaces;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Services.JwtServices;
using API.DbConects.DTOs.Client.TaiKhoan;

namespace API.Controllers.TaiKhoan_Controller
{
    [Route("api/khach-hang/dia-chi")]
    [ApiController]
    [Authorize(Roles = "KhachHang")]
    public class DiaChiKhachHangController : ControllerBase
    {
        private readonly IBaseService<KhachHang> _khachHangServices;
        private readonly IJwtServices _jwtServices;
        private readonly IDiaChiService _diaChiService;

        public DiaChiKhachHangController(
            IBaseService<KhachHang> khachHangServices,
            IJwtServices jwtServices,
            IDiaChiService diaChiService)
        {
            _khachHangServices = khachHangServices;
            _jwtServices = jwtServices;
            _diaChiService = diaChiService;
        }

        private async Task<Guid> GetCurrentUserId()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                throw new UnauthorizedAccessException("Token không hợp lệ");

            var taiKhoanId = _jwtServices.GetUserIdFromToken(token);
            if (taiKhoanId == null)
                throw new UnauthorizedAccessException("Không tìm thấy thông tin tài khoản");

            var khachHang = await _khachHangServices.GetFirstOrDefaultAsync(x => x.id_tai_khoan == taiKhoanId);
            if (khachHang == null)
                throw new UnauthorizedAccessException("Không tìm thấy thông tin khách hàng");

            return khachHang.id_khach_hang;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            try
            {
                var userId = await GetCurrentUserId();
                var addresses = await _diaChiService.GetDiaChiByKhachHangAsync(userId);
                return Ok(new
                {
                    message = "Lấy danh sách địa chỉ thành công",
                    addresses = addresses
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi lấy danh sách địa chỉ", error = ex.Message });
            }
        }

        [HttpGet("mac-dinh")]
        public async Task<IActionResult> GetDefaultAddress()
        {
            try
            {
                var userId = await GetCurrentUserId();
                var address = await _diaChiService.GetDiaChiMacDinhAsync(userId);

                if (address == null)
                    return NotFound(new { message = "Không tìm thấy địa chỉ mặc định" });

                return Ok(new
                {
                    message = "Lấy địa chỉ mặc định thành công",
                    address = address
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi lấy địa chỉ mặc định", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] CreateDiaChiDTO createDto)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var (success, message) = await _diaChiService.CreateDiaChiAsync(userId, createDto);

                if (success)
                {
                    var addresses = await _diaChiService.GetDiaChiByKhachHangAsync(userId);
                    return Ok(new
                    {
                        message = message,
                        addresses = addresses
                    });
                }
                return BadRequest(new { message = message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi thêm địa chỉ", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateDiaChiDTO updateDto)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var (success, message) = await _diaChiService.UpdateDiaChiAsync(id, userId, updateDto);

                if (success)
                {
                    var addresses = await _diaChiService.GetDiaChiByKhachHangAsync(userId);
                    return Ok(new
                    {
                        message = message,
                        addresses = addresses
                    });
                }
                return BadRequest(new { message = message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi cập nhật địa chỉ", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var (success, message) = await _diaChiService.DeleteDiaChiAsync(id, userId);

                if (success)
                {
                    var addresses = await _diaChiService.GetDiaChiByKhachHangAsync(userId);
                    return Ok(new
                    {
                        message = message,
                        addresses = addresses
                    });
                }
                return BadRequest(new { message = message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi xóa địa chỉ", error = ex.Message });
            }
        }

        [HttpPut("{id}/mac-dinh")]
        public async Task<IActionResult> SetDefaultAddress(Guid id)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var (success, message) = await _diaChiService.SetDiaChiMacDinhAsync(id, userId);

                if (success)
                {
                    var addresses = await _diaChiService.GetDiaChiByKhachHangAsync(userId);
                    return Ok(new
                    {
                        message = message,
                        addresses = addresses
                    });
                }
                return BadRequest(new { message = message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi đặt địa chỉ mặc định", error = ex.Message });
            }
        }
    }
}