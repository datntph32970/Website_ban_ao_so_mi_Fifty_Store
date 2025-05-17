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

namespace API.Services.Implementations
{
    public class HoaDonService : BaseService<HoaDon>, IHoaDonService
    {
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

        public HoaDonService(
            IBaseRepository<HoaDon> hoaDonRepository,
            IBaseRepository<HoaDonChiTiet> hoaDonChiTietRepository,
            IBaseRepository<SanPhamChiTiet> sanPhamChiTietRepository,
            IBaseRepository<SanPham> sanPhamRepository,
            IBaseRepository<KhachHang> khachHangRepository,
            IBaseRepository<NhanVien> nhanVienRepository,
            IBaseRepository<KhuyenMai> khuyenMaiRepository,
            IBaseRepository<PhuongThucThanhToan> phuongThucThanhToanRepository,
            IBaseRepository<GiamGia> giamGiaRepository,
            IBaseRepository<CuaHang> cuaHangRepository,
            IBaseRepository<GioHangChiTiet> gioHangChiTietRepository,
            IBaseRepository<DiaChi> diaChiRepository,
            VNPayService vnPayService) : base(hoaDonRepository)
        {
            _hoaDonRepository = hoaDonRepository;
            _hoaDonChiTietRepository = hoaDonChiTietRepository;
            _sanPhamChiTietRepository = sanPhamChiTietRepository;
            _sanPhamRepository = sanPhamRepository;
            _khachHangRepository = khachHangRepository;
            _nhanVienRepository = nhanVienRepository;
            _khuyenMaiRepository = khuyenMaiRepository;
            _giamGiaRepository = giamGiaRepository;
            _cuaHangRepository = cuaHangRepository;
            _gioHangChiTietRepository = gioHangChiTietRepository;
            _vnPayService = vnPayService;
            _phuongThucThanhToanRepository = phuongThucThanhToanRepository;
            _diaChiRepository = diaChiRepository;
        }

        public async Task<List<HoaDonAdminDTO>> GetAllHoaDonAdminDTOAsync()
        {
            var result = await _hoaDonRepository.GetAllWithIncludeAsync(q => q.Include(hd => hd.KhachHang)
                                                                       .Include(hd => hd.NhanVienXuLy)
                                                                       .Include(hd => hd.PhuongThucThanhToan)
                                                                       .Include(hd => hd.HoaDonChiTiets)
                                                                       .ThenInclude(hct => hct.SanPhamChiTiet)
                                                                       .ThenInclude(spct => spct.SanPham)
                                                                       .Include(hd => hd.HoaDonChiTiets)
                                                                       .ThenInclude(hct => hct.SanPhamChiTiet)
                                                                       .ThenInclude(spct => spct.MauSac)
                                                                       .Include(hd => hd.HoaDonChiTiets)
                                                                       .ThenInclude(hct => hct.SanPhamChiTiet)
                                                                       .ThenInclude(spct => spct.KichCo));
            var tasks = result.Select(async hd => new HoaDonAdminDTO
            {
                id_hoa_don = hd.id_hoa_don,
                ma_hoa_don = hd.ma_hoa_don,
                id_khach_hang = hd.id_khach_hang != null ? hd.id_khach_hang : null,
                ten_khach_hang = hd.id_khach_hang != null ? hd.KhachHang.ten_khach_hang : "Khách lẻ",
                ten_nguoi_xu_ly = hd.NhanVienXuLy?.ten_nhan_vien,
                sdt_khach_hang = hd.id_khach_hang != null ? hd.KhachHang.so_dien_thoai : null,
                dia_chi_nhan_hang = hd.id_khach_hang != null ? hd.dia_chi_nhan_hang : null,
                ghi_chu = hd.ghi_chu,
                loai_hoa_don = hd.loai_hoa_don,
                so_tien_khach_tra = hd.so_tien_khach_tra,
                phi_van_chuyen = hd.phi_van_chuyen ?? 0,
                id_phuong_thuc_thanh_toan = hd.id_phuong_thuc_thanh_toan?.ToString(),
                so_tien_thua_tra_khach = hd.so_tien_thua_tra_khach,
                tong_tien_don_hang = hd.tong_tien_don_hang ?? 0,
                so_tien_khuyen_mai = hd.id_khuyen_mai != null ? hd.so_tien_khuyen_mai : null,
                tong_tien_phai_thanh_toan = hd.tong_tien_phai_thanh_toan ?? 0,
                trang_thai = hd.trang_thai_hoa_don,
                ten_phuong_thuc_thanh_toan = hd.PhuongThucThanhToan?.ten_phuong_thuc_thanh_toan,
                ngay_tao = hd.ngay_tao,

                nhanVienXuLy = hd.NhanVienXuLy != null ? new NhanVien_HoaDonAdminDTO
                {
                    id_nhan_vien = hd.id_nhan_vien_xu_ly,
                    ma_nhan_vien = hd.NhanVienXuLy.ma_nhan_vien,
                    ten_nhan_vien = hd.NhanVienXuLy.ten_nhan_vien
                } : null,
                khachHang = hd.KhachHang != null ? new KhachHang_HoaDonAdminDTO
                {
                    id_khach_hang = hd.KhachHang.id_khach_hang,
                    ma_khach_hang = hd.KhachHang.ma_khach_hang,
                    ten_khach_hang = hd.KhachHang.ten_khach_hang,
                    sdt_khach_hang = hd.KhachHang.so_dien_thoai
                } : null,
                hoaDonChiTiets = hd.HoaDonChiTiets == null ? null : await MapHoaDonChiTietsAsync(hd.HoaDonChiTiets)
            });
            return (await Task.WhenAll(tasks)).ToList();
        }
        public async Task<HoaDonAdminDTO> GetByIdHoaDonAdminDTOAsync(Guid id)
        {
            var result = await _hoaDonRepository.GetByIdWithIncludeAsync(id, q => q.Include(hd => hd.KhachHang)
                                                                       .Include(hd => hd.NhanVienXuLy)
                                                                       .Include(hd => hd.KhuyenMai)
                                                                       .Include(hd => hd.CuaHang)
                                                                       .ThenInclude(ch => ch.HinhAnh)
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
                                                                       .ThenInclude(spct => spct.HinhAnhSanPhamChiTiets)
                                                                       .ThenInclude(hct => hct.HinhAnhs));

            if (result == null)
                return null;

            // Recalculate totals if order is in ChuaThanhToan or ChoTaiQuay status
            if (result.trang_thai_hoa_don == "ChuaThanhToan" || result.trang_thai_hoa_don == "ChoTaiQuay")
            {
                var (tongTienSauKhuyenMai, giaTriKhuyenMai) = await CapNhatTongTienVaGiaTriKhuyenMai(result.id_hoa_don);
                result.tong_tien_phai_thanh_toan = tongTienSauKhuyenMai;
                result.so_tien_khuyen_mai = giaTriKhuyenMai;
                await _hoaDonRepository.UpdateAsync(result);
            }

            return new HoaDonAdminDTO
            {
                id_hoa_don = result.id_hoa_don,
                ma_hoa_don = result.ma_hoa_don,
                id_khach_hang = result.id_khach_hang != null ? result.id_khach_hang : null,
                ten_khach_hang = result.id_khach_hang != null ? result.KhachHang.ten_khach_hang : "Khách lẻ",
                ten_nguoi_xu_ly = result.NhanVienXuLy?.ten_nhan_vien,
                sdt_khach_hang = result.id_khach_hang != null ? result.KhachHang.so_dien_thoai : null,
                dia_chi_nhan_hang = result.id_khach_hang != null ? result.dia_chi_nhan_hang : null,
                ghi_chu = result.ghi_chu,
                loai_hoa_don = result.loai_hoa_don,
                so_tien_khach_tra = result.so_tien_khach_tra,
                phi_van_chuyen = result.phi_van_chuyen,
                so_tien_thua_tra_khach = result.so_tien_thua_tra_khach,
                id_phuong_thuc_thanh_toan = result.id_phuong_thuc_thanh_toan?.ToString(),
                tong_tien_don_hang = result.tong_tien_don_hang ?? 0,
                so_tien_khuyen_mai = result.id_khuyen_mai != null ? result.so_tien_khuyen_mai : null,
                tong_tien_phai_thanh_toan = result.tong_tien_phai_thanh_toan ?? 0,
                trang_thai = result.trang_thai_hoa_don,
                ten_phuong_thuc_thanh_toan = result.PhuongThucThanhToan?.ten_phuong_thuc_thanh_toan,
                ngay_tao = result.ngay_tao,
                khuyenMai = result.KhuyenMai == null ? null : new KhuyenMai_HoaDonAdminDTO
                {
                    id_khuyen_mai = result.KhuyenMai.id_khuyen_mai,
                    ten_khuyen_mai = result.KhuyenMai.ten_khuyen_mai,
                    ma_khuyen_mai = result.KhuyenMai.ma_khuyen_mai,
                    loai_khuyen_mai = result.KhuyenMai.kieu_khuyen_mai,
                    gia_tri_khuyen_mai = result.KhuyenMai.gia_tri_giam,
                    gia_tri_giam_toi_da = result.KhuyenMai.gia_tri_giam_toi_da
                },
                cuaHang = result.CuaHang == null ? null : new CuaHang_HoaDonAdminDTO
                {
                    id_cua_hang = result.CuaHang.id_cua_hang,
                    ten_cua_hang = result.CuaHang.ten_cua_hang,
                    website = result.CuaHang.website,
                    email = result.CuaHang.email,
                    sdt = result.CuaHang.sdt,
                    dia_chi = result.CuaHang.dia_chi,
                    mo_ta = result.CuaHang.mo_ta,
                    hinh_anh_logo_cua_hang_url = result.CuaHang.HinhAnh?.url
                },
                nhanVienXuLy = result.NhanVienXuLy == null ? null : new NhanVien_HoaDonAdminDTO
                {
                    id_nhan_vien = result.NhanVienXuLy.id_nhan_vien,
                    ma_nhan_vien = result.NhanVienXuLy.ma_nhan_vien,
                    ten_nhan_vien = result.NhanVienXuLy.ten_nhan_vien
                },
                khachHang = result.KhachHang == null ? null : new KhachHang_HoaDonAdminDTO
                {
                    id_khach_hang = result.KhachHang.id_khach_hang,
                    ma_khach_hang = result.KhachHang.ma_khach_hang,
                    ten_khach_hang = result.KhachHang.ten_khach_hang,
                    sdt_khach_hang = result.KhachHang.so_dien_thoai
                },
                hoaDonChiTiets = result.HoaDonChiTiets == null ? null : await MapHoaDonChiTietsAsync(result.HoaDonChiTiets)
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
                    hd.trang_thai_hoa_don == "ChoTaiQuay");

                if (hoaDonTaiQuayDangChoXuLy.Count >= 15)
                {
                    return (false, "Đã đạt giới hạn tối đa 15 hóa đơn tại quầy đang chờ xử lý");
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
                        q => q.Include(hct => hct.SanPhamChiTiet));

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

            await CapNhatTongTienVaGiaTriKhuyenMai(id_hoa_don);
            return (true, "Thêm hóa đơn chi tiết thành công");
        }

        private async Task<decimal> TinhGiaSauGiamGia(SanPhamChiTiet sanPhamChiTiet)
        {
            decimal giaSauGiamGia = sanPhamChiTiet.gia_ban;
            if (sanPhamChiTiet.id_giam_gia != null)
            {
                var giamGia = await _giamGiaRepository.GetByIdAsync(sanPhamChiTiet.id_giam_gia.Value);
                if (giamGia != null)
                {
                    if (giamGia.kieu_giam_gia == "PhanTram")
                    {
                        giaSauGiamGia = sanPhamChiTiet.gia_ban * (1 - giamGia.gia_tri_giam / 100);
                    }
                    else if (giamGia.kieu_giam_gia == "SoTien")
                    {
                        giaSauGiamGia = sanPhamChiTiet.gia_ban - giamGia.gia_tri_giam;
                    }
                }
            }
            return Math.Max(0, giaSauGiamGia);
        }

        //tính tổng tiền đơn hàng chi tiết
        public async Task<(decimal tongTienSauKhuyenMai, decimal giaTriKhuyenMai)> CapNhatTongTienVaGiaTriKhuyenMai(Guid id_hoa_don)
        {
            var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(id_hoa_don, q => q.Include(hd => hd.HoaDonChiTiets));
            var tongTienDonHang = await TinhTongTienDonHang(id_hoa_don);

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
                .ThenInclude(spct => spct.KichCo));

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
                hoaDonChiTiets = result.HoaDonChiTiets == null ? null : await MapHoaDonChiTietsAsync(result.HoaDonChiTiets)
            };
        }
        private async Task<decimal> TinhTongTienDonHang(Guid id_hoa_don)
        {
            var hoaDon = await _hoaDonRepository.GetByIdWithIncludeAsync(id_hoa_don, q => q.Include(hd => hd.HoaDonChiTiets));
            decimal tongTien = 0;
            foreach (var hdct in hoaDon.HoaDonChiTiets)
            {
                var spct = await _sanPhamChiTietRepository.GetByIdAsync(hdct.id_san_pham_chi_tiet);
                hdct.don_gia = spct.gia_ban;
                hdct.gia_sau_giam_gia = await TinhTongTienSauGiamGiaSanPham(hdct.id_san_pham_chi_tiet);
                hdct.thanh_tien = hdct.gia_sau_giam_gia * hdct.so_luong;
                await _hoaDonChiTietRepository.UpdateAsync(hdct);
                tongTien += hdct.thanh_tien;
            }
            return tongTien;
        }
        private async Task<decimal> TinhTongTienSauGiamGiaSanPham(Guid id_san_pham_chi_tiet)
        {
            try
            {
                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdAsync(id_san_pham_chi_tiet);
                if (sanPhamChiTiet == null || sanPhamChiTiet.id_giam_gia == null)
                    return sanPhamChiTiet.gia_ban;
                decimal giaSauGiamGia = sanPhamChiTiet.gia_ban;

                var giamGia = await _giamGiaRepository.GetByIdAsync(sanPhamChiTiet.id_giam_gia.Value);
                if (giamGia == null)
                    return giaSauGiamGia;

                if (giamGia.kieu_giam_gia == "PhanTram")
                {
                    giaSauGiamGia = sanPhamChiTiet.gia_ban - (sanPhamChiTiet.gia_ban * (giamGia.gia_tri_giam / 100));
                    if (giaSauGiamGia < 0)
                        giaSauGiamGia = 0;
                }
                else if (giamGia.kieu_giam_gia == "SoTien")
                {
                    giaSauGiamGia = sanPhamChiTiet.gia_ban - giamGia.gia_tri_giam;
                    if (giaSauGiamGia < 0)
                        giaSauGiamGia = 0;
                }

                return giaSauGiamGia;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }
        private async Task<List<HoaDonChiTietAdminDTO>> MapHoaDonChiTietsAsync(IEnumerable<HoaDonChiTiet> chiTiets)
        {
            if (chiTiets == null || !chiTiets.Any())
            {
                return new List<HoaDonChiTietAdminDTO>();
            }

            // Lấy hóa đơn từ hóa đơn chi tiết đầu tiên (vì tất cả chi tiết đều thuộc cùng một hóa đơn)
            var firstChiTiet = chiTiets.First();
            var hoaDon = await _hoaDonRepository.GetByIdAsync(firstChiTiet.id_hoa_don);
            if (hoaDon == null)
            {
                return new List<HoaDonChiTietAdminDTO>();
            }

            var isChoTaiQuay = hoaDon.trang_thai_hoa_don == "ChoTaiQuay" || hoaDon.trang_thai_hoa_don == "ChuaThanhToan";
            var result = new List<HoaDonChiTietAdminDTO>();

            foreach (var hct in chiTiets)
            {
                try
                {
                    // Nếu là hóa đơn chờ tại quầy thì tính lại giá, ngược lại giữ nguyên giá cũ
                    decimal giaSauGiamGia = isChoTaiQuay
                        ? await TinhTongTienSauGiamGiaSanPham(hct.id_san_pham_chi_tiet)
                        : hct.gia_sau_giam_gia;

                    decimal donGia = isChoTaiQuay
                        ? (hct.SanPhamChiTiet?.gia_ban ?? hct.don_gia)  // Lấy giá hiện tại của sản phẩm hoặc giá gốc nếu không có
                        : hct.don_gia;                                   // Giữ nguyên giá gốc đã lưu

                    result.Add(new HoaDonChiTietAdminDTO
                    {
                        id_hoa_don_chi_tiet = hct.id_hoa_don_chi_tiet,
                        ma_hoa_don_chi_tiet = hct.ma_hoa_don_chi_tiet,
                        id_hoa_don = hct.id_hoa_don,
                        id_san_pham_chi_tiet = hct.id_san_pham_chi_tiet,
                        so_luong = hct.so_luong,
                        don_gia = donGia,

                        gia_sau_giam_gia = giaSauGiamGia,
                        gia_tri_khuyen_mai_cua_hoa_don_cho_hdct = hct.gia_tri_khuyen_mai_cua_hoa_don_cho_hdct,
                        thanh_tien = giaSauGiamGia * hct.so_luong,
                        trang_thai = hct.trang_thai,
                        ghi_chu = hct.ghi_chu,
                        ngay_sua = hct.ngay_sua,
                        ten_nhan_vien_xu_ly = hct.NhanVienXuLy?.ten_nhan_vien,
                        sanPhamChiTiet = hct.SanPhamChiTiet != null ? new SanPhamChiTiet_HoaDonChiTietAdminDTO
                        {
                            id_san_pham_chi_tiet = hct.SanPhamChiTiet.id_san_pham_chi_tiet,
                            ma_san_pham_chi_tiet = hct.SanPhamChiTiet.ma_san_pham_chi_tiet,
                            ten_san_pham = hct.SanPhamChiTiet.SanPham?.ten_san_pham,
                            url_anh_san_pham_chi_tiet = hct.SanPhamChiTiet.HinhAnhSanPhamChiTiets?.FirstOrDefault()?.HinhAnhs?.url,
                            ten_mau_sac = hct.SanPhamChiTiet.MauSac?.ten_mau_sac,
                            ten_kich_co = hct.SanPhamChiTiet.KichCo?.ten_kich_co
                        } : null
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi xử lý hóa đơn chi tiết: {ex.Message}");
                    // Tiếp tục với chi tiết tiếp theo nếu có lỗi
                    continue;
                }
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
    }
}