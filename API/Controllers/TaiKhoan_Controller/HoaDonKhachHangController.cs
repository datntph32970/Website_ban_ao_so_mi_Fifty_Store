using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Services.Interfaces;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.Services.JwtServices;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.TaiKhoan_Controller
{
    [Route("api/khach-hang/hoa-don")]
    [ApiController]
    [Authorize(Roles = "KhachHang")]
    public class HoaDonKhachHangController : ControllerBase
    {
        private readonly IBaseService<KhachHang> _khachHangServices;
        private readonly IJwtServices _jwtServices;
        private readonly IHoaDonService _hoaDonService;

        public HoaDonKhachHangController(
            IBaseService<KhachHang> khachHangServices,
            IJwtServices jwtServices,
            IHoaDonService hoaDonService)
        {
            _khachHangServices = khachHangServices;
            _jwtServices = jwtServices;
            _hoaDonService = hoaDonService;
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
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var userId = await GetCurrentUserId();
                var allOrders = await _hoaDonService.GetAllHoaDonAdminDTOAsync();
                var myOrders = allOrders.Where(h => h.id_khach_hang == userId).ToList();

                return Ok(new
                {
                    message = "Lấy danh sách hóa đơn thành công",
                    orders = myOrders
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi lấy danh sách hóa đơn", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetail(Guid id)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var order = await _hoaDonService.GetByIdHoaDonAdminDTOAsync(id);

                if (order == null || order.id_khach_hang != userId)
                    return NotFound(new { message = "Không tìm thấy hóa đơn" });

                return Ok(new
                {
                    message = "Lấy chi tiết hóa đơn thành công",
                    order = order
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi lấy chi tiết hóa đơn", error = ex.Message });
            }
        }
    }
}