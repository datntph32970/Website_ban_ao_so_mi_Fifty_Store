using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_San_Pham;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using API.DbConects.Entities.Entities_Khuyen_Mai;

namespace API.Controllers.SanPham_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private readonly ISanPhamService _sanPhamServices;
        private readonly IBaseService<SanPhamChiTiet> _sanPhamChiTietServices;
        private readonly IBaseService<HinhAnh> _hinhAnhServices;
        private readonly IBaseService<HinhAnhSanPhamChiTiet> _hinhAnhSanPhamChiTietServices;
        private readonly IBaseService<MauSac> _mauSacServices;
        private readonly IBaseService<HoaDonChiTiet> _hoaDonChiTietServices;
        private readonly IBaseService<KichCo> _kichCoServices;
        private readonly IJwtServices _jwtServices;
        private readonly IBaseService<SanPhamChiTietGiamGia> _sanPhamChiTietGiamGiaServices;
        private readonly IBaseService<GiamGia> _giamGiaServices;
        private static readonly object _lockObject = new object();

        public SanPhamController(ISanPhamService sanPhamServices, IBaseService<SanPhamChiTiet> sanPhamChiTietServices, IBaseService<HinhAnh> hinhAnhServices, IBaseService<HinhAnhSanPhamChiTiet> hinhAnhSanPhamChiTietServices, IBaseService<MauSac> mauSacServices, IBaseService<KichCo> kichCoServices, IJwtServices jwtServices, IBaseService<HoaDonChiTiet> hoaDonChiTietServices, IBaseService<SanPhamChiTietGiamGia> sanPhamChiTietGiamGiaServices, IBaseService<GiamGia> giamGiaServices)
        {
            _sanPhamServices = sanPhamServices;
            _sanPhamChiTietServices = sanPhamChiTietServices;
            _hinhAnhServices = hinhAnhServices;
            _hinhAnhSanPhamChiTietServices = hinhAnhSanPhamChiTietServices;
            _mauSacServices = mauSacServices;
            _kichCoServices = kichCoServices;
            _jwtServices = jwtServices;
            _hoaDonChiTietServices = hoaDonChiTietServices;
            _sanPhamChiTietGiamGiaServices = sanPhamChiTietGiamGiaServices;
            _giamGiaServices = giamGiaServices;
        }

        [HttpPost("lay-danh-sach-san-pham-admin-dto")]
        [Authorize(Roles = "NhanVien,Admin")]
        public async Task<IActionResult> GetAll([FromBody] ThamSoPhanTrangSanPhamDTO thamSo)
        {
            try
            {
                // Lấy tất cả sản phẩm với các thuộc tính liên quan
                var allSanPhams = await _sanPhamServices.GetAllSanPhamAdminDTOAsync();

                // Sắp xếp theo ngày tạo giảm dần
                allSanPhams = allSanPhams.OrderByDescending(sp => sp.ngay_tao).ToList();

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

        [HttpGet("lay-san-pham-admin-dto-theo-id")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _sanPhamServices.GetByIdSanPhamAdminDTOAsync(id);

            return Ok(result);
        }

        #region Private Validation Methods
        private async Task<IActionResult> ValidateSanPhamDTO(ThemSanPhamAdminDTO sanPhamDTO)
        {
            if (sanPhamDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (sanPhamDTO.ten_san_pham == null)
                return BadRequest("Yêu cầu nhập tên sản phẩm");
            if (sanPhamDTO.mo_ta == null)
                return BadRequest("Yêu cầu nhập mô tả");
            if (sanPhamDTO.id_kieu_dang == null)
                return BadRequest("Yêu cầu nhập mã kiểu dáng");
            if (sanPhamDTO.id_chat_lieu == null)
                return BadRequest("Yêu cầu nhập mã chất liệu");
            if (sanPhamDTO.id_thuong_hieu == null)
                return BadRequest("Yêu cầu nhập mã thương hiệu");
            if (sanPhamDTO.id_xuat_xu == null)
                return BadRequest("Yêu cầu nhập mã xuất xứ");
            if (sanPhamDTO.url_anh_mac_dinh == null)
                return BadRequest("chọn hình ảnh mặc định cho sản phẩm");

            var existingTenSanPham = await _sanPhamServices.ExistsAsync(x => x.ten_san_pham == sanPhamDTO.ten_san_pham);
            if (existingTenSanPham)
                return BadRequest("Tên sản phẩm đã tồn tại");

            var existingSanPham = await _sanPhamServices.ExistsAsync(x =>
                x.ten_san_pham == sanPhamDTO.ten_san_pham &&
                x.id_thuong_hieu == Guid.Parse(sanPhamDTO.id_thuong_hieu) &&
                x.id_chat_lieu == Guid.Parse(sanPhamDTO.id_chat_lieu) &&
                x.id_kieu_dang == Guid.Parse(sanPhamDTO.id_kieu_dang) &&
                x.id_xuat_xu == Guid.Parse(sanPhamDTO.id_xuat_xu));
            if (existingSanPham)
                return BadRequest("Thông tin sản phẩm đã tồn tại");

            return null;
        }

        private async Task<IActionResult> ValidateSanPhamChiTiet(ThemSanPhamAdminDTO sanPhamDTO)
        {
            if (sanPhamDTO.sanPhamChiTiets == null || sanPhamDTO.sanPhamChiTiets.Count == 0)
                return BadRequest("Yêu cầu nhập sản phẩm chi tiết");

            const int MAX_QUANTITY = 1000000; // Giới hạn số lượng tối đa

            foreach (var spct in sanPhamDTO.sanPhamChiTiets)
            {
                var mauSac = await _mauSacServices.GetByIdAsync(spct.id_mau_sac);
                var kichCo = await _kichCoServices.GetByIdAsync(spct.id_kich_co);
                var thongTinChiTiet = $"(Màu sắc: {mauSac?.ten_mau_sac}, Kích cỡ: {kichCo?.ten_kich_co})";

                if (spct.id_mau_sac == null)
                    return BadRequest($"Yêu cầu nhập mã màu sắc cho sản phẩm chi tiết {thongTinChiTiet}");
                if (spct.id_kich_co == null)
                    return BadRequest($"Yêu cầu nhập mã kích cỡ cho sản phẩm chi tiết {thongTinChiTiet}");
                if (spct.them_hinh_anh_spcts == null || spct.them_hinh_anh_spcts.Count == 0)
                    return BadRequest($"Yêu cầu chọn hình ảnh cho sản phẩm chi tiết {thongTinChiTiet}");

                // Kiểm tra số lượng
                if (spct.so_luong < 0)
                    return BadRequest($"Số lượng sản phẩm chi tiết {thongTinChiTiet} không được âm");

                if (spct.so_luong > MAX_QUANTITY)
                    return BadRequest($"Số lượng sản phẩm chi tiết {thongTinChiTiet} không được vượt quá {MAX_QUANTITY}");

                if (!int.TryParse(spct.so_luong.ToString(), out _))
                    return BadRequest($"Số lượng sản phẩm chi tiết {thongTinChiTiet} phải là số nguyên");

                if (spct.gia_ban <= 0)
                    return BadRequest($"Yêu cầu nhập giá bán lớn hơn 0 cho sản phẩm chi tiết {thongTinChiTiet}");
                if (spct.gia_nhap <= 0)
                    return BadRequest($"Yêu cầu nhập giá nhập lớn hơn 0 cho sản phẩm chi tiết {thongTinChiTiet}");
                if (spct.gia_ban < spct.gia_nhap)
                    return BadRequest($"Giá bán ({spct.gia_ban:N0}đ) phải lớn hơn hoặc bằng giá nhập ({spct.gia_nhap:N0}đ) cho sản phẩm chi tiết {thongTinChiTiet}");
            }

            return null;
        }

        private async Task<IActionResult> ValidateDuplicateSanPhamChiTiet(ThemSanPhamAdminDTO sanPhamDTO, Guid sanPhamId)
        {
            var duplicates = sanPhamDTO.sanPhamChiTiets
                .GroupBy(x => new { x.id_mau_sac, x.id_kich_co })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .FirstOrDefault();

            if (duplicates != null)
            {
                var duplicatesInDb = await _sanPhamChiTietServices.ExistsAsync(x =>
                    x.id_mau_sac == duplicates.id_mau_sac &&
                    x.id_kich_co == duplicates.id_kich_co &&
    x.id_san_pham == sanPhamId);

                if (duplicatesInDb)
                    return BadRequest("Màu sắc và kích cỡ này đã tồn tại trong sản phẩm");

                return BadRequest("Không được phép trùng lặp màu sắc và kích cỡ trong cùng một sản phẩm");
            }

            return null;
        }

        private async Task<IActionResult> ValidateHinhAnh(ThemSanPhamAdminDTO sanPhamDTO)
        {
            if (sanPhamDTO.sanPhamChiTiets.Any(x => x.them_hinh_anh_spcts == null || x.them_hinh_anh_spcts.Count == 0))
                return BadRequest("Yêu cầu chọn hình ảnh");

            foreach (var spct in sanPhamDTO.sanPhamChiTiets)
            {
                if (spct.them_hinh_anh_spcts != null)
                {
                    var duplicateImageNames = spct.them_hinh_anh_spcts
                        .GroupBy(x => x.hinh_anh_urls)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .FirstOrDefault();

                    if (duplicateImageNames != null && duplicateImageNames != "")
                    {
                        return BadRequest("Không được phép trùng lặp hình ảnh trong cùng một sản phẩm chi tiết");
                    }
                }
            }

            foreach (var spct in sanPhamDTO.sanPhamChiTiets)
            {
                foreach (var hinhAnh in spct.them_hinh_anh_spcts)
                {
                    var isImageUsedInOtherProductDetail = await _hinhAnhSanPhamChiTietServices.ExistsAsync(x =>
                        x.HinhAnhs.url == hinhAnh.hinh_anh_urls);

                    if (isImageUsedInOtherProductDetail)
                    {
                        return BadRequest($"Hình ảnh {hinhAnh.hinh_anh_urls} đã tồn tại trong sản phẩm chi tiết khác.");
                    }
                }
            }

            return null;
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
            hinhAnh.url = $"/images/products/{fileName}";

            return hinhAnh;
        }

        private async Task<bool> DeleteHinhAnh(HinhAnh hinhAnh)
        {
            try
            {
                var filePath = Path.Combine("wwwroot", hinhAnh.url.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return await _hinhAnhServices.DeleteAsync(hinhAnh.id_hinh_anh);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa file {hinhAnh.url}: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SaveHinhAnhSanPhamChiTiet(List<ThemHinhAnhSanPhamChiTietAdminDTO> hinhAnhs, Guid sanPhamChiTietId, string maSanPham, Guid mauSacId)
        {
            var mauSac = await _mauSacServices.GetByIdAsync(mauSacId);
            var maHinhAnh = $"Image-{maSanPham}-{RemoveDiacriticsAndClean(mauSac.ten_mau_sac).ToLower()}";
            var folderPath = Path.Combine("wwwroot", "images", "products");
            int count = 1;
            foreach (var hinhAnh in hinhAnhs)
            {
                var hinhAnhMoi = await SaveHinhAnh(hinhAnh.hinh_anh_urls, maHinhAnh + $"-{count}", folderPath);
                var hinhAnhResult = await _hinhAnhServices.CreateAsync(hinhAnhMoi);
                if (!hinhAnhResult) return false;

                var hinhanhsanphamchitiet = new HinhAnhSanPhamChiTiet
                {
                    id_hinh_anh_san_pham_chi_tiet = Guid.NewGuid(),
                    id_hinh_anh = hinhAnhMoi.id_hinh_anh,
                    id_san_pham_chi_tiet = sanPhamChiTietId,
                };

                var hinhAnhSPCTResult = await _hinhAnhSanPhamChiTietServices.CreateAsync(hinhanhsanphamchitiet);
                if (!hinhAnhSPCTResult) return false;
                count++;
            }

            return true;
        }
        #endregion

        [HttpPost("them-san-pham")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Add(ThemSanPhamAdminDTO sanPhamDTO)
        {
            try
            {
                // Validate sản phẩm
                var validateResult = await ValidateSanPhamDTO(sanPhamDTO);
                if (validateResult != null) return validateResult;

                // Validate sản phẩm chi tiết
                validateResult = await ValidateSanPhamChiTiet(sanPhamDTO);
                if (validateResult != null) return validateResult;

                // Validate trùng lặp sản phẩm chi tiết
                validateResult = await ValidateDuplicateSanPhamChiTiet(sanPhamDTO, Guid.NewGuid());
                if (validateResult != null) return validateResult;

                // Validate hình ảnh
                validateResult = await ValidateHinhAnh(sanPhamDTO);
                if (validateResult != null) return validateResult;

                var result = await _sanPhamServices.ExecuteInTransactionAsync(async () =>
                {
                    // Tạo sản phẩm mới
                    var sanPham = new SanPham
                    {
                        id_san_pham = Guid.NewGuid(),
                        ma_san_pham = await TaoMaSanPham(),
                        ten_san_pham = sanPhamDTO.ten_san_pham,
                        mo_ta = sanPhamDTO.mo_ta,
                        id_kieu_dang = Guid.Parse(sanPhamDTO.id_kieu_dang),
                        id_chat_lieu = Guid.Parse(sanPhamDTO.id_chat_lieu),
                        id_danh_muc = Guid.Parse(sanPhamDTO.id_danh_muc),
                        id_thuong_hieu = Guid.Parse(sanPhamDTO.id_thuong_hieu),
                        id_xuat_xu = Guid.Parse(sanPhamDTO.id_xuat_xu),
                        trang_thai = "HoatDong",
                        id_nguoi_tao = (Guid)GetIdNhanVien(),
                        ngay_tao = DateTime.Now
                    };

                    // Lưu ảnh mặc định
                    var hinhanhMacDinh = await SaveHinhAnh(sanPhamDTO.url_anh_mac_dinh, "Image-" + sanPham.ma_san_pham, Path.Combine("wwwroot", "images", "products"));
                    var hinhAnhSanPhamResult = await _hinhAnhServices.CreateAsync(hinhanhMacDinh);
                    if (!hinhAnhSanPhamResult) return false;
                    sanPham.id_anh_mac_dinh = hinhanhMacDinh.id_hinh_anh;

                    // Lưu sản phẩm
                    var sanPhamResult = await _sanPhamServices.CreateAsync(sanPham);
                    if (!sanPhamResult) return false;

                    // Lưu sản phẩm chi tiết và hình ảnh
                    foreach (var spct in sanPhamDTO.sanPhamChiTiets)
                    {
                        var sanPhamChiTiet = new SanPhamChiTiet
                        {
                            id_san_pham_chi_tiet = Guid.NewGuid(),
                            ma_san_pham_chi_tiet = $"{sanPham.ma_san_pham}-{RemoveDiacriticsAndClean(_mauSacServices.GetByIdAsync(spct.id_mau_sac).Result.ten_mau_sac).ToLower()}-{RemoveDiacriticsAndClean(_kichCoServices.GetByIdAsync(spct.id_kich_co).Result.ten_kich_co).ToLower()}",
                            id_san_pham = sanPham.id_san_pham,
                            id_mau_sac = spct.id_mau_sac,
                            id_kich_co = spct.id_kich_co,
                            so_luong = spct.so_luong,
                            gia_ban = spct.gia_ban,
                            gia_nhap = spct.gia_nhap,
                            trang_thai = "HoatDong",
                            id_nguoi_tao = (Guid)GetIdNhanVien(),
                            ngay_tao = DateTime.Now
                        };

                        var chiTietResult = await _sanPhamChiTietServices.CreateAsync(sanPhamChiTiet);
                        if (!chiTietResult) return false;

                        // Thêm giảm giá nếu có
                        if (spct.id_giam_gia != null && spct.id_giam_gia.Any())
                        {
                            var themGiamGiaResult = await ThemGiamGiaChoSanPhamChiTiet(sanPhamChiTiet.id_san_pham_chi_tiet, spct.id_giam_gia);
                            if (!themGiamGiaResult)
                            {
                                throw new InvalidOperationException("Không thể thêm giảm giá cho sản phẩm chi tiết do trùng thời gian với giảm giá khác");
                            }
                        }

                        // Lưu hình ảnh cho sản phẩm chi tiết
                        var saveHinhAnhResult = await SaveHinhAnhSanPhamChiTiet(spct.them_hinh_anh_spcts, sanPhamChiTiet.id_san_pham_chi_tiet, sanPham.ma_san_pham, spct.id_mau_sac);
                        if (!saveHinhAnhResult) return false;
                    }

                    return true;
                });

                if (!result)
                    return BadRequest("Đã có lỗi xảy ra khi thêm sản phẩm");

                return Ok("Thêm sản phẩm và chi tiết thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }
        #region Private Validation Methods for Update
        private async Task<IActionResult> ValidateUpdateSanPhamDTO(Guid id, SuaSanPhamAdminDTO sanPhamDTO)
        {
            if (sanPhamDTO == null)
                return BadRequest("Đối tượng không được để trống");
            if (string.IsNullOrEmpty(sanPhamDTO.ten_san_pham))
                return BadRequest("Yêu cầu nhập tên sản phẩm");
            if (string.IsNullOrEmpty(sanPhamDTO.mo_ta))
                return BadRequest("Yêu cầu nhập mô tả");
            if (string.IsNullOrEmpty(sanPhamDTO.id_kieu_dang))
                return BadRequest("Yêu cầu nhập mã kiểu dáng");
            if (string.IsNullOrEmpty(sanPhamDTO.id_chat_lieu))
                return BadRequest("Yêu cầu nhập mã chất liệu");
            if (string.IsNullOrEmpty(sanPhamDTO.id_thuong_hieu))
                return BadRequest("Yêu cầu nhập mã thương hiệu");
            if (string.IsNullOrEmpty(sanPhamDTO.id_xuat_xu))
                return BadRequest("Yêu cầu nhập mã xuất xứ");
            if (string.IsNullOrEmpty(sanPhamDTO.trang_thai))
                return BadRequest("Yêu cầu nhập trạng thái");

            var existingSanPham = await _sanPhamServices.GetByIdAsync(id);
            if (existingSanPham == null)
                return NotFound("Không tìm thấy sản phẩm");

            var existingTenSanPham = await _sanPhamServices.ExistsAsync(x =>
                x.ten_san_pham == sanPhamDTO.ten_san_pham &&
                x.id_san_pham != id);
            if (existingTenSanPham)
                return BadRequest("Tên sản phẩm đã tồn tại");

            return null;
        }

        private async Task<IActionResult> ValidateUpdateSanPhamChiTiet(SuaSanPhamAdminDTO sanPhamDTO)
        {
            if (sanPhamDTO.sanPhamChiTiets == null || !sanPhamDTO.sanPhamChiTiets.Any())
                return BadRequest("Danh sách sản phẩm chi tiết không được để trống");

            const int MAX_QUANTITY = 1000000; // Giới hạn số lượng tối đa

            foreach (var spct in sanPhamDTO.sanPhamChiTiets)
            {
                if (spct.id_mau_sac == Guid.Empty || spct.id_kich_co == Guid.Empty)
                    continue;

                var mauSac = await _mauSacServices.GetByIdAsync(spct.id_mau_sac);
                var kichCo = await _kichCoServices.GetByIdAsync(spct.id_kich_co);
                var thongTinChiTiet = $"(Màu sắc: {mauSac?.ten_mau_sac}, Kích cỡ: {kichCo?.ten_kich_co})";

                // Kiểm tra số lượng
                if (spct.so_luong < 0)
                    return BadRequest($"Số lượng sản phẩm chi tiết {thongTinChiTiet} không được âm");

                if (spct.so_luong > MAX_QUANTITY)
                    return BadRequest($"Số lượng sản phẩm chi tiết {thongTinChiTiet} không được vượt quá {MAX_QUANTITY}");

                if (!int.TryParse(spct.so_luong.ToString(), out _))
                    return BadRequest($"Số lượng sản phẩm chi tiết {thongTinChiTiet} phải là số nguyên");

                if (spct.gia_ban <= 0)
                    return BadRequest($"Giá bán sản phẩm chi tiết {thongTinChiTiet} phải lớn hơn 0");

                if (spct.gia_nhap <= 0)
                    return BadRequest($"Giá nhập sản phẩm chi tiết {thongTinChiTiet} phải lớn hơn 0");

                if (spct.gia_ban < spct.gia_nhap)
                    return BadRequest($"Giá bán ({spct.gia_ban:N0}đ) phải lớn hơn hoặc bằng giá nhập ({spct.gia_nhap:N0}đ) cho sản phẩm chi tiết {thongTinChiTiet}");

                if (string.IsNullOrEmpty(spct.trang_thai))
                    return BadRequest($"Yêu cầu nhập trạng thái cho sản phẩm chi tiết {thongTinChiTiet}");
            }

            return null;
        }

        private async Task<IActionResult> ValidateUpdateDuplicateSanPhamChiTiet(SuaSanPhamAdminDTO sanPhamDTO, Guid sanPhamId)
        {
            var duplicates = sanPhamDTO.sanPhamChiTiets
                .GroupBy(x => new { x.id_mau_sac, x.id_kich_co })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .FirstOrDefault();

            if (duplicates != null)
            {
                var duplicatesInDb = await _sanPhamChiTietServices.ExistsAsync(x =>
                    x.id_mau_sac == duplicates.id_mau_sac &&
                    x.id_kich_co == duplicates.id_kich_co &&
    x.id_san_pham == sanPhamId &&
                    !sanPhamDTO.sanPhamChiTiets.Any(spct => spct.id_san_pham_chi_tiet == x.id_san_pham_chi_tiet));

                if (duplicatesInDb)
                    return BadRequest("Màu sắc và kích cỡ này đã tồn tại trong sản phẩm");

                return BadRequest("Không được phép trùng lặp màu sắc và kích cỡ trong cùng một sản phẩm");
            }

            return null;
        }

        private async Task<IActionResult> ValidateUpdateHinhAnh(SuaSanPhamAdminDTO sanPhamDTO)
        {
            foreach (var spct in sanPhamDTO.sanPhamChiTiets)
            {
                if (spct.them_hinh_anh_spcts != null && spct.them_hinh_anh_spcts.Count > 0)
                {
                    var duplicateImageNames = spct.them_hinh_anh_spcts
                        .GroupBy(x => x.hinh_anh_urls)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .FirstOrDefault();

                    if (duplicateImageNames != null && duplicateImageNames != "")
                    {
                        return BadRequest("Không được phép trùng lặp hình ảnh trong cùng một sản phẩm chi tiết");
                    }
                }
                else
                    return BadRequest("Cần có hình ảnh cho sản phẩm chi tiết");
            }

            return null;
        }

        private async Task<bool> UpdateHinhAnhSanPhamChiTiet(SanPhamChiTiet existingChiTiet, List<ThemHinhAnhSanPhamChiTietAdminDTO> newImages, string maSanPham, Guid mauSacId)
        {
            try
            {
                // Xóa tất cả hình ảnh cũ của sản phẩm chi tiết này
                var hinhAnhs = await _hinhAnhSanPhamChiTietServices.GetByConditionAsync(x =>
                    x.id_san_pham_chi_tiet == existingChiTiet.id_san_pham_chi_tiet);

                foreach (var hinhAnh in hinhAnhs)
                {
                    await _hinhAnhSanPhamChiTietServices.DeleteAsync(hinhAnh.id_hinh_anh_san_pham_chi_tiet);
                    await _hinhAnhServices.DeleteAsync(hinhAnh.id_hinh_anh);
                }

                // Thêm hình ảnh mới
                if (newImages != null && newImages.Any())
                {
                    var mauSac = await _mauSacServices.GetByIdAsync(mauSacId);
                    var maHinhAnh = $"Image-{maSanPham}-{RemoveDiacriticsAndClean(mauSac.ten_mau_sac).ToLower()}";
                    var folderPath = Path.Combine("wwwroot", "images", "products");
                    int count = 1;

                    foreach (var hinhAnh in newImages)
                    {
                        var hinhAnhMoi = await SaveHinhAnh(hinhAnh.hinh_anh_urls, maHinhAnh + $"-{count}", folderPath);
                        var hinhAnhResult = await _hinhAnhServices.CreateAsync(hinhAnhMoi);
                        if (!hinhAnhResult) return false;

                        var hinhanhsanphamchitiet = new HinhAnhSanPhamChiTiet
                        {
                            id_hinh_anh_san_pham_chi_tiet = Guid.NewGuid(),
                            id_hinh_anh = hinhAnhMoi.id_hinh_anh,
                            id_san_pham_chi_tiet = existingChiTiet.id_san_pham_chi_tiet,
                        };

                        var hinhAnhSPCTResult = await _hinhAnhSanPhamChiTietServices.CreateAsync(hinhanhsanphamchitiet);
                        if (!hinhAnhSPCTResult) return false;
                        count++;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật hình ảnh: {ex.Message}");
                return false;
            }
        }
        #endregion

        [HttpPut("sua-san-pham")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Update(Guid id, SuaSanPhamAdminDTO sanPhamDTO)
        {
            try
            {
                // Validate sản phẩm
                var validateResult = await ValidateUpdateSanPhamDTO(id, sanPhamDTO);
                if (validateResult != null) return validateResult;

                // Validate sản phẩm chi tiết
                validateResult = await ValidateUpdateSanPhamChiTiet(sanPhamDTO);
                if (validateResult != null) return validateResult;

                // Validate trùng lặp sản phẩm chi tiết
                validateResult = await ValidateUpdateDuplicateSanPhamChiTiet(sanPhamDTO, id);
                if (validateResult != null) return validateResult;

                // Validate hình ảnh
                validateResult = await ValidateUpdateHinhAnh(sanPhamDTO);
                if (validateResult != null) return validateResult;

                var result = await UpdateSanPham(id, sanPhamDTO);
                if (result)
                {
                    return Ok("Cập nhật sản phẩm thành công");
                }
                return BadRequest("Cập nhật sản phẩm thất bại");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        private async Task<bool> UpdateSanPham(Guid id, SuaSanPhamAdminDTO sanPhamDTO)
        {
            try
            {
                var id_nhan_vien = GetIdNhanVien();
                // Lấy sản phẩm hiện tại với chi tiết và hình ảnh
                var existingSanPham = await _sanPhamServices.GetByIdWithIncludeAsync(id,
                    q => q.Include(sp => sp.SanPhamChiTiets)
                        .ThenInclude(spct => spct.HinhAnhSanPhamChiTiets)
                            .ThenInclude(ha => ha.HinhAnhs));

                if (existingSanPham == null)
                    return false;

                // Cập nhật thông tin sản phẩm
                existingSanPham.ten_san_pham = sanPhamDTO.ten_san_pham;
                existingSanPham.mo_ta = sanPhamDTO.mo_ta;
                existingSanPham.id_kieu_dang = Guid.Parse(sanPhamDTO.id_kieu_dang);
                existingSanPham.id_chat_lieu = Guid.Parse(sanPhamDTO.id_chat_lieu);
                existingSanPham.id_thuong_hieu = Guid.Parse(sanPhamDTO.id_thuong_hieu);
                existingSanPham.id_xuat_xu = Guid.Parse(sanPhamDTO.id_xuat_xu);
                existingSanPham.id_danh_muc = Guid.Parse(sanPhamDTO.id_danh_muc);
                existingSanPham.trang_thai = sanPhamDTO.trang_thai;
                existingSanPham.id_nguoi_sua = id_nhan_vien;
                existingSanPham.ngay_sua = DateTime.Now;
                // Xử lý ảnh mặc định mới nếu có
                if (!string.IsNullOrEmpty(sanPhamDTO.url_anh_mac_dinh))
                {
                    var oldDefaultImage = await _hinhAnhServices.GetByIdAsync(existingSanPham.id_anh_mac_dinh.Value);
                    if (oldDefaultImage != null)
                    {
                        await DeleteHinhAnh(oldDefaultImage);
                    }

                    var hinhanhMacDinh = await SaveHinhAnh(sanPhamDTO.url_anh_mac_dinh, "Image-" + existingSanPham.ma_san_pham, Path.Combine("wwwroot", "images", "products"));
                    var hinhAnhSanPhamResult = await _hinhAnhServices.CreateAsync(hinhanhMacDinh);
                    if (!hinhAnhSanPhamResult) return false;
                    existingSanPham.id_anh_mac_dinh = hinhanhMacDinh.id_hinh_anh;
                }

                await _sanPhamServices.UpdateAsync(existingSanPham);

                // Xử lý cập nhật chi tiết sản phẩm
                if (sanPhamDTO.sanPhamChiTiets != null)
                {
                    foreach (var spctDTO in sanPhamDTO.sanPhamChiTiets)
                    {
                        var existingChiTiet = existingSanPham.SanPhamChiTiets
                            .FirstOrDefault(spct => spct.id_san_pham_chi_tiet == spctDTO.id_san_pham_chi_tiet);

                        if (existingChiTiet != null)
                        {
                            // Cập nhật thông tin chi tiết
                            existingChiTiet.so_luong = spctDTO.so_luong;
                            existingChiTiet.gia_ban = spctDTO.gia_ban;
                            existingChiTiet.gia_nhap = spctDTO.gia_nhap;
                            existingChiTiet.trang_thai = spctDTO.trang_thai;
                            existingChiTiet.id_nguoi_sua = id_nhan_vien;
                            existingChiTiet.ngay_sua = DateTime.Now;

                            await _sanPhamChiTietServices.UpdateAsync(existingChiTiet);

                            // Cập nhật giảm giá
                            if (spctDTO.id_giam_gia != null)
                            {
                                // Lấy danh sách giảm giá hiện tại
                                var giamGiaHienTai = await _sanPhamChiTietGiamGiaServices.GetByConditionWithIncludeAsync(
                                    x => x.id_san_pham_chi_tiet == existingChiTiet.id_san_pham_chi_tiet,
                                    q => q.Include(gg => gg.GiamGia));

                                // Xóa các giảm giá không còn trong danh sách mới
                                foreach (var giamGia in giamGiaHienTai)
                                {
                                    if (!spctDTO.id_giam_gia.Contains(giamGia.id_giam_gia.ToString()))
                                    {
                                        await _sanPhamChiTietGiamGiaServices.DeleteAsync(giamGia.id);
                                    }
                                }

                                // Lọc ra các giảm giá mới (chưa có trong danh sách hiện tại)
                                var idGiamGiaHienTai = giamGiaHienTai.Select(x => x.id_giam_gia.ToString()).ToList();
                                var idGiamGiaMoi = spctDTO.id_giam_gia.Where(x => !idGiamGiaHienTai.Contains(x)).ToList();

                                // Thêm giảm giá mới nếu có
                                if (idGiamGiaMoi.Any())
                                {
                                    var themGiamGiaResult = await ThemGiamGiaChoSanPhamChiTiet(existingChiTiet.id_san_pham_chi_tiet, idGiamGiaMoi);
                                    if (!themGiamGiaResult)
                                    {
                                        throw new InvalidOperationException($"Không thể thêm giảm giá cho sản phẩm chi tiết {existingChiTiet.ma_san_pham_chi_tiet} do trùng thời gian với giảm giá khác");
                                    }
                                }
                            }
                            else
                            {
                                // Nếu không có giảm giá mới, xóa tất cả giảm giá hiện tại
                                var giamGiaHienTai = await _sanPhamChiTietGiamGiaServices.GetByConditionAsync(
                                    x => x.id_san_pham_chi_tiet == existingChiTiet.id_san_pham_chi_tiet);

                                foreach (var giamGia in giamGiaHienTai)
                                {
                                    await _sanPhamChiTietGiamGiaServices.DeleteAsync(giamGia.id);
                                }
                            }

                            // Cập nhật hình ảnh
                            var updateHinhAnhResult = await UpdateHinhAnhSanPhamChiTiet(
                                existingChiTiet,
                                spctDTO.them_hinh_anh_spcts,
                                existingSanPham.ma_san_pham,
                                spctDTO.id_mau_sac);
                            if (!updateHinhAnhResult) return false;
                        }
                        else
                        {
                            // Thêm sản phẩm chi tiết mới
                            var sanPhamChiTiet = new SanPhamChiTiet
                            {
                                id_san_pham_chi_tiet = Guid.NewGuid(),
                                ma_san_pham_chi_tiet = $"{existingSanPham.ma_san_pham}-{RemoveDiacriticsAndClean(_mauSacServices.GetByIdAsync(spctDTO.id_mau_sac).Result.ten_mau_sac).ToLower()}-{RemoveDiacriticsAndClean(_kichCoServices.GetByIdAsync(spctDTO.id_kich_co).Result.ten_kich_co).ToLower()}",
                                id_san_pham = existingSanPham.id_san_pham,
                                id_mau_sac = spctDTO.id_mau_sac,
                                id_kich_co = spctDTO.id_kich_co,
                                so_luong = spctDTO.so_luong,
                                gia_ban = spctDTO.gia_ban,
                                gia_nhap = spctDTO.gia_nhap,
                                trang_thai = "HoatDong",
                                id_nguoi_tao = (Guid)GetIdNhanVien(),
                                ngay_tao = DateTime.Now
                            };

                            var chiTietResult = await _sanPhamChiTietServices.CreateAsync(sanPhamChiTiet);
                            if (!chiTietResult) return false;

                            // Thêm giảm giá nếu có
                            if (spctDTO.id_giam_gia != null && spctDTO.id_giam_gia.Any())
                            {
                                var themGiamGiaResult = await ThemGiamGiaChoSanPhamChiTiet(sanPhamChiTiet.id_san_pham_chi_tiet, spctDTO.id_giam_gia);
                                if (!themGiamGiaResult)
                                {
                                    throw new InvalidOperationException($"Không thể thêm giảm giá cho sản phẩm chi tiết {sanPhamChiTiet.ma_san_pham_chi_tiet} do trùng thời gian với giảm giá khác");
                                }
                            }

                            // Thêm hình ảnh cho sản phẩm chi tiết mới
                            var saveHinhAnhResult = await SaveHinhAnhSanPhamChiTiet(
                                spctDTO.them_hinh_anh_spcts,
                                sanPhamChiTiet.id_san_pham_chi_tiet,
                                existingSanPham.ma_san_pham,
                                spctDTO.id_mau_sac);
                            if (!saveHinhAnhResult) return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpDelete("xoa-san-pham")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var sp = await _sanPhamServices.GetByIdWithIncludeAsync(id, q => q.Include(sp => sp.SanPhamChiTiets).ThenInclude(spct => spct.HoaDonChiTiets));
            if (sp == null)
                return NotFound("Sản phẩm không tồn tại");

            if (sp.SanPhamChiTiets.Any(spct => spct.HoaDonChiTiets.Any()))
                return BadRequest("Sản phẩm đã có hóa đơn không thể xóa");

            var result = await _sanPhamServices.ExecuteInTransactionAsync(async () =>
            {
                var sanPham = await _sanPhamServices.GetByIdAsync(id);
                if (sanPham == null)
                    return false;
                var sanPhamChiTiets = await _sanPhamChiTietServices.GetByConditionAsync(x => x.id_san_pham == id);
                if (sanPhamChiTiets == null)
                    return false;

                foreach (var spct in sanPhamChiTiets)
                {
                    var hinhAnhs = await _hinhAnhSanPhamChiTietServices.GetByConditionAsync(x =>
                        x.id_san_pham_chi_tiet == spct.id_san_pham_chi_tiet);
                    foreach (var hinhAnh in hinhAnhs)
                    {
                        await _hinhAnhSanPhamChiTietServices.DeleteAsync(hinhAnh.id_hinh_anh_san_pham_chi_tiet);
                        await _hinhAnhServices.DeleteAsync(hinhAnh.id_hinh_anh);
                    }
                    var deleteSPCT = await _sanPhamChiTietServices.DeleteAsync(spct.id_san_pham_chi_tiet);
                    if (!deleteSPCT)
                        return false;
                }
                // Xóa ảnh mặc định của sản phẩm
                if (sanPham.id_anh_mac_dinh != null)
                {
                    var hinhAnhMacDinh = await _hinhAnhServices.GetByIdAsync((Guid)sanPham.id_anh_mac_dinh);
                    if (hinhAnhMacDinh != null)
                    {
                        var deleteHinhAnhResult = await DeleteHinhAnh(hinhAnhMacDinh);
                        if (!deleteHinhAnhResult)
                            return false;
                    }
                }
                var deleteSanPham = await _sanPhamServices.DeleteAsync(id);
                if (!deleteSanPham)
                    return false;
                return true;
            });
            if (result) return Ok("Xóa sản phẩm thành công");
            return BadRequest("Đã có lỗi khi xóa sản phẩm");
        }
        private async Task<string> TaoMaSanPham()
        {
            var lastSanPham = await _sanPhamServices.GetAllAsync();
            if (lastSanPham.Count == 0)
                return "SP00001";
            int startNumber = int.Parse(lastSanPham.OrderByDescending(x => x.ma_san_pham).FirstOrDefault().ma_san_pham.Substring(2)) + 1;
            return $"SP{startNumber:D5}";
        }
        private Guid? GetIdNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                throw new UnauthorizedAccessException("Token không hợp lệ");
            Guid? idtnhanvien = _jwtServices.GetIdNhanVienFromToken(token);
            return idtnhanvien;
        }
        private static string RemoveDiacriticsAndClean(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Loại bỏ dấu
            var normalized = text.Normalize(NormalizationForm.FormD);
            var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
            var result = new string(chars.ToArray()).Normalize(NormalizationForm.FormC);

            // Loại bỏ khoảng trắng và ký tự đặc biệt
            result = Regex.Replace(result, @"[^a-zA-Z0-9]", "");

            return result;
        }
        [HttpGet("get-image/{fileName}")]
        [AllowAnonymous]
        public IActionResult GetImage(string fileName)
        {
            var imagePath = Path.Combine("wwwroot", "images", "products", fileName);
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound();
            }

            var imageBytes = System.IO.File.ReadAllBytes(imagePath);
            return File(imageBytes, "image/jpeg");
        }
        [HttpPut("update-trang-thai-san-pham/{id}")]
        [Authorize(Roles = "NhanVien,Admin")]
        public async Task<IActionResult> UpdateTrangThai(Guid id, [FromBody] CapNhatTrangThaiSanPhamDTO dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.trang_thai))
                    return BadRequest("Trạng thái không được để trống");

                if (dto.trang_thai != "HoatDong" && dto.trang_thai != "KhongHoatDong")
                    return BadRequest("Trạng thái không hợp lệ");

                // Lấy sản phẩm với chi tiết và hóa đơn
                var existingSanPham = await _sanPhamServices.GetByIdWithIncludeAsync(id,
                    q => q.Include(sp => sp.SanPhamChiTiets)
                          .ThenInclude(spct => spct.HoaDonChiTiets)
                          .ThenInclude(hdct => hdct.HoaDon));

                if (existingSanPham == null)
                    return NotFound("Không tìm thấy sản phẩm");

                // Nếu đang chuyển sang trạng thái không hoạt động
                if (dto.trang_thai == "KhongHoatDong")
                {
                    // Kiểm tra xem có sản phẩm chi tiết nào đang trong hóa đơn chưa hoàn thành không
                    var hasActiveInvoices = existingSanPham.SanPhamChiTiets
                        .Any(spct => spct.HoaDonChiTiets
                            .Any(hdct => hdct.HoaDon != null &&
                                (hdct.HoaDon.trang_thai_hoa_don == "ChoTaiQuay" ||
                                 hdct.HoaDon.trang_thai_hoa_don == "DangGiao" ||
                                 hdct.HoaDon.trang_thai_hoa_don == "DangXuLy")));

                    if (hasActiveInvoices)
                        return BadRequest("Không thể vô hiệu hóa sản phẩm đang có trong hóa đơn chưa hoàn thành");
                }

                var result = await _sanPhamServices.ExecuteInTransactionAsync(async () =>
                {
                    // Cập nhật trạng thái sản phẩm
                    existingSanPham.trang_thai = dto.trang_thai;
                    existingSanPham.ngay_sua = DateTime.Now;
                    existingSanPham.id_nguoi_sua = (Guid)GetIdNhanVien();
                    await _sanPhamServices.UpdateAsync(existingSanPham);

                    // Cập nhật trạng thái tất cả sản phẩm chi tiết
                    foreach (var spct in existingSanPham.SanPhamChiTiets)
                    {
                        spct.trang_thai = dto.trang_thai;
                        spct.ngay_sua = DateTime.Now;
                        spct.id_nguoi_sua = (Guid)GetIdNhanVien();
                        var updateResult = await _sanPhamChiTietServices.UpdateAsync(spct);
                        if (!updateResult)
                            return false;
                    }

                    return true;
                });

                if (!result)
                    return BadRequest("Đã có lỗi xảy ra khi cập nhật trạng thái sản phẩm");

                return Ok("Cập nhật trạng thái sản phẩm thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }
        [HttpDelete("xoa-san-pham-chi-tiet-theo-mau-sac")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> DeleteSanPhamChiTietByMauSac(Guid idSanPham, Guid idMauSac)
        {
            try
            {
                // Kiểm tra sản phẩm chi tiết có hóa đơn không
                var sanPhamChiTiets = await _sanPhamChiTietServices.GetByConditionAsync(x =>
                    x.id_san_pham == idSanPham &&
                    x.id_mau_sac == idMauSac);

                if (!sanPhamChiTiets.Any())
                    return NotFound("Không tìm thấy sản phẩm chi tiết");

                // Kiểm tra hóa đơn
                foreach (var spct in sanPhamChiTiets)
                {
                    var hoaDonChiTiets = await _hoaDonChiTietServices.GetByConditionAsync(x =>
                        x.id_san_pham_chi_tiet == spct.id_san_pham_chi_tiet);

                    if (hoaDonChiTiets.Any())
                        return BadRequest("Sản phẩm chi tiết đã có trong hóa đơn, không thể xóa");
                }

                var result = await _sanPhamServices.ExecuteInTransactionAsync(async () =>
                {
                    foreach (var spct in sanPhamChiTiets)
                    {
                        // Xóa hình ảnh liên quan
                        var hinhAnhs = await _hinhAnhSanPhamChiTietServices.GetByConditionAsync(x =>
                            x.id_san_pham_chi_tiet == spct.id_san_pham_chi_tiet);

                        foreach (var hinhAnh in hinhAnhs)
                        {
                            await _hinhAnhSanPhamChiTietServices.DeleteAsync(hinhAnh.id_hinh_anh_san_pham_chi_tiet);
                            await _hinhAnhServices.DeleteAsync(hinhAnh.id_hinh_anh);
                        }

                        // Xóa sản phẩm chi tiết
                        var deleteSPCT = await _sanPhamChiTietServices.DeleteAsync(spct.id_san_pham_chi_tiet);
                        if (!deleteSPCT)
                            return false;
                    }

                    return true;
                });

                if (!result)
                    return BadRequest("Đã có lỗi xảy ra khi xóa sản phẩm chi tiết");

                return Ok("Xóa sản phẩm chi tiết theo màu sắc thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpDelete("xoa-san-pham-chi-tiet-theo-kich-co")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> DeleteSanPhamChiTietByKichCo(Guid idSanPham, Guid idMauSac, Guid idKichCo)
        {
            try
            {
                // Kiểm tra sản phẩm chi tiết có hóa đơn không
                var sanPhamChiTiet = await _sanPhamChiTietServices.GetByConditionAsync(x =>
                    x.id_san_pham == idSanPham &&
                    x.id_mau_sac == idMauSac &&
                    x.id_kich_co == idKichCo);

                var spct = sanPhamChiTiet.FirstOrDefault();
                if (spct == null)
                    return NotFound("Không tìm thấy sản phẩm chi tiết");

                // Kiểm tra hóa đơn
                var hoaDonChiTiets = await _hoaDonChiTietServices.GetByConditionAsync(x =>
                    x.id_san_pham_chi_tiet == spct.id_san_pham_chi_tiet);

                if (hoaDonChiTiets.Any())
                    return BadRequest("Sản phẩm chi tiết đã có trong hóa đơn, không thể xóa");

                var result = await _sanPhamServices.ExecuteInTransactionAsync(async () =>
                {
                    // Xóa hình ảnh liên quan
                    var hinhAnhs = await _hinhAnhSanPhamChiTietServices.GetByConditionAsync(x =>
                        x.id_san_pham_chi_tiet == spct.id_san_pham_chi_tiet);

                    foreach (var hinhAnh in hinhAnhs)
                    {
                        await _hinhAnhSanPhamChiTietServices.DeleteAsync(hinhAnh.id_hinh_anh_san_pham_chi_tiet);
                        await _hinhAnhServices.DeleteAsync(hinhAnh.id_hinh_anh);
                    }

                    // Xóa sản phẩm chi tiết
                    return await _sanPhamChiTietServices.DeleteAsync(spct.id_san_pham_chi_tiet);
                });

                if (!result)
                    return BadRequest("Đã có lỗi xảy ra khi xóa sản phẩm chi tiết");

                return Ok("Xóa sản phẩm chi tiết theo kích cỡ thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpPost("lay-danh-sach-san-pham-hoat-dong")]
        public async Task<IActionResult> GetActiveProducts([FromBody] ThamSoPhanTrangSanPhamDTO thamSo)
        {
            try
            {
                // Lấy tất cả sản phẩm với các thuộc tính liên quan
                var allSanPhams = await _sanPhamServices.GetAllSanPhamAdminDTOAsync();
                // Sắp xếp theo ngày tạo giảm dần
                allSanPhams = allSanPhams.OrderByDescending(sp => sp.ngay_tao).ToList();
                var now = DateTime.Now;
                // Lọc sản phẩm có trạng thái HoatDong và có ít nhất một sản phẩm chi tiết HoatDong
                allSanPhams = allSanPhams.Where(sp =>
                    sp.trang_thai == "HoatDong" &&
                    sp.sanPhamChiTiets != null &&
                    sp.sanPhamChiTiets.Any(spct =>
                        spct.trang_thai == "HoatDong" &&
                        spct.so_luong > 0
                    )
                ).ToList();


                // Lọc lại danh sách sản phẩm chi tiết của từng sản phẩm, chỉ giữ lại các sản phẩm chi tiết HoatDong
                foreach (var sp in allSanPhams)
                {
                    sp.sanPhamChiTiets = sp.sanPhamChiTiets
                        .Where(spct => spct.trang_thai == "HoatDong" && spct.so_luong > 0)
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

        [HttpGet("lay-san-pham-hoat-dong-theo-id/{id}")]
        public async Task<IActionResult> GetActiveProductById(Guid id)
        {
            try
            {
                // Lấy thông tin sản phẩm với các thuộc tính liên quan
                var sanPham = await _sanPhamServices.GetByIdSanPhamAdminDTOAsync(id);

                if (sanPham == null)
                    return NotFound("Không tìm thấy sản phẩm");

                // Kiểm tra trạng thái sản phẩm
                if (sanPham.trang_thai != "HoatDong")
                    return BadRequest("Sản phẩm không hoạt động");

                // Lọc chỉ lấy các sản phẩm chi tiết có trạng thái HoatDong 
                if (sanPham.sanPhamChiTiets != null)
                {
                    var now = DateTime.Now;
                    sanPham.sanPhamChiTiets = sanPham.sanPhamChiTiets
                        .Where(spct => spct.trang_thai == "HoatDong")
                        .Select(spct =>
                        {
                            // Lấy giảm giá đang hoạt động
                            if (spct.giamGias != null)
                            {
                                spct.giamGias = spct.giamGias
                                    .Where(gg => gg.trang_thai == "HoatDong" &&
                                               gg.thoi_gian_bat_dau <= now &&
                                               gg.thoi_gian_ket_thuc >= now)
                                    .ToList();
                            }
                            return spct;
                        })
                        .ToList();

                    // Nếu không còn sản phẩm chi tiết nào hoạt động
                    if (!sanPham.sanPhamChiTiets.Any())
                        return BadRequest("Sản phẩm không còn chi tiết nào hoạt động");
                }
                else
                {
                    return BadRequest("Sản phẩm không có chi tiết");
                }

                return Ok(sanPham);
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpPut("update-trang-thai-san-pham-chi-tiet/{id}")]
        [Authorize(Roles = "NhanVien,Admin")]
        public async Task<IActionResult> UpdateTrangThaiSanPhamChiTiet(Guid id, [FromBody] CapNhatTrangThaiSanPhamDTO dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.trang_thai))
                    return BadRequest("Trạng thái không được để trống");

                if (dto.trang_thai != "HoatDong" && dto.trang_thai != "KhongHoatDong")
                    return BadRequest("Trạng thái không hợp lệ");

                var result = await _sanPhamServices.ExecuteInTransactionAsync(async () =>
                {
                    lock (_lockObject)
                    {
                        // Lấy sản phẩm chi tiết với khóa để tránh race condition
                        var existingSanPhamChiTiet = _sanPhamChiTietServices.GetByIdWithIncludeAsync(id,
                            q => q.Include(spct => spct.HoaDonChiTiets)
                                  .ThenInclude(hdct => hdct.HoaDon)
                                  .Include(spct => spct.SanPham)).Result;

                        if (existingSanPhamChiTiet == null)
                            return false;

                        // Nếu có hóa đơn và đang cố gắng chuyển sang trạng thái không hoạt động
                        if (existingSanPhamChiTiet.HoaDonChiTiets.Any() && dto.trang_thai == "KhongHoatDong")
                        {
                            var activeInvoices = existingSanPhamChiTiet.HoaDonChiTiets
                                .Any(hdct => hdct.HoaDon != null &&
                                    (hdct.HoaDon.trang_thai_hoa_don == "ChoTaiQuay" ||
                                     hdct.HoaDon.trang_thai_hoa_don == "DangGiao" ||
                                     hdct.HoaDon.trang_thai_hoa_don == "DangXuLy"));

                            if (activeInvoices)
                                return false;
                        }

                        // Cập nhật trạng thái sản phẩm chi tiết
                        existingSanPhamChiTiet.trang_thai = dto.trang_thai;
                        existingSanPhamChiTiet.ngay_sua = DateTime.Now;
                        existingSanPhamChiTiet.id_nguoi_sua = (Guid)GetIdNhanVien();

                        var updateResult = _sanPhamChiTietServices.UpdateAsync(existingSanPhamChiTiet).Result;
                        if (!updateResult)
                            return false;

                        // Lấy tất cả sản phẩm chi tiết của sản phẩm chính với khóa để tránh race condition
                        var allSanPhamChiTiet = _sanPhamChiTietServices.GetByConditionWithIncludeAsync(
                            x => x.id_san_pham == existingSanPhamChiTiet.id_san_pham,
                            q => q.Include(spct => spct.SanPham)).Result;

                        // Kiểm tra trạng thái của tất cả sản phẩm chi tiết
                        var allInactive = allSanPhamChiTiet.All(x => x.trang_thai == "KhongHoatDong");
                        var anyActive = allSanPhamChiTiet.Any(x => x.trang_thai == "HoatDong");

                        // Cập nhật trạng thái sản phẩm chính
                        var sanPham = existingSanPhamChiTiet.SanPham;
                        if (sanPham != null)
                        {
                            if (dto.trang_thai == "KhongHoatDong" && allInactive)
                            {
                                sanPham.trang_thai = "KhongHoatDong";
                                sanPham.ngay_sua = DateTime.Now;
                                sanPham.id_nguoi_sua = (Guid)GetIdNhanVien();
                                _sanPhamServices.UpdateAsync(sanPham).Wait();
                            }
                            else if (dto.trang_thai == "HoatDong" && anyActive)
                            {
                                sanPham.trang_thai = "HoatDong";
                                sanPham.ngay_sua = DateTime.Now;
                                sanPham.id_nguoi_sua = (Guid)GetIdNhanVien();
                                _sanPhamServices.UpdateAsync(sanPham).Wait();
                            }
                        }

                        return true;
                    }
                });

                if (!result)
                    return BadRequest("Đã có lỗi xảy ra khi cập nhật trạng thái sản phẩm chi tiết");

                return Ok("Cập nhật trạng thái sản phẩm chi tiết thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }

        private async Task<bool> KiemTraTrungThoiGianGiamGia(Guid idSanPhamChiTiet, string idGiamGia)
        {
            try
            {
                // Lấy thông tin giảm giá mới
                var giamGiaMoi = await _giamGiaServices.GetByIdAsync(Guid.Parse(idGiamGia));
                if (giamGiaMoi == null) return false;

                // Lấy danh sách giảm giá hiện tại của sản phẩm chi tiết
                var giamGiaHienTai = await _sanPhamChiTietGiamGiaServices.GetByConditionWithIncludeAsync(
                    x => x.id_san_pham_chi_tiet == idSanPhamChiTiet,
                    q => q.Include(gg => gg.GiamGia));

                foreach (var giamGia in giamGiaHienTai)
                {
                    // Kiểm tra trùng thời gian
                    if (giamGia.GiamGia.trang_thai == "HoatDong" &&
                        ((giamGiaMoi.thoi_gian_bat_dau <= giamGia.GiamGia.thoi_gian_ket_thuc &&
                          giamGiaMoi.thoi_gian_ket_thuc >= giamGia.GiamGia.thoi_gian_bat_dau) ||
                         (giamGia.GiamGia.thoi_gian_bat_dau <= giamGiaMoi.thoi_gian_ket_thuc &&
                          giamGia.GiamGia.thoi_gian_ket_thuc >= giamGiaMoi.thoi_gian_bat_dau)))
                    {
                        return true; // Có trùng thời gian
                    }
                }

                return false; // Không trùng thời gian
            }
            catch
            {
                return true; // Nếu có lỗi, coi như bị trùng để đảm bảo an toàn
            }
        }

        private async Task<bool> ThemGiamGiaChoSanPhamChiTiet(Guid idSanPhamChiTiet, List<string> idGiamGias)
        {
            try
            {
                if (idGiamGias == null || !idGiamGias.Any()) return true;

                // Kiểm tra trùng thời gian giữa các mã giảm giá mới
                for (int i = 0; i < idGiamGias.Count; i++)
                {
                    for (int j = i + 1; j < idGiamGias.Count; j++)
                    {
                        var giamGia1 = await _giamGiaServices.GetByIdAsync(Guid.Parse(idGiamGias[i]));
                        var giamGia2 = await _giamGiaServices.GetByIdAsync(Guid.Parse(idGiamGias[j]));

                        if (giamGia1 != null && giamGia2 != null && giamGia1.trang_thai == "HoatDong" && giamGia2.trang_thai == "HoatDong")
                        {
                            if ((giamGia1.thoi_gian_bat_dau <= giamGia2.thoi_gian_ket_thuc &&
                                 giamGia1.thoi_gian_ket_thuc >= giamGia2.thoi_gian_bat_dau) ||
                                (giamGia2.thoi_gian_bat_dau <= giamGia1.thoi_gian_ket_thuc &&
                                 giamGia2.thoi_gian_ket_thuc >= giamGia1.thoi_gian_bat_dau))
                            {
                                return false; // Có trùng thời gian giữa các mã giảm giá mới
                            }
                        }
                    }
                }

                // Lấy danh sách giảm giá hiện tại của sản phẩm chi tiết
                var giamGiaHienTai = await _sanPhamChiTietGiamGiaServices.GetByConditionWithIncludeAsync(
                    x => x.id_san_pham_chi_tiet == idSanPhamChiTiet,
                    q => q.Include(gg => gg.GiamGia));

                // Kiểm tra trùng thời gian với các giảm giá hiện tại
                foreach (var idGiamGia in idGiamGias)
                {
                    var giamGiaMoi = await _giamGiaServices.GetByIdAsync(Guid.Parse(idGiamGia));
                    if (giamGiaMoi == null || giamGiaMoi.trang_thai != "HoatDong") continue;

                    foreach (var giamGia in giamGiaHienTai)
                    {
                        if (giamGia.GiamGia.trang_thai == "HoatDong" &&
                            ((giamGiaMoi.thoi_gian_bat_dau <= giamGia.GiamGia.thoi_gian_ket_thuc &&
                              giamGiaMoi.thoi_gian_ket_thuc >= giamGia.GiamGia.thoi_gian_bat_dau) ||
                             (giamGia.GiamGia.thoi_gian_bat_dau <= giamGiaMoi.thoi_gian_ket_thuc &&
                              giamGia.GiamGia.thoi_gian_ket_thuc >= giamGiaMoi.thoi_gian_bat_dau)))
                        {
                            return false; // Có trùng thời gian với giảm giá hiện tại
                        }
                    }
                }
                // Kiểm tra xem giảm giá đã được áp dụng cho sản phẩm chi tiết chưa
                foreach (var idGiamGia in idGiamGias)
                {
                    var existingGiamGia = await _sanPhamChiTietGiamGiaServices.GetByConditionAsync(
                        x => x.id_san_pham_chi_tiet == idSanPhamChiTiet &&
                             x.id_giam_gia == Guid.Parse(idGiamGia));

                    if (existingGiamGia.Any())
                    {
                        return false; // Đã tồn tại giảm giá này cho sản phẩm chi tiết
                    }
                }

                // Thêm các giảm giá mới
                foreach (var idGiamGia in idGiamGias)
                {
                    var sanPhamChiTietGiamGia = new SanPhamChiTietGiamGia
                    {
                        id = Guid.NewGuid(),
                        id_san_pham_chi_tiet = idSanPhamChiTiet,
                        id_giam_gia = Guid.Parse(idGiamGia)
                    };
                    var result = await _sanPhamChiTietGiamGiaServices.CreateAsync(sanPhamChiTietGiamGia);
                    if (!result) return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
