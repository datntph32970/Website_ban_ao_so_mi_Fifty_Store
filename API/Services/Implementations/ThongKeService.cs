using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories.Interfaces;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Collections.Generic;

namespace API.Services.Implementations
{
    public class ThongKeService : IThongKeService
    {
        private readonly IBaseRepository<HoaDon> _hoaDonRepository;
        private readonly IBaseRepository<NhanVien> _nhanVienRepository;
        private readonly IBaseRepository<DanhMuc> _danhMucRepository;
        private readonly IBaseRepository<HoaDonChiTiet> _hoaDonChiTietRepository;
        private readonly IBaseRepository<SanPham> _sanPhamRepository;
        private readonly IBaseRepository<SanPhamChiTiet> _sanPhamChiTietRepository;
        private readonly Dictionary<string, (DateTime ExpiryTime, object Data)> _cache = new();
        private const int CACHE_DURATION_MINUTES = 5;
        private static readonly string[] VALID_TRANG_THAI = new[] { "DaThanhToan", "DaHoanThanh" };
        private static readonly string[] INVALID_TRANG_THAI_DON_HANG = new[] { "DaHuy", "DaTraHang", "DaXacNhanTraHang", "DaHoanTraToanBo" };

        public ThongKeService(IBaseRepository<HoaDon> hoaDonRepository, IBaseRepository<NhanVien> nhanVienRepository, IBaseRepository<DanhMuc> danhMucRepository, IBaseRepository<HoaDonChiTiet> hoaDonChiTietRepository, IBaseRepository<SanPham> sanPhamRepository, IBaseRepository<SanPhamChiTiet> sanPhamChiTietRepository)
        {
            _hoaDonRepository = hoaDonRepository;
            _nhanVienRepository = nhanVienRepository;
            _danhMucRepository = danhMucRepository;
            _hoaDonChiTietRepository = hoaDonChiTietRepository;
            _sanPhamRepository = sanPhamRepository;
            _sanPhamChiTietRepository = sanPhamChiTietRepository;
        }

        private string GenerateCacheKey(string prefix, params object[] parameters)
        {
            return $"{prefix}_{string.Join("_", parameters)}";
        }

        private T GetFromCache<T>(string key)
        {
            if (_cache.TryGetValue(key, out var cachedItem) && cachedItem.ExpiryTime > DateTime.Now)
            {
                return (T)cachedItem.Data;
            }
            return default;
        }

        private void SetCache(string key, object data)
        {
            _cache[key] = (DateTime.Now.AddMinutes(CACHE_DURATION_MINUTES), data);
        }

        private void ClearCache()
        {
            _cache.Clear();
        }

        private bool IsValidHoaDonTrangThai(string trangThai)
        {
            // Các trạng thái hợp lệ để tính doanh thu và thống kê:
            // - DaThanhToan: Đã thanh toán tại quầy
            // - DaHoanThanh: Đã hoàn thành giao hàng (online)
            // Không tính các trạng thái:
            // - ChoTaiQuay, DangChoXuLy: Chưa thanh toán
            // - DangChuanBi, DangGiaoHang: Đang trong quá trình xử lý
            // - HetHang, DaHuy: Không thành công
            // - ChuaThanhToan: Chưa thanh toán
            // - DaHoanTraMotPhan, DaHoanTraToanBo: Đã hoàn trả
            return VALID_TRANG_THAI.Contains(trangThai);
        }

        #region Tính tổng doanh thu
        //hàm tính tổng doamh thu theo tháng
        public async Task<decimal> TinhTongDoanhThuTheoThang(int thang, int nam)
        {
            try
            {
                var cacheKey = GenerateCacheKey("DoanhThuThang", thang, nam);
                var cachedResult = GetFromCache<decimal?>(cacheKey);
                if (cachedResult.HasValue)
                    return cachedResult.Value;

                var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd =>
                    hd.ngay_tao.Month == thang &&
                    hd.ngay_tao.Year == nam &&
                    VALID_TRANG_THAI.Contains(hd.trang_thai_hoa_don));

                // Log để debug
                Console.WriteLine($"Tháng {thang}/{nam}:");
                Console.WriteLine($"- Số hóa đơn hợp lệ: {hoaDon.Count()}");
                foreach (var hd in hoaDon)
                {
                    Console.WriteLine($"- Hóa đơn {hd.ma_hoa_don} - {hd.trang_thai_hoa_don}: {hd.tong_tien_phai_thanh_toan:N0} VND");
                }

                var tongDoanhThu = hoaDon.Sum(hd => (hd.tong_tien_phai_thanh_toan ?? 0) - (hd.phi_van_chuyen ?? 0));
                SetCache(cacheKey, tongDoanhThu);
                Console.WriteLine($"- Tổng doanh thu: {tongDoanhThu:N0} VND");

                return tongDoanhThu;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tính doanh thu tháng {thang}/{nam}: {ex.Message}");
                throw;
            }
        }
        //hàm tính tổng doamh thu theo năm
        public async Task<decimal> TinhTongDoanhThuTheoNam(int nam)
        {
            try
            {
                var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd =>
                    hd.ngay_tao.Year == nam &&
                    VALID_TRANG_THAI.Contains(hd.trang_thai_hoa_don));
                return hoaDon.Sum(hd => (hd.tong_tien_phai_thanh_toan ?? 0) - (hd.phi_van_chuyen ?? 0));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tính doanh thu năm {nam}: {ex.Message}");
                throw;
            }
        }
        //hàm tính tổng doamh thu theo ngày
        public async Task<decimal> TinhTongDoanhThuTheoNgay(DateOnly ngay)
        {
            try
            {
                DateTime dateTime = ngay.ToDateTime(TimeOnly.MinValue);
                var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd =>
                    hd.ngay_tao.Date == dateTime.Date &&
                    VALID_TRANG_THAI.Contains(hd.trang_thai_hoa_don));
                return hoaDon.Sum(hd => (hd.tong_tien_phai_thanh_toan ?? 0) - (hd.phi_van_chuyen ?? 0));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tính doanh thu ngày {ngay:dd/MM/yyyy}: {ex.Message}");
                throw;
            }
        }
        //hàm tính tổng doamh thu theo tuần
        public async Task<decimal> TinhTongDoanhThuTheoTuan(int tuan, int nam)
        {
            try
            {
                var cacheKey = GenerateCacheKey("DoanhThuTuan", tuan, nam);
                var cachedResult = GetFromCache<decimal?>(cacheKey);
                if (cachedResult.HasValue)
                    return cachedResult.Value;

                // Tính ngày bắt đầu và kết thúc của tuần
                var startDate = GetStartDateOfWeek(tuan, nam);
                var endDate = startDate.AddDays(7);

                Console.WriteLine($"Tính doanh thu từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}");

                // Lấy hóa đơn và lọc trạng thái hợp lệ trực tiếp trong query
                var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd =>
                    hd.ngay_tao >= startDate &&
                    hd.ngay_tao < endDate &&
                    VALID_TRANG_THAI.Contains(hd.trang_thai_hoa_don));

                var tongDoanhThu = hoaDon.Sum(hd => (hd.tong_tien_phai_thanh_toan ?? 0) - (hd.phi_van_chuyen ?? 0));
                SetCache(cacheKey, tongDoanhThu);

                Console.WriteLine($"- Tổng doanh thu: {tongDoanhThu:N0} VND");
                return tongDoanhThu;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tính doanh thu tuần {tuan} năm {nam}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        #endregion
        #region Tính tổng đơn hàng
        public DateTime GetStartDateOfWeek(int week, int year)
        {
            // Ngày đầu tiên của năm
            var jan1 = new DateTime(year, 1, 1);

            // Tìm ngày thứ 2 đầu tiên của năm
            var firstMonday = jan1.AddDays((8 - (int)jan1.DayOfWeek) % 7);
            if (jan1.DayOfWeek <= DayOfWeek.Thursday)
                firstMonday = firstMonday.AddDays(-7);

            // Tính ngày bắt đầu của tuần được chọn
            return firstMonday.AddDays((week - 1) * 7);
        }
        //hàm tính tổng đơn hàng theo tháng
        public async Task<int> TinhTongDonHangTheoThang(int thang, int nam)
        {
            var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd =>
                hd.ngay_tao.Month == thang &&
                hd.ngay_tao.Year == nam &&
                !INVALID_TRANG_THAI_DON_HANG.Contains(hd.trang_thai_hoa_don));
            return hoaDon.Count();
        }
        //hàm tính tổng đơn hàng theo năm
        public async Task<int> TinhTongDonHangTheoNam(int nam)
        {
            var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd =>
                hd.ngay_tao.Year == nam &&
                !INVALID_TRANG_THAI_DON_HANG.Contains(hd.trang_thai_hoa_don));
            return hoaDon.Count();
        }
        //hàm tính tổng đơn hàng theo tuần
        public async Task<int> TinhTongDonHangTheoTuan(int tuan, int nam)
        {
            var startDate = GetStartDateOfWeek(tuan, nam);
            var endDate = startDate.AddDays(7);
            var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd =>
                hd.ngay_tao >= startDate &&
                hd.ngay_tao < endDate &&
                !INVALID_TRANG_THAI_DON_HANG.Contains(hd.trang_thai_hoa_don));
            return hoaDon.Count();
        }
        //hàm tính tổng đơn hàng theo ngày
        public async Task<int> TinhTongDonHangTheoNgay(DateOnly ngay)
        {
            DateTime dateTime = ngay.ToDateTime(TimeOnly.MinValue);
            var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd =>
                hd.ngay_tao.Date == dateTime.Date &&
                !INVALID_TRANG_THAI_DON_HANG.Contains(hd.trang_thai_hoa_don));
            return hoaDon.Count();
        }
        #endregion
        #region Tính tổng nhân viên
        //hàm tính tổng nhân viên theo tháng
        public async Task<int> TinhTongNhanVienTheoThang(int thang, int nam)
        {
            var nhanVien = await _nhanVienRepository.GetByConditionAsync(nv => nv.ngay_tao.Month == thang && nv.ngay_tao.Year == nam);
            return nhanVien.Count();
        }
        //hàm tính tổng nhân viên theo năm
        public async Task<int> TinhTongNhanVienTheoNam(int nam)
        {
            var nhanVien = await _nhanVienRepository.GetByConditionAsync(nv => nv.ngay_tao.Year == nam);
            return nhanVien.Count();
        }
        //hàm lấy danh sách nhân viên có doanh thu cao nhất theo tháng
        public async Task<List<(NhanVien, decimal)>> LayDanhSachNhanVienCoDoanhThuCaoNhatTheoThang(int thang, int nam)
        {
            var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd => hd.ngay_tao.Month == thang && hd.ngay_tao.Year == nam);
            var nhanVienIds = hoaDon.GroupBy(hd => hd.id_nhan_vien_xu_ly)
                                           .OrderByDescending(g => g.Sum(hd => hd.tong_tien_phai_thanh_toan))
                                           .Take(10)
                                           .Select(g => g.Key)
                                           .ToList();
            var result = new List<(NhanVien, decimal)>();
            foreach (var nhanVienId in nhanVienIds)
            {
                if (nhanVienId != null)
                {
                    var nv = await _nhanVienRepository.GetByIdAsync(nhanVienId.Value);
                    if (nv != null)
                    {
                        var tongTien = hoaDon.Where(hd => hd.id_nhan_vien_xu_ly == nhanVienId)
                                           .Sum(hd => (hd.tong_tien_phai_thanh_toan ?? 0) - (hd.phi_van_chuyen ?? 0));
                        result.Add((nv, tongTien));
                    }
                }
            }
            return result;

        }
        //hàm lấy danh sách nhân viên có doanh thu cao nhất theo năm
        public async Task<List<(NhanVien, decimal)>> LayDanhSachNhanVienCoDoanhThuCaoNhatTheoNam(int nam)
        {
            var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd => hd.ngay_tao.Year == nam);
            var nhanVienIds = hoaDon.GroupBy(hd => hd.id_nhan_vien_xu_ly)
                                           .OrderByDescending(g => g.Sum(hd => hd.tong_tien_phai_thanh_toan ?? 0))
                                           .Take(10)
                                           .Select(g => g.Key)
                                           .ToList();
            var result = new List<(NhanVien, decimal)>();
            foreach (var nhanVienId in nhanVienIds)
            {
                if (nhanVienId != null)
                {
                    var nv = await _nhanVienRepository.GetByIdAsync(nhanVienId.Value);
                    if (nv != null)
                    {
                        var tongTien = hoaDon.Where(hd => hd.id_nhan_vien_xu_ly == nhanVienId)
                                           .Sum(hd => (hd.tong_tien_phai_thanh_toan ?? 0) - (hd.phi_van_chuyen ?? 0));
                        result.Add((nv, tongTien));
                    }
                }
            }
            return result;
        }
        #endregion
        #region Tính tổng sản phẩm mới
        //hàm tính tổng sản phẩm mới theo tháng
        public async Task<int> TinhTongSanPhamMoiTheoThang(int thang, int nam)
        {
            var sanPham = await _sanPhamRepository.GetByConditionAsync(sp => sp.ngay_tao.Month == thang && sp.ngay_tao.Year == nam);
            return sanPham.Count();
        }
        //hàm tính tổng sản phẩm mới theo năm
        public async Task<int> TinhTongSanPhamMoiTheoNam(int nam)
        {
            var sanPham = await _sanPhamRepository.GetByConditionAsync(sp => sp.ngay_tao.Year == nam);
            return sanPham.Count();
        }
        //hàm tính tổng sản phẩm mới theo tuần
        public async Task<int> TinhTongSanPhamMoiTheoTuan(int tuan, int nam)
        {
            var startDate = GetStartDateOfWeek(tuan, nam);
            var endDate = startDate.AddDays(7);
            var sanPham = await _sanPhamRepository.GetByConditionAsync(sp => sp.ngay_tao >= startDate && sp.ngay_tao < endDate);
            return sanPham.Count();
        }
        //hàm tính tổng sản phẩm đã bán theo tháng
        public async Task<int> TinhTongSanPhamDaBanTheoThang(int thang, int nam)
        {
            var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd => hd.ngay_tao.Month == thang && hd.ngay_tao.Year == nam);
            var hoaDonChiTiet = await _hoaDonChiTietRepository.GetByConditionAsync(hdc => hoaDon.Any(hd => hd.id_hoa_don == hdc.id_hoa_don));
            return hoaDonChiTiet.Sum(hdc => hdc.so_luong);
        }
        //hàm tính tổng sản phẩm đã bán theo năm
        public async Task<int> TinhTongSanPhamDaBanTheoNam(int nam)
        {
            var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd => hd.ngay_tao.Year == nam);
            var hoaDonChiTiet = await _hoaDonChiTietRepository.GetByConditionAsync(hdc => hoaDon.Any(hd => hd.id_hoa_don == hdc.id_hoa_don));
            return hoaDonChiTiet.Sum(hdc => hdc.so_luong);
        }
        //hàm tính tổng sản phẩm đã bán theo tuần
        public async Task<int> TinhTongSanPhamDaBanTheoTuan(int tuan, int nam)
        {
            var startDate = GetStartDateOfWeek(tuan, nam);
            var endDate = startDate.AddDays(7);
            var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd => hd.ngay_tao >= startDate && hd.ngay_tao < endDate);
            var hoaDonChiTiet = await _hoaDonChiTietRepository.GetByConditionAsync(hdc => hoaDon.Any(hd => hd.id_hoa_don == hdc.id_hoa_don));
            return hoaDonChiTiet.Sum(hdc => hdc.so_luong);
        }
        //hàm tính tổng sản phẩm đã bán theo ngày
        public async Task<int> TinhTongSanPhamDaBanTheoNgay(DateOnly ngay)
        {
            DateTime dateTime = ngay.ToDateTime(TimeOnly.MinValue);
            var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd => hd.ngay_tao.Date == dateTime.Date);
            var hoaDonChiTiet = await _hoaDonChiTietRepository.GetByConditionAsync(hdc => hoaDon.Any(hd => hd.id_hoa_don == hdc.id_hoa_don));
            return hoaDonChiTiet.Sum(hdc => hdc.so_luong);
        }
        // hàm lấy ra các sản phẩm bán chạy nhất theo tháng
        public async Task<List<SanPham>> LaySanPhamBanChayNhatTheoThang(int thang, int nam)
        {
            try
            {
                var cacheKey = GenerateCacheKey("SanPhamBanChayThang", thang, nam);
                var cachedResult = GetFromCache<List<SanPham>>(cacheKey);
                if (cachedResult != null)
                    return cachedResult;

                Console.WriteLine($"Bắt đầu tìm sản phẩm bán chạy tháng {thang}/{nam}");

                // Lấy danh sách hóa đơn trong tháng có trạng thái hợp lệ
                var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd =>
                    hd.ngay_tao.Month == thang &&
                    hd.ngay_tao.Year == nam &&
                    VALID_TRANG_THAI.Contains(hd.trang_thai_hoa_don));

                Console.WriteLine($"- Tìm thấy {hoaDon.Count()} hóa đơn hợp lệ");
                foreach (var hd in hoaDon)
                {
                    Console.WriteLine($"  + Hóa đơn {hd.ma_hoa_don} - {hd.trang_thai_hoa_don}");
                }

                if (!hoaDon.Any())
                    return new List<SanPham>();

                var hoaDonIds = hoaDon.Select(hd => hd.id_hoa_don).ToList();

                // Lấy và nhóm chi tiết hóa đơn theo sản phẩm
                var hoaDonChiTiet = await _hoaDonChiTietRepository.GetByConditionAsync(
                    hdc => hoaDonIds.Contains(hdc.id_hoa_don));

                Console.WriteLine($"- Tìm thấy {hoaDonChiTiet.Count()} chi tiết hóa đơn");

                var sanPhamChiTietBanChay = hoaDonChiTiet
                    .GroupBy(hdc => hdc.id_san_pham_chi_tiet)
                    .Select(g => new
                    {
                        IdSanPhamChiTiet = g.Key,
                        TongSoLuong = g.Sum(hdc => hdc.so_luong)
                    })
                    .OrderByDescending(x => x.TongSoLuong)
                    .Take(20)
                    .ToList();

                Console.WriteLine($"- Đã nhóm thành {sanPhamChiTietBanChay.Count} sản phẩm chi tiết");
                foreach (var sp in sanPhamChiTietBanChay)
                {
                    Console.WriteLine($"  + Sản phẩm chi tiết {sp.IdSanPhamChiTiet}: {sp.TongSoLuong} cái");
                }

                if (!sanPhamChiTietBanChay.Any())
                    return new List<SanPham>();

                // Lấy thông tin sản phẩm
                var sanPhamChiTietIds = sanPhamChiTietBanChay.Select(x => x.IdSanPhamChiTiet).ToList();
                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByConditionAsync(
                    spct => sanPhamChiTietIds.Contains(spct.id_san_pham_chi_tiet));

                Console.WriteLine($"- Tìm thấy {sanPhamChiTiet.Count()} sản phẩm chi tiết từ database");

                var sanPhamIds = sanPhamChiTiet.Select(spct => spct.id_san_pham).Distinct().ToList();
                var sanPhams = await _sanPhamRepository.GetByConditionAsync(
                    sp => sanPhamIds.Contains(sp.id_san_pham));

                Console.WriteLine($"- Kết quả cuối: {sanPhams.Count()} sản phẩm");
                foreach (var sp in sanPhams)
                {
                    Console.WriteLine($"  + Sản phẩm {sp.ma_san_pham}: {sp.ten_san_pham}");
                }

                var result = sanPhams.ToList();
                SetCache(cacheKey, result);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy sản phẩm bán chạy tháng {thang}/{nam}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        // hàm lấy ra các sản phẩm bán chạy nhất theo năm
        public async Task<List<SanPham>> LaySanPhamBanChayNhatTheoNam(int nam)
        {
            try
            {
                // Lấy danh sách hóa đơn trong năm có trạng thái hợp lệ
                var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd =>
                    hd.ngay_tao.Year == nam &&
                    VALID_TRANG_THAI.Contains(hd.trang_thai_hoa_don));

                var hoaDonIds = hoaDon.Select(hd => hd.id_hoa_don).ToList();

                // Lấy chi tiết hóa đơn và tính tổng số lượng bán theo sản phẩm chi tiết
                var hoaDonChiTiet = await _hoaDonChiTietRepository.GetByConditionAsync(
                    hdc => hoaDonIds.Contains(hdc.id_hoa_don) &&
                          hdc.trang_thai != "DaHuy" &&
                          hdc.trang_thai != "ChoTaiQuay" &&
                          hdc.trang_thai != "DangChoXuLy" &&
                          hdc.trang_thai != "HetHang" &&
                          hdc.trang_thai != "ChuaThanhToan" &&
                          hdc.trang_thai != "DaHoanTraMotPhan");

                // Nhóm theo sản phẩm chi tiết và tính tổng số lượng
                var sanPhamChiTietBanChay = hoaDonChiTiet
                    .GroupBy(hdc => hdc.id_san_pham_chi_tiet)
                    .Select(g => new
                    {
                        IdSanPhamChiTiet = g.Key,
                        TongSoLuong = g.Sum(hdc => hdc.so_luong)
                    })
                    .OrderByDescending(x => x.TongSoLuong)
                    .Take(20)  // Lấy top 20 để đảm bảo sau khi gộp theo sản phẩm còn đủ top 10
                    .ToList();

                if (!sanPhamChiTietBanChay.Any())
                    return new List<SanPham>();

                // Lấy thông tin sản phẩm chi tiết
                var sanPhamChiTietIds = sanPhamChiTietBanChay.Select(x => x.IdSanPhamChiTiet).ToList();
                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByConditionAsync(
                    spct => sanPhamChiTietIds.Contains(spct.id_san_pham_chi_tiet));

                // Lấy danh sách sản phẩm
                var sanPhamIds = sanPhamChiTiet.Select(spct => spct.id_san_pham).Distinct().ToList();
                var sanPhams = await _sanPhamRepository.GetByConditionAsync(
                    sp => sanPhamIds.Contains(sp.id_san_pham));

                return sanPhams.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy sản phẩm bán chạy theo năm: {ex.Message}");
                return new List<SanPham>();
            }
        }
        // hàm lấy ra các sản phẩm bán chạy nhất theo tuần
        public async Task<List<SanPham>> LaySanPhamBanChayNhatTheoTuan(int tuan, int nam)
        {
            try
            {
                var startDate = GetStartDateOfWeek(tuan, nam);
                var endDate = startDate.AddDays(7);

                // Lấy danh sách hóa đơn trong tuần có trạng thái hợp lệ
                var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd =>
                    hd.ngay_tao >= startDate &&
                    hd.ngay_tao < endDate &&
                    VALID_TRANG_THAI.Contains(hd.trang_thai_hoa_don));

                var hoaDonIds = hoaDon.Select(hd => hd.id_hoa_don).ToList();

                // Lấy chi tiết hóa đơn và tính tổng số lượng bán theo sản phẩm chi tiết
                var hoaDonChiTiet = await _hoaDonChiTietRepository.GetByConditionAsync(
                    hdc => hoaDonIds.Contains(hdc.id_hoa_don) &&
                          hdc.trang_thai != "DaHuy" &&
                          hdc.trang_thai != "ChoTaiQuay" &&
                          hdc.trang_thai != "DangChoXuLy" &&
                          hdc.trang_thai != "HetHang" &&
                          hdc.trang_thai != "ChuaThanhToan" &&
                          hdc.trang_thai != "DaHoanTraMotPhan");

                // Nhóm theo sản phẩm chi tiết và tính tổng số lượng
                var sanPhamChiTietBanChay = hoaDonChiTiet
                    .GroupBy(hdc => hdc.id_san_pham_chi_tiet)
                    .Select(g => new
                    {
                        IdSanPhamChiTiet = g.Key,
                        TongSoLuong = g.Sum(hdc => hdc.so_luong)
                    })
                    .OrderByDescending(x => x.TongSoLuong)
                    .Take(20)
                    .ToList();

                if (!sanPhamChiTietBanChay.Any())
                    return new List<SanPham>();

                var sanPhamChiTietIds = sanPhamChiTietBanChay.Select(x => x.IdSanPhamChiTiet).ToList();
                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByConditionAsync(
                    spct => sanPhamChiTietIds.Contains(spct.id_san_pham_chi_tiet));

                var sanPhamIds = sanPhamChiTiet.Select(spct => spct.id_san_pham).Distinct().ToList();
                var sanPhams = await _sanPhamRepository.GetByConditionAsync(
                    sp => sanPhamIds.Contains(sp.id_san_pham));

                return sanPhams.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy sản phẩm bán chạy theo tuần: {ex.Message}");
                return new List<SanPham>();
            }
        }
        //hàm lấy ra các sản phẩm bán chạy nhất theo ngày
        public async Task<List<SanPham>> LaySanPhamBanChayNhatTheoNgay(DateOnly ngay)
        {
            try
            {
                DateTime dateTime = ngay.ToDateTime(TimeOnly.MinValue);

                // Lấy danh sách hóa đơn trong ngày có trạng thái hợp lệ
                var hoaDon = await _hoaDonRepository.GetByConditionAsync(hd =>
                    hd.ngay_tao.Date == dateTime.Date &&
                    VALID_TRANG_THAI.Contains(hd.trang_thai_hoa_don));

                var hoaDonIds = hoaDon.Select(hd => hd.id_hoa_don).ToList();

                // Lấy chi tiết hóa đơn và tính tổng số lượng bán theo sản phẩm chi tiết
                var hoaDonChiTiet = await _hoaDonChiTietRepository.GetByConditionAsync(
                    hdc => hoaDonIds.Contains(hdc.id_hoa_don) &&
                          hdc.trang_thai != "DaHuy" &&
                          hdc.trang_thai != "ChoTaiQuay" &&
                          hdc.trang_thai != "DangChoXuLy" &&
                          hdc.trang_thai != "HetHang" &&
                          hdc.trang_thai != "ChuaThanhToan" &&
                          hdc.trang_thai != "DaHoanTraMotPhan");

                // Nhóm theo sản phẩm chi tiết và tính tổng số lượng
                var sanPhamChiTietBanChay = hoaDonChiTiet
                    .GroupBy(hdc => hdc.id_san_pham_chi_tiet)
                    .Select(g => new
                    {
                        IdSanPhamChiTiet = g.Key,
                        TongSoLuong = g.Sum(hdc => hdc.so_luong)
                    })
                    .OrderByDescending(x => x.TongSoLuong)
                    .Take(20)
                    .ToList();

                if (!sanPhamChiTietBanChay.Any())
                    return new List<SanPham>();

                var sanPhamChiTietIds = sanPhamChiTietBanChay.Select(x => x.IdSanPhamChiTiet).ToList();
                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByConditionAsync(
                    spct => sanPhamChiTietIds.Contains(spct.id_san_pham_chi_tiet));

                var sanPhamIds = sanPhamChiTiet.Select(spct => spct.id_san_pham).Distinct().ToList();
                var sanPhams = await _sanPhamRepository.GetByConditionAsync(
                    sp => sanPhamIds.Contains(sp.id_san_pham));

                return sanPhams.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy sản phẩm bán chạy theo ngày: {ex.Message}");
                return new List<SanPham>();
            }
        }
        #endregion
        //hàm tính số lượng sản phẩm chi tiết đã bán 
        public async Task<int> TinhSoLuongSanPhamChiTietDaBan(Guid id_san_pham_chi_tiet)
        {
            var hoaDonChiTiet = await _hoaDonChiTietRepository.GetByConditionAsync(hdc =>
                hdc.id_san_pham_chi_tiet == id_san_pham_chi_tiet &&
                hdc.trang_thai != "ChoTaiQuay" &&
                hdc.trang_thai != "DangChoXuLy" &&
                hdc.trang_thai != "HetHang" &&
                hdc.trang_thai != "ChuaThanhToan" &&
                hdc.trang_thai != "DaHuy" &&
                hdc.trang_thai != "DaHoanTraMotPhan");

            return hoaDonChiTiet.Sum(hdc => hdc.so_luong);
        }
        //hàm tính số lượng sản phẩm đã bán 
        public async Task<int> TinhSoLuongSanPhamDaBan(Guid id_san_pham)
        {
            try
            {
                var cacheKey = GenerateCacheKey("SoLuongBan", id_san_pham);
                var cachedResult = GetFromCache<int?>(cacheKey);
                if (cachedResult.HasValue)
                    return cachedResult.Value;

                // Lấy tất cả sản phẩm chi tiết và hóa đơn chi tiết trong một lần query
                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByConditionAsync(spct =>
                    spct.id_san_pham == id_san_pham);

                if (!sanPhamChiTiet.Any())
                    return 0;

                var spctIds = sanPhamChiTiet.Select(spct => spct.id_san_pham_chi_tiet).ToList();

                var hoaDonChiTiet = await _hoaDonChiTietRepository.GetByConditionAsync(hdc =>
                    spctIds.Contains(hdc.id_san_pham_chi_tiet) &&
                    hdc.trang_thai != "ChoTaiQuay" &&
                    hdc.trang_thai != "DangChoXuLy" &&
                    hdc.trang_thai != "HetHang" &&
                    hdc.trang_thai != "ChuaThanhToan" &&
                    hdc.trang_thai != "DaHuy" &&
                    hdc.trang_thai != "DaTraHang" &&
                    hdc.trang_thai != "DaXacNhanTraHang" &&
                    hdc.trang_thai != "DaHoanTraMotPhan");

                var tongSoLuong = hoaDonChiTiet.Sum(hdc => hdc.so_luong);
                SetCache(cacheKey, tongSoLuong);
                return tongSoLuong;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tính số lượng sản phẩm đã bán {id_san_pham}: {ex.Message}");
                return 0;
            }
        }
    }
}