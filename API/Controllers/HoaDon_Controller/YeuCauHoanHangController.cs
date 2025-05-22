using API.DbConects.DTOs.Admin.HoaDon;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_San_Pham;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.HoaDon_Controller
{
    // API/Controllers/HoaDon_Controller/YeuCauHoanHangController.cs
    [Route("api/[controller]")]
    [ApiController]
    public class YeuCauHoanHangController : ControllerBase
    {
        private readonly IBaseService<YeuCauHoanHang> _yeuCauHoanHangService;
        private readonly IBaseService<HoaDon> _hoaDonService;
        private readonly IBaseService<ChiTietHoanHang> _chiTietHoanHangService;
        private readonly IJwtServices _jwtServices;
        private readonly IBaseService<SanPhamChiTiet> _sanPhamCTService;
        private readonly IBaseService<HoaDonChiTiet> _hoaDonChiTietService;

        public YeuCauHoanHangController(
            IBaseService<YeuCauHoanHang> yeuCauHoanHangService,
            IBaseService<HoaDon> hoaDonService,
            IBaseService<ChiTietHoanHang> chiTietHoanHangService,
            IJwtServices jwtServices,
            IBaseService<SanPhamChiTiet> sanPhamCTService,
            IBaseService<HoaDonChiTiet> hoaDonChiTietService)
        {
            _yeuCauHoanHangService = yeuCauHoanHangService;
            _hoaDonService = hoaDonService;
            _chiTietHoanHangService = chiTietHoanHangService;
            _jwtServices = jwtServices;
            _sanPhamCTService = sanPhamCTService;
            _hoaDonChiTietService = hoaDonChiTietService;
        }
        private async Task<HinhAnh> SaveHinhAnh(string base64Image, string maHinhAnh, string folderPath)
        {
            var hinhAnh = new HinhAnh
            {
                id_hinh_anh = Guid.NewGuid(),
                ma_hinh_anh = maHinhAnh,
                id_nguoi_tao = (Guid)GetIdNhanVien(),
                ngay_tao = DateTime.Now
            };

            var fileName = $"{maHinhAnh}.jpg";
            var imagePath = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
            var imageData = Convert.FromBase64String(base64Image.Split(',')[1]);
            System.IO.File.WriteAllBytes(imagePath, imageData);
            hinhAnh.url = $"/images/yeu_cau_hoan_hang/{fileName}";

            return hinhAnh;
        }
        //lấy id nhân viên
        private Guid? GetIdNhanVien()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var idNhanVien = _jwtServices.GetIdNhanVienFromToken(token);
            return idNhanVien;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> TaoYeuCauHoanHang([FromBody] TaoYeuCauHoanHangDTO dto)
        {
            // Validate input
            if (dto == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (string.IsNullOrEmpty(dto.LyDoHoanHang))
                return BadRequest("Vui lòng nhập lý do hoàn hàng");

            if (dto.HinhAnhBase64 == null || !dto.HinhAnhBase64.Any())
                return BadRequest("Vui lòng tải lên ít nhất một hình ảnh sản phẩm");

            if (dto.ChiTietHoanHang == null || !dto.ChiTietHoanHang.Any())
                return BadRequest("Vui lòng chọn ít nhất một sản phẩm để hoàn hàng");

            // Kiểm tra hóa đơn
            var hoaDon = await _hoaDonService.GetByIdWithIncludeAsync(dto.IdHoaDon,
                q => q.Include(h => h.HoaDonChiTiets));

            if (hoaDon == null)
                return NotFound("Không tìm thấy hóa đơn");

            // Kiểm tra trạng thái hóa đơn
            if (hoaDon.trang_thai_hoa_don != "DaHoanThanh" && hoaDon.trang_thai_hoa_don != "DaNhanHang")
                return BadRequest("Chỉ có thể hoàn hàng với hóa đơn đã giao thành công hoặc đã nhận hàng");

            // Kiểm tra thời gian hoàn hàng (ví dụ: trong vòng 14 ngày)
            if ((DateTime.Now - hoaDon.ngay_tao).TotalDays > 14)
                return BadRequest("Hóa đơn đã quá thời hạn cho phép hoàn hàng (14 ngày)");

            var idKhachHang = GetIdKhachHang();
            if (idKhachHang == null)
                return Unauthorized("Không thể xác thực thông tin khách hàng");

            if (hoaDon.id_khach_hang != idKhachHang.Value)
                return BadRequest("Khách hàng không có quyền hoàn hàng");

            // Kiểm tra xem đã có yêu cầu hoàn hàng nào đang xử lý không
            var yeuCauDangXuLy = await _yeuCauHoanHangService.GetByConditionAsync(y =>
                y.IdHoaDon == dto.IdHoaDon &&
                (y.TrangThai == TrangThaiHoanHang.ChoXacNhan ||
                 y.TrangThai == TrangThaiHoanHang.DaXacNhan ||
                 y.TrangThai == TrangThaiHoanHang.DangXuLy));

            if (yeuCauDangXuLy.Any())
                return BadRequest("Đã có yêu cầu hoàn hàng đang được xử lý cho hóa đơn này");

            try
            {
                // Tạo yêu cầu hoàn hàng
                var yeuCauHoanHang = new YeuCauHoanHang
                {
                    Id = Guid.NewGuid(),
                    IdHoaDon = dto.IdHoaDon,
                    IdKhachHang = idKhachHang.Value,
                    LyDoHoanHang = dto.LyDoHoanHang,
                    MoTaChiTiet = dto.MoTaChiTiet,
                    TrangThai = TrangThaiHoanHang.ChoXacNhan,
                    NgayTao = DateTime.Now,
                    HinhAnhHoanHangs = new List<HinhAnhHoanHang>()
                };

                // Lưu các hình ảnh
                foreach (var hinhAnhBase64 in dto.HinhAnhBase64)
                {
                    var hinhAnh = await SaveHinhAnh(hinhAnhBase64, $"{hoaDon.ma_hoa_don}_HinhAnhYeuCauHoanHang_{Guid.NewGuid()}", "wwwroot/images/yeu_cau_hoan_hang");

                    yeuCauHoanHang.HinhAnhHoanHangs.Add(new HinhAnhHoanHang
                    {
                        Id = Guid.NewGuid(),
                        IdYeuCauHoanHang = yeuCauHoanHang.Id,
                        idHinhAnh = hinhAnh.id_hinh_anh
                    });
                }

                // Tính toán số tiền hoàn
                decimal tongTienHoan = 0;
                decimal tongTienHoaDon = hoaDon.HoaDonChiTiets.Sum(ct => ct.gia_sau_giam_gia * ct.so_luong);
                decimal tyLeKhuyenMai = hoaDon.so_tien_khuyen_mai > 0
                    ? (tongTienHoaDon - hoaDon.so_tien_khuyen_mai.Value) / tongTienHoaDon
                    : 1;

                foreach (var chiTiet in dto.ChiTietHoanHang)
                {
                    var sanPham = hoaDon.HoaDonChiTiets
                        .FirstOrDefault(ct => ct.id_san_pham_chi_tiet == chiTiet.id_san_pham_chi_tiet);

                    if (sanPham == null)
                        return BadRequest($"Không tìm thấy sản phẩm trong hóa đơn");

                    // Kiểm tra số lượng hoàn có hợp lệ không
                    if (chiTiet.SoLuong <= 0)
                        return BadRequest($"Số lượng hoàn phải lớn hơn 0");

                    if (chiTiet.SoLuong > sanPham.so_luong)
                        return BadRequest($"Số lượng hoàn vượt quá số lượng đã mua của sản phẩm");

                    // Tính giá trị hoàn trả cho sản phẩm này
                    decimal giaTriSanPham = sanPham.gia_sau_giam_gia * chiTiet.SoLuong;
                    decimal giaTriSauKhuyenMai = giaTriSanPham * tyLeKhuyenMai;
                    tongTienHoan += giaTriSauKhuyenMai;

                    // Tạo chi tiết hoàn hàng
                    var chiTietHoanHang = new ChiTietHoanHang
                    {
                        Id = Guid.NewGuid(),
                        IdYeuCauHoanHang = yeuCauHoanHang.Id,
                        id_san_pham_chi_tiet = chiTiet.id_san_pham_chi_tiet,
                        SoLuong = chiTiet.SoLuong,
                        DonGia = giaTriSauKhuyenMai / chiTiet.SoLuong,
                        LyDo = chiTiet.LyDo
                    };
                    await _chiTietHoanHangService.CreateAsync(chiTietHoanHang);
                }

                yeuCauHoanHang.SoTienHoan = tongTienHoan;
                await _yeuCauHoanHangService.CreateAsync(yeuCauHoanHang);

                return Ok(new
                {
                    Id = yeuCauHoanHang.Id,
                    TongTienHoan = tongTienHoan,
                    ChiTietHoanHang = dto.ChiTietHoanHang.Select(ct => new
                    {
                        IdSanPham = ct.id_san_pham_chi_tiet,
                        SoLuong = ct.SoLuong,
                        DonGia = hoaDon.HoaDonChiTiets
                            .First(x => x.id_san_pham_chi_tiet == ct.id_san_pham_chi_tiet)
                            .gia_sau_giam_gia * tyLeKhuyenMai
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                // Log lỗi
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu hoàn hàng");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetDanhSachYeuCau([FromQuery] ThamSoPhanTrangYeuCauHoanHangDTO thamSo)
        {
            try
            {
                // Validate input
                if (thamSo.trang_hien_tai < 1)
                    return BadRequest("Trang hiện tại phải lớn hơn 0");

                if (thamSo.so_phan_tu_tren_trang < 1)
                    return BadRequest("Số phần tử trên trang phải lớn hơn 0");

                if (thamSo.so_phan_tu_tren_trang > 100)
                    return BadRequest("Số phần tử trên trang không được vượt quá 100");

                // Validate date format
                DateTime? ngayTu = null;
                DateTime? ngayDen = null;

                if (!string.IsNullOrEmpty(thamSo.ngay_tao_tu))
                {
                    if (!DateTime.TryParse(thamSo.ngay_tao_tu, out DateTime parsedNgayTu))
                        return BadRequest("Định dạng ngày tạo từ không hợp lệ");
                    ngayTu = parsedNgayTu;
                }

                if (!string.IsNullOrEmpty(thamSo.ngay_tao_den))
                {
                    if (!DateTime.TryParse(thamSo.ngay_tao_den, out DateTime parsedNgayDen))
                        return BadRequest("Định dạng ngày tạo đến không hợp lệ");
                    ngayDen = parsedNgayDen;
                }

                if (ngayTu.HasValue && ngayDen.HasValue && ngayTu > ngayDen)
                    return BadRequest("Ngày tạo từ phải nhỏ hơn hoặc bằng ngày tạo đến");

                var yeuCauHoanHangs = await _yeuCauHoanHangService.GetAllWithIncludeAsync(
                    q => q.Include(y => y.HoaDon)
                         .Include(y => y.KhachHang)
                         .Include(y => y.ChiTietHoanHangs)
                         .ThenInclude(ct => ct.SanPhamChiTiet)
                         .Include(y => y.HinhAnhHoanHangs)
                         .ThenInclude(h => h.hinhAnh)
                );

                // Áp dụng bộ lọc
                if (!string.IsNullOrEmpty(thamSo.tim_kiem))
                {
                    var searchTerm = thamSo.tim_kiem.ToLower();
                    yeuCauHoanHangs = yeuCauHoanHangs.Where(y =>
                        y.HoaDon.ma_hoa_don.ToLower().Contains(searchTerm) ||
                        y.KhachHang.ten_khach_hang.ToLower().Contains(searchTerm) ||
                        y.KhachHang.so_dien_thoai.ToLower().Contains(searchTerm) ||
                        y.LyDoHoanHang.ToLower().Contains(searchTerm)
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(thamSo.trang_thai))
                {
                    if (!Enum.TryParse<TrangThaiHoanHang>(thamSo.trang_thai, out _))
                        return BadRequest("Trạng thái không hợp lệ");

                    yeuCauHoanHangs = yeuCauHoanHangs.Where(y => y.TrangThai.ToString() == thamSo.trang_thai).ToList();
                }

                if (ngayTu.HasValue)
                {
                    yeuCauHoanHangs = yeuCauHoanHangs.Where(y => y.NgayTao.Date >= ngayTu.Value.Date).ToList();
                }

                if (ngayDen.HasValue)
                {
                    yeuCauHoanHangs = yeuCauHoanHangs.Where(y => y.NgayTao.Date <= ngayDen.Value.Date).ToList();
                }

                // Phân trang
                var tongSoPhanTu = yeuCauHoanHangs.Count();
                var tongSoTrang = (int)Math.Ceiling((double)tongSoPhanTu / thamSo.so_phan_tu_tren_trang);
                var trangHienTai = Math.Max(1, Math.Min(thamSo.trang_hien_tai, tongSoTrang));

                var danhSach = yeuCauHoanHangs
                    .Skip((trangHienTai - 1) * thamSo.so_phan_tu_tren_trang)
                    .Take(thamSo.so_phan_tu_tren_trang)
                    .Select(y => new YeuCauHoanHangDTO
                    {
                        Id = y.Id,
                        MaHoaDon = y.HoaDon.ma_hoa_don,
                        TenKhachHang = y.KhachHang.ten_khach_hang,
                        LyDoHoanHang = y.LyDoHoanHang,
                        MoTaChiTiet = y.MoTaChiTiet,
                        HinhAnh = y.HinhAnhHoanHangs.Select(h => h.hinhAnh.url).ToList(),
                        SoTienHoan = y.SoTienHoan,
                        TrangThai = y.TrangThai,
                        NgayTao = y.NgayTao,
                        NgayCapNhat = y.NgayCapNhat,
                        GhiChu = y.GhiChu,
                        ChiTietHoanHang = y.ChiTietHoanHangs.Select(ct => new ChiTietHoanHangDTO
                        {
                            id_san_pham_chi_tiet = ct.id_san_pham_chi_tiet,
                            SoLuong = ct.SoLuong,
                            LyDo = ct.LyDo
                        }).ToList()
                    })
                    .ToList();

                return Ok(new PhanTrangYeuCauHoanHangDTO
                {
                    trang_hien_tai = trangHienTai,
                    so_phan_tu_tren_trang = thamSo.so_phan_tu_tren_trang,
                    tong_so_trang = tongSoTrang,
                    tong_so_phan_tu = tongSoPhanTu,
                    danh_sach = danhSach
                });
            }
            catch (Exception ex)
            {
                // Log lỗi
                return StatusCode(500, "Đã xảy ra lỗi khi lấy danh sách yêu cầu hoàn hàng");
            }
        }

        [HttpPut("{id}/trang-thai")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> CapNhatTrangThai(Guid id, [FromBody] CapNhatTrangThaiHoanHangDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Dữ liệu không hợp lệ");

                if (string.IsNullOrEmpty(dto.GhiChu) && dto.TrangThai == TrangThaiHoanHang.TuChoi)
                    return BadRequest("Vui lòng nhập lý do từ chối yêu cầu");

                var yeuCauHoanHang = await _yeuCauHoanHangService.GetByIdWithIncludeAsync(id,
                    q => q.Include(y => y.HoaDon)
                          .ThenInclude(h => h.HoaDonChiTiets)
                          .Include(y => y.ChiTietHoanHangs)
                          .ThenInclude(ct => ct.SanPhamChiTiet)
                          .Include(y => y.HinhAnhHoanHangs)
                          .ThenInclude(h => h.hinhAnh));

                if (yeuCauHoanHang == null)
                    return NotFound("Không tìm thấy yêu cầu hoàn hàng");

                // Kiểm tra trạng thái hiện tại
                if (yeuCauHoanHang.TrangThai == TrangThaiHoanHang.HoanThanh)
                    return BadRequest("Không thể cập nhật trạng thái của yêu cầu đã hoàn thành");

                if (yeuCauHoanHang.TrangThai == TrangThaiHoanHang.TuChoi)
                    return BadRequest("Không thể cập nhật trạng thái của yêu cầu đã bị từ chối");

                // Kiểm tra quyền chuyển trạng thái
                if (!IsValidStateTransition(yeuCauHoanHang.TrangThai, dto.TrangThai))
                    return BadRequest("Không thể chuyển sang trạng thái này từ trạng thái hiện tại");

                yeuCauHoanHang.TrangThai = dto.TrangThai;
                yeuCauHoanHang.NgayCapNhat = DateTime.Now;
                yeuCauHoanHang.GhiChu = dto.GhiChu;

                // Cập nhật trạng thái hóa đơn
                var hoaDon = yeuCauHoanHang.HoaDon;
                switch (dto.TrangThai)
                {
                    case TrangThaiHoanHang.DaXacNhan:
                        hoaDon.trang_thai_hoa_don = "DangXuLyHoanHang";
                        break;
                    case TrangThaiHoanHang.HoanThanh:
                        hoaDon.trang_thai_hoa_don = "DaHoanHang";
                        // Cập nhật tồn kho sản phẩm và trạng thái hóa đơn chi tiết
                        foreach (var chiTiet in yeuCauHoanHang.ChiTietHoanHangs)
                        {
                            var sanPhamCT = chiTiet.SanPhamChiTiet;
                            if (sanPhamCT == null)
                                continue;

                            // Kiểm tra chất lượng sản phẩm trước khi cập nhật tồn kho
                            if (string.IsNullOrEmpty(dto.GhiChu) || !dto.GhiChu.Contains("Sản phẩm còn nguyên vẹn"))
                            {
                                // Nếu sản phẩm không còn nguyên vẹn, không cập nhật tồn kho
                                continue;
                            }

                            // Cập nhật số lượng tồn kho
                            sanPhamCT.so_luong += chiTiet.SoLuong;

                            // Cập nhật trạng thái sản phẩm nếu cần
                            if (sanPhamCT.so_luong > 0 && sanPhamCT.trang_thai == "NgungHoatDong")
                            {
                                sanPhamCT.trang_thai = "ConHang";
                            }

                            await _sanPhamCTService.UpdateAsync(sanPhamCT);

                            // Cập nhật hóa đơn chi tiết
                            var hoaDonChiTiet = hoaDon.HoaDonChiTiets
                                .FirstOrDefault(hdct => hdct.id_san_pham_chi_tiet == chiTiet.id_san_pham_chi_tiet);

                            if (hoaDonChiTiet != null)
                            {
                                // Cập nhật số lượng đã hoàn trả
                                hoaDonChiTiet.so_luong_da_hoan_tra = (hoaDonChiTiet.so_luong_da_hoan_tra ?? 0) + chiTiet.SoLuong;

                                // Cập nhật trạng thái hoàn trả
                                if (hoaDonChiTiet.so_luong_da_hoan_tra >= hoaDonChiTiet.so_luong)
                                {
                                    hoaDonChiTiet.trang_thai_hoan_tra = "DaHoanTraToanBo";
                                }
                                else if (hoaDonChiTiet.so_luong_da_hoan_tra > 0)
                                {
                                    hoaDonChiTiet.trang_thai_hoan_tra = "DaHoanTraMotPhan";
                                }

                                await _hoaDonChiTietService.UpdateAsync(hoaDonChiTiet);
                            }
                        }

                        // Cập nhật trạng thái tổng thể của hóa đơn
                        var tongSoSanPham = hoaDon.HoaDonChiTiets.Count;
                        var soSanPhamDaHoanTra = hoaDon.HoaDonChiTiets
                            .Count(hdct => hdct.trang_thai_hoan_tra == "DaHoanTraToanBo");

                        if (soSanPhamDaHoanTra == tongSoSanPham)
                        {
                            hoaDon.trang_thai_hoa_don = "DaHoanTraToanBo";
                        }
                        else if (soSanPhamDaHoanTra > 0)
                        {
                            hoaDon.trang_thai_hoa_don = "DaHoanTraMotPhan";
                        }
                        break;
                    case TrangThaiHoanHang.TuChoi:
                        hoaDon.trang_thai_hoa_don = "TuChoiHoanHang";
                        break;
                }

                await _yeuCauHoanHangService.UpdateAsync(yeuCauHoanHang);
                await _hoaDonService.UpdateAsync(hoaDon);

                return Ok(new { message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                // Log lỗi
                return StatusCode(500, "Đã xảy ra lỗi khi cập nhật trạng thái");
            }
        }

        private bool IsValidStateTransition(TrangThaiHoanHang currentState, TrangThaiHoanHang newState)
        {
            switch (currentState)
            {
                case TrangThaiHoanHang.ChoXacNhan:
                    return newState == TrangThaiHoanHang.DaXacNhan ||
                           newState == TrangThaiHoanHang.TuChoi ||
                           newState == TrangThaiHoanHang.HuyBo;
                case TrangThaiHoanHang.DaXacNhan:
                    return newState == TrangThaiHoanHang.DangXuLy ||
                           newState == TrangThaiHoanHang.TuChoi ||
                           newState == TrangThaiHoanHang.HuyBo;
                case TrangThaiHoanHang.DangXuLy:
                    return newState == TrangThaiHoanHang.HoanThanh ||
                           newState == TrangThaiHoanHang.TuChoi ||
                           newState == TrangThaiHoanHang.HuyBo;
                default:
                    return false;
            }
        }
        //lấy id khách hàng
        private Guid? GetIdKhachHang()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var idKhachHang = _jwtServices.GetIdKhachHangFromToken(token);
            return idKhachHang;
        }
    }
}