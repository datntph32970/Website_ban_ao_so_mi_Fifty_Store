using API.DbConects.DTOs.Admin.HoaDon;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.HoaDon_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PhuongThucThanhToanController : ControllerBase
    {
        private readonly IBaseService<PhuongThucThanhToan> _phuongThucThanhToanService;
        private readonly IJwtServices _jwtServices;
        public PhuongThucThanhToanController(IBaseService<PhuongThucThanhToan> phuongThucThanhToanService, IJwtServices jwtServices)
        {
            _phuongThucThanhToanService = phuongThucThanhToanService;
            _jwtServices = jwtServices;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPhuongThucThanhToan()
        {
            var phuongThucThanhToan = await _phuongThucThanhToanService.GetAllAsync();
            return Ok(phuongThucThanhToan);
        }

        [HttpGet("lay-danh-sach-phuong-thuc-thanh-toan-hoat-dong")]
        public async Task<IActionResult> GetPhuongThucThanhToanHoatDong()
        {
            try
            {
                var phuongThucThanhToan = await _phuongThucThanhToanService.GetAllAsync();
                var phuongThucThanhToanHoatDong = phuongThucThanhToan.Where(x => x.trang_thai).ToList();
                return Ok(phuongThucThanhToanHoatDong);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("lay-danh-sach-phuong-thuc-thanh-toan-online-hoat-dong")]
        public async Task<IActionResult> GetPhuongThucThanhToanOnlineHoatDong()
        {
            try
            {
                var phuongThucThanhToan = await _phuongThucThanhToanService.GetAllAsync();
                var phuongThucThanhToanHoatDong = phuongThucThanhToan
                    .Where(x => x.trang_thai && x.ma_phuong_thuc_thanh_toan != "PTCKHOAN")
                    .ToList();
                return Ok(phuongThucThanhToanHoatDong);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetPhuongThucThanhToanById(Guid id)
        {
            var phuongThucThanhToan = await _phuongThucThanhToanService.GetByIdAsync(id);
            return Ok(phuongThucThanhToan);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePhuongThucThanhToan(ThemPhuongThucThanhToanDTO themPhuongThucThanhToanDTO)
        {
            var idNhanVien = GetIdNhanVien();
            if (idNhanVien == null)
                return Unauthorized();
            if (string.IsNullOrEmpty(themPhuongThucThanhToanDTO.ten_phuong_thuc_thanh_toan))
                return BadRequest("Tên phương thức thanh toán không được để trống");

            if (string.IsNullOrEmpty(themPhuongThucThanhToanDTO.mo_ta))
                return BadRequest("Mô tả phương thức thanh toán không được để trống");
            var maPhuongThucThanhToan = await TaoMaPhuongThucThanhToan();
            if (string.IsNullOrEmpty(themPhuongThucThanhToanDTO.ma_phuong_thuc_thanh_toan))
                themPhuongThucThanhToanDTO.ma_phuong_thuc_thanh_toan = maPhuongThucThanhToan;
            if (themPhuongThucThanhToanDTO.ma_phuong_thuc_thanh_toan.Length > 10)
                return BadRequest("Mã phương thức thanh toán không được vượt quá 10 ký tự");
            var phuongThucThanhToan = new PhuongThucThanhToan
            {
                ten_phuong_thuc_thanh_toan = themPhuongThucThanhToanDTO.ten_phuong_thuc_thanh_toan,
                ma_phuong_thuc_thanh_toan = themPhuongThucThanhToanDTO.ma_phuong_thuc_thanh_toan,
                mo_ta = themPhuongThucThanhToanDTO.mo_ta,
                trang_thai = true,
                id_nguoi_tao = idNhanVien.Value,
                ngay_tao = DateTime.Now
            };
            var result = await _phuongThucThanhToanService.CreateAsync(phuongThucThanhToan);
            if (!result)
                return BadRequest("Thêm phương thức thanh toán thất bại");
            return Ok("Thêm phương thức thanh toán thành công");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePhuongThucThanhToan(Guid id, SuaPhuongThucThanhToanDTO suaPhuongThucThanhToanDTO)
        {
            var idNhanVien = GetIdNhanVien();
            if (idNhanVien == null)
                return Unauthorized();
            if (id == Guid.Empty)
                return BadRequest("Id phương thức thanh toán không hợp lệ");
            if (string.IsNullOrEmpty(suaPhuongThucThanhToanDTO.ten_phuong_thuc_thanh_toan))
                return BadRequest("Tên phương thức thanh toán không được để trống");
            var phuongThucThanhToan = await _phuongThucThanhToanService.GetByIdAsync(id);
            if (phuongThucThanhToan == null)
                return BadRequest("Phương thức thanh toán không tồn tại");


            phuongThucThanhToan.ten_phuong_thuc_thanh_toan = suaPhuongThucThanhToanDTO.ten_phuong_thuc_thanh_toan;
            phuongThucThanhToan.mo_ta = suaPhuongThucThanhToanDTO.mo_ta;
            phuongThucThanhToan.id_nguoi_sua = idNhanVien.Value;
            phuongThucThanhToan.ngay_cap_nhat = DateTime.Now;
            var result = await _phuongThucThanhToanService.UpdateAsync(phuongThucThanhToan);
            if (!result)
                return BadRequest("Cập nhật phương thức thanh toán thất bại");
            return Ok("Cập nhật phương thức thanh toán thành công");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePhuongThucThanhToan(Guid id)
        {
            var idNhanVien = GetIdNhanVien();
            if (idNhanVien == null)
                return Unauthorized();
            if (id == Guid.Empty)
                return BadRequest("Id phương thức thanh toán không hợp lệ");
            var phuongThucThanhToan = await _phuongThucThanhToanService.DeleteAsync(id);
            return Ok(phuongThucThanhToan);
        }

        [HttpPut("cap-nhat-trang-thai/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTrangThaiPhuongThucThanhToan(Guid id)
        {
            try
            {
                var idNhanVien = GetIdNhanVien();
                if (idNhanVien == null)
                    return Unauthorized();

                if (id == Guid.Empty)
                    return BadRequest("Id phương thức thanh toán không hợp lệ");

                var phuongThucThanhToan = await _phuongThucThanhToanService.GetByIdAsync(id);
                if (phuongThucThanhToan == null)
                    return NotFound("Phương thức thanh toán không tồn tại");

                // Đảo ngược trạng thái
                phuongThucThanhToan.trang_thai = !phuongThucThanhToan.trang_thai;
                phuongThucThanhToan.id_nguoi_sua = idNhanVien.Value;
                phuongThucThanhToan.ngay_cap_nhat = DateTime.Now;

                var result = await _phuongThucThanhToanService.UpdateAsync(phuongThucThanhToan);
                if (!result)
                    return BadRequest("Cập nhật trạng thái phương thức thanh toán thất bại");

                return Ok(new
                {
                    message = "Cập nhật trạng thái phương thức thanh toán thành công",
                    trang_thai = phuongThucThanhToan.trang_thai
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }

        private Guid? GetIdNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                return null;
            return _jwtServices.GetIdNhanVienFromToken(token);
        }
        //tạo mã phương thức thanh toán 10 chữ ngẫu nhiên
        private async Task<string> TaoMaPhuongThucThanhToan()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string result;

            do
            {
                result = new string(Enumerable.Repeat(chars, 10)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
            }
            while ((await _phuongThucThanhToanService.GetAllAsync()).Any(x => x.ma_phuong_thuc_thanh_toan == result));

            return result;
        }
    }
}