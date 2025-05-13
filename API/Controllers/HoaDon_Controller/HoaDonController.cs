using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.DbConects.DTOs.Admin.HoaDon;
using Microsoft.VisualBasic;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_San_Pham;
using Microsoft.EntityFrameworkCore;
using API.DbConects.Entities.Entities_Khuyen_Mai;
using System.Linq;
namespace API.Controllers.HoaDon_Controller
{
    [Route("api/[controller]")]
    [ApiController]

    public class HoaDonController : ControllerBase
    {
        private readonly IHoaDonService _hoaDonService;
        private readonly IBaseService<HoaDonChiTiet> _hoaDonChiTietService;
        private readonly ISanPhamService _sanPhamService;
        private readonly IBaseService<SanPhamChiTiet> _sanPhamChiTietService;
        private readonly IBaseService<KhuyenMai> _khuyenMaiService;
        private readonly IBaseService<GiamGia> _giamGiaService;
        private readonly IJwtServices _jwtService;
        private readonly IKhachHangService _khachHangService;

        public HoaDonController(IHoaDonService hoaDonService, IJwtServices jwtService, IBaseService<HoaDonChiTiet> hoaDonChiTietService, ISanPhamService sanPhamService, IBaseService<SanPhamChiTiet> sanPhamChiTietService, IBaseService<KhuyenMai> khuyenMaiService, IBaseService<GiamGia> giamGiaService, IKhachHangService khachHangService)
        {
            _hoaDonService = hoaDonService;
            _jwtService = jwtService;
            _hoaDonChiTietService = hoaDonChiTietService;
            _sanPhamService = sanPhamService;
            _sanPhamChiTietService = sanPhamChiTietService;
            _khuyenMaiService = khuyenMaiService;
            _giamGiaService = giamGiaService;
            _khachHangService = khachHangService;
        }
        //lấy danh sách hóa đơn bán tại quầy có trạng thái 'ChoTaiQuay'
        [HttpGet("lay-danh-sach-hoa-don-ban-tai-quay-co-trang-thai-cho-tai-quay")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> LayDanhSachHoaDonBanTaiQuayCoTrangThaiChoTaiQuay()
        {
            var id_nguoi_lay = GetIdNhanVien();
            if (id_nguoi_lay == null)
                return Unauthorized();
            var result = await _hoaDonService.GetAllHoaDonAdminDTOAsync();
            var hoaDonBanTaiQuay = result.Where(x => x.loai_hoa_don == "TaiQuay" && x.trang_thai == "ChoTaiQuay" && x.nguoiTao.id_nhan_vien == id_nguoi_lay).ToList();
            return Ok(hoaDonBanTaiQuay);
        }
        [HttpPost("them-hoa-don-ban-tai-quay-moi")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> ThemHoaDonBanTaiQuayMoi()
        {
            var id_nguoi_tao = GetIdNhanVien();
            if (id_nguoi_tao == null)
                return Unauthorized();
            var result = await _hoaDonService.ThemHoaDonBanTaiQuayMoiAsync(id_nguoi_tao.Value);
            if (result.Item1)
                return Ok(result.Item2);
            return BadRequest(result.Item2);
        }
        [HttpDelete("xoa-hoa-don-ban-tai-quay")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> XoaHoaDonBanTaiQuay(Guid id_hoa_don)
        {
            var result = await _hoaDonService.XoaHoaDon(id_hoa_don);
            if (result.Item1)
                return Ok(result.Item2);
            return BadRequest(result.Item2);
        }
        [HttpDelete("xoa-hoa-don-chi-tiet")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> XoaHoaDonChiTiet(Guid id_hoa_don_chi_tiet)
        {
            var result = await _hoaDonService.XoaHoaDonChiTiet(id_hoa_don_chi_tiet);
            if (result.Item1)
                return Ok(result.Item2);
            return BadRequest(result.Item2);
        }
        // thêm mới hóa đơn chi tiết
        [HttpPost("them-hoa-don-chi-tiet")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> ThemHoaDonChiTiet(HoaDonChiTietBanTaiQuayDTO dto)
        {
            var id_nguoi_tao = GetIdNhanVien();
            if (id_nguoi_tao == null)
                return Unauthorized();
            var hoadon = await _hoaDonService.GetHoaDonBanTaiQuayByIdAsync(dto.id_hoa_don, id_nguoi_tao.Value);
            if (hoadon == null)
                return NotFound("Không tìm thấy hóa đơn");
            var result = await _hoaDonService.ThemHoaDonChiTiet(dto.id_hoa_don, dto.id_san_pham_chi_tiet, dto.so_luong, dto.ghi_chu);
            await _hoaDonService.CapNhatTongTienVaGiaTriKhuyenMai(dto.id_hoa_don);
            if (result.Item1)
                return Ok(result.Item2);
            return BadRequest(result.Item2);
        }
        // thêm mới hóa đơn chi tiết
        [HttpPost("sua-hoa-don-chi-tiet")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> SuaHoaDonChiTiet(HoaDonChiTietBanTaiQuayDTO dto)
        {
            var id_nguoi_tao = GetIdNhanVien();
            if (id_nguoi_tao == null)
                return Unauthorized();
            var hoadon = await _hoaDonService.GetHoaDonBanTaiQuayByIdAsync(dto.id_hoa_don, id_nguoi_tao.Value);
            if (hoadon == null)
                return NotFound("Không tìm thấy hóa đơn");
            var result = await _hoaDonService.CapNhatHoaDonChiTiet(dto.id_hoa_don, dto.id_san_pham_chi_tiet, dto.so_luong, dto.ghi_chu);
            await _hoaDonService.CapNhatTongTienVaGiaTriKhuyenMai(dto.id_hoa_don);
            if (result.Item1)
                return Ok(result.Item2);
            return BadRequest(result.Item2);
        }

        [HttpGet("lay-hoa-don-ban-tai-quay-theo-id/{id_hoa_don}")]
        public async Task<IActionResult> LayHoaDonBanTaiQuayTheoId(Guid id_hoa_don)
        {
            try
            {
                var id_nguoi_tao = GetIdNhanVien();
                if (id_nguoi_tao == null)
                    return Unauthorized();
                var result = await _hoaDonService.GetHoaDonBanTaiQuayByIdAsync(id_hoa_don, id_nguoi_tao.Value);
                if (result == null)
                {
                    return NotFound("Không tìm thấy hóa đơn");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("cap-nhat-hoa-don-ban-tai-quay")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> CapNhatHoaDon([FromBody] CapNhatHoaDonDTO dto)
        {
            try
            {
                var id_nguoi_sua = GetIdNhanVien();
                if (id_nguoi_sua == null)
                    return Unauthorized();

                var hoaDon = await _hoaDonService.GetByIdWithIncludeAsync(dto.id_hoa_don,
                    q => q.Include(hd => hd.HoaDonChiTiets)
                         .Include(hd => hd.KhachHang)
                         .Include(hd => hd.KhuyenMai));

                if (hoaDon == null)
                    return NotFound("Không tìm thấy hóa đơn");

                if (hoaDon.trang_thai_hoa_don != "ChoTaiQuay")
                    return BadRequest("Hóa đơn không ở trạng thái cho phép cập nhật");

                // Cập nhật khách hàng
                if (!string.IsNullOrEmpty(dto.id_khach_hang))
                {
                    var khachHang = await _khachHangService.GetByIdAsync(Guid.Parse(dto.id_khach_hang));
                    if (khachHang == null)
                        return NotFound("Không tìm thấy khách hàng");

                    hoaDon.id_khach_hang = Guid.Parse(dto.id_khach_hang);
                    hoaDon.ten_khach_hang = khachHang.ten_khach_hang;
                }
                else
                {
                    // Xóa thông tin khách hàng
                    hoaDon.id_khach_hang = null;
                    hoaDon.dia_chi_nhan_hang = null;
                    hoaDon.ten_khach_hang = null;
                }

                // Cập nhật khuyến mãi
                if (!string.IsNullOrEmpty(dto.id_khuyen_mai))
                {
                    var khuyenMai = await _khuyenMaiService.GetByIdAsync(Guid.Parse(dto.id_khuyen_mai));
                    if (khuyenMai == null)
                        return NotFound("Không tìm thấy khuyến mãi");

                    if (khuyenMai.trang_thai != "HoatDong")
                        return BadRequest("Khuyến mãi không còn hoạt động");

                    if (khuyenMai.thoi_gian_bat_dau > DateTime.Now || khuyenMai.thoi_gian_ket_thuc < DateTime.Now)
                        return BadRequest("Khuyến mãi không còn hiệu lực");
                    if (khuyenMai.so_luong_da_su_dung >= khuyenMai.so_luong_toi_da)
                        return BadRequest("Khuyến mãi đã hết số lượng");
                    if (hoaDon.KhuyenMai == null)
                    {
                        hoaDon.id_khuyen_mai = Guid.Parse(dto.id_khuyen_mai);
                        khuyenMai.so_luong_da_su_dung++;
                        await _khuyenMaiService.UpdateAsync(khuyenMai);
                    }
                    else if (hoaDon.KhuyenMai.id_khuyen_mai != khuyenMai.id_khuyen_mai)
                    {
                        var khuyenMaiCu = await _khuyenMaiService.GetByIdAsync(hoaDon.KhuyenMai.id_khuyen_mai);
                        khuyenMaiCu.so_luong_da_su_dung--;
                        await _khuyenMaiService.UpdateAsync(khuyenMaiCu);
                        hoaDon.id_khuyen_mai = Guid.Parse(dto.id_khuyen_mai);
                        khuyenMai.so_luong_da_su_dung++;
                        await _khuyenMaiService.UpdateAsync(khuyenMai);
                    }
                }
                else
                {
                    // Xóa khuyến mãi
                    hoaDon.id_khuyen_mai = null;
                    hoaDon.so_tien_khuyen_mai = 0;

                    if (hoaDon.KhuyenMai != null)
                    {
                    var khuyenMai = await _khuyenMaiService.GetByIdAsync(hoaDon.KhuyenMai.id_khuyen_mai);
                        khuyenMai.so_luong_da_su_dung--;
                        await _khuyenMaiService.UpdateAsync(khuyenMai);
                    }
                }

                // Cập nhật thông tin khác
                hoaDon.ghi_chu = dto.ghi_chu;

                // Cập nhật tổng tiền và giá trị khuyến mãi
                var (tongTienSauKhuyenMai, giaTriKhuyenMai) = await _hoaDonService.CapNhatTongTienVaGiaTriKhuyenMai(hoaDon.id_hoa_don);
                hoaDon.tong_tien_phai_thanh_toan = tongTienSauKhuyenMai;
                hoaDon.so_tien_khuyen_mai = giaTriKhuyenMai;

                var result = await _hoaDonService.UpdateAsync(hoaDon);
                if (!result)
                    return BadRequest("Cập nhật hóa đơn thất bại");

                return Ok(new
                {
                    message = "Cập nhật hóa đơn thành công",
                    hoa_don = await _hoaDonService.GetHoaDonBanTaiQuayByIdAsync(hoaDon.id_hoa_don, id_nguoi_sua.Value)
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        private Guid? GetIdNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                return null;
            return _jwtService.GetIdNhanVienFromToken(token);
        }
    }
}