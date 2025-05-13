using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_San_Pham;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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
        private readonly IBaseService<KichCo> _kichCoServices;
        private readonly IJwtServices _jwtServices;

        public SanPhamController(ISanPhamService sanPhamServices, IBaseService<SanPhamChiTiet> sanPhamChiTietServices, IBaseService<HinhAnh> hinhAnhServices, IBaseService<HinhAnhSanPhamChiTiet> hinhAnhSanPhamChiTietServices, IBaseService<MauSac> mauSacServices, IBaseService<KichCo> kichCoServices, IJwtServices jwtServices)
        {
            _sanPhamServices = sanPhamServices;
            _sanPhamChiTietServices = sanPhamChiTietServices;
            _hinhAnhServices = hinhAnhServices;
            _hinhAnhSanPhamChiTietServices = hinhAnhSanPhamChiTietServices;
            _mauSacServices = mauSacServices;
            _kichCoServices = kichCoServices;
            _jwtServices = jwtServices;
        }

        [HttpPost("lay-danh-sach-san-pham-admin-dto")]
        [Authorize(Roles = "NhanVien,Admin")]
        public async Task<IActionResult> GetAll([FromBody] ThamSoPhanTrangSanPhamDTO thamSo)
        {
            try
            {
                // Lấy tất cả sản phẩm với các thuộc tính liên quan
                var allSanPhams = await _sanPhamServices.GetAllSanPhamAdminDTOAsync();

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
            if (sanPhamDTO.sanPhamChiTiets.Any(x => x.id_mau_sac == null))
                return BadRequest("Yêu cầu nhập mã màu sắc");
            if (sanPhamDTO.sanPhamChiTiets.Any(x => x.id_kich_co == null))
                return BadRequest("Yêu cầu nhập mã kích cỡ");
            if (sanPhamDTO.sanPhamChiTiets.Any(x => x.them_hinh_anh_spcts == null || x.them_hinh_anh_spcts.Count == 0))
                return BadRequest("Yêu cầu chọn hình ảnh");
            if (sanPhamDTO.sanPhamChiTiets.Any(x => x.so_luong <= 0))
                return BadRequest("Yêu cầu nhập số lượng");
            if (sanPhamDTO.sanPhamChiTiets.Any(x => x.gia_ban <= 0))
                return BadRequest("Yêu cầu nhập giá bán");
            if (sanPhamDTO.sanPhamChiTiets.Any(x => x.gia_nhap <= 0))
                return BadRequest("Yêu cầu nhập giá nhập");

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

                        if (spct.id_giam_gia != null && spct.id_giam_gia != "")
                            sanPhamChiTiet.id_giam_gia = Guid.Parse(spct.id_giam_gia);

                        var chiTietResult = await _sanPhamChiTietServices.CreateAsync(sanPhamChiTiet);
                        if (!chiTietResult) return false;

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
                return BadRequest("Yêu cầu nhập sản phẩm chi tiết");

            foreach (var spct in sanPhamDTO.sanPhamChiTiets)
            {
                if (spct.so_luong < 0)
                    return BadRequest("Số lượng không được nhỏ hơn 0");
                if (spct.gia_ban <= 0)
                    return BadRequest("Giá bán phải lớn hơn 0");
                if (spct.gia_nhap <= 0)
                    return BadRequest("Giá nhập phải lớn hơn 0");
                if (string.IsNullOrEmpty(spct.trang_thai))
                    return BadRequest("Yêu cầu nhập trạng thái cho sản phẩm chi tiết");
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

                var result = await _sanPhamServices.ExecuteInTransactionAsync(async () =>
                {
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
                                if (spctDTO.id_giam_gia != null && spctDTO.id_giam_gia != "")
                                    existingChiTiet.id_giam_gia = Guid.Parse(spctDTO.id_giam_gia);
                                await _sanPhamChiTietServices.UpdateAsync(existingChiTiet);

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

                                if (spctDTO.id_giam_gia != null && spctDTO.id_giam_gia != "")
                                    sanPhamChiTiet.id_giam_gia = Guid.Parse(spctDTO.id_giam_gia);

                                var chiTietResult = await _sanPhamChiTietServices.CreateAsync(sanPhamChiTiet);
                                if (!chiTietResult) return false;

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
                });

                if (!result)
                    return BadRequest("Đã có lỗi xảy ra khi sửa sản phẩm");

                return Ok("Cập nhật sản phẩm thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }
        [HttpDelete("xoa-san-pham")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(Guid id)
        {
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
        [Authorize(Roles = "NhanVien")]
        public async Task<IActionResult> UpdateTrangThai(Guid id, [FromBody] CapNhatTrangThaiSanPhamDTO dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.trang_thai))
                    return BadRequest("Trạng thái không được để trống");

                if (dto.trang_thai != "HoatDong" && dto.trang_thai != "KhongHoatDong")
                    return BadRequest("Trạng thái không hợp lệ");

                var result = await _sanPhamServices.ExecuteInTransactionAsync(async () =>
                {
                    // Lấy sản phẩm hiện tại với chi tiết
                    var existingSanPham = await _sanPhamServices.GetByIdWithIncludeAsync(id,
                        q => q.Include(sp => sp.SanPhamChiTiets));

                    if (existingSanPham == null)
                        return false;

                    // Cập nhật trạng thái sản phẩm
                    existingSanPham.trang_thai = dto.trang_thai;
                    await _sanPhamServices.UpdateAsync(existingSanPham);

                    // Cập nhật trạng thái tất cả sản phẩm chi tiết
                    foreach (var spct in existingSanPham.SanPhamChiTiets)
                    {
                        spct.trang_thai = dto.trang_thai;
                        await _sanPhamChiTietServices.UpdateAsync(spct);
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
                var result = await _sanPhamServices.ExecuteInTransactionAsync(async () =>
                {
                    // Lấy sản phẩm chi tiết theo màu sắc
                    var sanPhamChiTiets = await _sanPhamChiTietServices.GetByConditionAsync(x =>
                        x.id_san_pham == idSanPham &&
                        x.id_mau_sac == idMauSac);

                    if (!sanPhamChiTiets.Any())
                        return false;

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
                var result = await _sanPhamServices.ExecuteInTransactionAsync(async () =>
                {
                    // Lấy sản phẩm chi tiết theo màu sắc và kích cỡ
                    var sanPhamChiTiet = (await _sanPhamChiTietServices.GetByConditionAsync(x =>
                        x.id_san_pham == idSanPham &&
                        x.id_mau_sac == idMauSac &&
                        x.id_kich_co == idKichCo)).FirstOrDefault();

                    if (sanPhamChiTiet == null)
                        return false;

                    // Xóa hình ảnh liên quan
                    var hinhAnhs = await _hinhAnhSanPhamChiTietServices.GetByConditionAsync(x =>
                        x.id_san_pham_chi_tiet == sanPhamChiTiet.id_san_pham_chi_tiet);

                    foreach (var hinhAnh in hinhAnhs)
                    {
                        await _hinhAnhSanPhamChiTietServices.DeleteAsync(hinhAnh.id_hinh_anh_san_pham_chi_tiet);
                        await _hinhAnhServices.DeleteAsync(hinhAnh.id_hinh_anh);
                    }

                    // Xóa sản phẩm chi tiết
                    var deleteSPCT = await _sanPhamChiTietServices.DeleteAsync(sanPhamChiTiet.id_san_pham_chi_tiet);
                    if (!deleteSPCT)
                        return false;

                    return true;
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

                // Lọc sản phẩm có trạng thái HoatDong và có ít nhất một sản phẩm chi tiết HoatDong
                allSanPhams = allSanPhams.Where(sp =>
                    sp.trang_thai == "HoatDong" &&
                    sp.sanPhamChiTiets != null &&
                    sp.sanPhamChiTiets.Any(spct => spct.trang_thai == "HoatDong" && spct.so_luong > 0)
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
                    sanPham.sanPhamChiTiets = sanPham.sanPhamChiTiets
                        .Where(spct => spct.trang_thai == "HoatDong")
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

    }
}
