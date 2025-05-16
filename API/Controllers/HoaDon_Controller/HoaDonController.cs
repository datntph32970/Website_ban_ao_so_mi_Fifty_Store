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

        public HoaDonController(IHoaDonService hoaDonService, IJwtServices jwtService, IBaseService<HoaDonChiTiet> hoaDonChiTietService, ISanPhamService sanPhamService, IBaseService<SanPhamChiTiet> sanPhamChiTietService, IBaseService<KhuyenMai> khuyenMaiService, IBaseService<GiamGia> giamGiaService, IKhachHangService khachHangService, IBaseService<PhuongThucThanhToan> phuongThucThanhToanService, IBaseService<NhanVien> nhanVienService)
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
        }
        //lấy thông tin chi tiết hóa đơn
        [HttpGet("{id_hoa_don}")]
        [Authorize(Roles = "Admin,NhanVien")]
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
                                                        x.nhanVienXuLy.id_nhan_vien == id_nguoi_lay).ToList();

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
                                                        x.nhanVienXuLy.id_nhan_vien == id_nguoi_lay).ToList();
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
                        MaNhanVien = hoaDon.nhanVienXuLy?.ma_nhan_vien
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
                        ThanhTien = ct.thanh_tien
                    }).ToList(),
                    ThongTinThanhToan = new
                    {
                        TongTienHang = hoaDon.tong_tien_don_hang,
                        GiamGia = hoaDon.so_tien_khuyen_mai ?? 0,
                        TongThanhToan = hoaDon.tong_tien_phai_thanh_toan,
                        PhuongThucThanhToan = hoaDon.phuong_thuc_thanh_toan ?? "Tiền mặt",
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
    }
}