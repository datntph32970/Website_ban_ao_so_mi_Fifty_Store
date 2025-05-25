using API.DbConects.DTOs.Admin.HoaDon;
using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.DTOs.Client.HoaDon;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Khuyen_Mai;
using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories.Interfaces;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace API.Services.Implementations
{
    public enum TrangThaiDonHang
    {
        ChuaThanhToan,
        DaThanhToan,
        DangChoXuLy,
        DaXacNhan,
        DangChuanBi,
        DangGiaoHang,
        DaNhanHang,
        DaHoanThanh,
        DaHuy,
        HetHang,
        ChoTaiQuay
    }

    public class TransactionHelper
    {
        private readonly IBaseRepository<HoaDon> _hoaDonRepository;
        private readonly IBaseRepository<HoaDonChiTiet> _hoaDonChiTietRepository;
        private readonly IBaseRepository<SanPhamChiTiet> _sanPhamChiTietRepository;
        private readonly IBaseService<KhuyenMai> _khuyenMaiServices;
        private readonly IBaseService<GiamGia> _giamGiaServices;
        public TransactionHelper(
            IBaseRepository<HoaDon> hoaDonRepository,
            IBaseRepository<HoaDonChiTiet> hoaDonChiTietRepository,
            IBaseRepository<SanPhamChiTiet> sanPhamChiTietRepository,
            IBaseService<GiamGia> giamGiaServices,
            IBaseService<KhuyenMai> khuyenMaiServices)
        {
            _hoaDonRepository = hoaDonRepository;
            _hoaDonChiTietRepository = hoaDonChiTietRepository;
            _sanPhamChiTietRepository = sanPhamChiTietRepository;
            _khuyenMaiServices = khuyenMaiServices;
            _giamGiaServices = giamGiaServices;
        }

        public async Task<bool> ExecuteInTransactionAsync(Func<Task<bool>> operation, int maxRetries = 3)
        {
            var retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    return await operation();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (retryCount == maxRetries - 1)
                        throw;

                    await Task.Delay(100 * (retryCount + 1)); // Exponential backoff
                    retryCount++;
                }
            }
            return false;
        }

        public async Task<bool> CapNhatSoLuongSanPhamAsync(Guid idSanPhamChiTiet, int soLuongThayDoi)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdAsync(idSanPhamChiTiet);
                if (sanPhamChiTiet == null)
                    return false;

                var soLuongMoi = sanPhamChiTiet.so_luong + soLuongThayDoi;
                if (soLuongMoi < 0)
                    return false;

                sanPhamChiTiet.so_luong = soLuongMoi;
                return await _sanPhamChiTietRepository.UpdateAsync(sanPhamChiTiet);
            });
        }

        public async Task<bool> XuLyHoanTienAsync(HoaDon hoaDon)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                // Cập nhật trạng thái hóa đơn
                hoaDon.trang_thai_hoa_don = "DaHuy";
                hoaDon.ngay_sua = DateTime.Now;

                // Hoàn lại số lượng khuyến mãi nếu có
                if (hoaDon.id_khuyen_mai.HasValue)
                {
                    var khuyenMai = await _khuyenMaiServices.GetByIdAsync(hoaDon.id_khuyen_mai.Value);
                    if (khuyenMai != null)
                    {
                        khuyenMai.so_luong_da_su_dung -= 1;
                        await _khuyenMaiServices.UpdateAsync(khuyenMai);
                    }
                }



                return await _hoaDonRepository.UpdateAsync(hoaDon);
            });
        }
    }

    public class HoaDonService : BaseService<HoaDon>, IHoaDonService
    {
        private readonly IEmailService _emailService;
        private readonly IBaseRepository<HoaDon> _hoaDonRepository;
        private readonly IBaseRepository<HoaDonChiTiet> _hoaDonChiTietRepository;
        private readonly IBaseRepository<SanPhamChiTiet> _sanPhamChiTietRepository;
        private readonly IBaseRepository<SanPham> _sanPhamRepository;
        private readonly IBaseRepository<KhachHang> _khachHangRepository;
        private readonly IBaseRepository<NhanVien> _nhanVienRepository;
        private readonly IBaseRepository<KhuyenMai> _khuyenMaiRepository;
        private readonly IBaseRepository<PhuongThucThanhToan> _phuongThucThanhToanRepository;
        private readonly IBaseRepository<GiamGia> _giamGiaRepository;
        private readonly IBaseRepository<DiaChi> _diaChiRepository;
        private readonly IBaseRepository<CuaHang> _cuaHangRepository;
        private readonly IBaseRepository<GioHangChiTiet> _gioHangChiTietRepository;
        private readonly VNPayService _vnPayService;
        private readonly IBaseService<KhuyenMai> _khuyenMaiServices;
        private readonly IBaseService<GiamGia> _giamGiaServices;
        private readonly TransactionHelper _transactionHelper;
        private readonly ILogger<HoaDonService> _logger;

        // Helper methods for status validation
        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            return TrangThaiDonHangHelper.IsValidTransition(currentStatus, newStatus);
        }

        private async Task<bool> ValidateNhanVienXuLy(Guid id_nhan_vien_xu_ly)
        {
            if (id_nhan_vien_xu_ly == Guid.Empty)
                return false;

            var nhanVien = await _nhanVienRepository.GetByIdAsync(id_nhan_vien_xu_ly);
            return nhanVien != null;
        }

        private async Task<bool> RetryOperation(Func<Task<bool>> operation, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    if (i == maxRetries - 1) throw;
                    await Task.Delay(1000 * (i + 1)); // Exponential backoff
                    Console.WriteLine($"Retry {i + 1}: {ex.Message}");
                }
            }
            return false;
        }

        public HoaDonService(
            IBaseRepository<HoaDon> hoaDonRepository,
            IBaseRepository<HoaDonChiTiet> hoaDonChiTietRepository,
            IBaseRepository<SanPhamChiTiet> sanPhamChiTietRepository,
            IBaseService<KhuyenMai> khuyenMaiServices,
            IBaseService<GiamGia> giamGiaServices,
            IBaseRepository<SanPham> sanPhamRepository,
            IBaseRepository<KhachHang> khachHangRepository,
            IBaseRepository<NhanVien> nhanVienRepository,
            IBaseRepository<KhuyenMai> khuyenMaiRepository,
            IBaseRepository<PhuongThucThanhToan> phuongThucThanhToanRepository,
            IBaseRepository<GiamGia> giamGiaRepository,
            IBaseRepository<CuaHang> cuaHangRepository,
            IBaseRepository<GioHangChiTiet> gioHangChiTietRepository,
            IBaseRepository<DiaChi> diaChiRepository,
            VNPayService vnPayService,
            IEmailService emailService,
            ILogger<HoaDonService> logger) : base(hoaDonRepository)
        {
            _hoaDonRepository = hoaDonRepository;
            _hoaDonChiTietRepository = hoaDonChiTietRepository;
            _sanPhamChiTietRepository = sanPhamChiTietRepository;
            _sanPhamRepository = sanPhamRepository;
            _khachHangRepository = khachHangRepository;
            _nhanVienRepository = nhanVienRepository;
            _khuyenMaiRepository = khuyenMaiRepository;
            _phuongThucThanhToanRepository = phuongThucThanhToanRepository;
            _giamGiaRepository = giamGiaRepository;
            _cuaHangRepository = cuaHangRepository;
            _gioHangChiTietRepository = gioHangChiTietRepository;
            _diaChiRepository = diaChiRepository;
            _vnPayService = vnPayService;
            _emailService = emailService;
            _khuyenMaiServices = khuyenMaiServices;
            _giamGiaServices = giamGiaServices;
            _transactionHelper = new TransactionHelper(_hoaDonRepository, _hoaDonChiTietRepository, _sanPhamChiTietRepository, _giamGiaServices, khuyenMaiServices);
            _logger = logger;
        }

        private string GetHoaDonCacheKey(Guid idHoaDon) => $"hoadon_{idHoaDon}";
        private string GetHoaDonListCacheKey() => "hoadon_list";

        private void ClearHoaDonCache(Guid idHoaDon)
        {
            // Cache removal logic has been removed as per instructions
        }

        private async Task<(decimal tongTienSauKhuyenMai, decimal giaTriKhuyenMai)> CapNhatTongTienVaGiaTriKhuyenMaiChoHoaDon(HoaDon hoaDon)
        {
            // Chỉ cập nhật cho hóa đơn có trạng thái ChoTaiQuay hoặc ChuaThanhToan
            if (hoaDon.trang_thai_hoa_don != "ChoTaiQuay" && hoaDon.trang_thai_hoa_don != "ChuaThanhToan")
            {
                return (hoaDon.tong_tien_don_hang - hoaDon.so_tien_khuyen_mai + (hoaDon.phi_van_chuyen ?? 0) ?? 0, hoaDon.so_tien_khuyen_mai ?? 0);
            }

            var (tongTienSauKhuyenMai, giaTriKhuyenMai) = await CapNhatTongTienVaGiaTriKhuyenMai(hoaDon.id_hoa_don);
            hoaDon.tong_tien_phai_thanh_toan = tongTienSauKhuyenMai;
            hoaDon.so_tien_khuyen_mai = giaTriKhuyenMai;
            await _hoaDonRepository.UpdateAsync(hoaDon);

            return (tongTienSauKhuyenMai, giaTriKhuyenMai);
        }

        public async Task<List<HoaDonAdminDTO>> GetAllHoaDonAdminDTOAsync()
        {
            var hoaDons = await _hoaDonRepository.GetAllWithIncludeAsync(
                q => q.Include(hd => hd.HoaDonChiTiets)
                     .ThenInclude(hct => hct.SanPhamChiTiet)
                     .ThenInclude(spct => spct.SanPham)
                     .Include(hd => hd.HoaDonChiTiets)
                     .ThenInclude(hct => hct.SanPhamChiTiet)
                     .ThenInclude(spct => spct.MauSac)
                     .Include(hd => hd.HoaDonChiTiets)
                     .ThenInclude(hct => hct.SanPhamChiTiet)
                     .ThenInclude(spct => spct.KichCo)
                     .Include(hd => hd.KhachHang)
                     .Include(hd => hd.NhanVienXuLy)
                     .Include(hd => hd.KhuyenMai)
                     .Include(hd => hd.PhuongThucThanhToan)
                     .Include(hd => hd.CuaHang));

            var result = new List<HoaDonAdminDTO>();

            foreach (var hoaDon in hoaDons.OrderByDescending(hd => hd.ngay_tao))
            {
                var hoaDonChiTiets = await MapHoaDonChiTietsAsync(hoaDon.HoaDonChiTiets);
                var tongTienDonHang = await TinhTongTienDonHang(hoaDon.id_hoa_don);
                var (tongTienSauKhuyenMai, giaTriKhuyenMai) = await CapNhatTongTienVaGiaTriKhuyenMaiChoHoaDon(hoaDon);

                result.Add(new HoaDonAdminDTO
                {
                    id_hoa_don = hoaDon.id_hoa_don,
                    ma_hoa_don = hoaDon.ma_hoa_don,
                    id_khach_hang = hoaDon.id_khach_hang != null ? hoaDon.id_khach_hang : null,
                    ten_khach_hang = hoaDon.id_khach_hang != null ? hoaDon.KhachHang.ten_khach_hang : "Khách lẻ",
                    ten_nguoi_xu_ly = hoaDon.NhanVienXuLy?.ten_nhan_vien,
                    sdt_khach_hang = hoaDon.id_khach_hang != null ? hoaDon.KhachHang.so_dien_thoai : null,
                    dia_chi_nhan_hang = hoaDon.id_khach_hang != null ? hoaDon.dia_chi_nhan_hang : null,
                    ghi_chu = hoaDon.ghi_chu,
                    loai_hoa_don = hoaDon.loai_hoa_don,
                    so_tien_khach_tra = hoaDon.so_tien_khach_tra,
                    phi_van_chuyen = hoaDon.phi_van_chuyen ?? 0,
                    id_phuong_thuc_thanh_toan = hoaDon.id_phuong_thuc_thanh_toan?.ToString(),
                    so_tien_thua_tra_khach = hoaDon.so_tien_thua_tra_khach,
                    tong_tien_don_hang = tongTienDonHang,
                    so_tien_khuyen_mai = giaTriKhuyenMai,
                    tong_tien_phai_thanh_toan = tongTienSauKhuyenMai,
                    trang_thai = hoaDon.trang_thai_hoa_don,
                    ten_phuong_thuc_thanh_toan = hoaDon.PhuongThucThanhToan?.ten_phuong_thuc_thanh_toan,
                    ngay_tao = hoaDon.ngay_tao,
                    nhanVienXuLy = hoaDon.NhanVienXuLy != null ? new NhanVien_HoaDonAdminDTO
                    {
                        id_nhan_vien = hoaDon.id_nhan_vien_xu_ly,
                        ma_nhan_vien = hoaDon.NhanVienXuLy.ma_nhan_vien,
                        ten_nhan_vien = hoaDon.NhanVienXuLy.ten_nhan_vien
                    } : null,
                    khachHang = hoaDon.KhachHang != null ? new KhachHang_HoaDonAdminDTO
                    {
                        id_khach_hang = hoaDon.KhachHang.id_khach_hang,
                        ma_khach_hang = hoaDon.KhachHang.ma_khach_hang,
                        ten_khach_hang = hoaDon.KhachHang.ten_khach_hang,
                        sdt_khach_hang = hoaDon.KhachHang.so_dien_thoai
                    } : null,
                    hoaDonChiTiets = hoaDonChiTiets
                });
            }

            return result;
        }
        public async Task<HoaDonAdminDTO> GetByIdHoaDonAdminDTOAsync(Guid id)
        {
            var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(id,
                q => q.Include(hd => hd.HoaDonChiTiets)
                     .ThenInclude(hct => hct.SanPhamChiTiet)
                     .ThenInclude(spct => spct.SanPham)
                     .Include(hd => hd.HoaDonChiTiets)
                     .ThenInclude(hct => hct.SanPhamChiTiet)
                     .ThenInclude(spct => spct.MauSac)
                     .Include(hd => hd.HoaDonChiTiets)
                     .ThenInclude(hct => hct.SanPhamChiTiet)
                     .ThenInclude(spct => spct.KichCo)
                     .Include(hd => hd.KhachHang)
                     .Include(hd => hd.NhanVienXuLy)
                     .Include(hd => hd.KhuyenMai)
                     .Include(hd => hd.PhuongThucThanhToan)
                     .Include(hd => hd.CuaHang));

            if (hoaDon == null)
                return null;

            var hoaDonChiTiets = await MapHoaDonChiTietsAsync(hoaDon.HoaDonChiTiets);
            var tongTienDonHang = await TinhTongTienDonHang(hoaDon.id_hoa_don);
            var (tongTienSauKhuyenMai, giaTriKhuyenMai) = await CapNhatTongTienVaGiaTriKhuyenMaiChoHoaDon(hoaDon);

            return new HoaDonAdminDTO
            {
                id_hoa_don = hoaDon.id_hoa_don,
                ma_hoa_don = hoaDon.ma_hoa_don,
                id_khach_hang = hoaDon.id_khach_hang != null ? hoaDon.id_khach_hang : null,
                ten_khach_hang = hoaDon.id_khach_hang != null ? hoaDon.KhachHang.ten_khach_hang : "Khách lẻ",
                ten_nguoi_xu_ly = hoaDon.NhanVienXuLy?.ten_nhan_vien,
                sdt_khach_hang = hoaDon.id_khach_hang != null ? hoaDon.KhachHang.so_dien_thoai : null,
                dia_chi_nhan_hang = hoaDon.id_khach_hang != null ? hoaDon.dia_chi_nhan_hang : null,
                ghi_chu = hoaDon.ghi_chu,
                ly_do_huy_don_hang = hoaDon.ly_do_huy_don_hang,
                loai_hoa_don = hoaDon.loai_hoa_don,
                so_tien_khach_tra = hoaDon.so_tien_khach_tra,
                phi_van_chuyen = hoaDon.phi_van_chuyen,
                so_tien_thua_tra_khach = hoaDon.so_tien_thua_tra_khach,
                id_phuong_thuc_thanh_toan = hoaDon.id_phuong_thuc_thanh_toan?.ToString(),
                tong_tien_don_hang = tongTienDonHang,
                so_tien_khuyen_mai = giaTriKhuyenMai,
                tong_tien_phai_thanh_toan = tongTienSauKhuyenMai,
                trang_thai = hoaDon.trang_thai_hoa_don,
                ten_phuong_thuc_thanh_toan = hoaDon.PhuongThucThanhToan?.ten_phuong_thuc_thanh_toan,
                ngay_tao = hoaDon.ngay_tao,
                khuyenMai = hoaDon.KhuyenMai == null ? null : new KhuyenMai_HoaDonAdminDTO
                {
                    id_khuyen_mai = hoaDon.KhuyenMai.id_khuyen_mai,
                    ten_khuyen_mai = hoaDon.KhuyenMai.ten_khuyen_mai,
                    ma_khuyen_mai = hoaDon.KhuyenMai.ma_khuyen_mai,
                    loai_khuyen_mai = hoaDon.KhuyenMai.kieu_khuyen_mai,
                    gia_tri_khuyen_mai = hoaDon.KhuyenMai.gia_tri_giam,
                    gia_tri_giam_toi_da = hoaDon.KhuyenMai.gia_tri_giam_toi_da
                },
                cuaHang = hoaDon.CuaHang == null ? null : new CuaHang_HoaDonAdminDTO
                {
                    id_cua_hang = hoaDon.CuaHang.id_cua_hang,
                    ten_cua_hang = hoaDon.CuaHang.ten_cua_hang,
                    website = hoaDon.CuaHang.website,
                    email = hoaDon.CuaHang.email,
                    sdt = hoaDon.CuaHang.sdt,
                    dia_chi = hoaDon.CuaHang.dia_chi,
                    mo_ta = hoaDon.CuaHang.mo_ta,
                    hinh_anh_logo_cua_hang_url = hoaDon.CuaHang.HinhAnh?.url
                },
                nhanVienXuLy = hoaDon.NhanVienXuLy == null ? null : new NhanVien_HoaDonAdminDTO
                {
                    id_nhan_vien = hoaDon.NhanVienXuLy.id_nhan_vien,
                    ma_nhan_vien = hoaDon.NhanVienXuLy.ma_nhan_vien,
                    ten_nhan_vien = hoaDon.NhanVienXuLy.ten_nhan_vien
                },
                khachHang = hoaDon.KhachHang == null ? null : new KhachHang_HoaDonAdminDTO
                {
                    id_khach_hang = hoaDon.KhachHang.id_khach_hang,
                    ma_khach_hang = hoaDon.KhachHang.ma_khach_hang,
                    ten_khach_hang = hoaDon.KhachHang.ten_khach_hang,
                    sdt_khach_hang = hoaDon.KhachHang.so_dien_thoai
                },
                hoaDonChiTiets = hoaDonChiTiets
            };
        }
        public async Task<List<HoaDonAdminDTO>> GetHoaDonBySanPhamChiTietAsync(Guid sanPhamChiTietId)
        {
            var result = await GetAllHoaDonAdminDTOAsync();
            return result.Where(hd => hd.hoaDonChiTiets.Any(hct => hct.id_san_pham_chi_tiet == sanPhamChiTietId)).ToList();
        }
        public async Task<(bool, string)> ThemHoaDonBanTaiQuayMoiAsync(Guid id_nhan_vien_xu_ly)
        {
            try
            {
                var result = await _nhanVienRepository.GetByIdAsync(id_nhan_vien_xu_ly);
                if (result == null)
                {
                    return (false, "Không tìm thấy nhân viên");
                }

                var hoaDonTaiQuayDangChoXuLy = await _hoaDonRepository.GetByConditionAsync(hd =>
                    hd.loai_hoa_don == "TaiQuay" &&
                    hd.trang_thai_hoa_don == "ChoTaiQuay" &&
                    hd.id_nhan_vien_xu_ly == id_nhan_vien_xu_ly);

                if (hoaDonTaiQuayDangChoXuLy.Count >= 10)
                {
                    return (false, "Bạn đã đạt giới hạn tối đa 10 hóa đơn tại quầy đang chờ xử lý");
                }
                var hoaDon = new HoaDon
                {
                    id_nhan_vien_xu_ly = id_nhan_vien_xu_ly,
                    ma_hoa_don = await TaoMaHoaDon(),
                    ngay_tao = DateTime.Now,
                    loai_hoa_don = "TaiQuay",
                    ten_nhan_vien = result.ten_nhan_vien,
                    trang_thai_hoa_don = "ChoTaiQuay",
                };
                var themhoadon = await _hoaDonRepository.CreateAsync(hoaDon);
                if (!themhoadon)
                {
                    return (false, "Thêm hóa đơn thất bại");
                }
                return (true, hoaDon.id_hoa_don.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        //xóa hóa đơn
        public async Task<(bool, string)> XoaHoaDon(Guid id_hoa_don)
        {
            try
            {

                var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(id_hoa_don,
                    q => q.Include(hd => hd.HoaDonChiTiets)
                         .ThenInclude(hct => hct.SanPhamChiTiet));

                if (hoaDon == null || hoaDon.trang_thai_hoa_don != "ChoTaiQuay")
                    return (false, "Hóa đơn không tồn tại hoặc không đang ở trạng thái chờ xử lý");

                // Cập nhật số lượng sản phẩm cho tất cả hóa đơn chi tiết
                if (hoaDon.HoaDonChiTiets != null && hoaDon.HoaDonChiTiets.Any())
                {
                    foreach (var hct in hoaDon.HoaDonChiTiets)
                    {
                        var xoahoadonchitiet = await XoaHoaDonChiTiet(hct.id_hoa_don_chi_tiet);
                        if (!xoahoadonchitiet.Item1)
                            return (false, "Xóa hóa đơn chi tiết thất bại");
                    }
                }
                if (hoaDon.id_khuyen_mai != null)
                {
                    // Xóa khuyến mãi
                    var khuyenMai = await _khuyenMaiRepository.GetByIdAsync(hoaDon.id_khuyen_mai.Value);
                    khuyenMai.so_luong_da_su_dung--;
                    await _khuyenMaiRepository.UpdateAsync(khuyenMai);
                }
                // Xóa hóa đơn
                var xoahoadon = await _hoaDonRepository.DeleteAsync(id_hoa_don);
                if (!xoahoadon)
                {
                    return (false, "Xóa hóa đơn thất bại");
                }
                return (true, "Xóa hóa đơn thành công");


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return (false, "Xóa hóa đơn thất bại");
            }
        }
        //xóa hóa đơn chi tiết
        public async Task<(bool, string)> XoaHoaDonChiTiet(Guid id_hoa_don_chi_tiet)
        {
            try
            {
                var result = await _hoaDonChiTietRepository.ExecuteInTransactionAsync(async () =>
                {
                    var hoaDonChiTiet = await _hoaDonChiTietRepository.GetByIdWithIncludeAsync(id_hoa_don_chi_tiet,
                        q => q.Include(hct => hct.SanPhamChiTiet).ThenInclude(spct => spct.SanPhamChiTietGiamGias)
                             .ThenInclude(spgg => spgg.GiamGia));

                    if (hoaDonChiTiet == null || hoaDonChiTiet.trang_thai != "ChoTaiQuay")
                        return false;

                    // Cập nhật số lượng sản phẩm chi tiết
                    if (hoaDonChiTiet.SanPhamChiTiet != null)
                    {
                        hoaDonChiTiet.SanPhamChiTiet.so_luong += hoaDonChiTiet.so_luong;
                        var updateSanPhamChiTiet = await _sanPhamChiTietRepository.UpdateAsync(hoaDonChiTiet.SanPhamChiTiet);
                        if (!updateSanPhamChiTiet)
                            return false;
                    }

                    // Xóa hóa đơn chi tiết
                    var xoahoadonchitiet = await _hoaDonChiTietRepository.DeleteAsync(id_hoa_don_chi_tiet);
                    if (!xoahoadonchitiet)
                        return false;




                    if (hoaDonChiTiet.SanPhamChiTiet?.SanPhamChiTietGiamGias != null)
                    {
                        // Tìm giảm giá đang được áp dụng cho sản phẩm này
                        var giamGiaDangApDung = hoaDonChiTiet.SanPhamChiTiet.SanPhamChiTietGiamGias
                            .FirstOrDefault(gg => gg.GiamGia != null &&
                                                gg.GiamGia.trang_thai == "HoatDong" &&
                                                gg.GiamGia.thoi_gian_bat_dau <= DateTime.Now &&
                                                gg.GiamGia.thoi_gian_ket_thuc >= DateTime.Now);

                        if (giamGiaDangApDung != null)
                        {
                            // Hoàn lại số lượng đã sử dụng của giảm giá
                            giamGiaDangApDung.GiamGia.so_luong_da_su_dung -= hoaDonChiTiet.so_luong;
                            await _giamGiaServices.UpdateAsync(giamGiaDangApDung.GiamGia);
                        }
                    }
                    // Cập nhật tổng tiền hóa đơn
                    await CapNhatTongTienVaGiaTriKhuyenMai(hoaDonChiTiet.id_hoa_don);
                    return true;
                });
                if (result)
                {
                    return (true, "Xóa hóa đơn chi tiết thành công");
                }
                return (false, "Không thể xóa hóa đơn chi tiết");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return (false, "Không thể xóa hóa đơn chi tiết");
            }
        }
        //validate và cập nhật hóa đơn chi tiết
        public async Task<(bool success, string message)> CapNhatHoaDonChiTiet(Guid id_hoa_don, Guid id_san_pham_chi_tiet, int so_luong, string ghi_chu)
        {
            try
            {
                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdWithIncludeAsync(id_san_pham_chi_tiet,
                    q => q.Include(spct => spct.SanPham)
                         .Include(spct => spct.MauSac)
                         .Include(spct => spct.KichCo));
                if (sanPhamChiTiet == null)
                    return (false, "Không tìm thấy sản phẩm chi tiết");

                var hoaDonChiTietDaTonTai = await _hoaDonChiTietRepository.GetByConditionAsync(hct =>
                    hct.id_hoa_don == id_hoa_don &&
                    hct.id_san_pham_chi_tiet == id_san_pham_chi_tiet &&
                    hct.trang_thai == "ChoTaiQuay");

                if (!hoaDonChiTietDaTonTai.Any())
                    return await ThemHoaDonChiTiet(id_hoa_don, id_san_pham_chi_tiet, so_luong, ghi_chu);

                return await CapNhatHoaDonChiTietTonTai(id_hoa_don, id_san_pham_chi_tiet, so_luong, ghi_chu, hoaDonChiTietDaTonTai.First(), sanPhamChiTiet);
            }
            catch (Exception ex)
            {
                return (false, $"Đã có lỗi xảy ra: {ex.Message}");
            }
        }

        private async Task<(bool success, string message)> CapNhatHoaDonChiTietTonTai(Guid id_hoa_don, Guid id_san_pham_chi_tiet, int so_luong, string ghi_chu, HoaDonChiTiet hoaDonChiTietUpdate, SanPhamChiTiet sanPhamChiTiet)
        {
            decimal gia_sau_giam_gia = await TinhGiaSauGiamGia(sanPhamChiTiet);

            // Tính số lượng thay đổi
            int soLuongThayDoi = so_luong - hoaDonChiTietUpdate.so_luong;

            // Kiểm tra số lượng trong kho sau khi thay đổi
            if (sanPhamChiTiet.so_luong < soLuongThayDoi)
                return (false, $"Số lượng sản phẩm trong kho không đủ. Còn lại: {sanPhamChiTiet.so_luong}");
            // Cập nhật số lượng giảm giá đã sử dụng
            if (sanPhamChiTiet.SanPhamChiTietGiamGias != null)
            {
                var giamGiaDangApDung = sanPhamChiTiet.SanPhamChiTietGiamGias
                    .FirstOrDefault(gg => gg.GiamGia != null &&
                                        gg.GiamGia.trang_thai == "HoatDong" &&
                                        gg.GiamGia.thoi_gian_bat_dau <= DateTime.Now &&
                                        gg.GiamGia.thoi_gian_ket_thuc >= DateTime.Now);

                if (giamGiaDangApDung != null)
                {
                    // Cập nhật số lượng đã sử dụng của giảm giá
                    giamGiaDangApDung.GiamGia.so_luong_da_su_dung += soLuongThayDoi;
                    var updateGiamGiaResult = await _giamGiaServices.UpdateAsync(giamGiaDangApDung.GiamGia);
                    if (!updateGiamGiaResult)
                        return (false, "Không thể cập nhật số lượng giảm giá đã sử dụng");
                }
            }
            // Cập nhật số lượng sản phẩm chi tiết
            sanPhamChiTiet.so_luong -= soLuongThayDoi;
            var updateSanPhamChiTiet = await _sanPhamChiTietRepository.UpdateAsync(sanPhamChiTiet);
            if (!updateSanPhamChiTiet)
                return (false, "Cập nhật số lượng sản phẩm trong kho thất bại");

            // Cập nhật hóa đơn chi tiết
            hoaDonChiTietUpdate.so_luong = so_luong;
            hoaDonChiTietUpdate.gia_sau_giam_gia = gia_sau_giam_gia;
            hoaDonChiTietUpdate.thanh_tien = hoaDonChiTietUpdate.so_luong * gia_sau_giam_gia;
            hoaDonChiTietUpdate.ghi_chu = ghi_chu;
            var updateHoaDonChiTiet = await _hoaDonChiTietRepository.UpdateAsync(hoaDonChiTietUpdate);
            if (!updateHoaDonChiTiet)
                return (false, "Cập nhật hóa đơn chi tiết thất bại");

            await CapNhatTongTienVaGiaTriKhuyenMai(id_hoa_don);
            return (true, "Cập nhật hóa đơn chi tiết thành công");
        }

        public async Task<(bool success, string message)> ThemHoaDonChiTiet(Guid id_hoa_don, Guid id_san_pham_chi_tiet, int so_luong, string ghi_chu)
        {
            var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdWithIncludeAsync(id_san_pham_chi_tiet,
                q => q.Include(spct => spct.SanPham)
                     .Include(spct => spct.MauSac)
                     .Include(spct => spct.KichCo));
            var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(id_hoa_don, q => q.Include(hd => hd.HoaDonChiTiets));
            if (hoaDon == null)
                return (false, "Hóa đơn không tồn tại");
            if (hoaDon.trang_thai_hoa_don != "ChoTaiQuay")
                return (false, "Hóa đơn không đang ở trạng thái chờ xử lý");
            if (hoaDon.HoaDonChiTiets.Any(hct => hct.id_san_pham_chi_tiet == id_san_pham_chi_tiet))
            {
                var hoaDonChiTiet = hoaDon.HoaDonChiTiets.FirstOrDefault(hct => hct.id_san_pham_chi_tiet == id_san_pham_chi_tiet);

                // Kiểm tra số lượng trong kho sau khi cộng dồn
                if (sanPhamChiTiet.so_luong < so_luong)
                    return (false, $"Số lượng sản phẩm trong kho không đủ. Còn lại: {sanPhamChiTiet.so_luong}");

                // Cập nhật số lượng tồn kho
                sanPhamChiTiet.so_luong -= so_luong;
                var updateSanPhamChiTiet1 = await _sanPhamChiTietRepository.UpdateAsync(sanPhamChiTiet);
                if (!updateSanPhamChiTiet1)
                    return (false, "Cập nhật số lượng sản phẩm trong kho thất bại");

                // Cập nhật số lượng và thành tiền trong hóa đơn chi tiết
                hoaDonChiTiet.so_luong += so_luong;
                decimal gia_sau_giam_gia1 = await TinhGiaSauGiamGia(sanPhamChiTiet);
                hoaDonChiTiet.thanh_tien = hoaDonChiTiet.so_luong * gia_sau_giam_gia1;
                hoaDonChiTiet.don_gia = sanPhamChiTiet.gia_ban;
                hoaDonChiTiet.ghi_chu = ghi_chu;
                hoaDonChiTiet.gia_sau_giam_gia = gia_sau_giam_gia1;
                var updateHoaDonChiTiet = await _hoaDonChiTietRepository.UpdateAsync(hoaDonChiTiet);
                if (!updateHoaDonChiTiet)
                    return (false, "Cập nhật hóa đơn chi tiết thất bại");

                // Cập nhật tổng tiền hóa đơn
                await CapNhatTongTienVaGiaTriKhuyenMai(id_hoa_don);
                return (true, "Cập nhật số lượng hóa đơn chi tiết thành công");
            }
            // Kiểm tra số lượng trong kho
            if (sanPhamChiTiet.so_luong < so_luong)
                return (false, $"Số lượng sản phẩm trong kho không đủ. Còn lại: {sanPhamChiTiet.so_luong}");

            decimal gia_sau_giam_gia = await TinhGiaSauGiamGia(sanPhamChiTiet);

            // Cập nhật số lượng sản phẩm chi tiết
            sanPhamChiTiet.so_luong -= so_luong;
            var updateSanPhamChiTiet = await _sanPhamChiTietRepository.UpdateAsync(sanPhamChiTiet);
            if (!updateSanPhamChiTiet)
                return (false, "Cập nhật số lượng sản phẩm trong kho thất bại");

            var newHoaDonChiTiet = new HoaDonChiTiet
            {
                id_hoa_don_chi_tiet = Guid.NewGuid(),
                ma_hoa_don_chi_tiet = await TaoMaHoaDonChiTiet(id_hoa_don),
                id_hoa_don = id_hoa_don,
                id_san_pham_chi_tiet = id_san_pham_chi_tiet,
                ten_san_pham = sanPhamChiTiet.SanPham?.ten_san_pham,
                ten_mau_sac = sanPhamChiTiet.MauSac?.ten_mau_sac,
                ten_kich_co = sanPhamChiTiet.KichCo?.ten_kich_co,
                so_luong = so_luong,
                don_gia = sanPhamChiTiet.gia_ban,
                gia_sau_giam_gia = gia_sau_giam_gia,
                thanh_tien = so_luong * gia_sau_giam_gia,
                gia_tri_khuyen_mai_cua_hoa_don_cho_hdct = 0,
                ghi_chu = ghi_chu,
                trang_thai = "ChoTaiQuay",
            };

            var createResult = await _hoaDonChiTietRepository.CreateAsync(newHoaDonChiTiet);
            if (!createResult)
                return (false, "Thêm hóa đơn chi tiết thất bại");

            // Hoàn lại số lượng giảm giá đã sử dụng
            var hoaDonChiTiets = await _hoaDonChiTietRepository.GetByConditionWithIncludeAsync(
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
                        giamGiaDangApDung.GiamGia.so_luong_da_su_dung += chiTiet.so_luong;
                        await _giamGiaServices.UpdateAsync(giamGiaDangApDung.GiamGia);
                    }
                }
            }

            await CapNhatTongTienVaGiaTriKhuyenMai(id_hoa_don);
            return (true, "Thêm hóa đơn chi tiết thành công");
        }

        private async Task<decimal> TinhGiaSauGiamGia(SanPhamChiTiet sanPhamChiTiet, bool includeGiamGia = false)
        {
            try
            {
                SanPhamChiTiet spct;
                if (includeGiamGia)
                {
                    // Lấy lại sản phẩm chi tiết với include giảm giá
                    spct = await _sanPhamChiTietRepository.GetByIdWithIncludeAsync(
                        sanPhamChiTiet.id_san_pham_chi_tiet,
                        q => q.Include(spct => spct.SanPhamChiTietGiamGias)
                             .ThenInclude(spctgg => spctgg.GiamGia));

                    if (spct == null)
                        return sanPhamChiTiet.gia_ban;
                }
                else
                {
                    spct = sanPhamChiTiet;
                }

                decimal giaSauGiamGia = spct.gia_ban;

                // Lấy giảm giá đang hoạt động
                var now = DateTime.Now;
                var giamGiaHienTai = spct.SanPhamChiTietGiamGias?
                    .Where(spctgg => spctgg.GiamGia.trang_thai == "HoatDong" &&
                           spctgg.GiamGia.thoi_gian_bat_dau <= now &&
                           spctgg.GiamGia.thoi_gian_ket_thuc >= now)
                    .Select(spctgg => spctgg.GiamGia)
                    .FirstOrDefault();

                if (giamGiaHienTai != null)
                {
                    if (giamGiaHienTai.kieu_giam_gia == "PhanTram")
                    {
                        giaSauGiamGia = giaSauGiamGia - (giaSauGiamGia * giamGiaHienTai.gia_tri_giam / 100);
                    }
                    else if (giamGiaHienTai.kieu_giam_gia == "SoTien")
                    {
                        giaSauGiamGia = giaSauGiamGia - giamGiaHienTai.gia_tri_giam;
                    }
                }

                return Math.Max(0, giaSauGiamGia);
            }
            catch (Exception)
            {
                return sanPhamChiTiet.gia_ban;
            }
        }

        private async Task<decimal> TinhTongTienSauGiamGiaSanPham(Guid id_san_pham_chi_tiet)
        {
            try
            {
                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdAsync(id_san_pham_chi_tiet);
                if (sanPhamChiTiet == null)
                    return 0;

                return await TinhGiaSauGiamGia(sanPhamChiTiet, true);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        //tính tổng tiền đơn hàng chi tiết
        public async Task<(decimal tongTienSauKhuyenMai, decimal giaTriKhuyenMai)> CapNhatTongTienVaGiaTriKhuyenMai(Guid id_hoa_don)
        {
            var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(id_hoa_don, q => q.Include(hd => hd.HoaDonChiTiets));
            var tongTienDonHang = await TinhTongTienDonHang(id_hoa_don);
            if (hoaDon.trang_thai_hoa_don != "ChoTaiQuay" && hoaDon.trang_thai_hoa_don != "ChuaThanhToan")
            {
                return (hoaDon.tong_tien_don_hang - hoaDon.so_tien_khuyen_mai ?? 0, hoaDon.so_tien_khuyen_mai ?? 0);
            }
            decimal giaTriKhuyenMai = 0;
            decimal tongTienSauKhuyenMai = tongTienDonHang + (hoaDon.phi_van_chuyen ?? 0);

            if (hoaDon.id_khuyen_mai == null)
            {
                hoaDon.tong_tien_don_hang = tongTienDonHang;
                hoaDon.so_tien_khuyen_mai = giaTriKhuyenMai;
                hoaDon.tong_tien_phai_thanh_toan = tongTienSauKhuyenMai;
                await _hoaDonRepository.UpdateAsync(hoaDon);
                return (Math.Max(0, tongTienSauKhuyenMai), giaTriKhuyenMai);
            }

            var khuyenMai = await _khuyenMaiRepository.GetByIdAsync(hoaDon.id_khuyen_mai.Value);
            if (khuyenMai == null)
            {
                hoaDon.tong_tien_don_hang = tongTienDonHang;

                hoaDon.so_tien_khuyen_mai = giaTriKhuyenMai;
                hoaDon.tong_tien_phai_thanh_toan = tongTienSauKhuyenMai;
                await _hoaDonRepository.UpdateAsync(hoaDon);
                return (Math.Max(0, tongTienSauKhuyenMai), giaTriKhuyenMai);
            }
            if (khuyenMai.kieu_khuyen_mai == "PhanTram")
            {
                giaTriKhuyenMai = tongTienDonHang * khuyenMai.gia_tri_giam / 100;
                giaTriKhuyenMai = Math.Min(giaTriKhuyenMai, khuyenMai.gia_tri_giam_toi_da);
                tongTienSauKhuyenMai = tongTienDonHang - giaTriKhuyenMai + (hoaDon.phi_van_chuyen ?? 0);
            }
            else if (khuyenMai.kieu_khuyen_mai == "TienMat")
            {
                giaTriKhuyenMai = khuyenMai.gia_tri_giam;
                tongTienSauKhuyenMai = tongTienDonHang - giaTriKhuyenMai + (hoaDon.phi_van_chuyen ?? 0);

            }
            if (tongTienSauKhuyenMai < 0)
            {
                tongTienSauKhuyenMai = 0;
            }
            if (giaTriKhuyenMai < 0)
            {
                giaTriKhuyenMai = 0;
            }
            hoaDon.tong_tien_don_hang = tongTienDonHang;
            hoaDon.so_tien_khuyen_mai = giaTriKhuyenMai;
            hoaDon.tong_tien_phai_thanh_toan = tongTienSauKhuyenMai;
            await _hoaDonRepository.UpdateAsync(hoaDon);
            return (Math.Max(0, tongTienSauKhuyenMai), giaTriKhuyenMai);
        }
        //tạo mã hóa đơn
        private async Task<string> TaoMaHoaDon()
        {
            while (true)
            {
                var random = new Random();
                var maHoaDon = "HD" + random.Next(10000000, 99999999);

                // Kiểm tra xem mã đã tồn tại chưa
                var hoaDonTonTai = await _hoaDonRepository.GetByConditionAsync(hd => hd.ma_hoa_don == maHoaDon);

                if (hoaDonTonTai.Count == 0)
                {
                    return maHoaDon;
                }
            }
        }
        private async Task<string> TaoMaHoaDonChiTiet(Guid id_hoa_don)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var randomPart = new Random().Next(1000, 9999).ToString();
            return $"HDCT-{timestamp}-{randomPart}";
        }
        public async Task<HoaDonAdminDTO> GetHoaDonBanTaiQuayByIdAsync(Guid id_hoa_don, Guid id_nhan_vien_xu_ly)
        {
            await CapNhatTongTienVaGiaTriKhuyenMai(id_hoa_don);
            var result = await _hoaDonRepository.GetByIdWithIncludeAsync(id_hoa_don, q => q
                .Include(hd => hd.KhachHang)
                .Include(hd => hd.NhanVienXuLy)
                .Include(hd => hd.KhuyenMai)
                .Include(hd => hd.PhuongThucThanhToan)
                .Include(hd => hd.HoaDonChiTiets)
                .ThenInclude(hct => hct.SanPhamChiTiet)
                .ThenInclude(spct => spct.SanPham)
                .Include(hd => hd.HoaDonChiTiets)
                .ThenInclude(hct => hct.SanPhamChiTiet)
                .ThenInclude(spct => spct.MauSac)
                .Include(hd => hd.HoaDonChiTiets)
                .ThenInclude(hct => hct.SanPhamChiTiet)
                .ThenInclude(spct => spct.KichCo)
                .Include(hd => hd.HoaDonChiTiets)
                .ThenInclude(hct => hct.SanPhamChiTiet)
                .ThenInclude(spct => spct.SanPhamChiTietGiamGias)
                .ThenInclude(spctgg => spctgg.GiamGia)
                .Include(hd => hd.HoaDonChiTiets)
                .ThenInclude(hct => hct.SanPhamChiTiet)
                .ThenInclude(spct => spct.KichCo)
                .Include(hd => hd.HoaDonChiTiets)
                .ThenInclude(hct => hct.SanPhamChiTiet)
                .ThenInclude(spct => spct.HinhAnhSanPhamChiTiets)
                .ThenInclude(ha => ha.HinhAnhs));

            if (result == null ||
                result.loai_hoa_don != "TaiQuay" ||
                result.trang_thai_hoa_don != "ChoTaiQuay" ||
                result.id_nhan_vien_xu_ly != id_nhan_vien_xu_ly)
            {
                return null;
            }

            return new HoaDonAdminDTO
            {
                id_hoa_don = result.id_hoa_don,
                ma_hoa_don = result.ma_hoa_don,
                id_khach_hang = result.id_khach_hang,
                ten_khach_hang = result.KhachHang?.ten_khach_hang ?? "Khách lẻ",
                ten_nguoi_xu_ly = result.NhanVienXuLy?.ten_nhan_vien,
                sdt_khach_hang = result.KhachHang?.so_dien_thoai,
                dia_chi_nhan_hang = result.dia_chi_nhan_hang,
                ghi_chu = result.ghi_chu,
                loai_hoa_don = result.loai_hoa_don,
                so_tien_khach_tra = result.so_tien_khach_tra,
                id_phuong_thuc_thanh_toan = result.id_phuong_thuc_thanh_toan?.ToString(),
                so_tien_thua_tra_khach = result.so_tien_thua_tra_khach,
                tong_tien_don_hang = result.tong_tien_don_hang ?? 0,
                so_tien_khuyen_mai = result.so_tien_khuyen_mai,
                tong_tien_phai_thanh_toan = result.tong_tien_phai_thanh_toan ?? 0,
                trang_thai = result.trang_thai_hoa_don,
                ten_phuong_thuc_thanh_toan = result.PhuongThucThanhToan?.ten_phuong_thuc_thanh_toan,
                ngay_tao = result.ngay_tao,

                nhanVienXuLy = result.NhanVienXuLy == null ? null : new NhanVien_HoaDonAdminDTO
                {
                    id_nhan_vien = result.NhanVienXuLy.id_nhan_vien,
                    ma_nhan_vien = result.NhanVienXuLy.ma_nhan_vien,
                    ten_nhan_vien = result.NhanVienXuLy.ten_nhan_vien
                },
                khachHang = result.KhachHang != null ? new KhachHang_HoaDonAdminDTO
                {
                    id_khach_hang = result.KhachHang.id_khach_hang,
                    ma_khach_hang = result.KhachHang.ma_khach_hang,
                    ten_khach_hang = result.KhachHang.ten_khach_hang,
                    sdt_khach_hang = result.KhachHang.so_dien_thoai
                } : null,
                khuyenMai = result.KhuyenMai != null ? new KhuyenMai_HoaDonAdminDTO
                {
                    id_khuyen_mai = result.KhuyenMai.id_khuyen_mai,
                    ten_khuyen_mai = result.KhuyenMai.ten_khuyen_mai,
                    ma_khuyen_mai = result.KhuyenMai.ma_khuyen_mai,
                    loai_khuyen_mai = result.KhuyenMai.kieu_khuyen_mai,
                    gia_tri_khuyen_mai = result.KhuyenMai.gia_tri_giam,
                    gia_tri_giam_toi_da = result.KhuyenMai.gia_tri_giam_toi_da
                } : null,
                hoaDonChiTiets = result.HoaDonChiTiets == null ? null : (result.HoaDonChiTiets.Select(hct =>
                {
                    var giaSauGiam = TinhGiaSauGiamGia(hct.SanPhamChiTiet, true).Result;
                    return new HoaDonChiTietAdminDTO
                    {
                        id_hoa_don_chi_tiet = hct.id_hoa_don_chi_tiet,
                        ma_hoa_don_chi_tiet = hct.ma_hoa_don_chi_tiet,
                        id_san_pham_chi_tiet = hct.id_san_pham_chi_tiet,
                        ma_san_pham_chi_tiet = hct.SanPhamChiTiet.ma_san_pham_chi_tiet,
                        ten_san_pham = hct.SanPhamChiTiet.SanPham.ten_san_pham,
                        ten_mau_sac = hct.SanPhamChiTiet.MauSac.ten_mau_sac,
                        ten_kich_co = hct.SanPhamChiTiet.KichCo.ten_kich_co,
                        so_luong = hct.so_luong,
                        don_gia = hct.don_gia,
                        gia_sau_giam_gia = hct.gia_sau_giam_gia,
                        gia_tri_khuyen_mai_cua_hoa_don_cho_hdct = hct.gia_tri_khuyen_mai_cua_hoa_don_cho_hdct,
                        thanh_tien = hct.thanh_tien,
                        ghi_chu = hct.ghi_chu,
                        trang_thai = hct.trang_thai,
                        url_anh = hct.SanPhamChiTiet.HinhAnhSanPhamChiTiets.FirstOrDefault()?.HinhAnhs?.url ?? hct.SanPhamChiTiet.SanPham.anhMacDinh?.url,
                        gia_sp_dang_ban = hct.SanPhamChiTiet.gia_ban,
                        gia_sau_giam_sp_dang_ban = giaSauGiam,
                        sanPhamChiTiet = new SanPhamChiTiet_HoaDonChiTietAdminDTO
                        {
                            id_san_pham_chi_tiet = hct.SanPhamChiTiet.id_san_pham_chi_tiet,
                            ma_san_pham_chi_tiet = hct.SanPhamChiTiet.ma_san_pham_chi_tiet,
                            ten_san_pham = hct.SanPhamChiTiet.SanPham.ten_san_pham,
                            ten_mau_sac = hct.SanPhamChiTiet.MauSac.ten_mau_sac,
                            ten_kich_co = hct.SanPhamChiTiet.KichCo.ten_kich_co,
                            url_anh_san_pham_chi_tiet = hct.SanPhamChiTiet.HinhAnhSanPhamChiTiets.FirstOrDefault()?.HinhAnhs?.url ?? hct.SanPhamChiTiet.SanPham.anhMacDinh?.url
                        }
                    };
                })).ToList()
            };
        }
        private async Task<decimal> TinhTongTienDonHang(Guid id_hoa_don)
        {
            var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(id_hoa_don, q => q.Include(hd => hd.HoaDonChiTiets));
            decimal tongTien = 0;
            if (hoaDon.trang_thai_hoa_don == "ChoTaiQuay" || hoaDon.trang_thai_hoa_don == "ChuaThanhToan")
            {
                foreach (var hdct in hoaDon.HoaDonChiTiets)
                {
                    var spct = await _sanPhamChiTietRepository.GetByIdAsync(hdct.id_san_pham_chi_tiet);
                    hdct.don_gia = spct.gia_ban;
                    hdct.gia_sau_giam_gia = await TinhTongTienSauGiamGiaSanPham(hdct.id_san_pham_chi_tiet);
                    hdct.thanh_tien = hdct.gia_sau_giam_gia * hdct.so_luong;
                    await _hoaDonChiTietRepository.UpdateAsync(hdct);
                    tongTien += hdct.thanh_tien;
                }
            }
            else
            {
                tongTien = hoaDon.tong_tien_don_hang ?? 0;
            }
            return tongTien;
        }
        private async Task<List<HoaDonChiTietAdminDTO>> MapHoaDonChiTietsAsync(IEnumerable<HoaDonChiTiet> chiTiets)
        {
            var chiTietIds = chiTiets.Select(ct => ct.id_hoa_don_chi_tiet).ToList();
            var chiTietDetails = await _hoaDonChiTietRepository.GetByConditionWithIncludeAsync(
                ct => chiTietIds.Contains(ct.id_hoa_don_chi_tiet),
                q => q.Include(ct => ct.SanPhamChiTiet)
                      .ThenInclude(spct => spct.SanPham)
                      .Include(ct => ct.SanPhamChiTiet)
                      .ThenInclude(spct => spct.MauSac)
                      .Include(ct => ct.SanPhamChiTiet)
                      .ThenInclude(spct => spct.KichCo)
                      .Include(ct => ct.SanPhamChiTiet)
                      .ThenInclude(spct => spct.SanPhamChiTietGiamGias)
                      .ThenInclude(spctgg => spctgg.GiamGia)
                      .Include(ct => ct.SanPhamChiTiet)
                      .ThenInclude(spct => spct.HinhAnhSanPhamChiTiets)
                      .ThenInclude(ha => ha.HinhAnhs));

            var result = new List<HoaDonChiTietAdminDTO>();
            foreach (var ct in chiTietDetails)
            {
                var giaSauGiam = TinhGiaSauGiamGia(ct.SanPhamChiTiet, true).Result;
                result.Add(new HoaDonChiTietAdminDTO
                {
                    id_hoa_don_chi_tiet = ct.id_hoa_don_chi_tiet,
                    ma_hoa_don_chi_tiet = ct.ma_hoa_don_chi_tiet,
                    id_san_pham_chi_tiet = ct.id_san_pham_chi_tiet,
                    ma_san_pham_chi_tiet = ct.SanPhamChiTiet.ma_san_pham_chi_tiet,
                    ten_san_pham = ct.SanPhamChiTiet.SanPham.ten_san_pham,
                    ten_mau_sac = ct.SanPhamChiTiet.MauSac.ten_mau_sac,
                    ten_kich_co = ct.SanPhamChiTiet.KichCo.ten_kich_co,
                    so_luong = ct.so_luong,
                    don_gia = ct.don_gia,                    // Giá gốc lúc mua
                    gia_sau_giam_gia = ct.gia_sau_giam_gia,  // Giá sau giảm giá lúc mua
                    gia_tri_khuyen_mai_cua_hoa_don_cho_hdct = ct.gia_tri_khuyen_mai_cua_hoa_don_cho_hdct,
                    thanh_tien = ct.thanh_tien,              // Thành tiền lúc mua
                    ghi_chu = ct.ghi_chu,
                    trang_thai = ct.trang_thai,
                    url_anh = ct.SanPhamChiTiet.HinhAnhSanPhamChiTiets.FirstOrDefault()?.HinhAnhs?.url ?? ct.SanPhamChiTiet.SanPham.anhMacDinh?.url,
                    gia_sp_dang_ban = ct.SanPhamChiTiet.gia_ban,                    // Sử dụng giá hiện tại
                    gia_sau_giam_sp_dang_ban = giaSauGiam,      // Sử dụng giá sau giảm giá hiện tại
                    sanPhamChiTiet = new SanPhamChiTiet_HoaDonChiTietAdminDTO
                    {
                        id_san_pham_chi_tiet = ct.SanPhamChiTiet.id_san_pham_chi_tiet,
                        ma_san_pham_chi_tiet = ct.SanPhamChiTiet.ma_san_pham_chi_tiet,
                        ten_san_pham = ct.SanPhamChiTiet.SanPham.ten_san_pham,
                        ten_mau_sac = ct.SanPhamChiTiet.MauSac.ten_mau_sac,
                        ten_kich_co = ct.SanPhamChiTiet.KichCo.ten_kich_co,
                        url_anh_san_pham_chi_tiet = ct.SanPhamChiTiet.HinhAnhSanPhamChiTiets.FirstOrDefault()?.HinhAnhs?.url ?? ct.SanPhamChiTiet.SanPham.anhMacDinh?.url
                    }
                });
            }
            return result;
        }
        public async Task<(bool success, string message)> ThanhToanHoaDonChoTaiQuay(Guid id_hoa_don)
        {
            var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(id_hoa_don, q => q.Include(hd => hd.HoaDonChiTiets));
            if (hoaDon == null)
                return (false, "Hóa đơn không tồn tại");
            if (hoaDon.trang_thai_hoa_don != "ChoTaiQuay")
                return (false, "Hóa đơn không đang ở trạng thái chờ tại quầy");
            if (hoaDon.tong_tien_phai_thanh_toan < 0)
                return (false, "Tổng tiền phải thanh toán không hợp lệ");
            if (hoaDon.so_tien_khach_tra < hoaDon.tong_tien_phai_thanh_toan)
                return (false, "Số tiền khách trả không đủ để thanh toán");
            if (hoaDon.id_phuong_thuc_thanh_toan == null)
                return (false, "Phương thức thanh toán không tồn tại");
            var cuaHang = await _cuaHangRepository.GetFirstOrDefaultAsync(x => x.id_cua_hang != Guid.Empty);
            if (cuaHang == null)
                return (false, "Cửa hàng không tồn tại");
            hoaDon.trang_thai_hoa_don = "DaThanhToan";
            hoaDon.id_cua_hang = cuaHang.id_cua_hang;
            foreach (var hct in hoaDon.HoaDonChiTiets)
            {
                hct.trang_thai = "DaThanhToan";
                hct.gia_tri_khuyen_mai_cua_hoa_don_cho_hdct = hoaDon.so_tien_khuyen_mai.HasValue ? hoaDon.so_tien_khuyen_mai.Value / hoaDon.HoaDonChiTiets.Count : 0;
                var updateHoaDonChiTiet = await _hoaDonChiTietRepository.UpdateAsync(hct);
                if (!updateHoaDonChiTiet)
                    return (false, "Cập nhật trạng thái hóa đơn chi tiết thất bại");

            }
            var updateHoaDon = await _hoaDonRepository.UpdateAsync(hoaDon);
            if (!updateHoaDon)
                return (false, "Cập nhật trạng thái hóa đơn thất bại");
            return (true, "Thanh toán hóa đơn thành công");
        }
        public async Task<(bool success, string message)> CapNhatHoaDonOnline(
            Guid idHoaDon,
            string? IddiaChiNhanHang,
            string? ghiChu,
            string? idKhuyenMai,
            string? idPhuongThucThanhToan,
            decimal phi_van_chuyen)
        {
            try
            {
                var success = await _hoaDonRepository.ExecuteInTransactionAsync(async () =>
                {
                    var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(idHoaDon,
                        q => q.Include(hd => hd.KhuyenMai)
                             .Include(hd => hd.HoaDonChiTiets)
                             .ThenInclude(hct => hct.SanPhamChiTiet)
                             .ThenInclude(spct => spct.SanPham)
                             .Include(hd => hd.HoaDonChiTiets)
                             .ThenInclude(hct => hct.SanPhamChiTiet)
                             .ThenInclude(spct => spct.MauSac)
                             .Include(hd => hd.HoaDonChiTiets)
                             .ThenInclude(hct => hct.SanPhamChiTiet)
                             .ThenInclude(spct => spct.KichCo));

                    if (hoaDon == null)
                        return false;

                    if (hoaDon.trang_thai_hoa_don != "ChuaThanhToan")
                        return false;

                    // Kiểm tra số lượng sản phẩm
                    foreach (var hoaDonChiTiet in hoaDon.HoaDonChiTiets)
                    {
                        var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdAsync(hoaDonChiTiet.id_san_pham_chi_tiet);
                        if (sanPhamChiTiet == null || sanPhamChiTiet.so_luong < hoaDonChiTiet.so_luong)
                            return false;
                    }

                    // Cập nhật địa chỉ nhận hàng
                    if (!string.IsNullOrEmpty(IddiaChiNhanHang))
                    {
                        var diaChiNhanHang = await _diaChiRepository.GetFirstOrDefaultAsync(x =>
                            x.id_dia_chi == Guid.Parse(IddiaChiNhanHang) &&
                            x.id_khach_hang == hoaDon.id_khach_hang);

                        if (diaChiNhanHang == null)
                            return false;

                        hoaDon.dia_chi_nhan_hang = $"{diaChiNhanHang.tinh}, {diaChiNhanHang.huyen}, {diaChiNhanHang.xa}, {diaChiNhanHang.dia_chi_cu_the}";
                        hoaDon.sdt_khach_hang = diaChiNhanHang.so_dien_thoai;
                        hoaDon.ten_khach_hang = diaChiNhanHang.ten_nguoi_nhan;
                    }

                    // Cập nhật phương thức thanh toán
                    if (!string.IsNullOrEmpty(idPhuongThucThanhToan))
                    {
                        var phuongThucThanhToan = await _phuongThucThanhToanRepository.GetByIdAsync(Guid.Parse(idPhuongThucThanhToan));
                        if (phuongThucThanhToan == null)
                            return false;

                        hoaDon.id_phuong_thuc_thanh_toan = phuongThucThanhToan.id_phuong_thuc_thanh_toan;
                    }

                    // Xử lý khuyến mãi
                    if (!string.IsNullOrEmpty(idKhuyenMai))
                    {
                        var khuyenMai = await _khuyenMaiRepository.GetByIdAsync(Guid.Parse(idKhuyenMai));
                        if (khuyenMai == null ||
                            khuyenMai.trang_thai != "HoatDong" ||
                            khuyenMai.thoi_gian_bat_dau > DateTime.Now ||
                            khuyenMai.thoi_gian_ket_thuc < DateTime.Now ||
                            khuyenMai.so_luong_da_su_dung >= khuyenMai.so_luong_toi_da ||
                            hoaDon.tong_tien_don_hang < khuyenMai.gia_tri_don_hang_toi_thieu)
                            return false;

                        // Nếu đã có khuyến mãi cũ, giảm số lượng sử dụng
                        if (hoaDon.KhuyenMai != null)
                        {
                            var khuyenMaiCu = await _khuyenMaiRepository.GetByIdAsync(hoaDon.KhuyenMai.id_khuyen_mai);
                            if (khuyenMaiCu != null)
                            {
                                khuyenMaiCu.so_luong_da_su_dung = Math.Max(0, khuyenMaiCu.so_luong_da_su_dung - 1);
                                if (!await _khuyenMaiRepository.UpdateAsync(khuyenMaiCu))
                                    return false;
                            }
                        }

                        // Cập nhật khuyến mãi mới
                        hoaDon.id_khuyen_mai = khuyenMai.id_khuyen_mai;
                        khuyenMai.so_luong_da_su_dung++;
                        if (!await _khuyenMaiRepository.UpdateAsync(khuyenMai))
                            return false;
                    }
                    else if (hoaDon.id_khuyen_mai.HasValue)
                    {
                        // Nếu xóa khuyến mãi, giảm số lượng sử dụng của khuyến mãi cũ
                        var khuyenMaiCu = await _khuyenMaiRepository.GetByIdAsync(hoaDon.id_khuyen_mai.Value);
                        if (khuyenMaiCu != null)
                        {
                            khuyenMaiCu.so_luong_da_su_dung = Math.Max(0, khuyenMaiCu.so_luong_da_su_dung - 1);
                            if (!await _khuyenMaiRepository.UpdateAsync(khuyenMaiCu))
                                return false;
                        }
                        hoaDon.id_khuyen_mai = null;
                        hoaDon.so_tien_khuyen_mai = 0;
                    }

                    // Cập nhật thông tin khác
                    hoaDon.ghi_chu = ghiChu;
                    hoaDon.phi_van_chuyen = phi_van_chuyen;
                    hoaDon.ngay_sua = DateTime.Now;

                    // Lưu thay đổi
                    if (!await _hoaDonRepository.UpdateAsync(hoaDon))
                        return false;

                    // Cập nhật tổng tiền và giá trị khuyến mãi
                    var (tongTienSauKhuyenMai, giaTriKhuyenMai) = await CapNhatTongTienVaGiaTriKhuyenMai(idHoaDon);

                    return true;
                });

                return success
                    ? (true, "Cập nhật hóa đơn thành công")
                    : (false, "Cập nhật hóa đơn thất bại");
            }
            catch (Exception ex)
            {
                // Log the error here
                return (false, "Đã xảy ra lỗi trong quá trình xử lý");
            }
        }
        public async Task<(bool success, string message)> XoaHoaDonChuaThanhToanQuaHan()
        {
            try
            {
                // Get all orders in ChuaThanhToan status
                var hoaDonChuaThanhToan = await _hoaDonRepository.GetByConditionWithIncludeAsync(
                    hd => hd.trang_thai_hoa_don == "ChuaThanhToan",
                    q => q.Include(hd => hd.HoaDonChiTiets)
                         .ThenInclude(hct => hct.SanPhamChiTiet)
                         .Include(hd => hd.KhuyenMai));

                var hoaDonQuaHan = hoaDonChuaThanhToan
                    .Where(x => (DateTime.Now - x.ngay_tao).TotalHours >= 1)
                    .ToList();

                if (!hoaDonQuaHan.Any())
                    return (true, "Không có hóa đơn quá hạn cần xử lý");

                foreach (var hoaDon in hoaDonQuaHan)
                {
                    // Decrease promotion usage count if applicable
                    if (hoaDon.KhuyenMai != null)
                    {
                        hoaDon.KhuyenMai.so_luong_da_su_dung = Math.Max(0, hoaDon.KhuyenMai.so_luong_da_su_dung - 1);
                        await _khuyenMaiRepository.UpdateAsync(hoaDon.KhuyenMai);
                    }
                    foreach (var item in hoaDon.HoaDonChiTiets)
                    {
                        await _hoaDonChiTietRepository.DeleteAsync(item.id_hoa_don_chi_tiet);
                    }

                    // Delete order
                    await _hoaDonRepository.DeleteAsync(hoaDon.id_hoa_don);
                }

                return (true, $"Đã xóa {hoaDonQuaHan.Count} hóa đơn quá hạn");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xử lý hóa đơn quá hạn: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> XacNhanDonHangAsync(Guid idHoaDon, Guid id_nhan_vien_xu_ly)
        {
            try
            {
                // Lấy thông tin đơn hàng kèm theo chi tiết và thông tin khách hàng
                var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(idHoaDon,
                    q => q.Include(hd => hd.HoaDonChiTiets)
                          .ThenInclude(hct => hct.SanPhamChiTiet).ThenInclude(spct => spct.SanPhamChiTietGiamGias).ThenInclude(spgg => spgg.GiamGia)
                          .Include(hd => hd.KhachHang));

                if (hoaDon == null)
                    return (false, "Không tìm thấy đơn hàng");

                // Kiểm tra trạng thái hiện tại
                if (hoaDon.trang_thai_hoa_don != "DangChoXuLy" && hoaDon.trang_thai_hoa_don != "HetHang")
                    return (false, "Đơn hàng không ở trạng thái chờ xử lý hoặc hết hàng");

                // Kiểm tra số lượng tồn kho
                foreach (var chiTiet in hoaDon.HoaDonChiTiets)
                {
                    if (chiTiet.SanPhamChiTiet.so_luong < chiTiet.so_luong)
                    {
                        return (false, $"Sản phẩm {chiTiet.ten_san_pham} - {chiTiet.ten_mau_sac} - {chiTiet.ten_kich_co} không đủ số lượng trong kho");
                    }
                }

                // Thực hiện trong transaction để đảm bảo tính nhất quán
                var success = await _hoaDonRepository.ExecuteInTransactionAsync(async () =>
                {
                    // Cập nhật trạng thái đơn hàng
                    hoaDon.trang_thai_hoa_don = "DaXacNhan";
                    hoaDon.ngay_sua = DateTime.Now;
                    hoaDon.id_nhan_vien_xu_ly = id_nhan_vien_xu_ly;

                    // Cập nhật trạng thái các chi tiết đơn hàng
                    foreach (var chiTiet in hoaDon.HoaDonChiTiets)
                    {
                        chiTiet.trang_thai = "DaXacNhan";
                        chiTiet.id_nhan_vien_xu_ly = id_nhan_vien_xu_ly;
                        var updateResult = await _hoaDonChiTietRepository.UpdateAsync(chiTiet);
                        if (!updateResult) return false;

                        // Trừ số lượng tồn kho
                        chiTiet.SanPhamChiTiet.so_luong -= chiTiet.so_luong;
                        var updateSanPhamResult = await _sanPhamChiTietRepository.UpdateAsync(chiTiet.SanPhamChiTiet);
                        if (!updateSanPhamResult) return false;
                        // Kiểm tra và cập nhật số lượng giảm giá
                        if (chiTiet.SanPhamChiTiet.SanPhamChiTietGiamGias != null && chiTiet.SanPhamChiTiet.SanPhamChiTietGiamGias.Any())
                        {
                            // Tìm giảm giá đang được áp dụng cho sản phẩm này
                            var giamGiaDangApDung = chiTiet.SanPhamChiTiet.SanPhamChiTietGiamGias
                                .FirstOrDefault(gg => gg.GiamGia != null &&
                                                    gg.GiamGia.trang_thai == "HoatDong" &&
                                                    gg.GiamGia.thoi_gian_bat_dau <= DateTime.Now &&
                                                    gg.GiamGia.thoi_gian_ket_thuc >= DateTime.Now);

                            if (giamGiaDangApDung != null)
                            {
                                // Cập nhật số lượng đã sử dụng của giảm giá
                                giamGiaDangApDung.GiamGia.so_luong_da_su_dung += chiTiet.so_luong;
                                var updateGiamGiaResult = await _giamGiaServices.UpdateAsync(giamGiaDangApDung.GiamGia);
                                if (!updateGiamGiaResult)
                                    return false;
                            }
                        }
                    }

                    // Cập nhật đơn hàng
                    var updateHoaDonResult = await _hoaDonRepository.UpdateAsync(hoaDon);
                    if (!updateHoaDonResult) return false;

                    // Gửi email thông báo cho khách hàng
                    if (!string.IsNullOrEmpty(hoaDon.KhachHang.email))
                    {
                        await GuiEmailCapNhatTrangThaiAsync(idHoaDon, "DaXacNhan");
                    }

                    return true;
                });

                return success
                    ? (true, "Xác nhận đơn hàng thành công")
                    : (false, "Không thể xác nhận đơn hàng. Vui lòng thử lại sau");
            }
            catch (Exception ex)
            {
                // Log lỗi ở đây
                return (false, $"Đã xảy ra lỗi khi xác nhận đơn hàng: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> GuiEmailCapNhatTrangThaiAsync(Guid idHoaDon, string trangThai)
        {
            try
            {
                var inforCuaHang = await _cuaHangRepository.GetFirstOrDefaultAsync(x => x.id_cua_hang != Guid.Empty);
                var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(idHoaDon,
                    q => q.Include(hd => hd.KhachHang)
                          .Include(hd => hd.HoaDonChiTiets)
                          .ThenInclude(hct => hct.SanPhamChiTiet)
                          .Include(hd => hd.PhuongThucThanhToan));

                if (hoaDon == null || hoaDon.KhachHang == null || string.IsNullOrEmpty(hoaDon.KhachHang.email))
                    return (false, "Không thể gửi email: Không tìm thấy thông tin đơn hàng hoặc email khách hàng");

                var trangThaiText = GetTrangThaiText(trangThai);
                var emailSubject = $"Cập nhật trạng thái đơn hàng {hoaDon.ma_hoa_don}";

                // Tạo nội dung chi tiết sản phẩm
                var chiTietSanPham = string.Join("<br/>", hoaDon.HoaDonChiTiets.Select(ct =>
                    $"- {ct.ten_san_pham} ({ct.ten_mau_sac}, {ct.ten_kich_co}): {ct.so_luong} x {ct.gia_sau_giam_gia:N0} VNĐ = {ct.thanh_tien:N0} VNĐ"
                ));

                var emailTemplate = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            background: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #2c3e50, #3498db);
            color: white;
            padding: 20px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
            font-weight: 600;
        }}
        .content {{
            padding: 30px;
        }}
        .greeting {{
            font-size: 18px;
            color: #2c3e50;
            margin-bottom: 20px;
        }}
        .order-info {{
            background-color: #f8f9fa;
            border-radius: 4px;
            padding: 20px;
            margin: 20px 0;
        }}
        .order-info h3 {{
            color: #2c3e50;
            margin-top: 0;
            border-bottom: 2px solid #3498db;
            padding-bottom: 10px;
        }}
        .info-item {{
            margin: 10px 0;
            display: flex;
            justify-content: space-between;
        }}
        .info-label {{
            color: #666;
            font-weight: 500;
        }}
        .info-value {{
            color: #2c3e50;
            font-weight: 600;
        }}
        .product-list {{
            background-color: #fff;
            border: 1px solid #e9ecef;
            border-radius: 4px;
            padding: 15px;
            margin: 15px 0;
        }}
        .product-item {{
            padding: 12px;
            border-bottom: 1px solid #eee;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }}
        .product-item:last-child {{
            border-bottom: none;
        }}
        .product-details {{
            flex-grow: 1;
        }}
        .product-name {{
            font-weight: 600;
            color: #2c3e50;
        }}
        .product-variant {{
            color: #666;
            font-size: 0.9em;
        }}
        .product-price {{
            text-align: right;
            color: #2c3e50;
        }}
        .price-detail {{
            background-color: #f8f9fa;
            border-radius: 4px;
            padding: 15px;
            margin-top: 20px;
        }}
        .price-row {{
            display: flex;
            justify-content: space-between;
            margin: 8px 0;
            padding: 5px 0;
        }}
        .total-row {{
            border-top: 2px solid #3498db;
            margin-top: 10px;
            padding-top: 10px;
            font-weight: 600;
            font-size: 1.1em;
        }}
        .status-message {{
            background-color: #e8f4f8;
            border-left: 4px solid #3498db;
            padding: 15px;
            margin: 20px 0;
            border-radius: 0 4px 4px 0;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            border-top: 1px solid #eee;
            color: #6c757d;
        }}
        .contact-info {{
            margin-top: 15px;
            font-size: 14px;
        }}
        .contact-item {{
            margin: 5px 0;
            color: #2c3e50;
        }}
        .highlight {{
            color: #e67e22;
            font-weight: 600;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Cập nhật trạng thái đơn hàng {hoaDon.ma_hoa_don}</h1>
        </div>
        
        <div class='content'>
            <div class='greeting'>
                Xin chào <strong>{hoaDon.KhachHang.ten_khach_hang}</strong>,
            </div>

            <div class='status-message'>
                <p>Đơn hàng của bạn đã được cập nhật trạng thái thành: <strong>{trangThaiText}</strong></p>
                <p>{GetTrangThaiMessage(trangThai, inforCuaHang)}</p>
            </div>

            <div class='order-info'>
                <h3>Thông tin đơn hàng</h3>
                <div class='info-item'>
                    <span class='info-label'>Mã đơn hàng:</span>
                    <span class='info-value'>{hoaDon.ma_hoa_don}</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Ngày đặt:</span>
                    <span class='info-value'>{hoaDon.ngay_tao:dd/MM/yyyy HH:mm}</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Phương thức thanh toán:</span>
                    <span class='info-value'>{hoaDon.PhuongThucThanhToan?.ten_phuong_thuc_thanh_toan}</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Địa chỉ nhận hàng:</span>
                    <span class='info-value'>{hoaDon.dia_chi_nhan_hang}</span>
                </div>
            </div>

            <h3 style='color: #2c3e50; margin: 25px 0 15px 0;'>Chi tiết sản phẩm</h3>
            <div class='product-list'>
                {string.Join("", hoaDon.HoaDonChiTiets.Select(ct => $@"
                    <div class='product-item' style='display: flex; justify-content: space-between; align-items: center; gap: 20px;'>
                        <div class='product-details'>
                            <div class='product-name'>{ct.ten_san_pham}</div>
                            <div class='product-variant'>{ct.ten_mau_sac}, {ct.ten_kich_co}</div>
                        </div>
                        <div class='product-price' style='text-align: right; min-width: 200px;'>
                            <div>{ct.so_luong} x {ct.gia_sau_giam_gia:N0} VNĐ</div>
                            <div class='highlight'>{ct.thanh_tien:N0} VNĐ</div>
                        </div>
                    </div>
                "))}
            </div>

            <div class='price-detail'>
                <div class='price-row'>
                    <span>Tổng tiền hàng:</span>
                    <span>{hoaDon.tong_tien_don_hang:N0} VNĐ</span>
                </div>
                <div class='price-row'>
                    <span>Phí vận chuyển:</span>
                    <span>{hoaDon.phi_van_chuyen:N0} VNĐ</span>
                </div>
                {(hoaDon.so_tien_khuyen_mai > 0 ? $@"
                <div class='price-row'>
                    <span>Giảm giá:</span>
                    <span>-{hoaDon.so_tien_khuyen_mai:N0} VNĐ</span>
                </div>
                " : "")}
                <div class='price-row total-row'>
                    <span>Tổng thanh toán:</span>
                    <span>{hoaDon.tong_tien_phai_thanh_toan:N0} VNĐ</span>
                </div>
            </div>
        </div>

        <div class='footer'>
            <p style='margin: 0;'>Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi qua:</p>
            <div class='contact-info'>
                <div class='contact-item'>
                    <strong>Email:</strong> {inforCuaHang.email}
                </div>
                <div class='contact-item'>
                    <strong>Hotline:</strong> {inforCuaHang.sdt}
                </div>
            </div>
            <p style='margin: 15px 0 0 0;'>Trân trọng,<br/><strong>Ban quản trị {inforCuaHang.ten_cua_hang}</strong></p>
        </div>
    </div>
</body>
</html>";

                var result = await _emailService.SendEmailAsync(
                    hoaDon.KhachHang.email,
                    emailSubject,
                    emailTemplate
                );

                return result
                    ? (true, "Gửi email thông báo thành công")
                    : (false, "Không thể gửi email thông báo");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi gửi email: {ex.Message}");
            }
        }

        private string GetTrangThaiText(string trangThai)
        {
            return TrangThaiDonHangHelper.GetTrangThaiText(trangThai);
        }

        private string GetTrangThaiMessage(string trangThai, CuaHang cuaHang)
        {
            return TrangThaiDonHangHelper.GetTrangThaiMessage(trangThai, cuaHang);
        }
        public async Task<(bool success, string message)> CapNhatTrangThaiDonHangAsync(Guid idHoaDon, string trangThai, Guid id_nhan_vien_xu_ly)
        {
            try
            {
                // Validate nhân viên xử lý
                if (!await ValidateNhanVienXuLy(id_nhan_vien_xu_ly))
                {
                    return (false, "Nhân viên xử lý không hợp lệ hoặc không tồn tại");
                }

                // Validate trạng thái
                if (!Enum.TryParse<TrangThaiDonHang>(trangThai, out _))
                {
                    return (false, "Trạng thái không hợp lệ");
                }

                if (!new[] { "DangChuanBi", "DangGiaoHang", "DaNhanHang", "DaHoanThanh" }.Contains(trangThai))
                {
                    return (false, "Trạng thái không hợp lệ. Trạng thái đơn hàng phải là DangChuanBi, DangGiaoHang, DaNhanHang hoặc DaHoanThanh");
                }

                // Lấy thông tin đơn hàng
                var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(idHoaDon,
                    q => q.Include(hd => hd.HoaDonChiTiets)
                          .Include(hd => hd.KhachHang)
                          .Include(hd => hd.PhuongThucThanhToan));

                if (hoaDon == null)
                {
                    return (false, "Không tìm thấy đơn hàng");
                }

                // Kiểm tra tính hợp lệ của luồng trạng thái
                if (!IsValidStatusTransition(hoaDon.trang_thai_hoa_don, trangThai))
                {
                    return (false, $"Không thể chuyển từ trạng thái {hoaDon.trang_thai_hoa_don} sang trạng thái {trangThai}");
                }

                // Kiểm tra điều kiện nghiệp vụ cụ thể cho từng trạng thái
                if (trangThai == "DaHoanThanh")
                {
                    if (hoaDon.PhuongThucThanhToan?.ma_phuong_thuc_thanh_toan == "TienMat" &&
                        (!hoaDon.so_tien_khach_tra.HasValue || hoaDon.so_tien_khach_tra < hoaDon.tong_tien_phai_thanh_toan))
                    {
                        return (false, "Chưa nhập đủ số tiền khách trả cho đơn hàng thanh toán tiền mặt");
                    }
                }

                // Thực hiện cập nhật trong transaction với retry logic
                var success = await RetryOperation(async () =>
                {
                    return await _hoaDonRepository.ExecuteInTransactionAsync(async () =>
                    {
                        // Cập nhật trạng thái đơn hàng
                        hoaDon.trang_thai_hoa_don = trangThai;
                        hoaDon.ngay_sua = DateTime.Now;
                        hoaDon.id_nhan_vien_xu_ly = id_nhan_vien_xu_ly;

                        // Cập nhật trạng thái các chi tiết đơn hàng
                        foreach (var chiTiet in hoaDon.HoaDonChiTiets)
                        {
                            chiTiet.trang_thai = trangThai;
                            chiTiet.ngay_sua = DateTime.Now;
                            chiTiet.id_nhan_vien_xu_ly = id_nhan_vien_xu_ly;

                            var updateChiTietResult = await _hoaDonChiTietRepository.UpdateAsync(chiTiet);
                            if (!updateChiTietResult) return false;
                        }

                        // Cập nhật đơn hàng
                        var updateResult = await _hoaDonRepository.UpdateAsync(hoaDon);
                        if (!updateResult) return false;

                        // Gửi email thông báo cho khách hàng
                        if (hoaDon.KhachHang?.email != null)
                        {
                            try
                            {
                                await GuiEmailCapNhatTrangThaiAsync(idHoaDon, trangThai);
                            }
                            catch (Exception ex)
                            {
                                // Log lỗi nhưng không dừng quy trình
                                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
                            }
                        }

                        return true;
                    });
                });

                if (success)
                {
                    var thongBao = trangThai switch
                    {
                        "DangChuanBi" => "Đơn hàng đang được chuẩn bị",
                        "DangGiaoHang" => "Đơn hàng đang được giao",
                        "DaHoanThanh" => "Đơn hàng đã giao thành công",
                        _ => $"Đã cập nhật trạng thái đơn hàng thành {trangThai}"
                    };
                    return (true, thongBao);
                }

                return (false, "Không thể cập nhật trạng thái đơn hàng. Vui lòng thử lại sau");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi cập nhật trạng thái đơn hàng: {ex.Message}");
                return (false, $"Đã xảy ra lỗi khi cập nhật trạng thái đơn hàng: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> DanhDauHetHangAsync(Guid idHoaDon, Guid id_nhan_vien_xu_ly)
        {
            try
            {
                // Kiểm tra nhân viên xử lý
                if (!await ValidateNhanVienXuLy(id_nhan_vien_xu_ly))
                    return (false, "Nhân viên không tồn tại hoặc không có quyền xử lý");
                var cuaHang = await _cuaHangRepository.GetFirstOrDefaultAsync(c => c.id_cua_hang != Guid.Empty);
                var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(idHoaDon,
                    q => q.Include(hd => hd.HoaDonChiTiets)
                         .ThenInclude(hct => hct.SanPhamChiTiet)
                         .ThenInclude(spct => spct.SanPham)
                         .Include(hd => hd.HoaDonChiTiets)
                         .ThenInclude(hct => hct.SanPhamChiTiet)
                         .ThenInclude(spct => spct.MauSac)
                         .Include(hd => hd.HoaDonChiTiets)
                         .ThenInclude(hct => hct.SanPhamChiTiet)
                         .ThenInclude(spct => spct.KichCo)
                         .Include(hd => hd.KhachHang)
                         .ThenInclude(kh => kh.TaiKhoan)
                         .Include(hd => hd.CuaHang));

                if (hoaDon == null)
                    return (false, "Không tìm thấy hóa đơn");

                if (hoaDon.trang_thai_hoa_don != "DangChoXuLy")
                    return (false, "Hóa đơn không ở trạng thái đang chờ xử lý");

                // Kiểm tra xem đã có sản phẩm nào hết hàng chưa
                var sanPhamHetHang = hoaDon.HoaDonChiTiets
                    .Where(hct => hct.SanPhamChiTiet.so_luong < hct.so_luong)
                    .Select(hct => $"{hct.SanPhamChiTiet.SanPham.ten_san_pham} - {hct.SanPhamChiTiet.MauSac.ten_mau_sac} - {hct.SanPhamChiTiet.KichCo.ten_kich_co}")
                    .ToList();

                if (!sanPhamHetHang.Any())
                    return (false, "Không có sản phẩm nào hết hàng trong đơn");

                // Cập nhật trạng thái hóa đơn và chi tiết
                hoaDon.trang_thai_hoa_don = "HetHang";
                hoaDon.ngay_sua = DateTime.Now;
                hoaDon.id_nhan_vien_xu_ly = id_nhan_vien_xu_ly;

                foreach (var hct in hoaDon.HoaDonChiTiets)
                {
                    hct.trang_thai = "HetHang";
                }

                // Cập nhật hóa đơn
                var updateResult = await _hoaDonRepository.UpdateAsync(hoaDon);
                if (!updateResult)
                    return (false, "Không thể cập nhật trạng thái hóa đơn");

                // Gửi email thông báo cho khách hàng nếu có email
                if (hoaDon.KhachHang?.email != null)
                {
                    try
                    {
                        var emailSubject = $"Cập nhật trạng thái đơn hàng {hoaDon.ma_hoa_don}";
                        var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            background: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #2c3e50, #3498db);
            color: white;
            padding: 20px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
            font-weight: 600;
        }}
        .content {{
            padding: 30px;
        }}
        .greeting {{
            font-size: 18px;
            color: #2c3e50;
            margin-bottom: 20px;
        }}
        .message-box {{
            background-color: #f8f9fa;
            border-left: 4px solid #e74c3c;
            padding: 15px;
            margin: 20px 0;
            border-radius: 0 4px 4px 0;
        }}
        .product-list {{
            background-color: #fff;
            border: 1px solid #e9ecef;
            border-radius: 4px;
            padding: 15px;
            margin: 15px 0;
        }}
        .product-item {{
            padding: 8px 0;
            border-bottom: 1px solid #eee;
        }}
        .product-item:last-child {{
            border-bottom: none;
        }}
        .highlight {{
            color: #e67e22;
            font-weight: 600;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            border-top: 1px solid #eee;
            color: #6c757d;
        }}
        .contact-info {{
            margin-top: 15px;
            font-size: 14px;
        }}
        .button {{
            display: inline-block;
            padding: 10px 20px;
            background-color: #3498db;
            color: white;
            text-decoration: none;
            border-radius: 4px;
            margin-top: 15px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Thông báo về đơn hàng của bạn</h1>
        </div>
        
        <div class='content'>
            <div class='greeting'>
                Xin chào <strong>{hoaDon.KhachHang.ten_khach_hang}</strong>,
            </div>

            <p>Chúng tôi rất tiếc phải thông báo rằng một số sản phẩm trong đơn hàng của bạn hiện đang tạm thời hết hàng hoặc chưa đủ số lượng.</p>

            <div class='order-info'>
                <h3>Thông tin đơn hàng</h3>
                <div class='info-item'>
                    <span class='info-label'>Mã đơn hàng:</span>
                    <span class='info-value'>{hoaDon.ma_hoa_don}</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Ngày đặt:</span>
                    <span class='info-value'>{hoaDon.ngay_tao:dd/MM/yyyy HH:mm}</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Phương thức thanh toán:</span>
                    <span class='info-value'>{hoaDon.PhuongThucThanhToan?.ten_phuong_thuc_thanh_toan}</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Địa chỉ nhận hàng:</span>
                    <span class='info-value'>{hoaDon.dia_chi_nhan_hang}</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Tổng tiền đơn hàng:</span>
                    <span class='info-value'>{hoaDon.tong_tien_don_hang:N0} VNĐ</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Phí vận chuyển:</span>
                    <span class='info-value'>{hoaDon.phi_van_chuyen:N0} VNĐ</span>
                </div>
                {(hoaDon.so_tien_khuyen_mai > 0 ? $@"
                <div class='info-item'>
                    <span class='info-label'>Giảm giá:</span>
                    <span class='info-value'>-{hoaDon.so_tien_khuyen_mai:N0} VNĐ</span>
                </div>
                " : "")}
                <div class='info-item'>
                    <span class='info-label'>Tổng thanh toán:</span>
                    <span class='info-value highlight'>{hoaDon.tong_tien_phai_thanh_toan:N0} VNĐ</span>
                </div>
            </div>

            <div class='message-box'>
                <h3 style='color: #e74c3c; margin-top: 0;'>Danh sách sản phẩm tạm hết hàng:</h3>
                <div class='product-list'>
                    {string.Join("", sanPhamHetHang.Select(sp => $"<div class='product-item'>• {sp}</div>"))}
                </div>
            </div>

            <p class='highlight'>Chúng tôi đang nỗ lực để bổ sung hàng trong thời gian sớm nhất và sẽ liên hệ lại với bạn ngay khi có hàng.</p>

            <p>Trong thời gian chờ đợi, bạn có thể:</p>
            <ul>
                <li>Chờ chúng tôi bổ sung hàng và liên hệ lại</li>
                <li>Chọn sản phẩm thay thế khác</li>
                <li>Liên hệ với chúng tôi để được hỗ trợ</li>
            </ul>

            <div style='text-align: center; margin: 25px 0;'>
                <a href='#' class='button'>Liên hệ hỗ trợ</a>
            </div>
        </div>

        <div class='footer'>
            <p style='margin: 0;'>Trân trọng,</p>
            <p style='margin: 5px 0; font-weight: bold;'>Ban quản trị {cuaHang.ten_cua_hang}</p>
            <div class='contact-info'>
                <p style='margin: 5px 0;'>Hotline: {cuaHang.sdt}</p>
                <p style='margin: 5px 0;'>Email: {cuaHang.email}</p>
            </div>
        </div>
    </div>
</body>
</html>";

                        await _emailService.SendEmailAsync(
                            hoaDon.KhachHang.email,
                            emailSubject,
                            emailBody
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi gửi email nhưng không làm ảnh hưởng đến quá trình xử lý
                        _logger.LogError($"Lỗi gửi email thông báo hết hàng: {ex.Message}");
                    }
                }

                return (true, "Đã cập nhật trạng thái hết hàng và gửi thông báo cho khách hàng");
            }
            catch (Exception ex)
            {
                return (false, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> HoanTienVNPayAsync(Guid idHoaDon)
        {
            try
            {
                var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(idHoaDon,
                    q => q.Include(hd => hd.KhachHang)
                         .Include(hd => hd.HoaDonChiTiets)
                         .ThenInclude(hct => hct.SanPhamChiTiet)
                         .Include(hd => hd.KhuyenMai));

                if (hoaDon == null)
                    return (false, "Không tìm thấy hóa đơn");

                if (hoaDon.trang_thai_hoa_don != "DaHuy")
                    return (false, "Chỉ có thể hoàn tiền cho đơn hàng đã hủy");

                // Cập nhật trạng thái hóa đơn

                hoaDon.ngay_sua = DateTime.Now;

                // Gửi email thông báo cho khách hàng
                var cuaHang = await _cuaHangRepository.GetFirstOrDefaultAsync(ch => true);
                var emailContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #2c3e50; margin-bottom: 10px;'>Thông Báo Hoàn Tiền</h1>
                            <p style='color: #7f8c8d; font-size: 16px;'>FIFTY STORE - Thời trang nam cao cấp</p>
                        </div>

                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 6px; margin-bottom: 20px;'>
                            <h2 style='color: #2c3e50; margin-bottom: 15px;'>Thông Tin Đơn Hàng</h2>
                            <p style='margin: 5px 0;'><strong>Mã đơn hàng:</strong> {hoaDon.ma_hoa_don}</p>
                            <p style='margin: 5px 0;'><strong>Ngày đặt hàng:</strong> {hoaDon.ngay_tao:dd/MM/yyyy HH:mm}</p>
                            <p style='margin: 5px 0;'><strong>Số tiền cần hoàn:</strong> <span style='color: #e74c3c; font-weight: bold;'>{hoaDon.tong_tien_phai_thanh_toan:N0} VNĐ</span></p>
                        </div>

                        <div style='margin-bottom: 20px;'>
                            <h3 style='color: #2c3e50; margin-bottom: 10px;'>Kính gửi quý khách {hoaDon.ten_khach_hang},</h3>
                            <p style='line-height: 1.6; color: #34495e;'>Chúng tôi đã nhận được yêu cầu hoàn tiền cho đơn hàng của quý khách. Để được hỗ trợ hoàn tiền nhanh chóng, quý khách vui lòng cung cấp thông tin theo hướng dẫn bên dưới.</p>
                        </div>

                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 6px; margin-bottom: 20px;'>
                            <h3 style='color: #2c3e50; margin-bottom: 15px;'>Hướng Dẫn Hoàn Tiền</h3>
                            <p style='margin-bottom: 10px;'><strong>Bước 1:</strong> Liên hệ với chúng tôi qua một trong các kênh sau:</p>
                            <ul style='list-style-type: none; padding-left: 0;'>
                                <li style='margin: 10px 0; padding: 10px; background-color: #fff; border-radius: 4px;'>
                                    <strong>📧 Email:</strong> {cuaHang?.email ?? "support@fiftystore.com"}
                                </li>
                                <li style='margin: 10px 0; padding: 10px; background-color: #fff; border-radius: 4px;'>
                                    <strong>📱 Facebook:</strong> <a href='https://facebook.com/fiftystore' style='color: #3498db; text-decoration: none;'>facebook.com/fiftystore</a>
                                </li>
                            </ul>
                        </div>

                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 6px; margin-bottom: 20px;'>
                            <h3 style='color: #2c3e50; margin-bottom: 15px;'>Thông Tin Cần Cung Cấp</h3>
                            <ul style='list-style-type: none; padding-left: 0;'>
                                <li style='margin: 10px 0; padding: 10px; background-color: #fff; border-radius: 4px;'>
                                    <strong>📝 Mã hóa đơn cần hoàn tiền</strong>
                                    <p style='margin: 5px 0 0 0; color: #7f8c8d; font-size: 13px;'>Vui lòng cung cấp mã hóa đơn: <strong style='color: #2c3e50;'>{hoaDon.ma_hoa_don}</strong></p>
                                </li>
                                <li style='margin: 10px 0; padding: 10px; background-color: #fff; border-radius: 4px;'>
                                    <strong>🏦 Số tài khoản ngân hàng</strong>
                                </li>
                                <li style='margin: 10px 0; padding: 10px; background-color: #fff; border-radius: 4px;'>
                                    <strong>🏛️ Tên ngân hàng</strong>
                                </li>
                                <li style='margin: 10px 0; padding: 10px; background-color: #fff; border-radius: 4px;'>
                                    <strong>👤 Tên chủ tài khoản</strong>
                                </li>
                                <li style='margin: 10px 0; padding: 10px; background-color: #fff; border-radius: 4px;'>
                                    <strong>📱 Số điện thoại liên hệ</strong>
                                </li>
                            </ul>
                        </div>

                        <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0;'>
                            <p style='color: #7f8c8d; font-size: 14px;'>Chúng tôi sẽ xử lý yêu cầu hoàn tiền của quý khách trong thời gian sớm nhất.</p>
                            <p style='color: #7f8c8d; font-size: 14px;'>Trân trọng,<br><strong>FIFTY STORE</strong></p>
                        </div>
                    </div>";

                await _emailService.SendEmailAsync(
                    hoaDon.KhachHang?.email,
                    $"Thông báo hoàn tiền đơn hàng {hoaDon.ma_hoa_don}",
                    emailContent
                );

                var updateResult = await _hoaDonRepository.UpdateAsync(hoaDon);
                if (!updateResult)
                    return (false, "Không thể cập nhật trạng thái hóa đơn");

                return (true, "Đã gửi thông báo hoàn tiền cho khách hàng. Vui lòng chờ khách hàng cung cấp thông tin tài khoản ngân hàng để thực hiện hoàn tiền.");
            }
            catch (Exception ex)
            {
                return (false, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        private string GetRefundReason(string trangThai)
        {
            return trangThai switch
            {
                "DaHuy" => "Đơn hàng đã bị hủy",
                "HetHang" => "Sản phẩm trong đơn hàng đã hết hàng",
                _ => "Yêu cầu hoàn tiền"
            };
        }

        public async Task<(bool success, string message)> HuyDonHangAsync(Guid idHoaDon, string lyDo, bool isKhachHangHuy = true, Guid? id_nhan_vien_xu_ly = null)
        {
            try
            {
                var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(idHoaDon,
                    q => q.Include(hd => hd.PhuongThucThanhToan),
                    q => q.Include(hd => hd.HoaDonChiTiets)
                         .ThenInclude(hct => hct.SanPhamChiTiet).ThenInclude(spct => spct.SanPhamChiTietGiamGias).ThenInclude(spgg => spgg.GiamGia));

                if (hoaDon == null)
                    return (false, "Không tìm thấy đơn hàng");

                if (!IsValidStatusTransition(hoaDon.trang_thai_hoa_don, "DaHuy"))
                    return (false, "Không thể hủy đơn hàng ở trạng thái hiện tại");

                hoaDon.ly_do_huy_don_hang = lyDo;

                if (id_nhan_vien_xu_ly.HasValue)
                    hoaDon.id_nhan_vien_xu_ly = id_nhan_vien_xu_ly;

                var success = await _transactionHelper.XuLyHoanTienAsync(hoaDon);
                if (hoaDon.PhuongThucThanhToan.ma_phuong_thuc_thanh_toan == "PTVNPAY")
                    await HoanTienVNPayAsync(hoaDon.id_hoa_don);
                if (success)
                {
                    ClearHoaDonCache(idHoaDon);
                    await GuiEmailCapNhatTrangThaiAsync(idHoaDon, "DaHuy");
                    return (true, "Hủy đơn hàng thành công");
                }

                return (false, "Không thể hủy đơn hàng");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi hủy đơn hàng: {ex.Message}");
            }
        }
        public async Task<(bool success, string message, Guid id_hoa_don)> TaoHoaDonOnlineTrangThaiChuaThanhToan(Guid id_khach_hang, decimal phi_van_chuyen)
        {
            var khachHang = await _khachHangRepository.GetByIdWithIncludeAsync(id_khach_hang, q => q.Include(x => x.DiaChis));
            if (khachHang == null)
                return (false, "Khách hàng không tồn tại", Guid.Empty);
            if (khachHang.DiaChis.Count == 0)
                return (false, "Khách hàng không có địa chỉ nhận hàng", Guid.Empty);
            var diaChi = khachHang.DiaChis.Where(x => x.dia_chi_mac_dinh == true).FirstOrDefault();
            if (diaChi == null)
                return (false, "Khách hàng không có địa chỉ nhận hàng", Guid.Empty);

            var cuaHang = await _cuaHangRepository.GetFirstOrDefaultAsync(x => x.id_cua_hang != Guid.Empty);
            var phuongThucThanhToan = await _phuongThucThanhToanRepository.GetFirstOrDefaultAsync(x => x.ten_phuong_thuc_thanh_toan == "Tiền mặt");
            if (phuongThucThanhToan == null)
                return (false, "Phương thức thanh toán tiền mặt không tồn tại", Guid.Empty);
            var hoadonNew = new HoaDon
            {
                id_hoa_don = Guid.NewGuid(),
                ma_hoa_don = await TaoMaHoaDon(),
                id_khach_hang = id_khach_hang,
                trang_thai_hoa_don = "ChuaThanhToan",
                loai_hoa_don = "Online",
                ngay_tao = DateTime.Now,
                phi_van_chuyen = phi_van_chuyen,
                id_phuong_thuc_thanh_toan = phuongThucThanhToan.id_phuong_thuc_thanh_toan
            };
            await _hoaDonRepository.CreateAsync(hoadonNew);

            var gioHangItems = await _gioHangChiTietRepository.GetByConditionWithIncludeAsync(x => x.id_khach_hang == id_khach_hang, q => q.Include(x => x.SanPhamChiTiet).ThenInclude(x => x.SanPham).Include(x => x.SanPhamChiTiet).ThenInclude(x => x.MauSac).Include(x => x.SanPhamChiTiet).ThenInclude(x => x.KichCo));

            decimal tongTienDonHang = 0;
            foreach (var item in gioHangItems)
            {
                if (item.so_luong > item.SanPhamChiTiet.so_luong)
                {
                    return (false, $"Số lượng sản phẩm {item.SanPhamChiTiet.SanPham.ten_san_pham} - {item.SanPhamChiTiet.MauSac.ten_mau_sac} - {item.SanPhamChiTiet.KichCo.ten_kich_co} không đủ {item.so_luong} sản phẩm", Guid.Empty);
                }
                var hoaDonChiTiet = new HoaDonChiTiet
                {
                    id_hoa_don_chi_tiet = Guid.NewGuid(),
                    id_hoa_don = hoadonNew.id_hoa_don,
                    ma_hoa_don_chi_tiet = await TaoMaHoaDonChiTiet(hoadonNew.id_hoa_don),
                    id_san_pham_chi_tiet = item.id_san_pham_chi_tiet,
                    ten_san_pham = item.SanPhamChiTiet.SanPham.ten_san_pham,
                    ten_mau_sac = item.SanPhamChiTiet.MauSac.ten_mau_sac,
                    ten_kich_co = item.SanPhamChiTiet.KichCo.ten_kich_co,
                    so_luong = item.so_luong,

                    don_gia = item.SanPhamChiTiet.gia_ban,
                    gia_sau_giam_gia = await TinhTongTienSauGiamGiaSanPham(item.id_san_pham_chi_tiet),
                    gia_tri_khuyen_mai_cua_hoa_don_cho_hdct = 0,
                    trang_thai = "ChuaThanhToan",
                    ghi_chu = null,
                };
                hoaDonChiTiet.thanh_tien = hoaDonChiTiet.gia_sau_giam_gia * hoaDonChiTiet.so_luong;
                tongTienDonHang += hoaDonChiTiet.thanh_tien;
                await _hoaDonChiTietRepository.CreateAsync(hoaDonChiTiet);


                var gioHangChiTiet = await _gioHangChiTietRepository.GetByIdAsync(item.id_gio_hang_chi_tiet);
                if (gioHangChiTiet != null)
                {
                    await _gioHangChiTietRepository.DeleteAsync(gioHangChiTiet.id_gio_hang_chi_tiet);
                }
            }
            hoadonNew.tong_tien_don_hang = tongTienDonHang;
            hoadonNew.tong_tien_phai_thanh_toan = tongTienDonHang + phi_van_chuyen;
            hoadonNew.ten_khach_hang = khachHang.ten_khach_hang;
            hoadonNew.sdt_khach_hang = khachHang.so_dien_thoai;
            hoadonNew.dia_chi_nhan_hang = diaChi.tinh + ", " + diaChi.huyen + ", " + diaChi.xa + ", " + diaChi.dia_chi_cu_the;
            hoadonNew.id_cua_hang = cuaHang.id_cua_hang == null ? Guid.Empty : cuaHang.id_cua_hang;
            await _hoaDonRepository.UpdateAsync(hoadonNew);
            return (true, "Tạo hóa đơn online thành công", hoadonNew.id_hoa_don);
        }

        public async Task<List<HoaDonAdminDTO>> GetHoaDonByTrangThaiAsync(string trangThai)
        {
            var hoaDons = await _hoaDonRepository.GetByConditionWithIncludeAsync(
                hd => hd.trang_thai_hoa_don == trangThai,
                q => q.Include(hd => hd.HoaDonChiTiets)
                     .ThenInclude(hct => hct.SanPhamChiTiet)
                     .ThenInclude(spct => spct.SanPham)
                     .Include(hd => hd.HoaDonChiTiets)
                     .ThenInclude(hct => hct.SanPhamChiTiet)
                     .ThenInclude(spct => spct.MauSac)
                     .Include(hd => hd.HoaDonChiTiets)
                     .ThenInclude(hct => hct.SanPhamChiTiet)
                     .ThenInclude(spct => spct.KichCo)
                     .Include(hd => hd.KhachHang)
                     .Include(hd => hd.NhanVienXuLy)
                     .Include(hd => hd.KhuyenMai)
                     .Include(hd => hd.PhuongThucThanhToan)
                     .Include(hd => hd.CuaHang)
            );

            hoaDons = hoaDons.OrderByDescending(hd => hd.ngay_tao).ToList();

            var result = new List<HoaDonAdminDTO>();

            foreach (var hoaDon in hoaDons)
            {
                var hoaDonChiTiets = await MapHoaDonChiTietsAsync(hoaDon.HoaDonChiTiets);
                var tongTienDonHang = await TinhTongTienDonHang(hoaDon.id_hoa_don);
                (decimal tongTienSauKhuyenMai, decimal giaTriKhuyenMai) = await CapNhatTongTienVaGiaTriKhuyenMaiChoHoaDon(hoaDon);

                result.Add(new HoaDonAdminDTO
                {
                    id_hoa_don = hoaDon.id_hoa_don,
                    ma_hoa_don = hoaDon.ma_hoa_don,
                    id_khach_hang = hoaDon.id_khach_hang != null ? hoaDon.id_khach_hang : null,
                    ten_khach_hang = hoaDon.id_khach_hang != null ? hoaDon.KhachHang.ten_khach_hang : "Khách lẻ",
                    ten_nguoi_xu_ly = hoaDon.NhanVienXuLy?.ten_nhan_vien,
                    sdt_khach_hang = hoaDon.id_khach_hang != null ? hoaDon.KhachHang.so_dien_thoai : null,
                    dia_chi_nhan_hang = hoaDon.id_khach_hang != null ? hoaDon.dia_chi_nhan_hang : null,
                    ghi_chu = hoaDon.ghi_chu,
                    loai_hoa_don = hoaDon.loai_hoa_don,
                    so_tien_khach_tra = hoaDon.so_tien_khach_tra,
                    phi_van_chuyen = hoaDon.phi_van_chuyen ?? 0,
                    id_phuong_thuc_thanh_toan = hoaDon.id_phuong_thuc_thanh_toan?.ToString(),
                    so_tien_thua_tra_khach = hoaDon.so_tien_thua_tra_khach,
                    tong_tien_don_hang = tongTienDonHang,
                    so_tien_khuyen_mai = giaTriKhuyenMai,
                    tong_tien_phai_thanh_toan = tongTienSauKhuyenMai,
                    trang_thai = hoaDon.trang_thai_hoa_don,
                    ten_phuong_thuc_thanh_toan = hoaDon.PhuongThucThanhToan?.ten_phuong_thuc_thanh_toan,
                    ngay_tao = hoaDon.ngay_tao,
                    nhanVienXuLy = hoaDon.NhanVienXuLy != null ? new NhanVien_HoaDonAdminDTO
                    {
                        id_nhan_vien = hoaDon.id_nhan_vien_xu_ly,
                        ma_nhan_vien = hoaDon.NhanVienXuLy.ma_nhan_vien,
                        ten_nhan_vien = hoaDon.NhanVienXuLy.ten_nhan_vien
                    } : null,
                    khachHang = hoaDon.KhachHang != null ? new KhachHang_HoaDonAdminDTO
                    {
                        id_khach_hang = hoaDon.KhachHang.id_khach_hang,
                        ma_khach_hang = hoaDon.KhachHang.ma_khach_hang,
                        ten_khach_hang = hoaDon.KhachHang.ten_khach_hang,
                        sdt_khach_hang = hoaDon.KhachHang.so_dien_thoai
                    } : null,
                    hoaDonChiTiets = hoaDonChiTiets
                });
            }

            return result;
        }
    }

    public static class TrangThaiDonHangHelper
    {
        public static readonly Dictionary<string, string[]> ValidTransitions = new()
        {
            { "ChuaThanhToan", new[] { "DaThanhToan", "DaHuy" } },
            { "DaThanhToan", new[] { "DangChoXuLy", "DaHuy" } },
            { "DangChoXuLy", new[] { "DaXacNhan", "DaHuy", "HetHang" } },
            { "HetHang", new[] { "DaXacNhan", "DaHuy" } },
            { "DaXacNhan", new[] { "DangChuanBi" } },
            { "DangChuanBi", new[] { "DangGiaoHang" } },
            { "DangGiaoHang", new[] { "DaNhanHang", "DaHuy" } },
            { "DaNhanHang", new[] { "DaHoanThanh" } },
            { "ChoTaiQuay", new[] { "DaHoanThanh" } }
        };

        public static bool IsValidTransition(string currentStatus, string newStatus)
        {
            if (!ValidTransitions.ContainsKey(currentStatus))
                return false;

            return ValidTransitions[currentStatus].Contains(newStatus);
        }

        public static string GetTrangThaiText(string trangThai) => trangThai switch
        {
            "ChuaThanhToan" => "Chưa thanh toán",
            "DaThanhToan" => "Đã thanh toán",
            "DangChoXuLy" => "Đang chờ xử lý",
            "DaXacNhan" => "Đã xác nhận",
            "DangChuanBi" => "Đang chuẩn bị",
            "DangGiaoHang" => "Đang giao hàng",
            "DaNhanHang" => "Đã nhận hàng",
            "DaHoanThanh" => "Đã hoàn thành",
            "DaHuy" => "Đã hủy",
            "HetHang" => "Hết hàng",
            "ChoTaiQuay" => "Chờ tại quầy",
            _ => "Không xác định"
        };

        public static string GetTrangThaiMessage(string trangThai, CuaHang cuaHang) => trangThai switch
        {
            "ChuaThanhToan" => "Vui lòng thanh toán đơn hàng để chúng tôi bắt đầu xử lý",
            "DaThanhToan" => "Đơn hàng đã được thanh toán thành công",
            "DangChoXuLy" => "Đơn hàng của bạn đang chờ nhân viên xác nhận",
            "DaXacNhan" => $"Đơn hàng đã được {cuaHang.ten_cua_hang} xác nhận",
            "DangChuanBi" => $"Đơn hàng đang được {cuaHang.ten_cua_hang} chuẩn bị",
            "DangGiaoHang" => "Đơn hàng đang được giao đến bạn",
            "DaNhanHang" => "Đơn hàng đã được giao và khách hàng đã nhận hàng",
            "DaHoanThanh" => "Đơn hàng đã được hoàn thành",
            "DaHuy" => "Đơn hàng đã bị hủy",
            "HetHang" => "Đơn hàng không thể thực hiện do hết hàng",
            "ChoTaiQuay" => $"Đơn hàng đang chờ bạn đến nhận tại {cuaHang.ten_cua_hang}",
            _ => "Trạng thái không xác định"
        };
    }
}