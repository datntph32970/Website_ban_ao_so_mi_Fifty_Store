using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_San_Pham;
using API.Repositories;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.SanPham_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class XuatXuController : ControllerBase
    {
        private readonly IBaseService<XuatXu> _xuatXuServices;
        private readonly IJwtServices _jwtServices;

        public XuatXuController(IBaseService<XuatXu> xuatXuServices, IJwtServices jwtServices)
        {
            _xuatXuServices = xuatXuServices;
            _jwtServices = jwtServices;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _xuatXuServices.GetAllWithIncludeAsync(cl => cl.Include(c => c.SanPhams)).Result;
            return Ok(result);
        }
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var result = _xuatXuServices.GetByIdWithIncludeAsync(id, cl => cl.Include(c => c.SanPhams)).Result;
            if (result == null)
                return NotFound("Không tìm thấy xuất xứ");
            return Ok(result);
        }
        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Add(ThemXuatXuAdminDTO xuatXuDTO)
        {
            if (xuatXuDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (xuatXuDTO.ten_xuat_xu == null)
                return BadRequest("Yêu cầu nhập tên thương hiệu");
            if (xuatXuDTO.mo_ta == null)
                return BadRequest("Yêu cầu nhập mô tả");

            // Kiểm tra trùng tên
            var existingXuatXu = _xuatXuServices.GetAllAsync().Result
                .FirstOrDefault(x => x.ten_xuat_xu.ToLower() == xuatXuDTO.ten_xuat_xu.ToLower());
            if (existingXuatXu != null)
                return BadRequest("Tên xuất xứ đã tồn tại");

            var xuatXu = new XuatXu
            {
                id_xuat_xu = Guid.NewGuid(),
                ma_xuat_xu = TaoMaXuatXu(),
                ten_xuat_xu = xuatXuDTO.ten_xuat_xu,
                mo_ta = xuatXuDTO.mo_ta,
                trang_thai = "HoatDong",
                id_nguoi_tao = (Guid)GetIdNhanVien(),
                ngay_tao = DateTime.Now
            };
            var result = _xuatXuServices.CreateAsync(xuatXu).Result;
            if (result) return Ok("Thêm xuất xứ thành công");
            return BadRequest("Đã có lỗi khi thêm xuất xứ");

        }
        [HttpPut]
        [Authorize(Roles = "Admin,NhanVien")]

        public IActionResult Update(Guid id, SuaXuatXuAdminDTO xuatXuDTO)
        {
            if (xuatXuDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (xuatXuDTO.ten_xuat_xu == null)
                return BadRequest("Yêu cầu nhập tên thương hiệu");
            if (xuatXuDTO.mo_ta == null)
                return BadRequest("Yêu cầu nhập mô tả");

            // Kiểm tra trùng tên với xuất xứ khác
            var existingXuatXu = _xuatXuServices.GetAllAsync().Result
                .FirstOrDefault(x => x.id_xuat_xu != id && x.ten_xuat_xu.ToLower() == xuatXuDTO.ten_xuat_xu.ToLower());
            if (existingXuatXu != null)
                return BadRequest("Tên xuất xứ đã tồn tại");

            var xuatXu = _xuatXuServices.GetByIdAsync(id).Result;
            if (xuatXu == null)
                return NotFound("Không tìm thấy xuất xứ");


            xuatXu.ten_xuat_xu = xuatXuDTO.ten_xuat_xu;
            xuatXu.mo_ta = xuatXuDTO.mo_ta;
            xuatXu.trang_thai = xuatXuDTO.trang_thai;
            xuatXu.id_nguoi_sua = (Guid)GetIdNhanVien();
            xuatXu.ngay_sua = DateTime.Now;
            var result = _xuatXuServices.UpdateAsync(xuatXu).Result;
            if (result) return Ok("Cập nhật xuất xứ thành công");
            return BadRequest("Đã có lỗi khi cập nhật xuất xứ");
        }
        [HttpDelete]
        [Authorize(Roles = "Admin,NhanVien")]

        public IActionResult Delete(Guid id)
        {
            var result = _xuatXuServices.DeleteAsync(id).Result;
            if (result) return Ok("Xóa xuất xứ thành công");
            return BadRequest("Đã có lỗi khi xóa xuất xứ");
        }
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrigins()
        {
            var allOrigins = await _xuatXuServices.GetAllWithIncludeAsync(cl => cl.Include(c => c.SanPhams));
            var activeOrigins = allOrigins.Where(c => c.trang_thai == "HoatDong").ToList();
            return Ok(activeOrigins);
        }
        private string TaoMaXuatXu()
        {
            var lastXuatXu = _xuatXuServices.GetAllAsync().Result.OrderByDescending(x => x.ma_xuat_xu).FirstOrDefault();
            if (lastXuatXu == null)
                return "XX0001";
            int startNumber = int.Parse(lastXuatXu.ma_xuat_xu.Substring(2)) + 1;
            return $"XX{startNumber:D4}";
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
