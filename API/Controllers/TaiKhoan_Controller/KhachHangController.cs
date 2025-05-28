using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Services.Interfaces;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Services.JwtServices;
using API.DbConects.DTOs.Admin.TaiKhoan;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Microsoft.VisualBasic;
using API.DbConects.DTOs.Admin.KhachHang;

namespace API.Controllers.TaiKhoan_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : ControllerBase
    {
        private readonly IBaseService<KhachHang> _khachHangServices;
        private readonly IBaseService<TaiKhoan> _taikhoanServices;
        private readonly IJwtServices _jwtServices;

        public KhachHangController(IBaseService<KhachHang> khachHangServices, IBaseService<TaiKhoan> taikhoanServices, IJwtServices jwtServices)
        {
            _khachHangServices = khachHangServices;
            _taikhoanServices = taikhoanServices;
            _jwtServices = jwtServices;
        }

        // GET: api/KhachHang
        [HttpGet]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetAll([FromQuery] ThamSoPhanTrangKhachHangDTO thamSo)
        {
            var khachHangs = await _khachHangServices.GetAllWithIncludeAsync(
                q => q.Include(kh => kh.TaiKhoan)
                     .Include(kh => kh.DiaChis)
                     .Include(kh => kh.HoaDons)
                     .Include(kh => kh.GioHangChiTiets)
            );

            // Tìm kiếm
            if (!string.IsNullOrEmpty(thamSo.tim_kiem))
            {
                var searchTerm = thamSo.tim_kiem.ToLower();
                khachHangs = khachHangs.Where(kh =>
                    (kh.ten_khach_hang != null && kh.ten_khach_hang.ToLower().Contains(searchTerm)) ||
                    (kh.so_dien_thoai != null && kh.so_dien_thoai.ToLower().Contains(searchTerm)) ||
                    (kh.email != null && kh.email.ToLower().Contains(searchTerm)) ||
                    (kh.TaiKhoan != null && kh.TaiKhoan.ten_dang_nhap != null && kh.TaiKhoan.ten_dang_nhap.ToLower().Contains(searchTerm)) ||
                    (kh.ma_khach_hang != null && kh.ma_khach_hang.ToLower().Contains(searchTerm)) ||
                    (kh.id_khach_hang.ToString().ToLower().Contains(searchTerm) &&
                     (kh.ten_khach_hang != null || kh.so_dien_thoai != null || kh.email != null ||
                      (kh.TaiKhoan != null && kh.TaiKhoan.ten_dang_nhap != null) || kh.ma_khach_hang != null))).ToList();
            }

            // Sắp xếp theo ngày tạo giảm dần
            khachHangs = khachHangs.OrderByDescending(kh => kh.ngay_tao).ToList();

            // Tính toán phân trang
            thamSo.tong_so_phan_tu = khachHangs.Count;
            thamSo.tong_so_trang = (int)Math.Ceiling((double)thamSo.tong_so_phan_tu / thamSo.so_phan_tu_tren_trang);
            thamSo.trang_hien_tai = Math.Max(1, Math.Min(thamSo.trang_hien_tai, thamSo.tong_so_trang));

            // Lấy dữ liệu cho trang hiện tại
            var danhSachKhachHang = khachHangs
                .Skip((thamSo.trang_hien_tai - 1) * thamSo.so_phan_tu_tren_trang)
                .Take(thamSo.so_phan_tu_tren_trang)
                .Select(kh => new KhachHangAdminDTO
                {
                    id_khach_hang = kh.id_khach_hang,
                    ma_khach_hang = kh.ma_khach_hang,
                    ten_khach_hang = kh.ten_khach_hang,
                    so_dien_thoai = kh.so_dien_thoai,
                    email = kh.email,
                    ngay_sinh = kh.ngay_sinh,
                    gioi_tinh = kh.gioi_tinh,
                    trang_thai = kh.trang_thai,
                    ngay_tao = kh.ngay_tao,
                })
                .ToList();
            var phanTrang = new PhanTrangKhachHangDTO
            {
                trang_hien_tai = thamSo.trang_hien_tai,
                so_phan_tu_tren_trang = thamSo.so_phan_tu_tren_trang,
                tong_so_trang = thamSo.tong_so_trang,
                tong_so_phan_tu = thamSo.tong_so_phan_tu,
                danh_sach = danhSachKhachHang
            };
            return Ok(phanTrang);
        }

        // GET: api/KhachHang/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var khachHang = await _khachHangServices.GetByIdWithIncludeAsync(id,
                q => q.Include(kh => kh.TaiKhoan)
                     .Include(kh => kh.DiaChis)
                     .Include(kh => kh.HoaDons)
                     .Include(kh => kh.GioHangChiTiets)
            );

            if (khachHang == null)
                return NotFound("Không tìm thấy khách hàng");

            return Ok(khachHang);
        }

        // GET: api/KhachHang/taikhoan/{idTaiKhoan}
        [HttpGet("taikhoan/{idTaiKhoan}")]
        [Authorize]
        public async Task<IActionResult> GetByTaiKhoanId(Guid idTaiKhoan)
        {
            var khachHang = await _khachHangServices.GetByIdWithIncludeAsync(
                idTaiKhoan,
                q => q.Include(kh => kh.TaiKhoan)
                     .Include(kh => kh.DiaChis)
                     .Include(kh => kh.HoaDons)
                     .Include(kh => kh.GioHangChiTiets)
            );

            if (khachHang == null)
                return NotFound("Không tìm thấy khách hàng");

            return Ok(khachHang);
        }

        // POST: api/KhachHang
        [HttpPost("them-khach-hang-mua-tai-quay")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Create([FromBody] ThemKhachHangMuaTaiQuayAdminDTO khachHangDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingKhachHang = await _khachHangServices.ExistsAsync(kh =>
                kh.so_dien_thoai == khachHangDTO.so_dien_thoai);

            if (existingKhachHang)
                return BadRequest("Khách hàng đã tồn tại với số điện thoại này");

            var khachHang = new KhachHang
            {
                id_khach_hang = Guid.NewGuid(),
                ma_khach_hang = GenerateMaKhachHang(),
                ten_khach_hang = khachHangDTO.ten_khach_hang,
                so_dien_thoai = khachHangDTO.so_dien_thoai,
                trang_thai = "HoatDong",
                ngay_tao = DateTime.Now
            };

            var result = await _khachHangServices.CreateAsync(khachHang);
            if (result == null)
                return BadRequest("Đã xảy ra lỗi khi tạo khách hàng");

            return Ok(khachHang.id_khach_hang);
        }

        // PUT: api/KhachHang/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SuaKhachHangAdminDTO khachHangDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingKhachHang = await _khachHangServices.GetByIdWithIncludeAsync(id,
                q => q.Include(kh => kh.TaiKhoan));

            if (existingKhachHang == null)
                return NotFound("Không tìm thấy khách hàng");

            // Kiểm tra trùng lặp với khách hàng khác
            var existingKhachHangKhac = await _khachHangServices.GetByConditionAsync(kh =>
                kh.id_khach_hang != id &&
                ((!string.IsNullOrEmpty(khachHangDTO.email) && kh.email == khachHangDTO.email) ||
                 (!string.IsNullOrEmpty(khachHangDTO.so_dien_thoai) && kh.so_dien_thoai == khachHangDTO.so_dien_thoai)));

            if (existingKhachHangKhac.Any())
            {
                var khachHangTrung = existingKhachHangKhac.First();
                if (khachHangTrung.email == khachHangDTO.email)
                    return BadRequest("Email đã được sử dụng bởi khách hàng khác");
                if (khachHangTrung.so_dien_thoai == khachHangDTO.so_dien_thoai)
                    return BadRequest("Số điện thoại đã được sử dụng bởi khách hàng khác");
            }

            // Cập nhật thông tin khách hàng
            existingKhachHang.ten_khach_hang = khachHangDTO.ten_khach_hang;
            existingKhachHang.ngay_sinh = khachHangDTO.ngay_sinh;
            existingKhachHang.so_dien_thoai = khachHangDTO.so_dien_thoai;
            existingKhachHang.email = khachHangDTO.email;
            existingKhachHang.gioi_tinh = khachHangDTO.gioi_tinh;
            existingKhachHang.trang_thai = khachHangDTO.trang_thai;

            // Cập nhật tài khoản nếu có
            if (existingKhachHang.id_tai_khoan != null && existingKhachHang.TaiKhoan != null)
            {
                existingKhachHang.TaiKhoan.trang_thai = khachHangDTO.trang_thai;
                var updateTaiKhoan = await _taikhoanServices.UpdateAsync(existingKhachHang.TaiKhoan);
                if (!updateTaiKhoan)
                    return BadRequest("Cập nhật trạng thái tài khoản thất bại");
            }

            var result = await _khachHangServices.UpdateAsync(existingKhachHang);
            if (!result)
                return BadRequest("Cập nhật thông tin khách hàng thất bại");

            return Ok(new { message = "Cập nhật khách hàng thành công", id = existingKhachHang.id_khach_hang });
        }

        // DELETE: api/KhachHang/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var existingKhachHang = await _khachHangServices.GetByIdWithIncludeAsync(id,
                    q => q.Include(kh => kh.HoaDons)
                         .Include(kh => kh.TaiKhoan)
                );

                if (existingKhachHang == null)
                    return NotFound("Không tìm thấy khách hàng");

                // Kiểm tra xem khách hàng có hóa đơn không
                if (existingKhachHang.HoaDons != null && existingKhachHang.HoaDons.Any())
                    return BadRequest("Không thể xóa khách hàng vì đã có hóa đơn liên quan");

                var result = await _khachHangServices.DeleteAsync(id);
                if (!result)
                    return BadRequest("Lỗi khi xóa khách hàng");

                // Nếu khách hàng có tài khoản, xóa tài khoản
                if (existingKhachHang.id_tai_khoan != null)
                {
                    var xoataikhoan = await _taikhoanServices.DeleteAsync(existingKhachHang.id_tai_khoan.Value);
                    if (!xoataikhoan)
                        return BadRequest("Lỗi khi xóa tài khoản của khách hàng");
                }

                return Ok("Xóa khách hàng thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // PATCH: api/KhachHang/{id}/trangthai
        [HttpPatch("{id}/trangthai")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> UpdateTrangThai(Guid id, [FromBody] CapNhatTrangThaiKhachHangDTO trangThaiDTO)
        {
            var result = await _khachHangServices.ExecuteInTransactionAsync(async () =>
            {
                var existingKhachHang = await _khachHangServices.GetByIdAsync(id);
                if (existingKhachHang == null)
                    return false;

                existingKhachHang.trang_thai = trangThaiDTO.trang_thai;

                var result = await _khachHangServices.UpdateAsync(existingKhachHang);
                if (!result)
                    return false;
                if (existingKhachHang.id_tai_khoan != null)
                {
                    var taikhoan = await _taikhoanServices.GetByIdAsync(existingKhachHang.id_tai_khoan.Value);
                    if (taikhoan != null)
                    {
                        taikhoan.trang_thai = trangThaiDTO.trang_thai;
                        var capnhattaikhoan = await _taikhoanServices.UpdateAsync(taikhoan);
                        if (capnhattaikhoan)
                            return true;
                    }
                    return false;
                }
                return true;
            });
            if (result)
                return Ok("Cập nhật trạng thái tài khoản thành công");

            return BadRequest("Đã xảy ra lỗi khi cập nhật trạng thái khách hàng");
        }

        [HttpGet("tim-kiem")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> TimKiemKhachHang([FromQuery] string? tuKhoa)
        {
            if (string.IsNullOrEmpty(tuKhoa))
                return BadRequest("Vui lòng nhập từ khóa tìm kiếm");

            var khachHangs = await _khachHangServices.GetAllWithIncludeAsync();

            var searchTerm = tuKhoa.ToLower();
            var danhSachKhachHang = khachHangs.Where(kh =>
                (kh.ten_khach_hang != null && kh.ten_khach_hang.ToLower().Contains(searchTerm)) ||
                (kh.so_dien_thoai != null && kh.so_dien_thoai.ToLower().Contains(searchTerm)))
                .Select(kh => new KhachHangAdminDTO
                {
                    id_khach_hang = kh.id_khach_hang,
                    ma_khach_hang = kh.ma_khach_hang,
                    ten_khach_hang = kh.ten_khach_hang,
                    so_dien_thoai = kh.so_dien_thoai,
                    email = kh.email,
                    ngay_sinh = kh.ngay_sinh,
                    gioi_tinh = kh.gioi_tinh,
                    trang_thai = kh.trang_thai,
                    ngay_tao = kh.ngay_tao
                }).ToList();

            return Ok(danhSachKhachHang);
        }

        private string GenerateMaKhachHang()
        {
            var lastKhachHang = _khachHangServices.GetAllAsync().Result.OrderByDescending(x => x.ma_khach_hang).FirstOrDefault();
            if (lastKhachHang == null)
                return "KH00001";
            int startNumber = int.Parse(lastKhachHang.ma_khach_hang.Substring(2)) + 1;
            return $"KH{startNumber:D5}";
        }
    }
}