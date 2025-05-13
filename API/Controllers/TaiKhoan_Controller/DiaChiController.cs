using API.DbConects.DTOs.Admin.TaiKhoan;
using API.DbConects.DTOs.Client.TaiKhoan;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.TaiKhoan_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DiaChiController : ControllerBase
    {
        private readonly IBaseService<DiaChi> _diaChiService;
        private readonly IBaseService<KhachHang> _khachHangService;
        private readonly IJwtServices _jwtService;

        public DiaChiController(
            IBaseService<DiaChi> diaChiService,
            IBaseService<KhachHang> khachHangService,
            IJwtServices jwtService)
        {
            _diaChiService = diaChiService;
            _khachHangService = khachHangService;
            _jwtService = jwtService;
        }

        [HttpGet("get-all-dia-chi")]
        public async Task<IActionResult> GetAllDiaChi()
        {
            var diaChis = await _diaChiService.GetAllWithIncludeAsync(
                q => q.Include(dc => dc.KhachHang)
            );
            return Ok(diaChis);
        }

        [HttpGet("get-dia-chi-by-id/{id}")]
        public async Task<IActionResult> GetDiaChiById(Guid id)
        {
            var diaChi = await _diaChiService.GetByIdWithIncludeAsync(id,
                q => q.Include(dc => dc.KhachHang)
            );

            if (diaChi == null)
                return NotFound("Không tìm thấy địa chỉ");

            return Ok(diaChi);
        }

        [HttpGet("get-dia-chi-by-khach-hang/{khachHangId}")]
        public async Task<IActionResult> GetDiaChiByKhachHang(Guid khachHangId)
        {
            var khachHang = await _khachHangService.GetByIdAsync(khachHangId);
            if (khachHang == null)
                return NotFound("Không tìm thấy khách hàng");

            var diaChis = await _diaChiService.GetAllWithIncludeAsync(
                q => q.Where(dc => dc.id_khach_hang == khachHangId)
                    .Include(dc => dc.KhachHang)
            );
            return Ok(diaChis);
        }

        [HttpPost("create-dia-chi")]
        public async Task<IActionResult> CreateDiaChi(ThemDiaChiClientDTO themDiaChiAdminDTO)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(themDiaChiAdminDTO.tinh))
                return BadRequest("Tỉnh không được để trống");

            if (string.IsNullOrEmpty(themDiaChiAdminDTO.huyen))
                return BadRequest("Huyện không được để trống");

            if (string.IsNullOrEmpty(themDiaChiAdminDTO.xa))
                return BadRequest("Xã không được để trống");

            if (themDiaChiAdminDTO.dia_chi_mac_dinh == null)
                themDiaChiAdminDTO.dia_chi_mac_dinh = false;

            // Check if customer exists
            var khachHang = await _khachHangService.GetByIdAsync(Guid.Parse(GetIDKhachHang()));
            if (khachHang == null)
                return NotFound("Không tìm thấy khách hàng");

            // Create DiaChi
            var diaChi = new DiaChi
            {
                id_dia_chi = Guid.NewGuid(),
                id_khach_hang = Guid.Parse(GetIDKhachHang()),
                tinh = themDiaChiAdminDTO.tinh,
                huyen = themDiaChiAdminDTO.huyen,
                xa = themDiaChiAdminDTO.xa,
                dia_chi_mac_dinh = themDiaChiAdminDTO.dia_chi_mac_dinh,
                ngay_tao = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            };

            var result = await _diaChiService.CreateAsync(diaChi);
            if (!result)
                return BadRequest("Lỗi khi tạo địa chỉ");

            return Ok("Tạo địa chỉ thành công");
        }

        [HttpPut("update-dia-chi/{id}")]
        public async Task<IActionResult> UpdateDiaChi(Guid id, SuaDiaChiClientDTO suaDiaChiClientDTO)
        {
            var diaChi = await _diaChiService.GetByIdAsync(id);
            if (diaChi == null)
                return NotFound("Không tìm thấy địa chỉ");

            // Validate required fields
            if (string.IsNullOrEmpty(suaDiaChiClientDTO.tinh))
                return BadRequest("Tỉnh không được để trống");

            if (string.IsNullOrEmpty(suaDiaChiClientDTO.huyen))
                return BadRequest("Huyện không được để trống");

            if (string.IsNullOrEmpty(suaDiaChiClientDTO.xa))
                return BadRequest("Xã không được để trống");

            // Check if customer exists
            var khachHang = await _khachHangService.GetByIdAsync(Guid.Parse(GetIDKhachHang()));
            if (khachHang == null)
                return NotFound("Không tìm thấy khách hàng");

            // Update DiaChi
            diaChi.id_khach_hang = Guid.Parse(GetIDKhachHang());
            diaChi.tinh = suaDiaChiClientDTO.tinh;
            diaChi.huyen = suaDiaChiClientDTO.huyen;
            diaChi.xa = suaDiaChiClientDTO.xa;
            diaChi.dia_chi_mac_dinh = suaDiaChiClientDTO.dia_chi_mac_dinh;
            diaChi.ngay_sua = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            var result = await _diaChiService.UpdateAsync(diaChi);
            if (!result)
                return BadRequest("Lỗi khi cập nhật địa chỉ");

            return Ok("Cập nhật địa chỉ thành công");
        }

        [HttpDelete("delete-dia-chi/{id}")]
        public async Task<IActionResult> DeleteDiaChi(Guid id)
        {
            var diaChi = await _diaChiService.GetByIdAsync(id);
            if (diaChi == null)
                return NotFound("Không tìm thấy địa chỉ");

            var result = await _diaChiService.DeleteAsync(id);
            if (!result)
                return BadRequest("Lỗi khi xóa địa chỉ");

            return Ok("Xóa địa chỉ thành công");
        }
        private string GetIDKhachHang()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var idKhachHang = _jwtService.GetUserIdFromToken(token);
            return idKhachHang.ToString();
        }
    }
}