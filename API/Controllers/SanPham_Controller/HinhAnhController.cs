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
    public class HinhAnhController : ControllerBase
    {
        private readonly IBaseService<HinhAnh> _hinhAnhServices;
        private readonly IBaseService<SanPhamChiTiet> _sanPhamChiTietServices;
        private readonly IJwtServices _jwtServices;

        public HinhAnhController(IBaseService<HinhAnh> hinhAnhServices, IJwtServices jwtServices, IBaseService<SanPhamChiTiet> sanPhamChiTietServices)
        {
            _hinhAnhServices = hinhAnhServices;
            _jwtServices = jwtServices;
            _sanPhamChiTietServices = sanPhamChiTietServices;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _hinhAnhServices.GetAllWithIncludeAsync(
            );
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _hinhAnhServices.GetByIdAsync(id);
            if (result == null) return NotFound("Không tìm thấy hình ảnh");
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Add([FromBody] ThemHinhAnhSanPhamChiTietAdminDTO hinhAnhDTO)
        {
            if (hinhAnhDTO == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (string.IsNullOrEmpty(hinhAnhDTO.hinh_anh_urls))
                return BadRequest("URL hình ảnh không được để trống");

            if (string.IsNullOrEmpty(hinhAnhDTO.id_san_pham_chi_tiet))
                return BadRequest("ID sản phẩm chi tiết không được để trống");

            var result = await _hinhAnhServices.ExecuteInTransactionAsync(async () =>
            {
                var hinhAnh = new HinhAnh
                {
                    id_hinh_anh = Guid.NewGuid(),
                    ma_hinh_anh = "Image-" + (await _sanPhamChiTietServices.GetByIdAsync(Guid.Parse(hinhAnhDTO.id_san_pham_chi_tiet))).ma_san_pham_chi_tiet,
                    url = hinhAnhDTO.hinh_anh_urls,
                    ngay_tao = DateTime.Now,
                    id_nguoi_tao = (Guid)GetIdNhanVien()
                };
                return await _hinhAnhServices.CreateAsync(hinhAnh);
            });

            if (result) return Ok("Thêm hình ảnh thành công");
            return BadRequest("Đã có lỗi khi thêm hình ảnh!");
        }

        [HttpPut]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SuaHinhAnhSanPhamChiTietAdminDTO hinhAnhDTO)
        {
            if (hinhAnhDTO == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (string.IsNullOrEmpty(hinhAnhDTO.hinh_anh_urls))
                return BadRequest("URL hình ảnh không được để trống");

            var result = await _hinhAnhServices.ExecuteInTransactionAsync(async () =>
            {
                var existingHinhAnh = await _hinhAnhServices.GetByIdAsync(id);
                if (existingHinhAnh == null) return false;

                existingHinhAnh.url = hinhAnhDTO.hinh_anh_urls;
                existingHinhAnh.id_nguoi_sua = (Guid)GetIdNhanVien();
                existingHinhAnh.ngay_sua = DateTime.Now;
                return await _hinhAnhServices.UpdateAsync(existingHinhAnh);
            });

            if (result) return Ok("Cập nhật hình ảnh thành công");
            return BadRequest("Đã có lỗi khi cập nhật hình ảnh!");
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _hinhAnhServices.DeleteAsync(id);
            if (result) return Ok("Xóa hình ảnh thành công");
            return BadRequest("Đã có lỗi khi xóa hình ảnh!");
        }

        private Guid? GetIdNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                throw new UnauthorizedAccessException("Token không hợp lệ");
            var idtnhanvien = _jwtServices.GetIdNhanVienFromToken(token);
            return idtnhanvien;
        }
    }
}