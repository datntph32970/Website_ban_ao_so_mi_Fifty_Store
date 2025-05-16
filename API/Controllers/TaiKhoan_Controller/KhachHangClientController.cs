using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Services.Interfaces;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Services.JwtServices;
using Microsoft.EntityFrameworkCore;
using API.DbConects.DTOs.Client.TaiKhoan;

namespace API.Controllers.TaiKhoan_Controller
{
    [Route("api/khach-hang")]
    [ApiController]
    [Authorize(Roles = "KhachHang")]
    public class KhachHangClientController : ControllerBase
    {
        private readonly IBaseService<KhachHang> _khachHangServices;
        private readonly IBaseService<TaiKhoan> _taikhoanServices;
        private readonly IJwtServices _jwtServices;

        public KhachHangClientController(
            IBaseService<KhachHang> khachHangServices,
            IBaseService<TaiKhoan> taikhoanServices,
            IJwtServices jwtServices)
        {
            _khachHangServices = khachHangServices;
            _taikhoanServices = taikhoanServices;
            _jwtServices = jwtServices;
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

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // Get user ID from token
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (token == null)
                    return Unauthorized(new { message = "Token không hợp lệ" });

                var taiKhoanId = _jwtServices.GetUserIdFromToken(token);
                if (taiKhoanId == null)
                    return Unauthorized(new { message = "Không tìm thấy thông tin tài khoản" });

                // Get customer information
                var khachHang = (await _khachHangServices.GetByConditionWithIncludeAsync(
                    x => x.id_tai_khoan == taiKhoanId,
                    q => q.Include(kh => kh.TaiKhoan)
                )).FirstOrDefault();

                if (khachHang == null)
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });

                // Create response object
                var profile = new
                {
                    id_khach_hang = khachHang.id_khach_hang,
                    ma_khach_hang = khachHang.ma_khach_hang,
                    ten_khach_hang = khachHang.ten_khach_hang,
                    email = khachHang.email,
                    so_dien_thoai = khachHang.so_dien_thoai,
                    ngay_sinh = khachHang.ngay_sinh,
                    gioi_tinh = khachHang.gioi_tinh,
                    trang_thai = khachHang.trang_thai,
                    ngay_tao = khachHang.ngay_tao,
                    tai_khoan = new
                    {
                        id_tai_khoan = khachHang.TaiKhoan.id_tai_khoan,
                        ten_dang_nhap = khachHang.TaiKhoan.ten_dang_nhap,
                        trang_thai = khachHang.TaiKhoan.trang_thai
                    }
                };

                return Ok(new
                {
                    message = "Lấy thông tin profile thành công",
                    profile = profile
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi lấy thông tin profile", error = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateKhachHangDTO updateDto)
        {
            try
            {
                var userId = await GetCurrentUserId();
                var khachHang = await _khachHangServices.GetByIdWithIncludeAsync(userId,
                    q => q.Include(kh => kh.TaiKhoan));

                if (khachHang == null)
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });

                // Kiểm tra trùng lặp email và số điện thoại
                if (!string.IsNullOrEmpty(updateDto.email) || !string.IsNullOrEmpty(updateDto.so_dien_thoai))
                {
                    var existingKhachHang = await _khachHangServices.GetByConditionAsync(kh =>
                        kh.id_khach_hang != userId &&
                        ((!string.IsNullOrEmpty(updateDto.email) && kh.email == updateDto.email) ||
                         (!string.IsNullOrEmpty(updateDto.so_dien_thoai) && kh.so_dien_thoai == updateDto.so_dien_thoai)));

                    if (existingKhachHang.Any())
                    {
                        var duplicate = existingKhachHang.First();
                        if (duplicate.email == updateDto.email)
                            return BadRequest(new { message = "Email đã được sử dụng bởi tài khoản khác" });
                        if (duplicate.so_dien_thoai == updateDto.so_dien_thoai)
                            return BadRequest(new { message = "Số điện thoại đã được sử dụng bởi tài khoản khác" });
                    }
                }

                // Cập nhật thông tin
                if (!string.IsNullOrEmpty(updateDto.ten_khach_hang))
                    khachHang.ten_khach_hang = updateDto.ten_khach_hang;
                if (!string.IsNullOrEmpty(updateDto.email))
                    khachHang.email = updateDto.email;
                if (!string.IsNullOrEmpty(updateDto.so_dien_thoai))
                    khachHang.so_dien_thoai = updateDto.so_dien_thoai;
                if (!string.IsNullOrEmpty(updateDto.ngay_sinh))
                    khachHang.ngay_sinh = DateOnly.Parse(updateDto.ngay_sinh);
                if (!string.IsNullOrEmpty(updateDto.gioi_tinh))
                    khachHang.gioi_tinh = updateDto.gioi_tinh;

                var result = await _khachHangServices.UpdateAsync(khachHang);
                if (!result)
                    return BadRequest(new { message = "Cập nhật thông tin thất bại" });

                // Lấy thông tin mới nhất
                khachHang = await _khachHangServices.GetByIdWithIncludeAsync(userId,
                    q => q.Include(kh => kh.TaiKhoan));

                var profile = new
                {
                    id_khach_hang = khachHang.id_khach_hang,
                    ma_khach_hang = khachHang.ma_khach_hang,
                    ten_khach_hang = khachHang.ten_khach_hang,
                    email = khachHang.email,
                    so_dien_thoai = khachHang.so_dien_thoai,
                    ngay_sinh = khachHang.ngay_sinh,
                    gioi_tinh = khachHang.gioi_tinh,
                    trang_thai = khachHang.trang_thai,
                    ngay_tao = khachHang.ngay_tao,
                    tai_khoan = new
                    {
                        id_tai_khoan = khachHang.TaiKhoan.id_tai_khoan,
                        ten_dang_nhap = khachHang.TaiKhoan.ten_dang_nhap,
                        trang_thai = khachHang.TaiKhoan.trang_thai
                    }
                };

                return Ok(new
                {
                    message = "Cập nhật thông tin thành công",
                    profile = profile
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi cập nhật thông tin", error = ex.Message });
            }
        }
    }
}