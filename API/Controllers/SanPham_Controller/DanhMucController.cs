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
    public class DanhMucController : Controller
    {
        private readonly IBaseService<DanhMuc> _danhMucServices;
        private readonly IJwtServices _jwtServices;

        public DanhMucController(IBaseService<DanhMuc> danhMucServices, IJwtServices jwtServices)
        {
            _danhMucServices = danhMucServices;
            _jwtServices = jwtServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _danhMucServices.GetAllWithIncludeAsync(cl => cl.Include(c => c.SanPhams));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _danhMucServices.GetByIdWithIncludeAsync(id, cl => cl.Include(c => c.SanPhams));
            if (result == null)
                return NotFound("Không tìm thấy danh mục");
            return Ok(result);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCategories()
        {
            var allCategories = await _danhMucServices.GetAllWithIncludeAsync(cl => cl.Include(c => c.SanPhams));
            var activeCategories = allCategories.Where(c => c.trang_thai == "HoatDong").ToList();
            return Ok(activeCategories);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Add(ThemDanhMucAdminDTO danhMucDTO)
        {
            if (danhMucDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (danhMucDTO.ten_danh_muc == null)
                return BadRequest("Yêu cầu nhập tên danh mục");

            // Kiểm tra trùng tên
            var existingDanhMuc = await _danhMucServices.GetAllAsync();
            if (existingDanhMuc.Any(x => x.ten_danh_muc.ToLower() == danhMucDTO.ten_danh_muc.ToLower()))
                return BadRequest("Tên danh mục đã tồn tại");

            var danhmuc = new DanhMuc
            {
                id_danh_muc = Guid.NewGuid(),
                ma_danh_muc = TaoMaDanhMuc(),
                ten_danh_muc = danhMucDTO.ten_danh_muc,
                mo_ta = danhMucDTO.mo_ta,
                trang_thai = "HoatDong",
                id_nguoi_tao = (Guid)GetIdNhanVien(),
                ngay_tao = DateTime.Now
            };
            var result = _danhMucServices.CreateAsync(danhmuc).Result;
            if (result) return Ok("Thêm danh mục thành công");
            return BadRequest("Đã có lỗi khi thêm danh mục");
        }

        [HttpPut]
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Update(Guid id, SuaDanhMucAdminDTO danhMucDTO)
        {
            if (danhMucDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (danhMucDTO.ten_danh_muc == null)
                return BadRequest("Yêu cầu nhập tên danh mục");
            if (danhMucDTO.mo_ta == null)
                return BadRequest("Yêu cầu nhập mô tả");
            if (danhMucDTO.trang_thai == null)
                return BadRequest("Yêu cầu nhập trạng thái");

            // Kiểm tra trùng tên với danh mục khác
            var existingDanhMuc = _danhMucServices.GetAllAsync().Result
                .FirstOrDefault(x => x.id_danh_muc != id && x.ten_danh_muc.ToLower() == danhMucDTO.ten_danh_muc.ToLower());
            if (existingDanhMuc != null)
                return BadRequest("Tên danh mục đã tồn tại");

            var danhmuc = _danhMucServices.GetByIdAsync(id).Result;
            if (danhmuc == null)
                return NotFound("Không tìm thấy danh mục");

            danhmuc.ten_danh_muc = danhMucDTO.ten_danh_muc;
            danhmuc.mo_ta = danhMucDTO.mo_ta;
            danhmuc.trang_thai = danhMucDTO.trang_thai;
            danhmuc.id_nguoi_sua = (Guid)GetIdNhanVien();
            danhmuc.ngay_sua = DateTime.Now;

            var result = _danhMucServices.UpdateAsync(danhmuc).Result;
            if (result) return Ok("Cập nhật danh mục thành công");
            return BadRequest("Đã có lỗi khi cập nhật danh mục");
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Delete(Guid id)
        {
            var result = _danhMucServices.DeleteAsync(id).Result;
            if (result) return Ok("Xóa danh mục thành công");
            return BadRequest("Đã có lỗi khi xóa danh mục");
        }
        private string TaoMaDanhMuc()
        {
            var lastDanhMuc = _danhMucServices.GetAllAsync().Result.OrderByDescending(x => x.ma_danh_muc).FirstOrDefault();
            if (lastDanhMuc == null)
                return "DM0001";
            int startNumber = int.Parse(lastDanhMuc.ma_danh_muc.Substring(2)) + 1;
            return $"DM{startNumber:D4}";
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
