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
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.DbConects.DTOs.Client.HoaDon;
using System.ComponentModel.DataAnnotations;
using API.Services;
namespace API.Controllers.HoaDon_Controller
{
    [Route("api/[controller]")]
    [ApiController]

    public class HoaDonController : ControllerBase
    {
        private readonly IHoaDonService _hoaDonService;
        private readonly IBaseService<HoaDonChiTiet> _hoaDonChiTietService;
        private readonly ISanPhamService _sanPhamService;
        private readonly IBaseService<PhuongThucThanhToan> _phuongThucThanhToanService;
        private readonly IBaseService<SanPhamChiTiet> _sanPhamChiTietService;
        private readonly IBaseService<KhuyenMai> _khuyenMaiService;
        private readonly IBaseService<GiamGia> _giamGiaService;
        private readonly IBaseService<NhanVien> _nhanVienService;
        private readonly IJwtServices _jwtService;
        private readonly IKhachHangService _khachHangService;
        private readonly VNPayService _vnPayService;

        public HoaDonController(
            IHoaDonService hoaDonService,
            IJwtServices jwtService,
            IBaseService<HoaDonChiTiet> hoaDonChiTietService,
            ISanPhamService sanPhamService,
            IBaseService<SanPhamChiTiet> sanPhamChiTietService,
            IBaseService<KhuyenMai> khuyenMaiService,
            IBaseService<GiamGia> giamGiaService,
            IKhachHangService khachHangService,
            IBaseService<PhuongThucThanhToan> phuongThucThanhToanService,
            IBaseService<NhanVien> nhanVienService,
            VNPayService vnPayService)
        {
            _hoaDonService = hoaDonService;
            _jwtService = jwtService;
            _hoaDonChiTietService = hoaDonChiTietService;
            _sanPhamService = sanPhamService;
            _sanPhamChiTietService = sanPhamChiTietService;
            _khuyenMaiService = khuyenMaiService;
            _giamGiaService = giamGiaService;
            _khachHangService = khachHangService;
            _phuongThucThanhToanService = phuongThucThanhToanService;
            _nhanVienService = nhanVienService;
            _vnPayService = vnPayService;
        }
        //lấy thông tin chi tiết hóa đơn
        [HttpGet("{id_hoa_don}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id_hoa_don)
        {
            try
            {
                var id_nhan_vien = GetIdNhanVien();
                if (id_nhan_vien == null)
                    return Unauthorized();
                var nhanVien = await _nhanVienService.GetByIdWithIncludeAsync(id_nhan_vien.Value, q => q.Include(x => x.TaiKhoanNhanVien));
                if (nhanVien.TaiKhoanNhanVien.chuc_vu == "Admin" || nhanVien.TaiKhoanNhanVien.chuc_vu == "NhanVien")
                {
                    var result = await _hoaDonService.GetByIdHoaDonAdminDTOAsync(id_hoa_don);
                    if (result == null)
                    {
                        return NotFound("Không tìm thấy hóa đơn");
                    }
                    return Ok(result);
                }
                else
                {
                    var hoadon = await _hoaDonService.GetByIdAsync(id_hoa_don);
                    if (hoadon == null)
                    {
                        return NotFound("Không tìm thấy hóa đơn");
                    }
                    if (hoadon.id_khach_hang != id_nhan_vien.Value)
                    {
                        return Unauthorized();
                    }
                    return Ok(hoadon);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //lấy danh sách hóa đơn bán tại quầy có trạng thái 'ChoTaiQuay'
        [HttpGet("lay-danh-sach-hoa-don-ban-tai-quay-co-trang-thai-cho-tai-quay")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> LayDanhSachHoaDonBanTaiQuayCoTrangThaiChoTaiQuay()
        {
            try
            {
                var id_nguoi_lay = GetIdNhanVien();
                if (id_nguoi_lay == null)
                    return Unauthorized();

                var result = await _hoaDonService.GetAllHoaDonAdminDTOAsync();
                var hoaDonBanTaiQuay = result.Where(x => x.loai_hoa_don == "TaiQuay" &&
                                                        x.trang_thai == "ChoTaiQuay" &&
                                                        x.nhanVienXuLy.id_nhan_vien == id_nguoi_lay)
                                            .OrderBy(x => x.ngay_tao) // Sắp xếp theo ngày tạo tăng dần
                                            .ToList();

                // Kiểm tra và xử lý hóa đơn quá hạn
                var hoaDonQuaHan = hoaDonBanTaiQuay.Where(x => (DateTime.Now - x.ngay_tao).TotalDays >= 1).ToList();

                if (hoaDonQuaHan.Any())
                {
                    foreach (var hoaDon in hoaDonQuaHan)
                    {
                        try
                        {
                            // Thực hiện trong transaction để đảm bảo tính nhất quán
                            var success = await _hoaDonService.ExecuteInTransactionAsync(async () =>
                            {
                                // Lấy thông tin chi tiết hóa đơn với đầy đủ thông tin liên quan
                                var hoaDonEntity = await _hoaDonService.GetByIdWithIncludeAsync(hoaDon.id_hoa_don,
                                    q => q.Include(hd => hd.HoaDonChiTiets)
                                         .ThenInclude(hct => hct.SanPhamChiTiet)
                                         .Include(hd => hd.KhuyenMai));

                                if (hoaDonEntity == null || hoaDonEntity.trang_thai_hoa_don != "ChoTaiQuay")
                                    return false;

                                // Hoàn trả số lượng sản phẩm
                                foreach (var chiTiet in hoaDonEntity.HoaDonChiTiets)
                                {
                                    if (chiTiet.SanPhamChiTiet != null)
                                    {
                                        chiTiet.SanPhamChiTiet.so_luong += chiTiet.so_luong;
                                        var updateResult = await _sanPhamChiTietService.UpdateAsync(chiTiet.SanPhamChiTiet);
                                        if (!updateResult) return false;
                                    }
                                }

                                // Giảm số lượng sử dụng khuyến mãi nếu có
                                if (hoaDonEntity.id_khuyen_mai.HasValue)
                                {
                                    var khuyenMai = await _khuyenMaiService.GetByIdAsync(hoaDonEntity.id_khuyen_mai.Value);
                                    if (khuyenMai != null)
                                    {
                                        khuyenMai.so_luong_da_su_dung = Math.Max(0, khuyenMai.so_luong_da_su_dung - 1);
                                        var updateResult = await _khuyenMaiService.UpdateAsync(khuyenMai);
                                        if (!updateResult) return false;
                                    }
                                }

                                // Xóa hóa đơn và các chi tiết liên quan
                                var deleteResult = await _hoaDonService.XoaHoaDon(hoaDon.id_hoa_don);
                                return deleteResult.Item1;
                            });

                            if (!success)
                            {
                                // Log lỗi nhưng không dừng xử lý các hóa đơn khác
                                Console.WriteLine($"Không thể xử lý hóa đơn quá hạn {hoaDon.ma_hoa_don}");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log lỗi nhưng không dừng xử lý các hóa đơn khác
                            Console.WriteLine($"Lỗi khi xử lý hóa đơn {hoaDon.ma_hoa_don}: {ex.Message}");
                            continue;
                        }
                    }

                    // Lấy lại danh sách sau khi xử lý
                    result = await _hoaDonService.GetAllHoaDonAdminDTOAsync();
                    hoaDonBanTaiQuay = result.Where(x => x.loai_hoa_don == "TaiQuay" &&
                                                        x.trang_thai == "ChoTaiQuay" &&
                                                        x.nhanVienXuLy.id_nhan_vien == id_nguoi_lay)
                                            .OrderBy(x => x.ngay_tao) // Sắp xếp theo ngày tạo tăng dần
                                            .ToList();
                }

                return Ok(
                    hoaDonBanTaiQuay
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Đã xảy ra lỗi: {ex.Message}"
                });
            }
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
        [Authorize(Roles = "Admin,NhanVien")]
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
                    hoaDon.sdt_khach_hang = khachHang.so_dien_thoai;
                }
                else
                {
                    // Xóa thông tin khách hàng
                    hoaDon.id_khach_hang = null;
                    hoaDon.dia_chi_nhan_hang = null;
                    hoaDon.ten_khach_hang = null;
                    hoaDon.sdt_khach_hang = null;
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
                if (dto.so_tien_khach_tra != null)
                {
                    hoaDon.so_tien_khach_tra = dto.so_tien_khach_tra;
                    hoaDon.so_tien_thua_tra_khach = Math.Max(0m, (decimal)(dto.so_tien_khach_tra - hoaDon.tong_tien_phai_thanh_toan));
                }
                if (!string.IsNullOrEmpty(dto.id_phuong_thuc_thanh_toan))
                {
                    var phuongThucThanhToan = await _phuongThucThanhToanService.GetByIdAsync(Guid.Parse(dto.id_phuong_thuc_thanh_toan));
                    if (phuongThucThanhToan == null)
                        return NotFound("Không tìm thấy phương thức thanh toán");
                    hoaDon.id_phuong_thuc_thanh_toan = Guid.Parse(dto.id_phuong_thuc_thanh_toan);
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
        [HttpPut("thanh-toan-hoa-don-cho-tai-quay/{id_hoa_don}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> ThanhToanHoaDonChoTaiQuay(Guid id_hoa_don)
        {
            try
            {
                var id_nguoi_thanh_toan = GetIdNhanVien();
                if (id_nguoi_thanh_toan == null)
                    return Unauthorized();
                var (success, message) = await _hoaDonService.ThanhToanHoaDonChoTaiQuay(id_hoa_don);
                if (!success)
                    return BadRequest(message);

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        [HttpGet("lay-danh-sach-hoa-don")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> LayDanhSachHoaDon([FromQuery] ThamSoPhanTrangHoaDonAdminDTO thamSo)
        {
            try
            {
                var danhSachHoaDon = (await _hoaDonService.GetAllHoaDonAdminDTOAsync()).OrderByDescending(x => x.ngay_tao).ToList();

                // Áp dụng bộ lọc
                if (!string.IsNullOrEmpty(thamSo.tim_kiem))
                {
                    danhSachHoaDon = danhSachHoaDon.Where(hd =>
                        hd.ma_hoa_don?.Contains(thamSo.tim_kiem, StringComparison.OrdinalIgnoreCase) == true ||
                        hd.ten_khach_hang?.Contains(thamSo.tim_kiem, StringComparison.OrdinalIgnoreCase) == true ||
                        hd.sdt_khach_hang?.Contains(thamSo.tim_kiem, StringComparison.OrdinalIgnoreCase) == true ||
                        hd.khachHang?.ma_khach_hang?.Contains(thamSo.tim_kiem, StringComparison.OrdinalIgnoreCase) == true ||
                        (hd.nhanVienXuLy?.ma_nhan_vien?.Contains(thamSo.tim_kiem, StringComparison.OrdinalIgnoreCase) == true) ||
                        hd.ten_nguoi_xu_ly?.Contains(thamSo.tim_kiem, StringComparison.OrdinalIgnoreCase) == true

                    ).ToList();
                }

                if (!string.IsNullOrEmpty(thamSo.trang_thai))
                {
                    danhSachHoaDon = danhSachHoaDon.Where(hd => hd.trang_thai == thamSo.trang_thai).ToList();
                }

                if (!string.IsNullOrEmpty(thamSo.loai_hoa_don))
                {
                    danhSachHoaDon = danhSachHoaDon.Where(hd => hd.loai_hoa_don == thamSo.loai_hoa_don).ToList();
                }

                if (!string.IsNullOrEmpty(thamSo.id_phuong_thuc_thanh_toan))
                {
                    danhSachHoaDon = danhSachHoaDon.Where(hd => hd.id_phuong_thuc_thanh_toan == thamSo.id_phuong_thuc_thanh_toan).ToList();
                }

                if (!string.IsNullOrEmpty(thamSo.ngay_tao_tu))
                {
                    var ngayTaoTu = DateTime.Parse(thamSo.ngay_tao_tu);
                    danhSachHoaDon = danhSachHoaDon.Where(hd => hd.ngay_tao.Date >= ngayTaoTu.Date).ToList();
                }

                if (!string.IsNullOrEmpty(thamSo.ngay_tao_den))
                {
                    var ngayTaoDen = DateTime.Parse(thamSo.ngay_tao_den);
                    danhSachHoaDon = danhSachHoaDon.Where(hd => hd.ngay_tao.Date <= ngayTaoDen.Date).ToList();
                }

                // Tính toán phân trang
                var tongSoPhanTu = danhSachHoaDon.Count;
                var tongSoTrang = (int)Math.Ceiling(tongSoPhanTu / (double)thamSo.so_phan_tu_tren_trang);
                thamSo.trang_hien_tai = Math.Max(1, Math.Min(thamSo.trang_hien_tai, tongSoTrang));

                // Lấy dữ liệu cho trang hiện tại
                var danhSachPhanTrang = danhSachHoaDon
                    .Skip((thamSo.trang_hien_tai - 1) * thamSo.so_phan_tu_tren_trang)
                    .Take(thamSo.so_phan_tu_tren_trang)
                    .ToList();

                var ketQua = new PhanTrangHoaDonAdminDTO
                {
                    trang_hien_tai = thamSo.trang_hien_tai,
                    so_phan_tu_tren_trang = thamSo.so_phan_tu_tren_trang,
                    tong_so_trang = tongSoTrang,
                    tong_so_phan_tu = tongSoPhanTu,
                    danh_sach = danhSachPhanTrang
                };

                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        [HttpGet("in-hoa-don/{id_hoa_don}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> InHoaDon(Guid id_hoa_don)
        {
            try
            {
                var id_nhan_vien = GetIdNhanVien();
                if (id_nhan_vien == null)
                    return Unauthorized();

                var hoaDon = await _hoaDonService.GetByIdHoaDonAdminDTOAsync(id_hoa_don);
                if (hoaDon == null)
                    return NotFound("Không tìm thấy hóa đơn");

                // Format the invoice content
                var invoiceContent = new
                {
                    ThongTinCuaHang = new
                    {
                        Logo = hoaDon.cuaHang?.hinh_anh_logo_cua_hang_url,
                        TenCuaHang = hoaDon.cuaHang?.ten_cua_hang ?? "FIFTY STORE",
                        DiaChi = hoaDon.cuaHang?.dia_chi ?? "Địa chỉ cửa hàng",
                        DienThoai = hoaDon.cuaHang?.sdt ?? "Số điện thoại",
                        Website = hoaDon.cuaHang?.website ?? "Website",
                        Email = hoaDon.cuaHang?.email ?? "Email"
                    },
                    ThongTinHoaDon = new
                    {
                        SoHoaDon = hoaDon.ma_hoa_don,
                        NgayLap = hoaDon.ngay_tao.ToString("dd/MM/yyyy HH:mm:ss"),
                        NhanVienBanHang = hoaDon.ten_nguoi_xu_ly,
                        MaNhanVien = hoaDon.nhanVienXuLy?.ma_nhan_vien,
                        MaKhuyenMai = hoaDon.khuyenMai?.ma_khuyen_mai
                    },
                    ThongTinKhachHang = new
                    {
                        TenKhachHang = hoaDon.ten_khach_hang,
                        MaKhachHang = hoaDon.khachHang?.ma_khach_hang ?? "Khách lẻ",
                        SoDienThoai = hoaDon.sdt_khach_hang ?? "Không có",
                        DiaChiGiaoHang = hoaDon.dia_chi_nhan_hang ?? "Mua tại cửa hàng"
                    },
                    ChiTietHoaDon = hoaDon.hoaDonChiTiets?.Select(ct => new
                    {
                        TenSanPham = ct.sanPhamChiTiet.ten_san_pham,
                        MauSac = ct.sanPhamChiTiet.ten_mau_sac,
                        KichCo = ct.sanPhamChiTiet.ten_kich_co,
                        SoLuong = ct.so_luong,
                        DonGia = ct.don_gia,
                        GiaSauGiamGia = ct.gia_sau_giam_gia,
                        ThanhTien = ct.thanh_tien,
                        maSPCT = ct.sanPhamChiTiet.ma_san_pham_chi_tiet
                    }).ToList(),
                    ThongTinThanhToan = new
                    {
                        TongTienHang = hoaDon.tong_tien_don_hang,
                        GiamGia = hoaDon.so_tien_khuyen_mai ?? 0,
                        TongThanhToan = hoaDon.tong_tien_phai_thanh_toan,
                        PhuongThucThanhToan = hoaDon.ten_phuong_thuc_thanh_toan,
                        TienKhachTra = hoaDon.so_tien_khach_tra ?? 0,
                        TienThua = hoaDon.so_tien_thua_tra_khach ?? 0
                    },
                    GhiChu = hoaDon.ghi_chu ?? "Không có ghi chú"
                };

                return Ok(new
                {
                    success = true,
                    data = invoiceContent,
                    message = "Lấy thông tin hóa đơn thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
        private Guid? GetIdNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                return null;
            return _jwtService.GetIdNhanVienFromToken(token);
        }
        private Guid? GetIdKhachHang()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                return null;
            return _jwtService.GetIdKhachHangFromToken(token);
        }


        [HttpPost("tao-hoa-don-online")]
        [Authorize(Roles = "KhachHang")]
        public async Task<IActionResult> TaoHoaDonOnline([FromBody] PhiVanChuyenRequest request)
        {
            try
            {
                var idKhachHang = GetIdKhachHang();
                if (idKhachHang == null)
                    return Unauthorized();
                var (success, message, id_hoa_don) = await _hoaDonService.TaoHoaDonOnlineTrangThaiChuaThanhToan(idKhachHang.Value, request.phi_van_chuyen);
                if (!success)
                    return BadRequest(message);

                return Ok(new
                {
                    message = message,
                    hoa_don = await _hoaDonService.GetByIdHoaDonAdminDTOAsync(id_hoa_don)
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        public class PhiVanChuyenRequest
        {
            public decimal phi_van_chuyen { get; set; }
        }
        //lấy thông tin chi tiết hóa đơn role khách hàng
        [HttpGet("lay-hoa-don-cua-khach-hang/{id_hoa_don}")]
        [Authorize(Roles = "KhachHang")]
        public async Task<IActionResult> GetByIdCuaKhachHang(Guid id_hoa_don)
        {
            try
            {
                var id_khach_hang = GetIdKhachHang();
                if (id_khach_hang == null)
                    return Unauthorized();
                var khachHang = await _khachHangService.GetByIdWithIncludeAsync(id_khach_hang.Value, q => q.Include(x => x.TaiKhoan));
                if (khachHang.TaiKhoan.chuc_vu == "KhachHang")
                {
                    var result = await _hoaDonService.GetByIdHoaDonAdminDTOAsync(id_hoa_don);
                    if (result == null)
                    {
                        return NotFound("Không tìm thấy hóa đơn");
                    }
                    return Ok(result);
                }
                else
                {
                    var hoadon = await _hoaDonService.GetByIdHoaDonAdminDTOAsync(id_hoa_don);
                    if (hoadon == null)
                    {
                        return NotFound("Không tìm thấy hóa đơn");
                    }
                    if (hoadon.id_khach_hang != id_khach_hang.Value)
                    {
                        return Unauthorized();
                    }
                    return Ok(hoadon);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật thông tin hóa đơn online
        /// </summary>
        /// <param name="id_hoa_don">ID của hóa đơn cần cập nhật</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Thông tin hóa đơn sau khi cập nhật</returns>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="404">Không tìm thấy hóa đơn</response>
        [HttpPut("cap-nhat-hoa-don-online/{id_hoa_don}")]
        [Authorize(Roles = "KhachHang")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CapNhatHoaDonOnline(Guid id_hoa_don, [FromBody] CapNhatHoaDonOnlineRequest request)
        {
            try
            {
                // Validate customer ID
                var idKhachHang = GetIdKhachHang();
                if (idKhachHang == null)
                    return Unauthorized("Không thể xác thực thông tin khách hàng");

                // Basic request validation
                if (request == null)
                    return BadRequest("Dữ liệu cập nhật không hợp lệ");

                if (request.phi_van_chuyen < 0)
                    return BadRequest("Phí vận chuyển không được âm");

                // Validate order exists and belongs to customer
                var hoaDon = await _hoaDonService.GetByIdAsync(id_hoa_don);
                if (hoaDon == null)
                    return NotFound("Không tìm thấy hóa đơn");

                if (hoaDon.id_khach_hang != idKhachHang)
                    return Unauthorized("Bạn không có quyền cập nhật hóa đơn này");

                if (hoaDon.trang_thai_hoa_don != "ChuaThanhToan")
                    return BadRequest("Chỉ có thể cập nhật hóa đơn ở trạng thái chưa thanh toán");

                if (hoaDon.loai_hoa_don != "Online")
                    return BadRequest("Chỉ có thể cập nhật hóa đơn online");

                // Validate shipping address if provided
                if (!string.IsNullOrEmpty(request.id_dia_chi_nhan_hang))
                {
                    var diaChiExists = await _khachHangService.GetByIdWithIncludeAsync(idKhachHang.Value,
                        q => q.Include(x => x.DiaChis));

                    if (diaChiExists?.DiaChis == null ||
                        !diaChiExists.DiaChis.Any(d => d.id_dia_chi.ToString() == request.id_dia_chi_nhan_hang))
                    {
                        return BadRequest("Địa chỉ nhận hàng không hợp lệ");
                    }
                }

                // Validate payment method if provided
                if (!string.IsNullOrEmpty(request.id_phuong_thuc_thanh_toan))
                {
                    var phuongThucThanhToan = await _phuongThucThanhToanService.GetByIdAsync(
                        Guid.Parse(request.id_phuong_thuc_thanh_toan));

                    if (phuongThucThanhToan == null)
                        return BadRequest("Phương thức thanh toán không tồn tại");
                }

                // Validate promotion if provided
                if (!string.IsNullOrEmpty(request.id_khuyen_mai))
                {
                    var khuyenMai = await _khuyenMaiService.GetByIdAsync(Guid.Parse(request.id_khuyen_mai));
                    if (khuyenMai == null)
                        return BadRequest("Khuyến mãi không tồn tại");

                    if (khuyenMai.trang_thai != "HoatDong")
                        return BadRequest("Khuyến mãi không còn hoạt động");

                    if (khuyenMai.thoi_gian_bat_dau > DateTime.Now)
                        return BadRequest("Khuyến mãi chưa đến thời gian áp dụng");

                    if (khuyenMai.thoi_gian_ket_thuc < DateTime.Now)
                        return BadRequest("Khuyến mãi đã hết thời gian áp dụng");

                    if (khuyenMai.so_luong_da_su_dung >= khuyenMai.so_luong_toi_da)
                        return BadRequest("Khuyến mãi đã hết lượt sử dụng");

                    // Kiểm tra giá trị đơn hàng tối thiểu
                    if (hoaDon.tong_tien_don_hang < khuyenMai.gia_tri_don_hang_toi_thieu)
                        return BadRequest(
                            $"Giá trị đơn hàng chưa đạt giá trị tối thiểu để áp dụng khuyến mãi. " +
                            $"Tối thiểu: {khuyenMai.gia_tri_don_hang_toi_thieu:N0} VNĐ");
                }

                // Update order
                var result = await _hoaDonService.CapNhatHoaDonOnline(
                    id_hoa_don,
                    request.id_dia_chi_nhan_hang,
                    request.ghi_chu,
                    request.id_khuyen_mai,
                    request.id_phuong_thuc_thanh_toan,
                    request.phi_van_chuyen
                );

                if (!result.success)
                    return BadRequest(result.message);

                // Get updated order details
                var hoaDonCapNhat = await _hoaDonService.GetByIdHoaDonAdminDTOAsync(id_hoa_don);

                return Ok(new
                {
                    message = result.message,
                    hoa_don = hoaDonCapNhat
                });
            }
            catch (FormatException)
            {
                return BadRequest("Định dạng dữ liệu không hợp lệ");
            }
            catch (Exception ex)
            {
                // Log the error here
                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý. Vui lòng thử lại sau.");
            }
        }

        public class CapNhatHoaDonOnlineRequest
        {
            public string? id_dia_chi_nhan_hang { get; set; }
            public decimal phi_van_chuyen { get; set; }
            public string? ghi_chu { get; set; }
            public string? id_khuyen_mai { get; set; }
            public string? id_phuong_thuc_thanh_toan { get; set; }
        }

        /// <summary>
        /// Áp dụng mã khuyến mãi cho hóa đơn
        /// </summary>
        /// <param name="id_hoa_don">ID của hóa đơn</param>
        /// <param name="ma_khuyen_mai">Mã khuyến mãi</param>
        /// <returns>Thông tin hóa đơn sau khi áp dụng khuyến mãi</returns>
        /// <response code="200">Áp dụng khuyến mãi thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="404">Không tìm thấy hóa đơn hoặc khuyến mãi</response>
        [HttpPost("ap-dung-khuyen-mai/{id_hoa_don}")]
        [Authorize(Roles = "KhachHang")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApDungKhuyenMai(Guid id_hoa_don, [FromBody] ApDungKhuyenMaiRequest request)
        {
            try
            {
                // Validate customer ID
                var idKhachHang = GetIdKhachHang();
                if (idKhachHang == null)
                    return Unauthorized("Không thể xác thực thông tin khách hàng");

                // Validate order exists and belongs to customer
                var hoaDon = await _hoaDonService.GetByIdWithIncludeAsync(id_hoa_don,
                    q => q.Include(hd => hd.KhuyenMai));

                if (hoaDon == null)
                    return NotFound("Không tìm thấy hóa đơn");

                if (hoaDon.id_khach_hang != idKhachHang)
                    return Unauthorized("Bạn không có quyền cập nhật hóa đơn này");

                if (hoaDon.trang_thai_hoa_don != "ChuaThanhToan")
                    return BadRequest("Chỉ có thể áp dụng khuyến mãi cho hóa đơn chưa thanh toán");

                // Find promotion by code
                var khuyenMai = await _khuyenMaiService.GetFirstOrDefaultAsync(km =>
                    km.ma_khuyen_mai.ToLower() == request.ma_khuyen_mai.ToLower());

                if (khuyenMai == null)
                    return NotFound("Không tìm thấy mã khuyến mãi");

                if (khuyenMai.trang_thai != "HoatDong")
                    return BadRequest("Mã khuyến mãi không còn hoạt động");

                if (khuyenMai.thoi_gian_bat_dau > DateTime.Now)
                    return BadRequest("Mã khuyến mãi chưa đến thời gian áp dụng");

                if (khuyenMai.thoi_gian_ket_thuc < DateTime.Now)
                    return BadRequest("Mã khuyến mãi đã hết thời gian áp dụng");

                if (khuyenMai.so_luong_da_su_dung >= khuyenMai.so_luong_toi_da)
                    return BadRequest("Mã khuyến mãi đã hết lượt sử dụng");

                // Kiểm tra xem khách hàng đã sử dụng khuyến mãi này chưa
                var hoaDonDaSuDungKhuyenMai = await _hoaDonService.GetByConditionWithIncludeAsync(hd =>
                    hd.id_khach_hang == idKhachHang &&
                    hd.id_khuyen_mai == khuyenMai.id_khuyen_mai &&
                    hd.trang_thai_hoa_don != "DaHuy" && // Không tính các đơn đã hủy
                    hd.id_hoa_don != id_hoa_don); // Không tính đơn hiện tại

                if (hoaDonDaSuDungKhuyenMai.Any())
                    return BadRequest("Bạn đã sử dụng mã khuyến mãi này trước đó");

                // Kiểm tra giá trị đơn hàng tối thiểu
                if (hoaDon.tong_tien_don_hang < khuyenMai.gia_tri_don_hang_toi_thieu)
                    return BadRequest(
                        $"Giá trị đơn hàng chưa đạt giá trị tối thiểu để áp dụng khuyến mãi. " +
                        $"Tối thiểu: {khuyenMai.gia_tri_don_hang_toi_thieu:N0} VNĐ");

                // Remove old promotion if exists
                if (hoaDon.id_khuyen_mai.HasValue)
                {
                    var khuyenMaiCu = await _khuyenMaiService.GetByIdAsync(hoaDon.id_khuyen_mai.Value);
                    if (khuyenMaiCu != null)
                    {
                        khuyenMaiCu.so_luong_da_su_dung = Math.Max(0, khuyenMaiCu.so_luong_da_su_dung - 1);
                        await _khuyenMaiService.UpdateAsync(khuyenMaiCu);
                    }
                }

                // Apply new promotion
                hoaDon.id_khuyen_mai = khuyenMai.id_khuyen_mai;
                khuyenMai.so_luong_da_su_dung++;
                await _khuyenMaiService.UpdateAsync(khuyenMai);

                // Update order totals
                var (tongTienSauKhuyenMai, giaTriKhuyenMai) = await _hoaDonService.CapNhatTongTienVaGiaTriKhuyenMai(id_hoa_don);

                // Get updated order details
                var hoaDonCapNhat = await _hoaDonService.GetByIdHoaDonAdminDTOAsync(id_hoa_don);

                return Ok(new
                {
                    message = "Áp dụng mã khuyến mãi thành công",
                    hoa_don = hoaDonCapNhat
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý. Vui lòng thử lại sau.");
            }
        }
        //lấy hóa đơn theo mã
        [HttpGet("lay-hoa-don-theo-ma/{ma_hoa_don}")]
        [Authorize(Roles = "KhachHang")]
        public async Task<IActionResult> LayHoaDonTheoMa(string ma_hoa_don)
        {
            var hoaDon = await _hoaDonService.GetAllHoaDonAdminDTOAsync();
            var hoaDonTheoMa = hoaDon.FirstOrDefault(h => h.ma_hoa_don == ma_hoa_don);
            if (hoaDonTheoMa == null)
            {
                return NotFound("Không tìm thấy hóa đơn");
            }
            return Ok(hoaDonTheoMa);
        }

        public class ApDungKhuyenMaiRequest
        {
            [Required(ErrorMessage = "Mã khuyến mãi không được để trống")]
            public string ma_khuyen_mai { get; set; }
        }

        /// <summary>
        /// Xác nhận đặt hàng và chuyển hướng thanh toán nếu cần
        /// </summary>
        /// <param name="id_hoa_don">ID của hóa đơn cần xác nhận</param>
        /// <returns>Thông tin xác nhận hoặc URL thanh toán VNPay</returns>
        [HttpPost("xac-nhan-dat-hang/{id_hoa_don}")]
        [Authorize(Roles = "KhachHang")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> XacNhanDatHang(Guid id_hoa_don)
        {
            try
            {
                // Validate customer ID
                var idKhachHang = GetIdKhachHang();
                if (idKhachHang == null)
                    return Unauthorized("Không thể xác thực thông tin khách hàng");

                // Get order details
                var hoaDon = await _hoaDonService.GetByIdWithIncludeAsync(id_hoa_don,
                    q => q.Include(hd => hd.PhuongThucThanhToan)
                         .Include(hd => hd.HoaDonChiTiets)
                         .ThenInclude(hct => hct.SanPhamChiTiet).ThenInclude(spct => spct.SanPhamChiTietGiamGias).ThenInclude(spgg => spgg.GiamGia));

                if (hoaDon == null)
                    return NotFound("Không tìm thấy hóa đơn");

                if (hoaDon.id_khach_hang != idKhachHang)
                    return Unauthorized("Bạn không có quyền xác nhận hóa đơn này");

                if (hoaDon.trang_thai_hoa_don != "ChuaThanhToan")
                    return BadRequest("Hóa đơn không ở trạng thái chờ thanh toán");
                if (!hoaDon.HoaDonChiTiets.Any())
                    return BadRequest("Lỗi nhận sản phẩm trong giỏ hàng vui lòng tải lại!");
                // Validate order items
                foreach (var hoaDonChiTiet in hoaDon.HoaDonChiTiets)
                {
                    if (hoaDonChiTiet.SanPhamChiTiet.so_luong < hoaDonChiTiet.so_luong)
                        return BadRequest($"Sản phẩm {hoaDonChiTiet.ten_san_pham} - {hoaDonChiTiet.ten_mau_sac} - {hoaDonChiTiet.ten_kich_co} không đủ số lượng");

                    // Tìm giảm giá đang được áp dụng cho sản phẩm này
                    var giamGiaDangApDung = hoaDonChiTiet.SanPhamChiTiet.SanPhamChiTietGiamGias
                        .FirstOrDefault(gg => gg.GiamGia != null &&
                                            gg.GiamGia.trang_thai == "HoatDong" &&
                                            gg.GiamGia.thoi_gian_bat_dau <= DateTime.Now &&
                                            gg.GiamGia.thoi_gian_ket_thuc >= DateTime.Now);
                    if (giamGiaDangApDung != null)
                    {
                        hoaDonChiTiet.id_giam_gia_cua_sp = giamGiaDangApDung.id_giam_gia;
                        await _hoaDonChiTietService.UpdateAsync(hoaDonChiTiet);
                        var ggSP = await _giamGiaService.GetByIdAsync(giamGiaDangApDung.id_giam_gia);
                        if (ggSP != null)
                        {
                            ggSP.so_luong_da_su_dung += hoaDonChiTiet.so_luong;
                            await _giamGiaService.UpdateAsync(ggSP);
                        }
                    }
                }

                // Check payment method
                if (hoaDon.PhuongThucThanhToan?.ma_phuong_thuc_thanh_toan == "PTVNPAY")
                {
                    // Create transaction ID from timestamp and random number
                    var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                    var random = new Random();
                    var randomNum = random.Next(1000, 9999);
                    var transactionId = long.Parse($"{timestamp}{randomNum}");

                    // Create VNPay payment URL
                    var paymentUrl = _vnPayService.CreatePaymentUrl(
                        transactionId,
                        (long)hoaDon.tong_tien_phai_thanh_toan.Value,
                        $"Thanh toán đơn hàng {hoaDon.ma_hoa_don}"
                    );

                    // Lưu mã giao dịch vào ghi chú để đối chiếu sau này
                    hoaDon.ghi_chu = $"VNPay Transaction ID: {transactionId} - {hoaDon.ghi_chu}";
                    await _hoaDonService.UpdateAsync(hoaDon);

                    return Ok(new
                    {
                        redirect_url = paymentUrl,
                        message = "Vui lòng tiếp tục thanh toán qua VNPay"
                    });
                }

                // For COD or other payment methods
                hoaDon.trang_thai_hoa_don = "DangChoXuLy";
                hoaDon.ngay_sua = DateTime.Now;
                await _hoaDonService.GuiEmailCapNhatTrangThaiAsync(id_hoa_don, "DangChoXuLy");
                var updateResult = await _hoaDonService.UpdateAsync(hoaDon);
                if (!updateResult)
                    return BadRequest("Không thể cập nhật trạng thái hóa đơn");

                return Ok(new
                {
                    message = "Đặt hàng thành công",
                    hoa_don = await _hoaDonService.GetByIdHoaDonAdminDTOAsync(id_hoa_don)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi trong quá trình xử lý: " + ex.Message });
            }
        }

        [HttpDelete("xoa-hoa-don-chua-thanh-toan-qua-han")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> XoaHoaDonChuaThanhToanQuaHan()
        {
            try
            {
                var (success, message) = await _hoaDonService.XoaHoaDonChuaThanhToanQuaHan();
                if (!success)
                    return BadRequest(message);

                return Ok(message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        [HttpGet("lay-danh-sach-hoa-don-cua-khach-hang")]
        [Authorize(Roles = "KhachHang")]
        public async Task<IActionResult> LayDanhSachHoaDonCuaKhachHang([FromQuery] ThamSoPhanTrangHoaDonAdminDTO thamSo)
        {
            try
            {
                var idKhachHang = GetIdKhachHang();
                if (idKhachHang == null)
                    return Unauthorized("Không thể xác thực thông tin khách hàng");

                var danhSachHoaDon = (await _hoaDonService.GetAllHoaDonAdminDTOAsync())
                    .Where(x => x.id_khach_hang == idKhachHang)
                    .OrderByDescending(x => x.ngay_tao)
                    .ToList();

                // Áp dụng tìm kiếm
                if (!string.IsNullOrEmpty(thamSo.tim_kiem))
                {
                    var searchTerm = thamSo.tim_kiem.ToLower();
                    danhSachHoaDon = danhSachHoaDon.Where(hd =>
                        hd.ma_hoa_don.ToLower().Contains(searchTerm) ||
                        (hd.ten_khach_hang != null && hd.ten_khach_hang.ToLower().Contains(searchTerm)) ||
                        (hd.sdt_khach_hang != null && hd.sdt_khach_hang.ToLower().Contains(searchTerm)) ||
                        (hd.ma_hoa_don != null && hd.ma_hoa_don.ToLower().Contains(searchTerm)) ||
                        (hd.dia_chi_nhan_hang != null && hd.dia_chi_nhan_hang.ToLower().Contains(searchTerm))
                    ).ToList();
                }


                // Áp dụng bộ lọc
                if (!string.IsNullOrEmpty(thamSo.trang_thai))
                {
                    danhSachHoaDon = danhSachHoaDon.Where(hd => hd.trang_thai == thamSo.trang_thai).ToList();
                }

                if (!string.IsNullOrEmpty(thamSo.ngay_tao_tu))
                {
                    var ngayTaoTu = DateTime.Parse(thamSo.ngay_tao_tu);
                    danhSachHoaDon = danhSachHoaDon.Where(hd => hd.ngay_tao.Date >= ngayTaoTu.Date).ToList();
                }

                if (!string.IsNullOrEmpty(thamSo.ngay_tao_den))
                {
                    var ngayTaoDen = DateTime.Parse(thamSo.ngay_tao_den);
                    danhSachHoaDon = danhSachHoaDon.Where(hd => hd.ngay_tao.Date <= ngayTaoDen.Date).ToList();
                }

                // Tính toán phân trang
                var tongSoPhanTu = danhSachHoaDon.Count;
                var tongSoTrang = (int)Math.Ceiling(tongSoPhanTu / (double)thamSo.so_phan_tu_tren_trang);
                thamSo.trang_hien_tai = Math.Max(1, Math.Min(thamSo.trang_hien_tai, tongSoTrang));

                // Lấy dữ liệu cho trang hiện tại
                var danhSachPhanTrang = danhSachHoaDon
                    .Skip((thamSo.trang_hien_tai - 1) * thamSo.so_phan_tu_tren_trang)
                    .Take(thamSo.so_phan_tu_tren_trang)
                    .ToList();

                var ketQua = new PhanTrangHoaDonAdminDTO
                {
                    trang_hien_tai = thamSo.trang_hien_tai,
                    so_phan_tu_tren_trang = thamSo.so_phan_tu_tren_trang,
                    tong_so_trang = tongSoTrang,
                    tong_so_phan_tu = tongSoPhanTu,
                    danh_sach = danhSachPhanTrang
                };

                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }


        /// <summary>
        /// Xác nhận đơn hàng (chuyển từ DangChoXuLy sang DaXacNhan)
        /// </summary>
        [HttpPut("xac-nhan-don-hang/{id_hoa_don}")]
        [Authorize(Roles = "Admin,NhanVien")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> XacNhanDonHang(Guid id_hoa_don)
        {
            try
            {
                var id_nhan_vien = GetIdNhanVien();
                if (id_nhan_vien == null)
                    return Unauthorized("Không thể xác thực thông tin nhân viên");

                var (success, message) = await _hoaDonService.XacNhanDonHangAsync(id_hoa_don, id_nhan_vien.Value);
                if (!success)
                    return BadRequest(message);

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Đánh dấu đơn hàng là hết hàng
        /// </summary>
        [HttpPut("danh-dau-het-hang/{id_hoa_don}")]
        [Authorize(Roles = "Admin,NhanVien")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DanhDauHetHang(Guid id_hoa_don)
        {
            try
            {
                var id_nhan_vien = GetIdNhanVien();
                if (id_nhan_vien == null)
                    return Unauthorized("Không thể xác thực thông tin nhân viên");

                var (success, message) = await _hoaDonService.DanhDauHetHangAsync(id_hoa_don, id_nhan_vien.Value);
                if (!success)
                    return BadRequest(message);

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        /// <summary>
        /// Cập nhật trạng thái giao hàng (DangChuanBi, DangGiaoHang, DaNhanHang, DaHoanThanh)
        /// </summary>
        [HttpPut("cap-nhat-trang-thai-giao-hang/{id_hoa_don}")]
        [Authorize(Roles = "Admin,NhanVien")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CapNhatTrangThaiGiaoHang(Guid id_hoa_don, [FromBody] TrangThaiRequest request)
        {
            try
            {
                var id_nhan_vien = GetIdNhanVien();
                if (id_nhan_vien == null)
                    return Unauthorized("Không thể xác thực thông tin nhân viên");

                var (success, message) = await _hoaDonService.CapNhatTrangThaiDonHangAsync(id_hoa_don, request.trang_thai, id_nhan_vien.Value);
                if (!success)
                    return BadRequest(message);

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        public class TrangThaiRequest
        {
            [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
            [RegularExpression("^(DangChuanBi|DangGiaoHang|DaNhanHang|DaHoanThanh)$",
                ErrorMessage = "Trạng thái không hợp lệ. Chỉ chấp nhận: DangChuanBi, DangGiaoHang, DaNhanHang, DaHoanThanh")]
            public string trang_thai { get; set; }
        }

        /// <summary>
        /// Hủy đơn hàng bởi admin hoặc nhân viên
        /// </summary>
        [HttpPut("huy-don-hang-admin/{id_hoa_don}")]
        [Authorize(Roles = "Admin,NhanVien")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> HuyDonHangAdmin(Guid id_hoa_don, [FromBody] HuyDonRequest request)
        {
            try
            {
                var id_nhan_vien = GetIdNhanVien();
                if (id_nhan_vien == null)
                    return Unauthorized("Không thể xác thực thông tin nhân viên");

                // Admin/nhân viên hủy đơn hàng (isKhachHangHuy = false)
                var (success, message) = await _hoaDonService.HuyDonHangAsync(id_hoa_don, request.ly_do, false, id_nhan_vien.Value);
                if (!success)
                    return BadRequest(message);

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Hủy đơn hàng bởi khách hàng
        /// </summary>
        [HttpPut("huy-don-hang-khach-hang/{id_hoa_don}")]
        [Authorize(Roles = "KhachHang")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> HuyDonHangKhachHang(Guid id_hoa_don, [FromBody] HuyDonRequest request)
        {
            try
            {
                var id_khach_hang = GetIdKhachHang();
                if (id_khach_hang == null)
                    return Unauthorized("Không thể xác thực thông tin khách hàng");

                // Kiểm tra xem đơn hàng có thuộc về khách hàng này không
                var hoaDon = await _hoaDonService.GetByIdAsync(id_hoa_don);
                if (hoaDon == null)
                    return NotFound("Không tìm thấy đơn hàng");

                if (hoaDon.id_khach_hang != id_khach_hang)
                    return Unauthorized("Bạn không có quyền hủy đơn hàng này");

                // Khách hàng hủy đơn hàng (isKhachHangHuy = true)
                var (success, message) = await _hoaDonService.HuyDonHangAsync(id_hoa_don, request.ly_do, true);
                if (!success)
                    return BadRequest(message);

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        public class HuyDonRequest
        {
            [Required(ErrorMessage = "Vui lòng nhập lý do hủy đơn")]
            public string ly_do { get; set; }
        }

        /// <summary>
        /// Thủ công hoàn tiền VNPay cho đơn hàng
        /// </summary>
        [HttpPost("hoan-tien-vnpay/{id_hoa_don}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> HoanTienVNPay(Guid id_hoa_don)
        {
            try
            {
                var id_admin = GetIdNhanVien();
                if (id_admin == null)
                    return Unauthorized("Không thể xác thực thông tin admin");

                var (success, message) = await _hoaDonService.HoanTienVNPayAsync(id_hoa_don);
                if (!success)
                    return BadRequest(message);

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        [HttpPost("tra-hang-tai-quay/{id_hoa_don}")]
        [Authorize(Roles = "Admin,NhanVien")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TraHangTaiQuay(Guid id_hoa_don, [FromBody] TraHangTaiQuayRequest request)
        {
            try
            {
                var id_nhan_vien = GetIdNhanVien();
                if (id_nhan_vien == null)
                    return Unauthorized("Không thể xác thực thông tin nhân viên");

                // Kiểm tra hóa đơn tồn tại và lấy thông tin chi tiết
                var hoaDon = await _hoaDonService.GetByIdWithIncludeAsync(id_hoa_don,
                    q => q.Include(hd => hd.HoaDonChiTiets)
                         .ThenInclude(hct => hct.SanPhamChiTiet)
                         .Include(hd => hd.KhuyenMai));

                if (hoaDon == null)
                    return NotFound("Không tìm thấy hóa đơn");

                // Kiểm tra trạng thái hóa đơn
                if (hoaDon.trang_thai_hoa_don != "DaHoanThanh" && hoaDon.trang_thai_hoa_don != "DaThanhToan")
                    return BadRequest("Chỉ có thể trả hàng cho đơn hàng đã hoàn thành");

                // Kiểm tra thời gian trả hàng (trong vòng 7 ngày)
                if ((DateTime.Now - hoaDon.ngay_tao).TotalDays > 7)
                    return BadRequest("Đã quá thời gian cho phép trả hàng (7 ngày)");

                // Thực hiện trả hàng trong transaction
                var result = await _hoaDonService.ExecuteInTransactionAsync(async () =>
                {
                    // Cập nhật số lượng sản phẩm
                    foreach (var chiTiet in hoaDon.HoaDonChiTiets)
                    {
                        if (chiTiet.SanPhamChiTiet != null)
                        {
                            chiTiet.SanPhamChiTiet.so_luong += chiTiet.so_luong;
                            var updateResult = await _sanPhamChiTietService.UpdateAsync(chiTiet.SanPhamChiTiet);
                            if (!updateResult) return false;
                        }
                        chiTiet.trang_thai = "DaTraHang";
                        await _hoaDonChiTietService.UpdateAsync(chiTiet);

                    }

                    // Giảm số lượng sử dụng khuyến mãi nếu có
                    if (hoaDon.id_khuyen_mai.HasValue)
                    {
                        var khuyenMai = await _khuyenMaiService.GetByIdAsync(hoaDon.id_khuyen_mai.Value);
                        if (khuyenMai != null)
                        {
                            khuyenMai.so_luong_da_su_dung = Math.Max(0, khuyenMai.so_luong_da_su_dung - 1);
                            var updateResult = await _khuyenMaiService.UpdateAsync(khuyenMai);
                            if (!updateResult) return false;
                        }
                    }
                    // Hoàn lại số lượng giảm giá đã sử dụng
                    var hoaDonChiTiets = await _hoaDonChiTietService.GetByConditionWithIncludeAsync(
                        hct => hct.id_hoa_don == hoaDon.id_hoa_don,
                        q => q.Include(hct => hct.SanPhamChiTiet)
                             .ThenInclude(spct => spct.SanPhamChiTietGiamGias)
                             .ThenInclude(spgg => spgg.GiamGia)
                    );

                    foreach (var chiTiet in hoaDonChiTiets)
                    {
                        if (chiTiet.SanPhamChiTiet?.SanPhamChiTietGiamGias != null)
                        {
                            // Tìm giảm giá đang được áp dụng cho sản phẩm này
                            var giamGiaDangApDung = chiTiet.SanPhamChiTiet.SanPhamChiTietGiamGias
                                .FirstOrDefault(gg => gg.GiamGia != null &&
                                                    gg.GiamGia.trang_thai == "HoatDong" &&
                                                    gg.GiamGia.thoi_gian_bat_dau <= DateTime.Now &&
                                                    gg.GiamGia.thoi_gian_ket_thuc >= DateTime.Now);

                            if (giamGiaDangApDung != null)
                            {
                                // Hoàn lại số lượng đã sử dụng của giảm giá
                                giamGiaDangApDung.GiamGia.so_luong_da_su_dung -= chiTiet.so_luong;
                                await _giamGiaService.UpdateAsync(giamGiaDangApDung.GiamGia);
                            }
                        }
                    }

                    // Cập nhật trạng thái hóa đơn
                    hoaDon.trang_thai_hoa_don = "DaTraHang";
                    hoaDon.ngay_sua = DateTime.Now;
                    hoaDon.id_nhan_vien_xu_ly = id_nhan_vien;
                    hoaDon.ly_do_tra_hang = $"{request.ly_do}";

                    var updateHoaDonResult = await _hoaDonService.UpdateAsync(hoaDon);
                    if (!updateHoaDonResult) return false;

                    // Gửi email thông báo
                    await _hoaDonService.GuiEmailCapNhatTrangThaiAsync(id_hoa_don, "DaTraHang");

                    return true;
                });

                if (!result)
                    return BadRequest("Không thể xử lý yêu cầu trả hàng");

                return Ok(new
                {
                    message = "Xử lý trả hàng thành công",
                    hoa_don = await _hoaDonService.GetByIdHoaDonAdminDTOAsync(id_hoa_don)
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        public class TraHangTaiQuayRequest
        {
            [Required(ErrorMessage = "Vui lòng nhập lý do trả hàng")]
            public string ly_do { get; set; }
        }

        [HttpPost("yeu-cau-tra-hang/{id_hoa_don}")]
        [Authorize(Roles = "KhachHang")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> YeuCauTraHang(Guid id_hoa_don, [FromForm] YeuCauTraHangRequest request)
        {
            var id_khach_hang = GetIdKhachHang();
            if (!id_khach_hang.HasValue)
                return Unauthorized();

            var result = await _hoaDonService.YeuCauTraHangAsync(id_hoa_don, id_khach_hang.Value, request.ly_do_tra_hang, request.hinh_anh_tra_hang);
            if (!result.success)
                return BadRequest(result.message);

            return Ok(result.message);
        }

        [HttpPut("xac-nhan-tra-hang/{id_hoa_don}")]
        [Authorize(Roles = "Admin,NhanVien")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> XacNhanTraHang(Guid id_hoa_don)
        {
            var id_nhan_vien = GetIdNhanVien();
            if (!id_nhan_vien.HasValue)
                return Unauthorized();

            var result = await _hoaDonService.XacNhanTraHangAsync(id_hoa_don, id_nhan_vien.Value);
            if (!result.success)
                return BadRequest(result.message);

            return Ok(result.message);
        }

        [HttpPut("hoan-thanh-tra-hang/{id_hoa_don}")]
        [Authorize(Roles = "Admin,NhanVien")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> HoanThanhTraHang(Guid id_hoa_don)
        {
            var id_nhan_vien = GetIdNhanVien();
            if (!id_nhan_vien.HasValue)
                return Unauthorized();

            var result = await _hoaDonService.HoanThanhTraHangAsync(id_hoa_don, id_nhan_vien.Value);
            if (!result.success)
                return BadRequest(result.message);

            return Ok(result.message);
        }

        [HttpPut("tu-choi-tra-hang/{id_hoa_don}")]
        [Authorize(Roles = "Admin,NhanVien")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TuChoiTraHang(Guid id_hoa_don, [FromBody] TuChoiTraHangRequest request)
        {
            try
            {
                var id_nhan_vien = GetIdNhanVien();
                if (!id_nhan_vien.HasValue)
                    return Unauthorized("Không thể xác thực thông tin nhân viên");

                var result = await _hoaDonService.TuChoiTraHangAsync(id_hoa_don, id_nhan_vien.Value, request.ly_do_tu_choi);
                if (!result.success)
                    return BadRequest(result.message);

                return Ok(new { message = result.message });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        public class TuChoiTraHangRequest
        {
            [Required(ErrorMessage = "Vui lòng nhập lý do từ chối trả hàng")]
            public string ly_do_tu_choi { get; set; }
        }

        public class YeuCauTraHangRequest
        {
            [Required(ErrorMessage = "Vui lòng nhập lý do trả hàng")]
            public string ly_do_tra_hang { get; set; }

            [Required(ErrorMessage = "Vui lòng cung cấp hình ảnh sản phẩm")]
            public IFormFile hinh_anh_tra_hang { get; set; }
        }
    }
}