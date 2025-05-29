using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_San_Pham;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.SanPham_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class KieuDangController : ControllerBase
    {
        private readonly IBaseService<KieuDang> _kieuDangServices;
        private readonly IJwtServices _jwtServices;

        public KieuDangController(IBaseService<KieuDang> kieuDangServices, IJwtServices jwtServices)
        {
            _kieuDangServices = kieuDangServices;
            _jwtServices = jwtServices;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _kieuDangServices.GetAllWithIncludeAsync(cl => cl.Include(c => c.SanPhams)).Result;
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var result = _kieuDangServices.GetByIdWithIncludeAsync(id, cl => cl.Include(c => c.SanPhams)).Result;
            if (result == null)
                return NotFound("Không tìm thấy kiểu dáng");
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Add(ThemKieuDangAdminDTO kieuDangDTO)
        {
            if (kieuDangDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (kieuDangDTO.ten_kieu_dang == null)
                return BadRequest("Yêu cầu nhập tên kiểu dáng");
            if (kieuDangDTO.mo_ta == null)
                return BadRequest("Yêu cầu nhập mô tả");

            // Kiểm tra trùng tên
            var existingKieuDang = _kieuDangServices.GetAllAsync().Result
                .FirstOrDefault(x => x.ten_kieu_dang.Trim().ToLower() == kieuDangDTO.ten_kieu_dang.Trim().ToLower());
            if (existingKieuDang != null)
                return BadRequest("Tên kiểu dáng đã tồn tại");

            var kieuDang = new KieuDang
            {
                id_kieu_dang = Guid.NewGuid(),
                ma_kieu_dang = TaoMaKieuDang(),
                ten_kieu_dang = kieuDangDTO.ten_kieu_dang,
                mo_ta = kieuDangDTO.mo_ta,
                trang_thai = "HoatDong",
                id_nguoi_tao = (Guid)GetIdNhanVien(),
                ngay_tao = DateTime.Now
            };

            var result = _kieuDangServices.CreateAsync(kieuDang).Result;
            if (result) return Ok("Thêm kiểu dáng thành công");
            return BadRequest("Đã có lỗi khi thêm kiểu dáng");
        }

        [HttpPut]
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Update(Guid id, SuaKieuDangAdminDTO kieuDangDTO)
        {
            if (kieuDangDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (kieuDangDTO.ten_kieu_dang == null)
                return BadRequest("Yêu cầu nhập tên kiểu dáng");
            if (kieuDangDTO.mo_ta == null)
                return BadRequest("Yêu cầu nhập mô tả");

            // Kiểm tra trùng tên với kiểu dáng khác
            var existingKieuDang = _kieuDangServices.GetAllAsync().Result
                .FirstOrDefault(x => x.id_kieu_dang != id && x.ten_kieu_dang.Trim().ToLower() == kieuDangDTO.ten_kieu_dang.Trim().ToLower());
            if (existingKieuDang != null)
                return BadRequest("Tên kiểu dáng đã tồn tại");

            var kieuDang = _kieuDangServices.GetByIdAsync(id).Result;
            if (kieuDang == null)
                return NotFound("Không tìm thấy kiểu dáng");

            kieuDang.ten_kieu_dang = kieuDangDTO.ten_kieu_dang;
            kieuDang.mo_ta = kieuDangDTO.mo_ta;
            kieuDang.trang_thai = kieuDangDTO.trang_thai;
            kieuDang.id_nguoi_sua = (Guid)GetIdNhanVien();
            kieuDang.ngay_sua = DateTime.Now;

            var result = _kieuDangServices.UpdateAsync(kieuDang).Result;
            if (result) return Ok("Cập nhật kiểu dáng thành công");
            return BadRequest("Đã có lỗi khi cập nhật kiểu dáng");
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var kieuDang = await _kieuDangServices.GetByIdWithIncludeAsync(id, cl => cl.Include(c => c.SanPhams));
            if (kieuDang == null)
                return NotFound("Không tìm thấy kiểu dáng");

            if (kieuDang.SanPhams != null && kieuDang.SanPhams.Any())
                return BadRequest("Không thể xóa kiểu dáng này vì đang có sản phẩm đang sử dụng");

            var result = await _kieuDangServices.DeleteAsync(id);
            if (result) return Ok("Xóa kiểu dáng thành công");
            return BadRequest("Đã có lỗi khi xóa kiểu dáng");
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveStyles()
        {
            var allStyles = await _kieuDangServices.GetAllWithIncludeAsync(cl => cl.Include(c => c.SanPhams));
            var activeStyles = allStyles.Where(c => c.trang_thai == "HoatDong").ToList();
            return Ok(activeStyles);
        }

        private string TaoMaKieuDang()
        {
            var lastKieuDang = _kieuDangServices.GetAllAsync().Result.OrderByDescending(x => x.ma_kieu_dang).FirstOrDefault();
            if (lastKieuDang == null)
                return "KD0001";
            int startNumber = int.Parse(lastKieuDang.ma_kieu_dang.Substring(2)) + 1;
            return $"KD{startNumber:D4}";
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
