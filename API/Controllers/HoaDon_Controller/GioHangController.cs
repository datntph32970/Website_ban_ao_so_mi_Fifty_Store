using API.DbConects.DTOs.Client.HoaDon;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.HoaDon_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "KhachHang")]
    public class GioHangController : ControllerBase
    {
        private readonly IGioHangService _gioHangService;
        private readonly IJwtServices _jwtServices;
        private readonly IBaseService<KhachHang> _khachHangService;

        public GioHangController(
            IGioHangService gioHangService,
            IJwtServices jwtServices,
            IBaseService<KhachHang> khachHangService)
        {
            _gioHangService = gioHangService;
            _jwtServices = jwtServices;
            _khachHangService = khachHangService;
        }

        private async Task<Guid> GetCurrentUserId()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                throw new UnauthorizedAccessException("Token không hợp lệ");

            var taiKhoanId = _jwtServices.GetUserIdFromToken(token);
            if (taiKhoanId == null)
                throw new UnauthorizedAccessException("Không tìm thấy thông tin tài khoản");

            var khachHang = await _khachHangService.GetFirstOrDefaultAsync(x => x.id_tai_khoan == taiKhoanId);
            if (khachHang == null)
                throw new UnauthorizedAccessException("Không tìm thấy thông tin khách hàng");

            return khachHang.id_khach_hang;
        }

        private decimal CalculateCartTotal(IEnumerable<GioHangItemClientDTO> cartItems)
        {
            return cartItems.Sum(x => x.so_luong * (x.gia_sau_giam ?? x.gia_ban));
        }

        [HttpGet("my-cart")]
        public async Task<IActionResult> GetMyCart()
        {
            try
            {
                var userId = await GetCurrentUserId();
                var gioHang = await _gioHangService.GetGioHangByKhachHangAsync(userId);
                if (gioHang == null || !gioHang.Any())
                    return Ok(new { message = "Giỏ hàng trống", items = new List<object>(), totalItems = 0, totalAmount = 0 });

                return Ok(new
                {
                    message = "Lấy giỏ hàng thành công",
                    items = gioHang,
                    totalItems = gioHang.Count(),
                    totalAmount = CalculateCartTotal(gioHang)
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi lấy giỏ hàng", error = ex.Message });
            }
        }

        [HttpPost("add-item")]
        public async Task<IActionResult> AddToCart(Guid idSanPhamChiTiet, int soLuong)
        {
            try
            {
                if (soLuong <= 0)
                    return BadRequest(new { message = "Số lượng phải lớn hơn 0" });

                var userId = await GetCurrentUserId();
                var (success, message) = await _gioHangService.ThemSanPhamVaoGioHangAsync(userId, idSanPhamChiTiet, soLuong);

                if (success)
                {
                    var updatedCart = await _gioHangService.GetGioHangByKhachHangAsync(userId);
                    return Ok(new
                    {
                        message,
                        items = updatedCart,
                        totalItems = updatedCart.Count(),
                        totalAmount = CalculateCartTotal(updatedCart)
                    });
                }
                return BadRequest(new { message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi thêm vào giỏ hàng", error = ex.Message });
            }
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity(Guid idGioHangChiTiet, int soLuong)
        {
            try
            {
                if (soLuong < 0)
                    return BadRequest(new { message = "Số lượng không hợp lệ" });

                var userId = await GetCurrentUserId();
                var (success, message) = await _gioHangService.CapNhatSoLuongAsync(idGioHangChiTiet, soLuong);

                if (success)
                {
                    var updatedCart = await _gioHangService.GetGioHangByKhachHangAsync(userId);
                    return Ok(new
                    {
                        message,
                        items = updatedCart,
                        totalItems = updatedCart.Count(),
                        totalAmount = CalculateCartTotal(updatedCart)
                    });
                }
                return BadRequest(new { message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi cập nhật số lượng", error = ex.Message });
            }
        }

        [HttpDelete("remove-item/{idGioHangChiTiet}")]
        public async Task<IActionResult> RemoveFromCart(Guid idGioHangChiTiet)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var (success, message) = await _gioHangService.XoaSanPhamKhoiGioHangAsync(idGioHangChiTiet);

                if (success)
                {
                    var updatedCart = await _gioHangService.GetGioHangByKhachHangAsync(userId);
                    return Ok(new
                    {
                        message,
                        items = updatedCart ?? new List<GioHangItemClientDTO>(),
                        totalItems = updatedCart?.Count() ?? 0,
                        totalAmount = updatedCart != null ? CalculateCartTotal(updatedCart) : 0
                    });
                }
                return BadRequest(new { message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi xóa sản phẩm", error = ex.Message });
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = await GetCurrentUserId();
                var (success, message) = await _gioHangService.XoaGioHangAsync(userId);

                if (success)
                {
                    return Ok(new
                    {
                        message,
                        items = new List<GioHangItemClientDTO>(),
                        totalItems = 0,
                        totalAmount = 0
                    });
                }
                return BadRequest(new { message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi xóa giỏ hàng", error = ex.Message });
            }
        }

        [HttpGet("check-quantity/{idSanPhamChiTiet}")]
        public async Task<IActionResult> CheckProductQuantity(Guid idSanPhamChiTiet)
        {
            try
            {
                var (success, message, soLuong) = await _gioHangService.KiemTraSoLuongTonAsync(idSanPhamChiTiet);
                if (success)
                    return Ok(new { message, availableQuantity = soLuong });
                return BadRequest(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi kiểm tra số lượng tồn", error = ex.Message });
            }
        }

        [HttpGet("selected-items")]
        public async Task<IActionResult> GetSelectedItems()
        {
            try
            {
                var userId = await GetCurrentUserId();
                var gioHang = await _gioHangService.GetGioHangDaChonAsync(userId);
                if (gioHang == null || !gioHang.Any())
                    return Ok(new { message = "Chưa có sản phẩm nào được chọn", items = new List<GioHangItemClientDTO>(), totalItems = 0, totalAmount = 0 });

                return Ok(new
                {
                    message = "Lấy danh sách sản phẩm đã chọn thành công",
                    items = gioHang,
                    totalItems = gioHang.Count(),
                    totalAmount = CalculateCartTotal(gioHang)
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi lấy danh sách sản phẩm đã chọn", error = ex.Message });
            }
        }

        [HttpPut("update-status/{idGioHangChiTiet}")]
        public async Task<IActionResult> UpdateCartItemStatus(Guid idGioHangChiTiet, [FromQuery] bool trangThai)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var (success, message) = await _gioHangService.CapNhatTrangThaiGioHangAsync(idGioHangChiTiet, trangThai);

                if (success)
                {
                    var updatedCart = await _gioHangService.GetGioHangByKhachHangAsync(userId);
                    return Ok(new
                    {
                        message,
                        items = updatedCart,
                        totalItems = updatedCart.Count(),
                        totalAmount = CalculateCartTotal(updatedCart)
                    });
                }
                return BadRequest(new { message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi cập nhật trạng thái", error = ex.Message });
            }
        }
    }
}