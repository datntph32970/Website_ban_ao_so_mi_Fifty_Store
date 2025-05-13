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

namespace API.Controllers.KhuyenMai_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiamGiaController : ControllerBase
    {
        private readonly IBaseService<GiamGia> _giamGiaServices;
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
        private static readonly Dictionary<string, (DateTime Expiry, object Data)> _cache = new();

        public GiamGiaController(IBaseService<GiamGia> giamGiaServices, IJwtServices jwtServices, IBaseService<SanPhamChiTiet> sanPhamChiTietServices, IBaseService<SanPham> sanPhamServices, IBaseService<ThuongHieu> thuongHieuServices, IBaseService<DanhMuc> danhMucServices, IBaseService<ChatLieu> chatLieuServices, IBaseService<KieuDang> kieuDangServices, IBaseService<XuatXu> xuatXuServices, IBaseService<KichCo> kichCoServices, IBaseService<MauSac> mauSacServices, ISanPhamService sanPham_Service)
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
        }

        private void ClearCache()
        {
            _cache.Clear();
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
            var cacheKey = $"giamgia_{trang_thai}_{tim_kiem}_{kieu_giam_gia}_{thoi_gian_bat_dau}_{thoi_gian_ket_thuc}_{page}_{pageSize}_{sortBy}_{ascending}";

            if (_cache.TryGetValue(cacheKey, out var cachedData) && cachedData.Expiry > DateTime.Now)
            {
                return Ok(cachedData.Data);
            }

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

            _cache[cacheKey] = (DateTime.Now.AddMinutes(1), result);
            return Ok(result);
        }
        // lấy danh sách giảm giá đang hoạt động
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveDiscounts()
        {
            var now = DateTime.Now;
            var giamGias = await _giamGiaServices.GetByConditionAsync(g => g.trang_thai == "HoatDong" &&
                g.thoi_gian_bat_dau <= now &&
                g.thoi_gian_ket_thuc >= now &&
                g.so_luong_da_su_dung < g.so_luong_toi_da);
            return Ok(giamGias);
        }

        // GET: api/GiamGia/{id} á
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var giamGia = await _giamGiaServices.GetByIdWithIncludeAsync(id,
                q => q.Include(gg => gg.SanPhamChiTiets)
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
            if (giamGia == null) return BadRequest("Dữ liệu không hợp lệ");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingGiamGia = await _giamGiaServices.ExistsAsync(g => g.ten_giam_gia == giamGia.ten_giam_gia);
            if (existingGiamGia)
                return BadRequest("Tên giảm giá đã tồn tại");
            if (giamGia.ma_giam_gia == null || giamGia.ma_giam_gia == "")
                giamGia.ma_giam_gia = await GenerateMaGiamGia();
            else
            {
                var existingGiamGiaMa = await _giamGiaServices.ExistsAsync(g => g.ma_giam_gia == giamGia.ma_giam_gia);
                if (existingGiamGiaMa)
                    return BadRequest("Mã giảm giá đã tồn tại");
            }
            var giamgia = new GiamGia
            {
                id_giam_gia = Guid.NewGuid(),
                ma_giam_gia = giamGia.ma_giam_gia,
                ten_giam_gia = giamGia.ten_giam_gia,
                mo_ta = giamGia.mo_ta,
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
            if (result == null) return BadRequest("Đã xảy ra lỗi khi tạo mã giảm giá");

            ClearCache(); // Clear cache after creating new discount
            return Ok("Tạo mã giảm giá thành công");
        }

        // PUT: api/GiamGia/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SuaGiamGiaAdminDTO giamGiaDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingGiamGia = await _giamGiaServices.GetByIdAsync(id);
            if (existingGiamGia == null) return NotFound("Không tìm thấy mã giảm giá");

            var existingGiamGiaKhacTen = await _giamGiaServices.ExistsAsync(g =>
                g.ten_giam_gia == giamGiaDTO.ten_giam_gia &&
                g.id_giam_gia != id);

            if (existingGiamGiaKhacTen)
                return BadRequest("Tên giảm giá đã tồn tại");
            if (giamGiaDTO.ma_giam_gia == null || giamGiaDTO.ma_giam_gia == "")
            {
                return BadRequest("Mã giảm giá không được để trống");
            }
            var existingGiamGiaKhacMa = await _giamGiaServices.ExistsAsync(g =>
                g.ma_giam_gia == giamGiaDTO.ma_giam_gia &&
                g.id_giam_gia != id);
            if (existingGiamGiaKhacMa)
                return BadRequest("Mã giảm giá đã tồn tại");

            existingGiamGia.ten_giam_gia = giamGiaDTO.ten_giam_gia;
            existingGiamGia.mo_ta = giamGiaDTO.mo_ta;
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
                ClearCache(); // Clear cache after updating discount
                return Ok("Cập nhật mã giảm giá thành công");
            }
            return BadRequest("Đã xảy ra lỗi khi cập nhật mã giảm giá");
        }

        // DELETE: api/GiamGia/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingGiamGia = await _giamGiaServices.GetByIdWithIncludeAsync(id, q => q.Include(gg => gg.SanPhamChiTiets));
            if (existingGiamGia == null) return NotFound("Không tìm thấy mã giảm giá");

            foreach (var sanPhamChiTiet in existingGiamGia.SanPhamChiTiets)
            {
                sanPhamChiTiet.id_giam_gia = null;
                await _sanPhamChiTietServices.UpdateAsync(sanPhamChiTiet);
            }

            var result = await _giamGiaServices.DeleteAsync(id);
            if (result)
            {
                ClearCache(); // Clear cache after deleting discount
                return Ok("Xóa mã giảm giá thành công");
            }
            return BadRequest("Đã xảy ra lỗi khi xóa mã giảm giá");
        }
        [HttpGet("lay-danh-sach-san-pham-co-the-giam-gia")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetSanPhamCoTheGiamGia(
            string? timkiem,
            string? id_danh_muc,
            string? id_thuong_hieu,
            string? trang_thai_giam_gia,
            int trang_hien_tai = 1,
            int so_phan_tu_tren_trang = 10,
            string? sap_xep_theo = null,
            bool sap_xep_tang = true)
        {
            try
            {
                var sanPhamGiamGia = await _sanPhamChiTietServices.GetAllWithIncludeAsync(
                    q => q.Include(spct => spct.SanPham)
                         .ThenInclude(sp => sp.ThuongHieu)
                         .Include(spct => spct.SanPham)
                         .ThenInclude(sp => sp.DanhMuc)
                         .Include(spct => spct.SanPham)
                         .ThenInclude(sp => sp.KieuDang)
                         .Include(spct => spct.SanPham)
                         .ThenInclude(sp => sp.ChatLieu)
                         .Include(spct => spct.SanPham)
                         .ThenInclude(sp => sp.XuatXu)
                         .Include(spct => spct.SanPham)
                         .ThenInclude(sp => sp.anhMacDinh)
                         .Include(spct => spct.MauSac)
                         .Include(spct => spct.KichCo)
                         .Include(spct => spct.GiamGia)
                         .Include(spct => spct.HinhAnhSanPhamChiTiets)
                            .ThenInclude(ha => ha.HinhAnhs));

                var sanPhamGiamGiaDTO = sanPhamGiamGia
                    .Where(spct => spct.so_luong > 0 && spct.trang_thai == "HoatDong")
                    .Where(spct => string.IsNullOrEmpty(timkiem) || spct.SanPham.ten_san_pham.Contains(timkiem) || spct.SanPham.ma_san_pham.Contains(timkiem))
                    .Where(spct => id_danh_muc == null || spct.SanPham.DanhMuc.id_danh_muc == Guid.Parse(id_danh_muc))
                    .Where(spct => id_thuong_hieu == null || spct.SanPham.ThuongHieu.id_thuong_hieu == Guid.Parse(id_thuong_hieu))
                    .Where(spct =>
                        string.IsNullOrEmpty(trang_thai_giam_gia) ||
                        (trang_thai_giam_gia == "ChuaCoGiamGia" && spct.id_giam_gia == null) ||
                        (trang_thai_giam_gia == "CoGiamGia" && spct.id_giam_gia != null &&
                         spct.GiamGia.thoi_gian_ket_thuc >= DateTime.Now && spct.GiamGia.trang_thai == "HoatDong"))
                    .GroupBy(spct => spct.SanPham.id_san_pham)
                    .Select(group => new SanPhamDTO
                    {
                        id_san_pham = group.First().SanPham.id_san_pham,
                        ma_san_pham = group.First().SanPham.ma_san_pham,
                        ten_san_pham = group.First().SanPham.ten_san_pham,
                        mo_ta = group.First().SanPham.mo_ta,
                        trang_thai = group.First().SanPham.trang_thai,
                        url_anh_mac_dinh = group.First().SanPham.anhMacDinh?.url,
                        ten_thuong_hieu = group.First().SanPham.ThuongHieu?.ten_thuong_hieu,
                        ten_danh_muc = group.First().SanPham.DanhMuc?.ten_danh_muc,
                        ten_kieu_dang = group.First().SanPham.KieuDang?.ten_kieu_dang,
                        ten_chat_lieu = group.First().SanPham.ChatLieu?.ten_chat_lieu,
                        ten_xuat_xu = group.First().SanPham.XuatXu?.ten_xuat_xu,
                        ngay_tao = group.First().SanPham.ngay_tao,
                        ngay_sua = group.First().SanPham.ngay_sua,
                        sanPhamChiTiets = group.Select(spct => new SanPhamChiTietDTO
                        {
                            id_san_pham_chi_tiet = spct.id_san_pham_chi_tiet,
                            ma_san_pham_chi_tiet = spct.ma_san_pham_chi_tiet,
                            so_luong = spct.so_luong,
                            gia_ban = spct.gia_ban,
                            gia_nhap = spct.gia_nhap,
                            trang_thai = spct.trang_thai,
                            ngay_tao = spct.ngay_tao,
                            ngay_sua = spct.ngay_sua,
                            hinhAnhSanPhamChiTiets = spct.HinhAnhSanPhamChiTiets.Select(ha => new HinhAnhSanPhamChiTietAdminDTO
                            {
                                hinh_anh_urls = ha.HinhAnhs.url,
                                id_hinh_anh = ha.HinhAnhs.id_hinh_anh
                            }).ToList(),
                            ten_mau_sac = spct.MauSac.ten_mau_sac,
                            ten_kich_co = spct.KichCo.ten_kich_co,
                            giamGia = spct.GiamGia != null ? new GiamGiaAdminDTO
                            {
                                id_giam_gia = spct.GiamGia.id_giam_gia,
                                ma_giam_gia = spct.GiamGia.ma_giam_gia,
                                ten_giam_gia = spct.GiamGia.ten_giam_gia,
                                kieu_giam_gia = spct.GiamGia.kieu_giam_gia,
                                gia_tri_giam = spct.GiamGia.gia_tri_giam,
                                thoi_gian_bat_dau = spct.GiamGia.thoi_gian_bat_dau,
                                thoi_gian_ket_thuc = spct.GiamGia.thoi_gian_ket_thuc,
                                trang_thai = spct.GiamGia.trang_thai
                            } : null
                        }).ToList()
                    }).ToList();

                // Áp dụng sắp xếp nếu có
                if (!string.IsNullOrEmpty(sap_xep_theo))
                {
                    sanPhamGiamGiaDTO = sap_xep_theo.ToLower() switch
                    {
                        "ten_san_pham" => sap_xep_tang
                            ? sanPhamGiamGiaDTO.OrderBy(sp => sp.ten_san_pham).ToList()
                            : sanPhamGiamGiaDTO.OrderByDescending(sp => sp.ten_san_pham).ToList(),
                        "ma_san_pham" => sap_xep_tang
                            ? sanPhamGiamGiaDTO.OrderBy(sp => sp.ma_san_pham).ToList()
                            : sanPhamGiamGiaDTO.OrderByDescending(sp => sp.ma_san_pham).ToList(),
                        "ngay_tao" => sap_xep_tang
                            ? sanPhamGiamGiaDTO.OrderBy(sp => sp.ngay_tao).ToList()
                            : sanPhamGiamGiaDTO.OrderByDescending(sp => sp.ngay_tao).ToList(),
                        "gia_ban" => sap_xep_tang
                            ? sanPhamGiamGiaDTO.OrderBy(sp => sp.sanPhamChiTiets.Min(spct => spct.gia_ban)).ToList()
                            : sanPhamGiamGiaDTO.OrderByDescending(sp => sp.sanPhamChiTiets.Min(spct => spct.gia_ban)).ToList(),
                        _ => sanPhamGiamGiaDTO
                    };
                }

                // Tính toán phân trang
                var tongSoPhanTu = sanPhamGiamGiaDTO.Count;
                var tongSoTrang = (int)Math.Ceiling(tongSoPhanTu / (double)so_phan_tu_tren_trang);
                trang_hien_tai = Math.Max(1, Math.Min(trang_hien_tai, tongSoTrang));

                // Lấy danh sách sản phẩm cho trang hiện tại
                var sanPhamsTrangHienTai = sanPhamGiamGiaDTO
                    .Skip((trang_hien_tai - 1) * so_phan_tu_tren_trang)
                    .Take(so_phan_tu_tren_trang)
                    .ToList();

                // Tạo kết quả phân trang
                var result = new
                {
                    trang_hien_tai = trang_hien_tai,
                    so_phan_tu_tren_trang = so_phan_tu_tren_trang,
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
            if (now < giamGia.thoi_gian_bat_dau || now > giamGia.thoi_gian_ket_thuc)
                return BadRequest("Mã giảm giá không còn hiệu lực");

            var products = await _sanPhamChiTietServices.GetByConditionAsync(
                sp => dto.san_pham_chi_tiet_ids.Contains(sp.id_san_pham_chi_tiet.ToString()));

            if (!products.Any())
                return BadRequest("Không tìm thấy sản phẩm hợp lệ để thêm vào giảm giá");

            foreach (var product in products)
            {
                product.id_giam_gia = Guid.Parse(dto.id_giam_gia);
                await _sanPhamChiTietServices.UpdateAsync(product);
            }

            ClearCache(); // Clear cache after adding discount to products
            return Ok("Thêm sản phẩm vào giảm giá thành công");
        }

        [HttpDelete("xoa-giam_gia-khoi-san-pham-chi-tiet")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> XoaGiamGiaKhoiSanPhamChiTiet([FromBody] XoaGiamGiaKhoiSanPhamChiTietDTO dto)
        {
            if (dto == null || !dto.san_pham_chi_tiet_ids.Any())
                return BadRequest("Danh sách sản phẩm không được để trống");

            var products = await _sanPhamChiTietServices.GetByConditionAsync(
                sp => dto.san_pham_chi_tiet_ids.Contains(sp.id_san_pham_chi_tiet.ToString()));

            if (!products.Any())
                return BadRequest("Không tìm thấy sản phẩm hợp lệ để xóa khỏi giảm giá");

            foreach (var product in products)
            {
                product.id_giam_gia = null;
                await _sanPhamChiTietServices.UpdateAsync(product);
            }

            ClearCache(); // Clear cache after removing discount from products
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
                    sp.sanPhamChiTiets.Any(spct => spct.giamGia != null && spct.giamGia.id_giam_gia == id)
                ).ToList();

                // Lọc lại danh sách sản phẩm chi tiết của từng sản phẩm, chỉ giữ lại các sản phẩm chi tiết có giảm giá với id được chỉ định
                foreach (var sp in allSanPhams)
                {
                    sp.sanPhamChiTiets = sp.sanPhamChiTiets
                        .Where(spct => spct.giamGia != null && spct.giamGia.id_giam_gia == id)
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
                            ? allSanPhams.OrderBy(sp => sp.sanPhamChiTiets != null ? sp.sanPhamChiTiets.Min(spct => spct.gia_ban) : 0).ToList()
                            : allSanPhams.OrderByDescending(sp => sp.sanPhamChiTiets != null ? sp.sanPhamChiTiets.Min(spct => spct.gia_ban) : 0).ToList(),
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
                    .Where(spct => spct.giamGia != null && spct.giamGia.id_giam_gia == id_giam_gia)
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
    }
}
