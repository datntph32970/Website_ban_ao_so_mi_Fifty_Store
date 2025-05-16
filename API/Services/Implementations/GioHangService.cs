using API.DbConects.DTOs.Client.HoaDon;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Khuyen_Mai;
using API.DbConects.Entities.Entities_San_Pham;
using API.Repositories.Interfaces;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Implementations
{
    public class GioHangService : BaseService<GioHangChiTiet>, IGioHangService
    {
        private readonly IBaseRepository<SanPhamChiTiet> _sanPhamChiTietRepository;

        public GioHangService(
            IBaseRepository<GioHangChiTiet> repository,
            IBaseRepository<SanPhamChiTiet> sanPhamChiTietRepository) : base(repository)
        {
            _sanPhamChiTietRepository = sanPhamChiTietRepository;
        }

        public async Task<IEnumerable<GioHangItemClientDTO>> GetGioHangByKhachHangAsync(Guid idKhachHang)
        {
            var gioHangChiTiets = await _repository.GetByConditionWithIncludeAsync(
                g => g.id_khach_hang == idKhachHang,
                g => g.Include(g => g.SanPhamChiTiet)
                    .ThenInclude(g => g.SanPham)
                    .ThenInclude(sp => sp.anhMacDinh)
                    .Include(g => g.SanPhamChiTiet)
                    .ThenInclude(g => g.MauSac)
                    .Include(g => g.SanPhamChiTiet)
                    .ThenInclude(g => g.KichCo)
                    .Include(g => g.SanPhamChiTiet)
                    .ThenInclude(spct => spct.GiamGia)
                    .Include(k => k.SanPhamChiTiet)
                    .ThenInclude(spct => spct.HinhAnhSanPhamChiTiets)
                    .ThenInclude(haspct => haspct.HinhAnhs)
            );

            return gioHangChiTiets.Select(g => new GioHangItemClientDTO
            {
                id_gio_hang_chi_tiet = g.id_gio_hang_chi_tiet,
                id_san_pham_chi_tiet = g.id_san_pham_chi_tiet,
                ma_san_pham_chi_tiet = g.SanPhamChiTiet.ma_san_pham_chi_tiet,
                ten_san_pham = g.SanPhamChiTiet.SanPham.ten_san_pham,
                ten_mau_sac = g.SanPhamChiTiet.MauSac.ten_mau_sac,
                ten_kich_co = g.SanPhamChiTiet.KichCo.ten_kich_co,
                so_luong = g.so_luong,
                gia_ban = g.SanPhamChiTiet.gia_ban,
                gia_sau_giam = g.SanPhamChiTiet.GiamGia != null ? TinhGiaSauGiam(g.SanPhamChiTiet.gia_ban, g.SanPhamChiTiet.GiamGia) : null,
                url_anh = g.SanPhamChiTiet.HinhAnhSanPhamChiTiets.FirstOrDefault()?.HinhAnhs?.url ?? g.SanPhamChiTiet.SanPham.anhMacDinh?.url,
                trang_thai = g.trang_thai,
                so_luong_ton = g.SanPhamChiTiet.so_luong,

                // Thông tin giảm giá
                id_giam_gia = g.SanPhamChiTiet.GiamGia?.id_giam_gia,
                ten_giam_gia = g.SanPhamChiTiet.GiamGia?.ten_giam_gia,
                kieu_giam_gia = g.SanPhamChiTiet.GiamGia?.kieu_giam_gia,
                gia_tri_giam = g.SanPhamChiTiet.GiamGia?.gia_tri_giam,
                thoi_gian_bat_dau = g.SanPhamChiTiet.GiamGia?.thoi_gian_bat_dau,
                thoi_gian_ket_thuc = g.SanPhamChiTiet.GiamGia?.thoi_gian_ket_thuc
            });
        }

        public async Task<IEnumerable<GioHangItemClientDTO>> GetGioHangDaChonAsync(Guid idKhachHang)
        {
            var gioHangChiTiets = await _repository.GetByConditionWithIncludeAsync(
                g => g.id_khach_hang == idKhachHang && g.trang_thai == true,
                g => g.Include(g => g.SanPhamChiTiet)
                    .ThenInclude(g => g.SanPham)
                    .ThenInclude(sp => sp.anhMacDinh)
                    .Include(g => g.SanPhamChiTiet)
                    .ThenInclude(g => g.MauSac)
                    .Include(g => g.SanPhamChiTiet)
                    .ThenInclude(g => g.KichCo)
                    .Include(g => g.SanPhamChiTiet)
                    .ThenInclude(spct => spct.GiamGia)
                    .Include(k => k.SanPhamChiTiet)
                    .ThenInclude(spct => spct.HinhAnhSanPhamChiTiets)
                    .ThenInclude(haspct => haspct.HinhAnhs)
            );

            return gioHangChiTiets.Select(g => new GioHangItemClientDTO
            {
                id_gio_hang_chi_tiet = g.id_gio_hang_chi_tiet,
                id_san_pham_chi_tiet = g.id_san_pham_chi_tiet,
                ma_san_pham_chi_tiet = g.SanPhamChiTiet.ma_san_pham_chi_tiet,
                ten_san_pham = g.SanPhamChiTiet.SanPham.ten_san_pham,
                ten_mau_sac = g.SanPhamChiTiet.MauSac.ten_mau_sac,
                ten_kich_co = g.SanPhamChiTiet.KichCo.ten_kich_co,
                so_luong = g.so_luong,
                gia_ban = g.SanPhamChiTiet.gia_ban,
                gia_sau_giam = g.SanPhamChiTiet.GiamGia != null ? TinhGiaSauGiam(g.SanPhamChiTiet.gia_ban, g.SanPhamChiTiet.GiamGia) : null,
                url_anh = g.SanPhamChiTiet.HinhAnhSanPhamChiTiets.FirstOrDefault()?.HinhAnhs?.url ?? g.SanPhamChiTiet.SanPham.anhMacDinh?.url,
                trang_thai = g.trang_thai,
                so_luong_ton = g.SanPhamChiTiet.so_luong,

                // Thông tin giảm giá
                id_giam_gia = g.SanPhamChiTiet.GiamGia?.id_giam_gia,
                ten_giam_gia = g.SanPhamChiTiet.GiamGia?.ten_giam_gia,
                kieu_giam_gia = g.SanPhamChiTiet.GiamGia?.kieu_giam_gia,
                gia_tri_giam = g.SanPhamChiTiet.GiamGia?.gia_tri_giam,
                thoi_gian_bat_dau = g.SanPhamChiTiet.GiamGia?.thoi_gian_bat_dau,
                thoi_gian_ket_thuc = g.SanPhamChiTiet.GiamGia?.thoi_gian_ket_thuc
            });
        }

        private decimal TinhGiaSauGiam(decimal giaBan, GiamGia giamGia)
        {
            if (giamGia == null) return giaBan;

            if (giamGia.kieu_giam_gia == "PhanTram")
            {
                var giaSauGiam = giaBan - (giaBan * (giamGia.gia_tri_giam / 100));
                return giaSauGiam < 0 ? 0 : giaSauGiam;
            }
            else if (giamGia.kieu_giam_gia == "SoTien")
            {
                var giaSauGiam = giaBan - giamGia.gia_tri_giam;
                return giaSauGiam < 0 ? 0 : giaSauGiam;
            }

            return giaBan;
        }

        public async Task<(bool success, string message)> ThemSanPhamVaoGioHangAsync(Guid idKhachHang, Guid idSanPhamChiTiet, int soLuong)
        {
            try
            {
                if (soLuong <= 0)
                    return (false, "Số lượng phải lớn hơn 0");

                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdAsync(idSanPhamChiTiet);
                if (sanPhamChiTiet == null)
                    return (false, "Sản phẩm không tồn tại");

                if (sanPhamChiTiet.trang_thai != "HoatDong")
                    return (false, "Sản phẩm hiện không khả dụng");

                if (sanPhamChiTiet.so_luong < soLuong)
                    return (false, $"Số lượng tồn không đủ. Hiện chỉ còn {sanPhamChiTiet.so_luong} sản phẩm");

                var existingItem = (await _repository.GetByConditionAsync(
                    g => g.id_khach_hang == idKhachHang &&
                         g.id_san_pham_chi_tiet == idSanPhamChiTiet
                )).FirstOrDefault();

                if (existingItem != null)
                {
                    var newQuantity = existingItem.so_luong + soLuong;
                    if (sanPhamChiTiet.so_luong < newQuantity)
                        return (false, $"Số lượng tồn không đủ. Hiện chỉ còn {sanPhamChiTiet.so_luong} sản phẩm");

                    existingItem.so_luong = newQuantity;
                    var updateResult = await _repository.UpdateAsync(existingItem);
                    return updateResult
                        ? (true, "Cập nhật số lượng trong giỏ hàng thành công")
                        : (false, "Không thể cập nhật số lượng trong giỏ hàng");
                }

                var gioHangChiTiet = new GioHangChiTiet
                {
                    id_gio_hang_chi_tiet = Guid.NewGuid(),
                    id_khach_hang = idKhachHang,
                    id_san_pham_chi_tiet = idSanPhamChiTiet,
                    so_luong = soLuong
                };

                var result = await _repository.CreateAsync(gioHangChiTiet);
                return result
                    ? (true, "Thêm sản phẩm vào giỏ hàng thành công")
                    : (false, "Không thể thêm sản phẩm vào giỏ hàng");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi thêm sản phẩm vào giỏ hàng: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> CapNhatSoLuongAsync(Guid idGioHangChiTiet, int soLuong)
        {
            try
            {
                if (soLuong < 0)
                    return (false, "Số lượng không được âm");

                var gioHangChiTiet = await _repository.GetByIdAsync(idGioHangChiTiet);
                if (gioHangChiTiet == null)
                    return (false, "Không tìm thấy sản phẩm trong giỏ hàng");

                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdAsync(gioHangChiTiet.id_san_pham_chi_tiet);
                if (sanPhamChiTiet == null)
                    return (false, "Sản phẩm không tồn tại");

                if (sanPhamChiTiet.trang_thai != "HoatDong")
                    return (false, "Sản phẩm hiện không khả dụng");

                if (sanPhamChiTiet.so_luong < soLuong)
                    return (false, $"Số lượng tồn không đủ. Hiện chỉ còn {sanPhamChiTiet.so_luong} sản phẩm");

                // Nếu số lượng = 0, xóa sản phẩm khỏi giỏ hàng
                if (soLuong == 0)
                {
                    var deleteResult = await _repository.DeleteAsync(idGioHangChiTiet);
                    return deleteResult
                        ? (true, "Đã xóa sản phẩm khỏi giỏ hàng")
                        : (false, "Không thể xóa sản phẩm khỏi giỏ hàng");
                }

                gioHangChiTiet.so_luong = soLuong;
                var result = await _repository.UpdateAsync(gioHangChiTiet);
                return result
                    ? (true, "Cập nhật số lượng thành công")
                    : (false, "Không thể cập nhật số lượng");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật số lượng: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> XoaSanPhamKhoiGioHangAsync(Guid idGioHangChiTiet)
        {
            try
            {
                var gioHangChiTiet = await _repository.GetByIdAsync(idGioHangChiTiet);
                if (gioHangChiTiet == null)
                    return (false, "Không tìm thấy sản phẩm trong giỏ hàng");

                var result = await _repository.DeleteAsync(idGioHangChiTiet);
                return result
                    ? (true, "Xóa sản phẩm khỏi giỏ hàng thành công")
                    : (false, "Không thể xóa sản phẩm khỏi giỏ hàng");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa sản phẩm khỏi giỏ hàng: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> XoaGioHangAsync(Guid idKhachHang)
        {
            try
            {
                var gioHangChiTiets = await _repository.GetByConditionAsync(g => g.id_khach_hang == idKhachHang);
                if (!gioHangChiTiets.Any())
                    return (true, "Giỏ hàng đã trống");

                var result = await _repository.DeleteRangeAsync(gioHangChiTiets);
                return result
                    ? (true, "Xóa giỏ hàng thành công")
                    : (false, "Không thể xóa giỏ hàng");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa giỏ hàng: {ex.Message}");
            }
        }

        public async Task<(bool success, string message, int? soLuong)> KiemTraSoLuongTonAsync(Guid idSanPhamChiTiet)
        {
            try
            {
                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdAsync(idSanPhamChiTiet);
                if (sanPhamChiTiet == null)
                    return (false, "Sản phẩm không tồn tại", null);

                if (sanPhamChiTiet.trang_thai != "HoatDong")
                    return (false, "Sản phẩm hiện không khả dụng", null);

                return (true, "Lấy số lượng tồn thành công", sanPhamChiTiet.so_luong);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi kiểm tra số lượng tồn: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message)> CapNhatTrangThaiGioHangAsync(Guid idGioHangChiTiet, bool trangThai)
        {
            try
            {
                var gioHangChiTiet = await _repository.GetByIdAsync(idGioHangChiTiet);
                if (gioHangChiTiet == null)
                    return (false, "Không tìm thấy sản phẩm trong giỏ hàng");

                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdAsync(gioHangChiTiet.id_san_pham_chi_tiet);
                if (sanPhamChiTiet == null)
                    return (false, "Sản phẩm không tồn tại");

                if (sanPhamChiTiet.trang_thai != "HoatDong")
                    return (false, "Sản phẩm hiện không khả dụng");

                if (sanPhamChiTiet.so_luong < gioHangChiTiet.so_luong)
                    return (false, $"Số lượng tồn không đủ. Hiện chỉ còn {sanPhamChiTiet.so_luong} sản phẩm");

                gioHangChiTiet.trang_thai = trangThai;
                var result = await _repository.UpdateAsync(gioHangChiTiet);
                return result
                    ? (true, "Cập nhật trạng thái thành công")
                    : (false, "Không thể cập nhật trạng thái");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật trạng thái: {ex.Message}");
            }
        }
    }
}