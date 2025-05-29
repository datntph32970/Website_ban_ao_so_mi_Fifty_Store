using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_San_Pham;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.SanPham_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MauSacController : ControllerBase
    {
        private readonly IBaseService<MauSac> _mauSacServices;
        private readonly IJwtServices _jwtServices;

        public MauSacController(IBaseService<MauSac> mauSacServices, IJwtServices jwtServices)
        {
            _mauSacServices = mauSacServices;
            _jwtServices = jwtServices;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mauSacServices.GetAllWithIncludeAsync(
                q => q.Include(m => m.SanPhamChiTiets)
            );
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mauSacServices.GetByIdWithIncludeAsync(id,
                q => q.Include(m => m.SanPhamChiTiets)
            );
            if (result == null) return NotFound("Không tìm thấy màu sắc");
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Add([FromBody] ThemMauSacAdminDTO mauSacDTO)
        {
            if (mauSacDTO == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (string.IsNullOrEmpty(mauSacDTO.ten_mau_sac))
                return BadRequest("Tên màu sắc không được để trống");

            // Kiểm tra trùng tên
            var existingMauSac = await _mauSacServices.GetAllAsync();
            if (existingMauSac.Any(x => x.ten_mau_sac.Trim().ToLower() == mauSacDTO.ten_mau_sac.Trim().ToLower()))
                return BadRequest("Tên màu sắc đã tồn tại");

            var result = await _mauSacServices.ExecuteInTransactionAsync(async () =>
            {
                var mauSac = new MauSac
                {
                    id_mau_sac = Guid.NewGuid(),
                    ma_mau_sac = await TaoMaMauSac(),
                    mo_ta = mauSacDTO.mo_ta,
                    ten_mau_sac = mauSacDTO.ten_mau_sac,
                    trang_thai = "HoatDong",
                    ngay_tao = DateTime.Now,
                    id_nguoi_tao = (Guid)GetIdNhanVien()
                };

                return await _mauSacServices.CreateAsync(mauSac);
            });

            if (result) return Ok("Thêm màu sắc thành công");
            return BadRequest("Đã có lỗi khi thêm màu sắc!");
        }

        [HttpPut]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SuaMauSacAdminDTO mauSacDTO)
        {
            if (mauSacDTO == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (string.IsNullOrEmpty(mauSacDTO.ten_mau_sac))
                return BadRequest("Tên màu sắc không được để trống");

            // Kiểm tra trùng tên với màu sắc khác
            var existingMauSac = await _mauSacServices.GetAllAsync();
            if (existingMauSac.Any(x => x.id_mau_sac != id && x.ten_mau_sac.Trim().ToLower() == mauSacDTO.ten_mau_sac.Trim().ToLower()))
                return BadRequest("Tên màu sắc đã tồn tại");

            var result = await _mauSacServices.ExecuteInTransactionAsync(async () =>
            {
                var existingMauSac = await _mauSacServices.GetByIdAsync(id);
                if (existingMauSac == null) return false;

                existingMauSac.ten_mau_sac = mauSacDTO.ten_mau_sac;
                existingMauSac.trang_thai = mauSacDTO.trang_thai;
                existingMauSac.mo_ta = mauSacDTO.mo_ta;
                existingMauSac.id_nguoi_sua = (Guid)GetIdNhanVien();
                existingMauSac.ngay_sua = DateTime.Now;

                return await _mauSacServices.UpdateAsync(existingMauSac);
            });

            if (result) return Ok("Cập nhật màu sắc thành công");
            return BadRequest("Đã có lỗi khi cập nhật màu sắc!");
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var mauSac = await _mauSacServices.GetByIdWithIncludeAsync(id, q => q.Include(m => m.SanPhamChiTiets));
            if (mauSac == null)
                return NotFound("Không tìm thấy màu sắc");

            if (mauSac.SanPhamChiTiets != null && mauSac.SanPhamChiTiets.Any())
                return BadRequest("Không thể xóa màu sắc này vì đang có sản phẩm chi tiết đang sử dụng");

            var result = await _mauSacServices.DeleteAsync(id);
            if (result) return Ok("Xóa màu sắc thành công");
            return BadRequest("Đã có lỗi khi xóa màu sắc!");
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveColors()
        {
            var allColors = await _mauSacServices.GetAllWithIncludeAsync(
                q => q.Include(m => m.SanPhamChiTiets)
            );
            var activeColors = allColors.Where(m => m.trang_thai == "HoatDong").ToList();
            return Ok(activeColors);
        }

        private Guid? GetIdNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                throw new UnauthorizedAccessException("Token không hợp lệ");
            var idtnhanvien = _jwtServices.GetIdNhanVienFromToken(token);
            return idtnhanvien;
        }

        private async Task<string> TaoMaMauSac()
        {
            var lastMauSac = await _mauSacServices.GetAllAsync();
            if (lastMauSac == null || !lastMauSac.Any())
                return "MS00001";

            var lastMaMauSac = lastMauSac.OrderByDescending(x => x.ma_mau_sac).First().ma_mau_sac;
            var numberPart = int.Parse(lastMaMauSac.Substring(2)) + 1;
            return $"MS{numberPart:D5}";
        }
    }
}
