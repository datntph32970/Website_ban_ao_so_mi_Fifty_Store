using API.DbConects.DTOs.Admin.KhuyenMai;
using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_Khuyen_Mai;
using API.DbConects.Entities.Entities_San_Pham;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.DTOs.KhuyenMai_DTOs;
using System.Linq.Expressions;
using API.Services;
using System.Collections.Concurrent;

namespace API.Controllers.KhuyenMai_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiamGiaController : ControllerBase
    {
        private readonly IBaseService<GiamGia> _giamGiaServices;
        private readonly IBaseService<SanPhamChiTietGiamGia> _sanPhamChiTietGiamGiaServices;
        private readonly IBaseService<SanPhamChiTiet> _sanPhamChiTietServices;
        private readonly IBaseService<SanPham> _sanPhamServices;
        private readonly ISanPhamService _sanPham_Service;
        private readonly IBaseService<ThuongHieu> _thuongHieuServices;
        private readonly IBaseService<DanhMuc> _danhMucServices;
        private readonly IBaseService<ChatLieu> _chatLieuServices;
        private readonly IBaseService<KieuDang> _kieuDangServices;
        private readonly IBaseService<XuatXu> _xuatXuServices;
        private readonly IBaseService<KichCo> _kichCoServices;
        private readonly IBaseService<MauSac> _mauSacServices;
        private readonly IJwtServices _jwtServices;

        public GiamGiaController(IBaseService<GiamGia> giamGiaServices, IJwtServices jwtServices, IBaseService<SanPhamChiTiet> sanPhamChiTietServices, IBaseService<SanPham> sanPhamServices, IBaseService<ThuongHieu> thuongHieuServices, IBaseService<DanhMuc> danhMucServices, IBaseService<ChatLieu> chatLieuServices, IBaseService<KieuDang> kieuDangServices, IBaseService<XuatXu> xuatXuServices, IBaseService<KichCo> kichCoServices, IBaseService<MauSac> mauSacServices, ISanPhamService sanPham_Service, IBaseService<SanPhamChiTietGiamGia> sanPhamChiTietGiamGiaServices)
        {
            _giamGiaServices = giamGiaServices;
            _jwtServices = jwtServices;
            _sanPhamChiTietServices = sanPhamChiTietServices;
            _sanPhamServices = sanPhamServices;
            _thuongHieuServices = thuongHieuServices;
            _danhMucServices = danhMucServices;
            _chatLieuServices = chatLieuServices;
            _kieuDangServices = kieuDangServices;
            _xuatXuServices = xuatXuServices;
            _kichCoServices = kichCoServices;
            _sanPham_Service = sanPham_Service;
            _sanPhamChiTietGiamGiaServices = sanPhamChiTietGiamGiaServices;
        }

        private Expression<Func<GiamGia, bool>> BuildFilterPredicate(
            string? trang_thai,
            string? tim_kiem,
            string? kieu_giam_gia,
            DateTime? thoi_gian_bat_dau,
            DateTime? thoi_gian_ket_thuc)
        {
            var predicate = PredicateBuilder.New<GiamGia>(true);

            if (!string.IsNullOrEmpty(trang_thai))
            {
                predicate = predicate.And(x => x.trang_thai == trang_thai);
            }

            if (!string.IsNullOrEmpty(tim_kiem))
            {
                var searchTerm = tim_kiem.ToLower();
                predicate = predicate.And(x =>
                    x.ten_giam_gia.ToLower().Contains(searchTerm) ||
                    x.ma_giam_gia.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(kieu_giam_gia))
            {
                predicate = predicate.And(x => x.kieu_giam_gia == kieu_giam_gia);
            }

            if (thoi_gian_bat_dau.HasValue)
            {
                predicate = predicate.And(x => x.thoi_gian_bat_dau >= thoi_gian_bat_dau.Value);
            }

            if (thoi_gian_ket_thuc.HasValue)
            {
                predicate = predicate.And(x => x.thoi_gian_ket_thuc <= thoi_gian_ket_thuc.Value);
            }

            return predicate;
        }

        private IOrderedEnumerable<GiamGia> ApplySorting(
            IEnumerable<GiamGia> giamGias,
            string? sortBy,
            bool ascending)
        {
            return sortBy?.ToLower() switch
            {
                "ten_giam_gia" => ascending ?
                    giamGias.OrderBy(x => x.ten_giam_gia) :
                    giamGias.OrderByDescending(x => x.ten_giam_gia),
                "ma_giam_gia" => ascending ?
                    giamGias.OrderBy(x => x.ma_giam_gia) :
                    giamGias.OrderByDescending(x => x.ma_giam_gia),
                "thoi_gian_bat_dau" => ascending ?
                    giamGias.OrderBy(x => x.thoi_gian_bat_dau) :
                    giamGias.OrderByDescending(x => x.thoi_gian_bat_dau),
                "thoi_gian_ket_thuc" => ascending ?
                    giamGias.OrderBy(x => x.thoi_gian_ket_thuc) :
                    giamGias.OrderByDescending(x => x.thoi_gian_ket_thuc),
                _ => ascending ?
                    giamGias.OrderBy(x => x.ngay_tao) :
                    giamGias.OrderByDescending(x => x.ngay_tao)
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? trang_thai,
            string? tim_kiem,
            string? kieu_giam_gia,
            DateTime? thoi_gian_bat_dau,
            DateTime? thoi_gian_ket_thuc,
            int page = 1,
            int pageSize = 10,
            string? sortBy = "ngay_tao",
            bool ascending = false)
        {
            await AutoDeactivateExpiredDiscounts();
            try
            {
                var predicate = BuildFilterPredicate(trang_thai, tim_kiem, kieu_giam_gia, thoi_gian_bat_dau, thoi_gian_ket_thuc);
                var giamGias = await _giamGiaServices.GetByConditionAsync(predicate);
                var sortedGiamGias = ApplySorting(giamGias, sortBy, ascending);

                var totalItems = sortedGiamGias.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var pagedGiamGias = sortedGiamGias
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new
                {
                    Data = pagedGiamGias,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }
        // lấy danh sách giảm giá đang hoạt động
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveDiscounts()
        {
            await AutoDeactivateExpiredDiscounts();
            try
            {
                var now = DateTime.Now;
                var giamGias = await _giamGiaServices.GetAllWithIncludeAsync(
                    q => q.Include(gg => gg.SanPhamChiTietGiamGias)
                         .ThenInclude(spct => spct.SanPhamChiTiet)
                         .ThenInclude(spct => spct.SanPham));

                var activeGiamGias = giamGias.Where(g =>
                    g.trang_thai == "HoatDong" &&
                    g.thoi_gian_ket_thuc > now &&
                    g.so_luong_da_su_dung < g.so_luong_toi_da
                ).Select(g => new
                {
                    id_giam_gia = g.id_giam_gia,
                    ma_giam_gia = g.ma_giam_gia,
                    ten_giam_gia = g.ten_giam_gia,
                    mo_ta = g.mo_ta,
                    kieu_giam_gia = g.kieu_giam_gia,
                    gia_tri_giam = g.gia_tri_giam,
                    so_luong_da_su_dung = g.so_luong_da_su_dung,
                    so_luong_toi_da = g.so_luong_toi_da,
                    thoi_gian_bat_dau = g.thoi_gian_bat_dau,
                    thoi_gian_ket_thuc = g.thoi_gian_ket_thuc,
                    trang_thai = g.trang_thai,
                    so_san_pham_ap_dung = g.SanPhamChiTietGiamGias?.Count(spgg =>
                        spgg.SanPhamChiTiet != null &&
                        spgg.SanPhamChiTiet.trang_thai == "HoatDong" &&
                        spgg.SanPhamChiTiet.so_luong > 0) ?? 0
                }).ToList();

                return Ok(activeGiamGias);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong GetActiveDiscounts: {ex}");
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }

        // GET: api/GiamGia/{id} á
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            await AutoDeactivateExpiredDiscounts();
            var giamGia = await _giamGiaServices.GetByIdWithIncludeAsync(id,
                q => q.Include(gg => gg.SanPhamChiTietGiamGias)
                     .ThenInclude(spct => spct.SanPhamChiTiet)
                     .ThenInclude(spct => spct.SanPham)
                     .Include(gg => gg.NguoiTao)
                     .Include(gg => gg.NguoiSua));

            if (giamGia == null)
                return NotFound("Không tìm thấy mã giảm giá");

            return Ok(giamGia);
        }

        // POST: api/GiamGia
        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Create([FromBody] ThemGiamGiaAdminDTO giamGia)
        {
            try
            {
                if (giamGia == null)
                    return BadRequest("Dữ liệu không hợp lệ");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Kiểm tra thời gian
                if (giamGia.thoi_gian_bat_dau >= giamGia.thoi_gian_ket_thuc)
                    return BadRequest("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc");

                if (giamGia.thoi_gian_ket_thuc <= DateTime.Now)
                    return BadRequest("Thời gian kết thúc phải lớn hơn thời gian hiện tại");

                // Kiểm tra giá trị giảm
                if (giamGia.kieu_giam_gia == "PhanTram" && (giamGia.gia_tri_giam <= 0 || giamGia.gia_tri_giam > 100))
                    return BadRequest("Giá trị giảm theo phần trăm phải nằm trong khoảng 1-100");

                if (giamGia.kieu_giam_gia == "SoTien" && giamGia.gia_tri_giam <= 0)
                    return BadRequest("Giá trị giảm theo số tiền phải lớn hơn 0");

                // Kiểm tra số lượng
                if (giamGia.so_luong_toi_da <= 0)
                    return BadRequest("Số lượng tối đa phải lớn hơn 0");

                var existingGiamGia = await _giamGiaServices.ExistsAsync(g => g.ten_giam_gia == giamGia.ten_giam_gia);
                if (existingGiamGia)
                    return BadRequest("Tên giảm giá đã tồn tại");

                // Xử lý mã giảm giá
                if (string.IsNullOrEmpty(giamGia.ma_giam_gia))
                {
                    giamGia.ma_giam_gia = await GenerateMaGiamGia();
                }
                else
                {
                    giamGia.ma_giam_gia = giamGia.ma_giam_gia.Replace(" ", "").ToUpper();
                    var existingGiamGiaMa = await _giamGiaServices.ExistsAsync(g => g.ma_giam_gia == giamGia.ma_giam_gia);
                    if (existingGiamGiaMa)
                        return BadRequest("Mã giảm giá đã tồn tại");
                }

                var giamgia = new GiamGia
                {
                    id_giam_gia = Guid.NewGuid(),
                    ma_giam_gia = giamGia.ma_giam_gia,
                    ten_giam_gia = giamGia.ten_giam_gia.Trim(),
                    mo_ta = giamGia.mo_ta?.Trim(),
                    kieu_giam_gia = giamGia.kieu_giam_gia,
                    gia_tri_giam = giamGia.gia_tri_giam,
                    so_luong_da_su_dung = 0,
                    so_luong_toi_da = giamGia.so_luong_toi_da,
                    thoi_gian_bat_dau = giamGia.thoi_gian_bat_dau,
                    thoi_gian_ket_thuc = giamGia.thoi_gian_ket_thuc,
                    trang_thai = giamGia.trang_thai.ToString(),
                    ngay_tao = DateTime.Now,
                    id_nguoi_tao = GetIdNhanVien()
                };

                var result = await _giamGiaServices.CreateAsync(giamgia);
                if (!result)
                    return BadRequest("Đã xảy ra lỗi khi tạo mã giảm giá");

                return Ok(new
                {
                    message = "Tạo mã giảm giá thành công",
                    data = giamgia
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        // PUT: api/GiamGia/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SuaGiamGiaAdminDTO giamGiaDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingGiamGia = await _giamGiaServices.GetByIdWithIncludeAsync(id,
                    q => q.Include(gg => gg.SanPhamChiTietGiamGias)
                         .ThenInclude(spct => spct.SanPhamChiTiet));

                if (existingGiamGia == null)
                    return NotFound("Không tìm thấy mã giảm giá");

                // Kiểm tra thời gian
                if (giamGiaDTO.thoi_gian_bat_dau >= giamGiaDTO.thoi_gian_ket_thuc)
                    return BadRequest("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc");

                // Nếu giảm giá đã được áp dụng, không cho phép sửa thời gian bắt đầu về tương lai
                if (existingGiamGia.SanPhamChiTietGiamGias.Any() &&
                    giamGiaDTO.thoi_gian_bat_dau > DateTime.Now)
                    return BadRequest("Không thể sửa thời gian bắt đầu về tương lai khi giảm giá đã được áp dụng");

                // Kiểm tra giá trị giảm
                if (giamGiaDTO.kieu_giam_gia == "PhanTram" &&
                    (giamGiaDTO.gia_tri_giam <= 0 || giamGiaDTO.gia_tri_giam > 100))
                    return BadRequest("Giá trị giảm theo phần trăm phải nằm trong khoảng 1-100");

                if (giamGiaDTO.kieu_giam_gia == "SoTien" && giamGiaDTO.gia_tri_giam <= 0)
                    return BadRequest("Giá trị giảm theo số tiền phải lớn hơn 0");

                // Kiểm tra số lượng
                if (giamGiaDTO.so_luong_toi_da < existingGiamGia.so_luong_da_su_dung)
                    return BadRequest("Số lượng tối đa không thể nhỏ hơn số lượng đã sử dụng");

                var existingGiamGiaKhacTen = await _giamGiaServices.ExistsAsync(g =>
                    g.ten_giam_gia.ToLower() == giamGiaDTO.ten_giam_gia.ToLower() &&
                    g.id_giam_gia != id);

                if (existingGiamGiaKhacTen)
                    return BadRequest("Tên giảm giá đã tồn tại");

                if (string.IsNullOrEmpty(giamGiaDTO.ma_giam_gia))
                    return BadRequest("Mã giảm giá không được để trống");

                giamGiaDTO.ma_giam_gia = giamGiaDTO.ma_giam_gia.Replace(" ", "").ToUpper();
                var existingGiamGiaKhacMa = await _giamGiaServices.ExistsAsync(g =>
                    g.ma_giam_gia == giamGiaDTO.ma_giam_gia &&
                    g.id_giam_gia != id);

                if (existingGiamGiaKhacMa)
                    return BadRequest("Mã giảm giá đã tồn tại");

                // Cập nhật thông tin
                existingGiamGia.ten_giam_gia = giamGiaDTO.ten_giam_gia.Trim();
                existingGiamGia.mo_ta = giamGiaDTO.mo_ta?.Trim();
                existingGiamGia.ma_giam_gia = giamGiaDTO.ma_giam_gia;
                existingGiamGia.kieu_giam_gia = giamGiaDTO.kieu_giam_gia;
                existingGiamGia.gia_tri_giam = giamGiaDTO.gia_tri_giam;
                existingGiamGia.so_luong_toi_da = giamGiaDTO.so_luong_toi_da;
                existingGiamGia.thoi_gian_bat_dau = giamGiaDTO.thoi_gian_bat_dau;
                existingGiamGia.thoi_gian_ket_thuc = giamGiaDTO.thoi_gian_ket_thuc;
                existingGiamGia.trang_thai = giamGiaDTO.trang_thai;
                existingGiamGia.ngay_cap_nhat = DateTime.Now;
                existingGiamGia.id_nguoi_cap_nhat = GetIdNhanVien();

                var result = await _giamGiaServices.UpdateAsync(existingGiamGia);
                if (result)
                {
                    return Ok(new
                    {
                        message = "Cập nhật mã giảm giá thành công",
                        data = existingGiamGia
                    });
                }

                return BadRequest("Đã xảy ra lỗi khi cập nhật mã giảm giá");
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        // DELETE: api/GiamGia/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingGiamGia = await _giamGiaServices.GetByIdWithIncludeAsync(id, q => q.Include(gg => gg.SanPhamChiTietGiamGias)
            .ThenInclude(spct => spct.SanPhamChiTiet));
            if (existingGiamGia == null) return NotFound("Không tìm thấy mã giảm giá");

            foreach (var sanPhamChiTietGiamGia in existingGiamGia.SanPhamChiTietGiamGias)
            {
                await _sanPhamChiTietGiamGiaServices.DeleteAsync(sanPhamChiTietGiamGia.id);
            }

            var result = await _giamGiaServices.DeleteAsync(id);
            if (result)
            {
                return Ok("Xóa mã giảm giá thành công");
            }
            return BadRequest("Đã xảy ra lỗi khi xóa mã giảm giá");
        }
        [HttpPost("lay-danh-sach-san-pham-co-the-giam-gia/{id_giam_gia}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetSanPhamCoTheGiamGia(Guid id_giam_gia, [FromBody] ThamSoPhanTrangSanPhamDTO thamSoPhanTrangSanPhamDTO)
        {
            try
            {
                // Lấy thông tin giảm giá được chọn
                var giamGia = await _giamGiaServices.GetByIdAsync(id_giam_gia);
                if (giamGia == null)
                    return NotFound("Không tìm thấy giảm giá");

                var sanPhamGiamGia = await _sanPham_Service.GetAllSanPhamAdminDTOAsync();

                var sanPhamGiamGiaDTO = sanPhamGiamGia
                    // Lọc sản phẩm có ít nhất 1 biến thể đang hoạt động và còn hàng
                    .Where(spct => spct.sanPhamChiTiets != null && spct.sanPhamChiTiets.Any(ct => ct.so_luong > 0 && ct.trang_thai == "HoatDong"))
                    .Select(sp => new
                    {
                        SanPham = sp,
                        SanPhamChiTietHopLe = sp.sanPhamChiTiets.Where(ct =>
                            ct.so_luong > 0 &&
                            ct.trang_thai == "HoatDong" &&
                            !(ct.giamGias != null && ct.giamGias.Any(gg =>
                                gg.trang_thai == "HoatDong" &&
                                giamGia.thoi_gian_bat_dau <= gg.thoi_gian_ket_thuc &&
                                giamGia.thoi_gian_ket_thuc >= gg.thoi_gian_bat_dau
                            ))
                        ).ToList()
                    })
                    .Where(x => x.SanPhamChiTietHopLe.Any()) // Chỉ lấy sản phẩm có ít nhất 1 biến thể hợp lệ
                    .Select(x =>
                    {
                        var sp = x.SanPham;
                        sp.sanPhamChiTiets = x.SanPhamChiTietHopLe;
                        return sp;
                    })
                    // Tìm kiếm theo tên hoặc mã
                    .Where(spct => string.IsNullOrEmpty(thamSoPhanTrangSanPhamDTO.tim_kiem) ||
                        spct.ten_san_pham.Contains(thamSoPhanTrangSanPhamDTO.tim_kiem, StringComparison.OrdinalIgnoreCase) ||
                        spct.ma_san_pham.Contains(thamSoPhanTrangSanPhamDTO.tim_kiem, StringComparison.OrdinalIgnoreCase))
                    // Lọc theo danh mục
                    .Where(spct => thamSoPhanTrangSanPhamDTO.id_danh_muc == null || !thamSoPhanTrangSanPhamDTO.id_danh_muc.Any() ||
                        (spct.danhMuc != null && thamSoPhanTrangSanPhamDTO.id_danh_muc.Contains(spct.danhMuc.id_danh_muc.ToString())))
                    // Lọc theo thương hiệu
                    .Where(spct => thamSoPhanTrangSanPhamDTO.id_thuong_hieu == null || !thamSoPhanTrangSanPhamDTO.id_thuong_hieu.Any() ||
                        (spct.thuongHieu != null && thamSoPhanTrangSanPhamDTO.id_thuong_hieu.Contains(spct.thuongHieu.id_thuong_hieu.ToString())))
                    // Lọc theo kiểu dáng
                    .Where(spct => thamSoPhanTrangSanPhamDTO.id_kieu_dang == null || !thamSoPhanTrangSanPhamDTO.id_kieu_dang.Any() ||
                        (spct.kieuDang != null && thamSoPhanTrangSanPhamDTO.id_kieu_dang.Contains(spct.kieuDang.id_kieu_dang.ToString())))
                    // Lọc theo chất liệu
                    .Where(spct => thamSoPhanTrangSanPhamDTO.id_chat_lieu == null || !thamSoPhanTrangSanPhamDTO.id_chat_lieu.Any() ||
                        (spct.chatLieu != null && thamSoPhanTrangSanPhamDTO.id_chat_lieu.Contains(spct.chatLieu.id_chat_lieu.ToString())))
                    // Lọc theo xuất xứ
                    .Where(spct => thamSoPhanTrangSanPhamDTO.id_xuat_xu == null || !thamSoPhanTrangSanPhamDTO.id_xuat_xu.Any() ||
                        (spct.xuatXu != null && thamSoPhanTrangSanPhamDTO.id_xuat_xu.Contains(spct.xuatXu.id_xuat_xu.ToString())))
                    // Lọc theo khoảng giá
                    .Where(spct => !thamSoPhanTrangSanPhamDTO.gia_tu.HasValue || spct.sanPhamChiTiets.Any(ct => ct.gia_ban >= thamSoPhanTrangSanPhamDTO.gia_tu.Value))
                    .Where(spct => !thamSoPhanTrangSanPhamDTO.gia_den.HasValue || spct.sanPhamChiTiets.Any(ct => ct.gia_ban <= thamSoPhanTrangSanPhamDTO.gia_den.Value))
                    .ToList();

                // Thêm log để debug
                Console.WriteLine($"Tổng số sản phẩm: {sanPhamGiamGia.Count()}");
                Console.WriteLine($"Số sản phẩm sau khi lọc: {sanPhamGiamGiaDTO.Count()}");
                Console.WriteLine($"Thời gian giảm giá: {giamGia.thoi_gian_bat_dau:dd/MM/yyyy HH:mm} - {giamGia.thoi_gian_ket_thuc:dd/MM/yyyy HH:mm}");
                foreach (var sp in sanPhamGiamGiaDTO)
                {
                    Console.WriteLine($"Sản phẩm {sp.ten_san_pham} có {sp.sanPhamChiTiets.Count} biến thể hợp lệ");
                }

                // Áp dụng sắp xếp nếu có
                if (!string.IsNullOrEmpty(thamSoPhanTrangSanPhamDTO.sap_xep_theo))
                {
                    sanPhamGiamGiaDTO = thamSoPhanTrangSanPhamDTO.sap_xep_theo.ToLower() switch
                    {
                        "ten_san_pham" => thamSoPhanTrangSanPhamDTO.sap_xep_tang
                            ? sanPhamGiamGiaDTO.OrderBy(sp => sp.ten_san_pham).ToList()
                            : sanPhamGiamGiaDTO.OrderByDescending(sp => sp.ten_san_pham).ToList(),
                        "ma_san_pham" => thamSoPhanTrangSanPhamDTO.sap_xep_tang
                            ? sanPhamGiamGiaDTO.OrderBy(sp => sp.ma_san_pham).ToList()
                            : sanPhamGiamGiaDTO.OrderByDescending(sp => sp.ma_san_pham).ToList(),
                        "ngay_tao" => thamSoPhanTrangSanPhamDTO.sap_xep_tang
                            ? sanPhamGiamGiaDTO.OrderBy(sp => sp.ngay_tao).ToList()
                            : sanPhamGiamGiaDTO.OrderByDescending(sp => sp.ngay_tao).ToList(),
                        "gia_ban" => thamSoPhanTrangSanPhamDTO.sap_xep_tang
                            ? sanPhamGiamGiaDTO.OrderBy(sp => sp.sanPhamChiTiets.Min(spct => spct.gia_ban)).ToList()
                            : sanPhamGiamGiaDTO.OrderByDescending(sp => sp.sanPhamChiTiets.Min(spct => spct.gia_ban)).ToList(),
                        _ => sanPhamGiamGiaDTO
                    };
                }

                // Tính toán phân trang
                var tongSoPhanTu = sanPhamGiamGiaDTO.Count;
                var tongSoTrang = (int)Math.Ceiling(tongSoPhanTu / (double)thamSoPhanTrangSanPhamDTO.so_phan_tu_tren_trang);
                thamSoPhanTrangSanPhamDTO.trang_hien_tai = Math.Max(1, Math.Min(thamSoPhanTrangSanPhamDTO.trang_hien_tai, tongSoTrang));

                // Lấy danh sách sản phẩm cho trang hiện tại
                var sanPhamsTrangHienTai = sanPhamGiamGiaDTO
                    .Skip((thamSoPhanTrangSanPhamDTO.trang_hien_tai - 1) * thamSoPhanTrangSanPhamDTO.so_phan_tu_tren_trang)
                    .Take(thamSoPhanTrangSanPhamDTO.so_phan_tu_tren_trang)
                    .ToList();

                // Tạo kết quả phân trang
                var result = new
                {
                    trang_hien_tai = thamSoPhanTrangSanPhamDTO.trang_hien_tai,
                    so_phan_tu_tren_trang = thamSoPhanTrangSanPhamDTO.so_phan_tu_tren_trang,
                    tong_so_trang = tongSoTrang,
                    tong_so_phan_tu = tongSoPhanTu,
                    danh_sach = sanPhamsTrangHienTai
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpGet("{id_giam_gia}/san-pham/{id_san_pham}")]
        public async Task<IActionResult> GetSanPhamChiTietDangGiamGia(Guid id_giam_gia, Guid id_san_pham)
        {
            try
            {
                // Lấy sản phẩm theo id
                var sanPham = await _sanPham_Service.GetByIdSanPhamAdminDTOAsync(id_san_pham);
                if (sanPham == null)
                {
                    return NotFound("Không tìm thấy sản phẩm");
                }

                // Lọc chỉ giữ lại các sản phẩm chi tiết có giảm giá với id được chỉ định
                sanPham.sanPhamChiTiets = sanPham.sanPhamChiTiets
                    .Where(spct => spct.giamGias != null && spct.giamGias.Any(giamGia => giamGia.id_giam_gia == id_giam_gia))
                    .ToList();

                if (!sanPham.sanPhamChiTiets.Any())
                {
                    return NotFound($"Không tìm thấy sản phẩm chi tiết nào đang áp dụng giảm giá với ID: {id_giam_gia}");
                }

                return Ok(sanPham);
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpGet("thong-ke/{id}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetDiscountStatistics(Guid id)
        {
            try
            {
                var giamGia = await _giamGiaServices.GetByIdWithIncludeAsync(id,
                    q => q.Include(gg => gg.SanPhamChiTietGiamGias)
                         .ThenInclude(spct => spct.SanPhamChiTiet)
                         .ThenInclude(spct => spct.SanPham));

                if (giamGia == null)
                    return NotFound("Không tìm thấy mã giảm giá");

                var now = DateTime.Now;
                var statistics = new
                {
                    id_giam_gia = giamGia.id_giam_gia,
                    ma_giam_gia = giamGia.ma_giam_gia,
                    ten_giam_gia = giamGia.ten_giam_gia,
                    trang_thai = giamGia.trang_thai,
                    thoi_gian_bat_dau = giamGia.thoi_gian_bat_dau,
                    thoi_gian_ket_thuc = giamGia.thoi_gian_ket_thuc,
                    con_hieu_luc = giamGia.thoi_gian_ket_thuc >= now && giamGia.trang_thai == "HoatDong",
                    so_luong_da_su_dung = giamGia.so_luong_da_su_dung,
                    so_luong_toi_da = giamGia.so_luong_toi_da,
                    ti_le_su_dung = giamGia.so_luong_toi_da > 0
                        ? (double)giamGia.so_luong_da_su_dung / giamGia.so_luong_toi_da * 100
                        : 0,
                    tong_bien_the_ap_dung = giamGia.SanPhamChiTietGiamGias.Count,
                    bien_the_dang_ap_dung = giamGia.SanPhamChiTietGiamGias
                        .Count(spgg => spgg.SanPhamChiTiet.trang_thai == "HoatDong" &&
                                     spgg.SanPhamChiTiet.so_luong > 0),
                    danh_sach_bien_the = giamGia.SanPhamChiTietGiamGias
                        .GroupBy(spgg => spgg.SanPhamChiTiet.SanPham)
                        .Select(g => new
                        {
                            id_san_pham = g.Key.id_san_pham,
                            ma_san_pham = g.Key.ma_san_pham,
                            ten_san_pham = g.Key.ten_san_pham,
                            so_luong_bien_the = g.Count(),
                            bien_the = g.Select(spgg => new
                            {
                                id_san_pham_chi_tiet = spgg.SanPhamChiTiet.id_san_pham_chi_tiet,
                                ma_san_pham_chi_tiet = spgg.SanPhamChiTiet.ma_san_pham_chi_tiet,
                                so_luong = spgg.SanPhamChiTiet.so_luong,
                                trang_thai = spgg.SanPhamChiTiet.trang_thai
                            })
                        }).ToList()
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        [HttpGet("thong-ke-tong-hop")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetOverallDiscountStatistics()
        {
            try
            {
                var now = DateTime.Now;
                var allGiamGia = await _giamGiaServices.GetAllWithIncludeAsync(
                    q => q.Include(gg => gg.SanPhamChiTietGiamGias)
                         .ThenInclude(spct => spct.SanPhamChiTiet));

                var statistics = new
                {
                    tong_so_giam_gia = allGiamGia.Count,
                    dang_hoat_dong = allGiamGia.Count(g => g.trang_thai == "HoatDong"),
                    da_ket_thuc = allGiamGia.Count(g => g.thoi_gian_ket_thuc < now),
                    chua_bat_dau = allGiamGia.Count(g => g.thoi_gian_bat_dau > now),
                    dang_ap_dung = allGiamGia.Count(g =>
                        g.trang_thai == "HoatDong" &&
                        g.thoi_gian_bat_dau <= now &&
                        g.thoi_gian_ket_thuc >= now),
                    thong_ke_theo_thang = allGiamGia
                        .GroupBy(g => new { g.thoi_gian_bat_dau.Year, g.thoi_gian_bat_dau.Month })
                        .Select(g => new
                        {
                            nam = g.Key.Year,
                            thang = g.Key.Month,
                            so_luong = g.Count(),
                            dang_hoat_dong = g.Count(x => x.trang_thai == "HoatDong"),
                            da_su_dung = g.Sum(x => x.so_luong_da_su_dung)
                        })
                        .OrderByDescending(x => x.nam)
                        .ThenByDescending(x => x.thang)
                        .Take(12)
                        .ToList()
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        private async Task AutoDeactivateExpiredDiscounts()
        {
            try
            {
                var now = DateTime.Now;
                var expiredDiscounts = await _giamGiaServices.GetByConditionAsync(g =>
                    g.trang_thai == "HoatDong" &&
                    (g.thoi_gian_ket_thuc < now || g.so_luong_da_su_dung >= g.so_luong_toi_da));

                foreach (var discount in expiredDiscounts)
                {
                    discount.trang_thai = "KhongHoatDong";
                    discount.ngay_cap_nhat = now;
                    await _giamGiaServices.UpdateAsync(discount);
                }
            }
            catch (Exception)
            {
                // Log error if needed
            }
        }

        [HttpPost("cap-nhat-trang-thai-giam-gia")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDiscountStatuses()
        {
            try
            {
                var now = DateTime.Now;
                var updatedDiscounts = await _giamGiaServices.GetByConditionAsync(g =>
                    g.trang_thai == "HoatDong" &&
                    (g.thoi_gian_ket_thuc < now || g.so_luong_da_su_dung >= g.so_luong_toi_da));

                var count = 0;
                foreach (var discount in updatedDiscounts)
                {
                    discount.trang_thai = "KhongHoatDong";
                    discount.ngay_cap_nhat = now;
                    await _giamGiaServices.UpdateAsync(discount);
                    count++;
                }

                return Ok(new
                {
                    message = $"Đã cập nhật trạng thái cho {count} mã giảm giá",
                    deactivated_count = count
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        private Guid GetIdNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                throw new UnauthorizedAccessException("Token không hợp lệ");

            var idtnhanvien = _jwtServices.GetIdNhanVienFromToken(token);
            if (!idtnhanvien.HasValue)
                throw new UnauthorizedAccessException("Token không chứa thông tin nhân viên hợp lệ");

            return idtnhanvien.Value;
        }

        private async Task<string> GenerateMaGiamGia()
        {
            string maGiamGia;
            do
            {
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                string randomPart = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                maGiamGia = $"GG{randomPart}";
            } while (await _giamGiaServices.ExistsAsync(x => x.ma_giam_gia == maGiamGia));

            return maGiamGia;
        }

        [HttpPost("them-giam_gia-vao-san-pham-chi-tiet")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> ThemGiamGiaVaoSanPhamChiTiet([FromBody] ThemGiamGiaVaoSanPhamChiTietDTO dto)
        {
            if (dto == null || !dto.san_pham_chi_tiet_ids.Any())
                return BadRequest("Danh sách sản phẩm chi tiết không được để trống");

            var giamGia = await _giamGiaServices.GetByIdAsync(Guid.Parse(dto.id_giam_gia));
            if (giamGia == null)
                return NotFound("Không tìm thấy mã giảm giá");

            if (giamGia.trang_thai != "HoatDong")
                return BadRequest("Mã giảm giá không ở trạng thái hoạt động");

            var now = DateTime.Now;
            if (now > giamGia.thoi_gian_ket_thuc)
                return BadRequest("Mã giảm giá không còn hiệu lực");



            // Lấy tất cả sản phẩm chi tiết cần thêm giảm giá
            var spcts = await _sanPhamChiTietServices.GetByConditionWithIncludeAsync(
                sp => dto.san_pham_chi_tiet_ids.Contains(sp.id_san_pham_chi_tiet.ToString()),
                sp => sp.Include(x => x.SanPhamChiTietGiamGias)
                    .ThenInclude(spgg => spgg.GiamGia));

            if (!spcts.Any())
                return BadRequest("Không tìm thấy sản phẩm chi tiết hợp lệ để thêm vào giảm giá");

            var errorMessages = new List<string>();
            var skippedProducts = new List<string>();

            foreach (var spct in spcts)
            {
                // Kiểm tra xem sản phẩm đã có giảm giá này chưa
                if (spct.SanPhamChiTietGiamGias.Any(spgg => spgg.id_giam_gia == giamGia.id_giam_gia))
                {
                    skippedProducts.Add(spct.ma_san_pham_chi_tiet);
                    continue;
                }

                // Kiểm tra xem sản phẩm chi tiết đã có giảm giá khác trong khoảng thời gian này chưa
                var existingOverlappingDiscounts = spct.SanPhamChiTietGiamGias
                    .Where(spgg =>
                        spgg.GiamGia.trang_thai == "HoatDong" &&
                        spgg.id_giam_gia != giamGia.id_giam_gia && // Loại trừ giảm giá hiện tại
                        ((giamGia.thoi_gian_bat_dau <= spgg.GiamGia.thoi_gian_ket_thuc &&
                          giamGia.thoi_gian_ket_thuc >= spgg.GiamGia.thoi_gian_bat_dau) ||
                         (spgg.GiamGia.thoi_gian_bat_dau <= giamGia.thoi_gian_ket_thuc &&
                          spgg.GiamGia.thoi_gian_ket_thuc >= giamGia.thoi_gian_bat_dau)))
                    .ToList();

                if (existingOverlappingDiscounts.Any())
                {
                    var overlappingDiscountNames = string.Join(", ", existingOverlappingDiscounts.Select(x => x.GiamGia.ma_giam_gia));
                    errorMessages.Add($"Sản phẩm chi tiết {spct.ma_san_pham_chi_tiet} đã có giảm giá ({overlappingDiscountNames}) trong khoảng thời gian này");
                    continue;
                }
                // Kiểm tra xem đã tồn tại bản ghi với id_giam_gia và id_san_pham_chi_tiet này chưa
                var existingRecord = await _sanPhamChiTietGiamGiaServices.GetByConditionAsync(
                    x => x.id_giam_gia == giamGia.id_giam_gia &&
                         x.id_san_pham_chi_tiet == spct.id_san_pham_chi_tiet);

                if (existingRecord.Any())
                {
                    skippedProducts.Add(spct.ma_san_pham_chi_tiet);
                    continue;
                }

                var spctg = new SanPhamChiTietGiamGia
                {
                    id = Guid.NewGuid(),
                    id_san_pham_chi_tiet = spct.id_san_pham_chi_tiet,
                    id_giam_gia = giamGia.id_giam_gia,
                };
                await _sanPhamChiTietGiamGiaServices.CreateAsync(spctg);
            }

            var response = new Dictionary<string, object>();

            if (errorMessages.Any())
            {
                response["errors"] = errorMessages;
            }

            if (skippedProducts.Any())
            {
                response["skipped"] = $"Các sản phẩm sau đã có giảm giá này và được bỏ qua: {string.Join(", ", skippedProducts)}";
            }

            if (!errorMessages.Any() && !skippedProducts.Any())
            {
                response["message"] = "Thêm sản phẩm vào giảm giá thành công";
            }
            else if (!errorMessages.Any())
            {
                response["message"] = "Thêm sản phẩm vào giảm giá thành công (một số sản phẩm được bỏ qua do đã có giảm giá này)";
            }

            return Ok(response);
        }

        [HttpDelete("xoa-giam_gia-khoi-san-pham-chi-tiet")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> XoaGiamGiaKhoiSanPhamChiTiet([FromBody] XoaGiamGiaKhoiSanPhamChiTietDTO dto)
        {
            if (dto == null || !dto.san_pham_chi_tiet_ids.Any())
                return BadRequest("Danh sách sản phẩm không được để trống");

            // Lấy danh sách sản phẩm chi tiết cần xóa giảm giá
            var spcts = await _sanPhamChiTietServices.GetByConditionWithIncludeAsync(
                sp => dto.san_pham_chi_tiet_ids.Contains(sp.id_san_pham_chi_tiet.ToString()),
                sp => sp.Include(x => x.SanPhamChiTietGiamGias)
                    .ThenInclude(spgg => spgg.GiamGia));

            if (!spcts.Any())
                return BadRequest("Không tìm thấy sản phẩm hợp lệ để xóa khỏi giảm giá");

            foreach (var spct in spcts)
            {
                // Xóa các mối quan hệ giảm giá của sản phẩm chi tiết với giảm giá cụ thể
                var spctgsToDelete = spct.SanPhamChiTietGiamGias
                    .Where(spgg => spgg.id_giam_gia == dto.id_giam_gia)
                    .ToList();

                foreach (var spctg in spctgsToDelete)
                {
                    var result = await _sanPhamChiTietGiamGiaServices.DeleteAsync(spctg.id);
                    if (!result)
                    {
                        return BadRequest("Không thể xóa mối quan hệ giảm giá");
                    }
                }
            }

            return Ok("Xóa sản phẩm khỏi giảm giá thành công");
        }

        [HttpPost("{id}/san-pham")]
        public async Task<IActionResult> GetSanPhamDangGiamGia(Guid id, [FromBody] ThamSoPhanTrangSanPhamDTO thamSo)
        {
            try
            {
                // Lấy tất cả sản phẩm với các thuộc tính liên quan
                var allSanPhams = await _sanPham_Service.GetAllSanPhamAdminDTOAsync();
                // Lọc sản phẩm có sản phẩm chi tiết đang áp dụng giảm giá với id được chỉ định
                allSanPhams = allSanPhams.Where(sp =>
                    sp.sanPhamChiTiets != null &&
                    sp.sanPhamChiTiets.Any(spct => spct.giamGias != null && spct.giamGias.Any(giamGia => giamGia.id_giam_gia == id))
                ).ToList();

                // Lọc lại danh sách sản phẩm chi tiết của từng sản phẩm, chỉ giữ lại các sản phẩm chi tiết có giảm giá với id được chỉ định
                foreach (var sp in allSanPhams)
                {
                    sp.sanPhamChiTiets = sp.sanPhamChiTiets
                        .Where(spct => spct.giamGias != null && spct.giamGias.Any(giamGia => giamGia.id_giam_gia == id))
                        .ToList();
                }

                // Tính giá lớn nhất từ tất cả sản phẩm chi tiết
                var giaLonNhat = allSanPhams
                    .Where(sp => sp.sanPhamChiTiets != null && sp.sanPhamChiTiets.Any())
                    .SelectMany(sp => sp.sanPhamChiTiets)
                    .DefaultIfEmpty(new SanPhamChiTietAdminDTO { gia_ban = 0 })
                    .Max(spct => spct.gia_ban);

                // Áp dụng tìm kiếm nếu có
                if (!string.IsNullOrEmpty(thamSo.tim_kiem))
                {
                    allSanPhams = allSanPhams.Where(sp =>
                        sp.ten_san_pham.Contains(thamSo.tim_kiem, StringComparison.OrdinalIgnoreCase) ||
                        sp.ma_san_pham.Contains(thamSo.tim_kiem, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // Áp dụng bộ lọc thương hiệu
                if (thamSo.id_thuong_hieu != null && thamSo.id_thuong_hieu.Any())
                {
                    allSanPhams = allSanPhams.Where(sp =>
                        sp.thuongHieu != null && thamSo.id_thuong_hieu.Contains(sp.thuongHieu.id_thuong_hieu.ToString())
                    ).ToList();
                }

                // Áp dụng bộ lọc danh mục
                if (thamSo.id_danh_muc != null && thamSo.id_danh_muc.Any())
                {
                    allSanPhams = allSanPhams.Where(sp =>
                        sp.danhMuc != null && thamSo.id_danh_muc.Contains(sp.danhMuc.id_danh_muc.ToString())
                    ).ToList();
                }

                // Áp dụng bộ lọc kiểu dáng
                if (thamSo.id_kieu_dang != null && thamSo.id_kieu_dang.Any())
                {
                    allSanPhams = allSanPhams.Where(sp =>
                        sp.kieuDang != null && thamSo.id_kieu_dang.Contains(sp.kieuDang.id_kieu_dang.ToString())
                    ).ToList();
                }

                // Áp dụng bộ lọc chất liệu
                if (thamSo.id_chat_lieu != null && thamSo.id_chat_lieu.Any())
                {
                    allSanPhams = allSanPhams.Where(sp =>
                        sp.chatLieu != null && thamSo.id_chat_lieu.Contains(sp.chatLieu.id_chat_lieu.ToString())
                    ).ToList();
                }

                // Áp dụng bộ lọc xuất xứ
                if (thamSo.id_xuat_xu != null && thamSo.id_xuat_xu.Any())
                {
                    allSanPhams = allSanPhams.Where(sp =>
                        sp.xuatXu != null && thamSo.id_xuat_xu.Contains(sp.xuatXu.id_xuat_xu.ToString())
                    ).ToList();
                }

                // Áp dụng bộ lọc khoảng giá
                if (thamSo.gia_tu.HasValue)
                {
                    allSanPhams = allSanPhams.Where(sp =>
                        sp.sanPhamChiTiets != null && sp.sanPhamChiTiets.Any(spct => spct.gia_ban >= thamSo.gia_tu.Value)
                    ).ToList();
                }

                if (thamSo.gia_den.HasValue)
                {
                    allSanPhams = allSanPhams.Where(sp =>
                        sp.sanPhamChiTiets != null && sp.sanPhamChiTiets.Any(spct => spct.gia_ban <= thamSo.gia_den.Value)
                    ).ToList();
                }

                // Áp dụng sắp xếp nếu có
                if (!string.IsNullOrEmpty(thamSo.sap_xep_theo))
                {
                    allSanPhams = thamSo.sap_xep_theo.ToLower() switch
                    {
                        "ten_san_pham" => thamSo.sap_xep_tang
                            ? allSanPhams.OrderBy(sp => sp.ten_san_pham).ToList()
                            : allSanPhams.OrderByDescending(sp => sp.ten_san_pham).ToList(),
                        "ma_san_pham" => thamSo.sap_xep_tang
                            ? allSanPhams.OrderBy(sp => sp.ma_san_pham).ToList()
                            : allSanPhams.OrderByDescending(sp => sp.ma_san_pham).ToList(),
                        "ngay_tao" => thamSo.sap_xep_tang
                            ? allSanPhams.OrderBy(sp => sp.ngay_tao).ToList()
                            : allSanPhams.OrderByDescending(sp => sp.ngay_tao).ToList(),
                        "gia_ban" => thamSo.sap_xep_tang
                            ? allSanPhams.OrderBy(sp => sp.sanPhamChiTiets.Min(spct => spct.gia_ban)).ToList()
                            : allSanPhams.OrderByDescending(sp => sp.sanPhamChiTiets.Min(spct => spct.gia_ban)).ToList(),
                        _ => allSanPhams
                    };
                }

                // Tính toán phân trang
                var tongSoPhanTu = allSanPhams.Count;
                var tongSoTrang = (int)Math.Ceiling(tongSoPhanTu / (double)thamSo.so_phan_tu_tren_trang);
                thamSo.trang_hien_tai = Math.Max(1, Math.Min(thamSo.trang_hien_tai, tongSoTrang));

                // Lấy danh sách sản phẩm cho trang hiện tại
                var sanPhamsTrangHienTai = allSanPhams
                    .Skip((thamSo.trang_hien_tai - 1) * thamSo.so_phan_tu_tren_trang)
                    .Take(thamSo.so_phan_tu_tren_trang)
                    .ToList();

                // Tạo kết quả phân trang
                var result = new PhanTrangSanPhamDTO
                {
                    trang_hien_tai = thamSo.trang_hien_tai,
                    so_phan_tu_tren_trang = thamSo.so_phan_tu_tren_trang,
                    tong_so_trang = tongSoTrang,
                    tong_so_phan_tu = tongSoPhanTu,
                    gia_lon_nhat = giaLonNhat,
                    danh_sach = sanPhamsTrangHienTai
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }
    }
}
