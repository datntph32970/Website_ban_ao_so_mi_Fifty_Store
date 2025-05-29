using API.DbConects.DTOs.Admin.KhuyenMai;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Khuyen_Mai;
using API.Services;
using API.Services.Interfaces;
using API.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace API.Controllers.KhuyenMai_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhuyenMaiController : ControllerBase
    {
        private readonly IBaseService<KhuyenMai> _khuyenMaiServices;
        private readonly IBaseService<HoaDon> _hoaDonServices;
        private readonly IJwtServices _jwtServices;
        private static readonly Dictionary<string, (DateTime Expiry, object Data)> _cache = new();

        public KhuyenMaiController(IBaseService<KhuyenMai> khuyenMaiServices, IBaseService<HoaDon> hoaDonServices, IJwtServices jwtServices)
        {
            _khuyenMaiServices = khuyenMaiServices;
            _hoaDonServices = hoaDonServices;
            _jwtServices = jwtServices;
        }

        private void ClearCache()
        {
            _cache.Clear();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetAll(string? trang_thai, string? tim_kiem, string? kieu_khuyen_mai, DateTime? thoi_gian_bat_dau, DateTime? thoi_gian_ket_thuc)
        {
            var cacheKey = $"khuyenmai_{trang_thai}_{tim_kiem}_{kieu_khuyen_mai}_{thoi_gian_bat_dau}_{thoi_gian_ket_thuc}";
            if (_cache.TryGetValue(cacheKey, out var cachedData) && cachedData.Expiry > DateTime.Now)
            {
                return Ok(cachedData.Data);
            }

            var predicate = PredicateBuilder.New<KhuyenMai>(true);

            if (!string.IsNullOrEmpty(trang_thai))
            {
                predicate = predicate.And(x => x.trang_thai == trang_thai);
            }
            if (!string.IsNullOrEmpty(tim_kiem))
            {
                var searchTerm = tim_kiem.ToLower();
                predicate = predicate.And(x =>
                    x.ten_khuyen_mai.ToLower().Contains(searchTerm) ||
                    x.ma_khuyen_mai.ToLower().Contains(searchTerm));
            }
            if (!string.IsNullOrEmpty(kieu_khuyen_mai))
            {
                predicate = predicate.And(x => x.kieu_khuyen_mai == kieu_khuyen_mai);
            }
            if (thoi_gian_bat_dau.HasValue)
            {
                predicate = predicate.And(x => x.thoi_gian_bat_dau >= thoi_gian_bat_dau.Value);
            }
            if (thoi_gian_ket_thuc.HasValue)
            {
                predicate = predicate.And(x => x.thoi_gian_ket_thuc <= thoi_gian_ket_thuc.Value);
            }

            var khuyenMais = await _khuyenMaiServices.GetByConditionAsync(predicate);
            _cache[cacheKey] = (DateTime.Now.AddMinutes(1), khuyenMais); // Giảm thời gian cache xuống 1 phút
            return Ok(khuyenMais);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("ID không hợp lệ");

            var result = await _khuyenMaiServices.GetByIdWithIncludeAsync(id,
                q => q.Include(k => k.NguoiTao)
                     .Include(k => k.NguoiSua)
                     .Include(k => k.HoaDons)
            );
            if (result == null)
                return NotFound("Không tìm thấy khuyến mãi");
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Add([FromBody] ThemKhuyenMaiAdminDTO khuyenMaiDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra kiểu giảm giá
            if (khuyenMaiDTO.kieu_khuyen_mai != "PhanTram" && khuyenMaiDTO.kieu_khuyen_mai != "TienMat")
                return BadRequest("Kiểu giảm giá phải là PhanTram hoặc TienMat");

            // Kiểm tra trùng tên
            var existingKhuyenMai = await _khuyenMaiServices.GetAllAsync();
            if (existingKhuyenMai.Any(x => x.ten_khuyen_mai.ToLower().Trim() == khuyenMaiDTO.ten_khuyen_mai.ToLower().Trim()))
                return BadRequest("Tên khuyến mãi đã tồn tại");

            // Kiểm tra thời gian
            if (khuyenMaiDTO.thoi_gian_ket_thuc <= khuyenMaiDTO.thoi_gian_bat_dau)
                return BadRequest("Thời gian kết thúc phải lớn hơn thời gian bắt đầu");

            // Kiểm tra thời gian bắt đầu phải lớn hơn thời gian hiện tại
            if (khuyenMaiDTO.thoi_gian_bat_dau <= DateTime.Now)
                return BadRequest("Thời gian bắt đầu phải lớn hơn thời gian hiện tại");

            // Kiểm tra giá trị giảm giá
            if (khuyenMaiDTO.kieu_khuyen_mai == "PhanTram" && khuyenMaiDTO.gia_tri_giam_toi_da < 0)
                return BadRequest("Giá trị giảm tối đa không được nhỏ hơn 0");

            if (khuyenMaiDTO.kieu_khuyen_mai == "TienMat")
            {
                if (khuyenMaiDTO.gia_tri_giam_toi_da < khuyenMaiDTO.gia_tri_giam)
                    return BadRequest("Giá trị giảm tối đa phải lớn hơn hoặc bằng 0");
            }
            if (khuyenMaiDTO.ma_khuyen_mai == null || khuyenMaiDTO.ma_khuyen_mai == "")
            {
                khuyenMaiDTO.ma_khuyen_mai = await TaoMaKhuyenMai();
            }
            else if (khuyenMaiDTO.ma_khuyen_mai.Contains(" "))
            {
                khuyenMaiDTO.ma_khuyen_mai = khuyenMaiDTO.ma_khuyen_mai.Replace(" ", "");
            }
            var khuyenMai = new KhuyenMai
            {
                id_khuyen_mai = Guid.NewGuid(),
                ma_khuyen_mai = khuyenMaiDTO.ma_khuyen_mai,
                ten_khuyen_mai = khuyenMaiDTO.ten_khuyen_mai,
                mo_ta = khuyenMaiDTO.mo_ta,
                kieu_khuyen_mai = khuyenMaiDTO.kieu_khuyen_mai,
                gia_tri_don_hang_toi_thieu = khuyenMaiDTO.gia_tri_don_hang_toi_thieu,
                gia_tri_giam_toi_da = khuyenMaiDTO.gia_tri_giam_toi_da,
                so_luong_toi_da = khuyenMaiDTO.so_luong_toi_da,
                so_luong_da_su_dung = 0,
                gia_tri_giam = khuyenMaiDTO.gia_tri_giam,
                thoi_gian_bat_dau = khuyenMaiDTO.thoi_gian_bat_dau,
                thoi_gian_ket_thuc = khuyenMaiDTO.thoi_gian_ket_thuc,
                trang_thai = "HoatDong",
                ngay_tao = DateTime.Now,
                id_nguoi_tao = (Guid)GetIdNhanVien()
            };

            var result = await _khuyenMaiServices.CreateAsync(khuyenMai);
            if (result)
            {
                ClearCache(); // Clear cache after creating new promotion
                return Ok("Thêm khuyến mãi thành công");
            }
            return BadRequest("Đã có lỗi khi thêm khuyến mãi");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SuaKhuyenMaiAdminDTO khuyenMaiDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id == Guid.Empty)
                return BadRequest("ID không hợp lệ");

            // Kiểm tra kiểu giảm giá
            if (khuyenMaiDTO.kieu_khuyen_mai != "PhanTram" && khuyenMaiDTO.kieu_khuyen_mai != "TienMat")
                return BadRequest("Kiểu giảm giá phải là PhanTram hoặc TienMat");

            var existingKhuyenMai = await _khuyenMaiServices.GetByIdAsync(id);
            if (existingKhuyenMai == null)
                return NotFound("Không tìm thấy khuyến mãi");

            // Kiểm tra trùng tên với khuyến mãi khác
            var existingKhuyenMaiKhac = await _khuyenMaiServices.GetAllAsync();
            if (existingKhuyenMaiKhac.Any(x => x.id_khuyen_mai != id && x.ten_khuyen_mai.Trim().ToLower() == khuyenMaiDTO.ten_khuyen_mai.Trim().ToLower()))
                return BadRequest("Tên khuyến mãi đã tồn tại");
            if (khuyenMaiDTO.ma_khuyen_mai == null || khuyenMaiDTO.ma_khuyen_mai == "")
                return BadRequest("Mã khuyến mãi không được để trống");
            // Kiểm tra thời gian
            if (khuyenMaiDTO.thoi_gian_ket_thuc <= khuyenMaiDTO.thoi_gian_bat_dau)
                return BadRequest("Thời gian kết thúc phải lớn hơn thời gian bắt đầu");

            // Kiểm tra thời gian bắt đầu phải lớn hơn thời gian hiện tại
            if (khuyenMaiDTO.thoi_gian_bat_dau <= DateTime.Now)
                return BadRequest("Thời gian bắt đầu phải lớn hơn thời gian hiện tại");

            // Kiểm tra giá trị giảm giá
            if (khuyenMaiDTO.kieu_khuyen_mai == "PhanTram" && khuyenMaiDTO.gia_tri_giam_toi_da < 0)
                return BadRequest("Giá trị giảm tối đa không được nhỏ hơn 0");

            if (khuyenMaiDTO.gia_tri_giam_toi_da < khuyenMaiDTO.gia_tri_giam)
                return BadRequest("Giá trị giảm tối đa phải lớn hơn hoặc bằng giá trị giảm tối thiểu");
            if (khuyenMaiDTO.kieu_khuyen_mai == "TienMat")
            {
                if (khuyenMaiDTO.gia_tri_giam_toi_da < khuyenMaiDTO.gia_tri_giam)
                    return BadRequest("Giá trị giảm tối đa phải lớn hơn hoặc bằng 0");
            }

            existingKhuyenMai.ten_khuyen_mai = khuyenMaiDTO.ten_khuyen_mai;
            existingKhuyenMai.mo_ta = khuyenMaiDTO.mo_ta;
            existingKhuyenMai.gia_tri_giam = khuyenMaiDTO.gia_tri_giam;
            existingKhuyenMai.ma_khuyen_mai = khuyenMaiDTO.ma_khuyen_mai;
            existingKhuyenMai.kieu_khuyen_mai = khuyenMaiDTO.kieu_khuyen_mai;
            existingKhuyenMai.gia_tri_don_hang_toi_thieu = khuyenMaiDTO.gia_tri_don_hang_toi_thieu;
            existingKhuyenMai.gia_tri_giam_toi_da = khuyenMaiDTO.gia_tri_giam_toi_da;
            existingKhuyenMai.so_luong_toi_da = khuyenMaiDTO.so_luong_toi_da;
            existingKhuyenMai.thoi_gian_bat_dau = khuyenMaiDTO.thoi_gian_bat_dau;
            existingKhuyenMai.thoi_gian_ket_thuc = khuyenMaiDTO.thoi_gian_ket_thuc;
            existingKhuyenMai.id_nguoi_sua = (Guid)GetIdNhanVien();
            existingKhuyenMai.ngay_sua = DateTime.Now;

            var result = await _khuyenMaiServices.UpdateAsync(existingKhuyenMai);
            if (result)
            {
                ClearCache(); // Clear cache after updating promotion
                return Ok("Cập nhật khuyến mãi thành công");
            }
            return BadRequest("Đã có lỗi khi cập nhật khuyến mãi");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("ID không hợp lệ");

            var existingKhuyenMai = await _khuyenMaiServices.GetByIdWithIncludeAsync(id,
                q => q.Include(k => k.HoaDons)
            );
            if (existingKhuyenMai == null)
                return NotFound("Không tìm thấy khuyến mãi");

            if (existingKhuyenMai.HoaDons != null && existingKhuyenMai.HoaDons.Any())
                return BadRequest("Không thể xóa khuyến mãi đã được sử dụng trong hóa đơn");

            var result = await _khuyenMaiServices.DeleteAsync(id);
            if (result)
            {
                ClearCache(); // Clear cache after deleting promotion
                return Ok("Xóa khuyến mãi thành công");
            }
            return BadRequest("Đã có lỗi khi xóa khuyến mãi");
        }

        [HttpPatch("{id}/trangthai")]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> UpdateTrangThai(Guid id, [FromBody] CapNhatTrangThaiKhuyenMaiDTO trangThaiDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id == Guid.Empty)
                return BadRequest("ID không hợp lệ");

            var existingKhuyenMai = await _khuyenMaiServices.GetByIdAsync(id);
            if (existingKhuyenMai == null)
                return NotFound("Không tìm thấy khuyến mãi");

            // Kiểm tra trạng thái hợp lệ
            if (trangThaiDTO.trang_thai != "HoatDong" && trangThaiDTO.trang_thai != "KhongHoatDong")
                return BadRequest("Trạng thái không hợp lệ");

            existingKhuyenMai.trang_thai = trangThaiDTO.trang_thai;
            existingKhuyenMai.id_nguoi_sua = (Guid)GetIdNhanVien();
            existingKhuyenMai.ngay_sua = DateTime.Now;

            var result = await _khuyenMaiServices.UpdateAsync(existingKhuyenMai);
            if (result)
            {
                ClearCache(); // Clear cache after updating promotion status
                return Ok("Cập nhật trạng thái khuyến mãi thành công");
            }
            return BadRequest("Đã có lỗi khi cập nhật trạng thái khuyến mãi");
        }

        private async Task<string> TaoMaKhuyenMai()
        {
            var random = new Random();
            string prefix = "KM";
            string randomPart;
            bool isDuplicate;
            var existingKhuyenMai = await _khuyenMaiServices.GetAllAsync();

            do
            {
                // Tạo chuỗi ngẫu nhiên gồm 5 ký tự (chữ và số)
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                randomPart = new string(Enumerable.Repeat(chars, 9)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                string newMa = $"{prefix}{randomPart}";

                // Kiểm tra trùng lặp
                isDuplicate = existingKhuyenMai != null &&
                             existingKhuyenMai.Any(x => x.ma_khuyen_mai == newMa);
            } while (isDuplicate);

            return $"{prefix}{randomPart}";
        }

        private Guid? GetIdNhanVien()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
                throw new UnauthorizedAccessException("Token không hợp lệ");
            var idtnhanvien = _jwtServices.GetIdNhanVienFromToken(token);
            return idtnhanvien;
        }

        [HttpGet("khuyen-mai-hoat-dong")]
        public async Task<IActionResult> GetActivePromotions(string? search, string? id_hoa_don)
        {
            try
            {
                var now = DateTime.Now;

                // Lấy thông tin khách hàng từ token nếu có
                Guid? idKhachHang = null;
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (token != null)
                {
                    idKhachHang = _jwtServices.GetIdKhachHangFromToken(token);
                }

                // Lấy tổng tiền hóa đơn nếu có id_hoa_don
                decimal tongTienHoaDon = 0;
                if (!string.IsNullOrEmpty(id_hoa_don))
                {
                    var hoaDon = await _hoaDonServices.GetByIdAsync(Guid.Parse(id_hoa_don));
                    if (hoaDon != null)
                    {
                        tongTienHoaDon = (decimal)hoaDon.tong_tien_don_hang;
                    }
                }

                // Lấy danh sách khuyến mãi đang hoạt động
                var khuyenMais = await _khuyenMaiServices.GetByConditionAsync(km =>
                    km.trang_thai == "HoatDong" &&
                    km.thoi_gian_bat_dau <= now &&
                    km.thoi_gian_ket_thuc >= now &&
                    km.so_luong_da_su_dung < km.so_luong_toi_da &&
                    (string.IsNullOrEmpty(search) ||
                     km.ten_khuyen_mai.Contains(search) ||
                     km.ma_khuyen_mai.Contains(search)) &&
                    (string.IsNullOrEmpty(id_hoa_don) ||
                     km.gia_tri_don_hang_toi_thieu <= tongTienHoaDon)
                );

                // Nếu là khách hàng, loại bỏ các khuyến mãi đã sử dụng
                if (idKhachHang.HasValue)
                {
                    var hoaDonKhachHang = await _hoaDonServices.GetByConditionAsync(hd =>
                        hd.id_khach_hang == idKhachHang &&
                        hd.id_khuyen_mai != null &&
                        hd.trang_thai_hoa_don != "DaHuy");

                    var khuyenMaiDaSuDung = hoaDonKhachHang.Select(hd => hd.id_khuyen_mai).ToList();
                    khuyenMais = khuyenMais.Where(km => !khuyenMaiDaSuDung.Contains(km.id_khuyen_mai)).ToList();
                }

                // Tính toán và sắp xếp khuyến mãi theo giá trị thực tế
                var khuyenMaisSapXep = khuyenMais
                    .Select(km => new
                    {
                        KhuyenMai = km,
                        GiaTriThucTe = km.kieu_khuyen_mai == "PhanTram"
                            ? Math.Min(tongTienHoaDon * (decimal)km.gia_tri_giam / 100, (decimal)km.gia_tri_giam_toi_da)
                            : Math.Min((decimal)km.gia_tri_giam, (decimal)km.gia_tri_giam_toi_da)
                    })
                    .OrderByDescending(x => x.GiaTriThucTe)
                    .ThenBy(x => x.KhuyenMai.gia_tri_don_hang_toi_thieu)
                    .Take(10)
                    .Select(x => new
                    {
                        x.KhuyenMai,
                        GiaTriThucTe = x.GiaTriThucTe,
                        GiaTriHienThi = x.KhuyenMai.kieu_khuyen_mai == "PhanTram"
                            ? $"{x.KhuyenMai.gia_tri_giam}% (Tối đa {x.KhuyenMai.gia_tri_giam_toi_da:N0} VNĐ)"
                            : $"{x.KhuyenMai.gia_tri_giam:N0} VNĐ (Tối đa {x.KhuyenMai.gia_tri_giam_toi_da:N0} VNĐ)"
                    });

                return Ok(new
                {
                    tong_tien_hoa_don = tongTienHoaDon,
                    khuyen_mais = khuyenMaisSapXep
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra: {ex.Message}");
            }
        }


    }
}