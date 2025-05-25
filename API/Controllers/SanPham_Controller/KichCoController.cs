using API.DbConects.Entities.Entities_San_Pham;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DbConects.DTOs.Admin.SanPham;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.SanPham_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class KichCoController : ControllerBase
    {
        private readonly IBaseService<KichCo> _kichCoServices;
        private readonly IJwtServices _jwtServices;

        public KichCoController(IBaseService<KichCo> kichCoServices, IJwtServices jwtServices)
        {
            _kichCoServices = kichCoServices;
            _jwtServices = jwtServices;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _kichCoServices.GetAllWithIncludeAsync(
                q => q.Include(k => k.SanPhamChiTiets)
            );
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _kichCoServices.GetByIdWithIncludeAsync(id,
                q => q.Include(k => k.SanPhamChiTiets)
            );
            if (result == null) return NotFound("Không tìm thấy kích cỡ");
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Add([FromBody] ThemKichCoAdminDTO kichCoDTO)
        {
            if (kichCoDTO == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (string.IsNullOrEmpty(kichCoDTO.ten_kich_co))
                return BadRequest("Tên kích cỡ không được để trống");

            // Kiểm tra trùng tên
            var existingKichCo = await _kichCoServices.GetAllAsync();
            if (existingKichCo.Any(x => x.ten_kich_co.ToLower() == kichCoDTO.ten_kich_co.ToLower()))
                return BadRequest("Tên kích cỡ đã tồn tại");

            var result = await _kichCoServices.ExecuteInTransactionAsync(async () =>
            {
                var kichCo = new KichCo
                {
                    id_kich_co = Guid.NewGuid(),
                    ma_kich_co = await TaoMaKichCo(),
                    mo_ta = kichCoDTO.mo_ta,
                    ten_kich_co = kichCoDTO.ten_kich_co,
                    trang_thai = "HoatDong",
                    ngay_tao = DateTime.Now,
                    id_nguoi_tao = (Guid)GetIdNhanVien()
                };

                return await _kichCoServices.CreateAsync(kichCo);
            });

            if (result) return Ok("Thêm kích cỡ thành công");
            return BadRequest("Đã có lỗi khi thêm kích cỡ!");
        }

        [HttpPut]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SuaKichCoAdminDTO kichCoDTO)
        {
            if (kichCoDTO == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (string.IsNullOrEmpty(kichCoDTO.ten_kich_co))
                return BadRequest("Tên kích cỡ không được để trống");

            // Kiểm tra trùng tên với kích cỡ khác
            var existingKichCo = await _kichCoServices.GetAllAsync();
            if (existingKichCo.Any(x => x.id_kich_co != id && x.ten_kich_co.ToLower() == kichCoDTO.ten_kich_co.ToLower()))
                return BadRequest("Tên kích cỡ đã tồn tại");

            var result = await _kichCoServices.ExecuteInTransactionAsync(async () =>
            {
                var existingKichCo = await _kichCoServices.GetByIdAsync(id);
                if (existingKichCo == null) return false;

                existingKichCo.ten_kich_co = kichCoDTO.ten_kich_co;
                existingKichCo.trang_thai = kichCoDTO.trang_thai;
                existingKichCo.mo_ta = kichCoDTO.mo_ta;
                existingKichCo.id_nguoi_sua = (Guid)GetIdNhanVien();
                existingKichCo.ngay_sua = DateTime.Now;

                return await _kichCoServices.UpdateAsync(existingKichCo);
            });

            if (result) return Ok("Cập nhật kích cỡ thành công");
            return BadRequest("Đã có lỗi khi cập nhật kích cỡ!");
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _kichCoServices.DeleteAsync(id);
            if (result) return Ok("Xóa kích cỡ thành công");
            return BadRequest("Đã có lỗi khi xóa kích cỡ!");
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSizes()
        {
            var allSizes = await _kichCoServices.GetAllWithIncludeAsync(
                q => q.Include(k => k.SanPhamChiTiets)
            );
            var activeSizes = allSizes.Where(k => k.trang_thai == "HoatDong").ToList();
            return Ok(activeSizes);
        }

        private Guid? GetIdNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                throw new UnauthorizedAccessException("Token không hợp lệ");
            var idtnhanvien = _jwtServices.GetIdNhanVienFromToken(token);
            return idtnhanvien;
        }

        private async Task<string> TaoMaKichCo()
        {
            var lastKichCo = await _kichCoServices.GetAllAsync();
            if (lastKichCo == null || !lastKichCo.Any())
                return "KC00001";

            var lastMaKichCo = lastKichCo.OrderByDescending(x => x.ma_kich_co).First().ma_kich_co;
            var numberPart = int.Parse(lastMaKichCo.Substring(2)) + 1;
            return $"KC{numberPart:D5}";
        }
    }
}
