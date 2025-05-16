using API.DbConects.DTOs.Admin.HoaDon;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_San_Pham;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace API.Controllers.HoaDon_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CuaHangController : ControllerBase
    {
        private readonly ICuaHangService _cuaHangService;
        private readonly IJwtServices _jwtServices;
        private readonly IBaseService<HinhAnh> _hinhAnhServices;
        public CuaHangController(ICuaHangService cuaHangService, IJwtServices jwtServices, IBaseService<HinhAnh> hinhAnhServices)
        {
            _cuaHangService = cuaHangService;
            _jwtServices = jwtServices;
            _hinhAnhServices = hinhAnhServices;
        }

        [HttpGet("get-thong-tin-cua-hang")]
        public async Task<IActionResult> GetThongTinCuaHang()
        {
            var cuaHang = await _cuaHangService.GetCuaHangFirstOrDefaultAsync();
            if (cuaHang == null)
                return NotFound("Cửa hàng không tồn tại");
            string? hinhAnhUrl = null;
            if (cuaHang.id_hinh_anh != null)
            {
            var hinhAnh = await _hinhAnhServices.GetByIdAsync(cuaHang.id_hinh_anh.Value);
                hinhAnhUrl = hinhAnh?.url;
            }

            var cuahangdto = new CuaHangDTO
            {
                id_cua_hang = cuaHang.id_cua_hang,
                ten_cua_hang = cuaHang.ten_cua_hang,
                dia_chi = cuaHang.dia_chi,
                sdt = cuaHang.sdt,
                email = cuaHang.email,
                website = cuaHang.website,
                mo_ta = cuaHang.mo_ta,
                hinh_anh_url = hinhAnhUrl
            };
            return Ok(cuahangdto);
        }

        [HttpPut("update-thong-tin-cua-hang")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateThongTinCuaHang(CuaHang cuaHangUpdate)
        {
            var idNhanVien = GetIdNhanVien();
            if (idNhanVien == null)
                return Unauthorized();
            cuaHangUpdate.id_nguoi_sua = idNhanVien.Value;
            var cuaHang = await _cuaHangService.UpdateCuaHangFirstOrDefaultAsync(cuaHangUpdate);
            return Ok(cuaHang);
        }

        private Guid? GetIdNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                return null;
            return _jwtServices.GetIdNhanVienFromToken(token);
        }

        [HttpPost("upload-logo")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadLogo([FromBody] string base64Image)
        {
            try
            {
                if (string.IsNullOrEmpty(base64Image))
                    return BadRequest("Không có dữ liệu hình ảnh");

                // Validate base64 image
                if (!base64Image.StartsWith("data:image"))
                    return BadRequest("Dữ liệu không phải là hình ảnh hợp lệ");

                var cuaHang = await _cuaHangService.GetCuaHangFirstOrDefaultAsync();
                if (cuaHang == null)
                    return BadRequest("Cửa hàng không tồn tại");

                var idNhanVien = GetIdNhanVien();
                if (idNhanVien == null)
                    return Unauthorized();

                // Thực hiện trong transaction để đảm bảo tính nhất quán
                var result = await _cuaHangService.ExecuteInTransactionAsync(async () =>
                {
                    try
                    {
                        // Xóa hình ảnh cũ nếu có
                        if (cuaHang.id_hinh_anh.HasValue)
                        {
                            var existingHinhAnh = await _hinhAnhServices.GetByIdAsync(cuaHang.id_hinh_anh.Value);
                            if (existingHinhAnh != null)
                            {
                                // Xóa file cũ
                                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingHinhAnh.url.TrimStart('/'));
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    System.IO.File.Delete(oldFilePath);
                                }
                                await _hinhAnhServices.DeleteAsync(existingHinhAnh.id_hinh_anh);
                            }
                        }

                        // Tạo hình ảnh mới
                        var hinhAnh = new HinhAnh
                        {
                            id_hinh_anh = Guid.NewGuid(),
                            ma_hinh_anh = $"logo_{DateTime.Now:yyyyMMddHHmmss}",
                            id_nguoi_tao = idNhanVien.Value,
                            ngay_tao = DateTime.Now
                        };

                        // Lưu file
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo");
                        Directory.CreateDirectory(folderPath); // Tạo thư mục nếu chưa tồn tại

                        var fileName = $"{hinhAnh.ma_hinh_anh}.jpg";
                        var imagePath = Path.Combine(folderPath, fileName);

                        // Xử lý base64 image
                        var base64Data = base64Image.Split(',').Last();
                        var imageBytes = Convert.FromBase64String(base64Data);

                        // Validate file size (ví dụ: max 5MB)
                        if (imageBytes.Length > 5 * 1024 * 1024)
                            return false;

                        await System.IO.File.WriteAllBytesAsync(imagePath, imageBytes);
                        hinhAnh.url = $"/images/logo/{fileName}";

                        // Lưu thông tin hình ảnh vào database
                        var hinhAnhResult = await _hinhAnhServices.CreateAsync(hinhAnh);
                        if (!hinhAnhResult)
                            return false;

                        // Cập nhật thông tin cửa hàng
                        cuaHang.id_hinh_anh = hinhAnh.id_hinh_anh;
                        cuaHang.id_nguoi_sua = idNhanVien.Value;
                        var updateResult = await _cuaHangService.UpdateAsync(cuaHang);
                        if (!updateResult)
                            return false;

                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                });

                if (!result)
                    return BadRequest("Lỗi khi cập nhật logo");

                return Ok(new
                {
                    message = "Cập nhật logo thành công",
                    url = cuaHang.id_hinh_anh != null ?
                    (await _hinhAnhServices.GetByIdAsync(cuaHang.id_hinh_anh.Value))?.url : null
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }
    }
}