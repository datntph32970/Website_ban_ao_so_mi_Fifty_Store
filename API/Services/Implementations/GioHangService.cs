using API.DbConects.DTOs.Client.HoaDon;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Khuyen_Mai;
using API.DbConects.Entities.Entities_San_Pham;
using API.Repositories.Interfaces;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace API.Services.Implementations
{
    public class GioHangService : BaseService<GioHangChiTiet>, IGioHangService
    {
        private readonly IBaseRepository<SanPhamChiTiet> _sanPhamChiTietRepository;
        private readonly IMemoryCache _cache;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        private const int CACHE_DURATION_SECONDS = 30;
        private const int MAX_ITEMS_PER_CART = 30;
        private const int MAX_QUANTITY_PER_ITEM = 100;

        public GioHangService(
            IBaseRepository<GioHangChiTiet> repository,
            IBaseRepository<SanPhamChiTiet> sanPhamChiTietRepository,
            IMemoryCache cache) : base(repository)
        {
            _sanPhamChiTietRepository = sanPhamChiTietRepository;
            _cache = cache;
        }

        private string GetCacheKey(Guid idKhachHang) => $"cart_{idKhachHang}";

        private SemaphoreSlim GetLock(string key)
        {
            return _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
        }

        private async Task<GiamGia> GetActiveDiscount(SanPhamChiTiet sanPhamChiTiet)
        {
            if (sanPhamChiTiet.SanPhamChiTietGiamGias == null || !sanPhamChiTiet.SanPhamChiTietGiamGias.Any())
                return null;

            var now = DateTime.Now;
            return sanPhamChiTiet.SanPhamChiTietGiamGias
                .Where(spctgg => spctgg.GiamGia.trang_thai == "HoatDong" &&
                       spctgg.GiamGia.thoi_gian_bat_dau <= now &&
                       spctgg.GiamGia.thoi_gian_ket_thuc >= now)
                .Select(spctgg => spctgg.GiamGia)
                .FirstOrDefault();
        }

        public async Task<IEnumerable<GioHangItemClientDTO>> GetGioHangByKhachHangAsync(Guid idKhachHang)
        {
            var cacheKey = GetCacheKey(idKhachHang);
            if (_cache.TryGetValue(cacheKey, out IEnumerable<GioHangItemClientDTO> cachedCart))
            {
                return cachedCart;
            }

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
                    .ThenInclude(spct => spct.SanPhamChiTietGiamGias)
                    .ThenInclude(spctgg => spctgg.GiamGia)
                    .Include(k => k.SanPhamChiTiet)
                    .ThenInclude(spct => spct.HinhAnhSanPhamChiTiets)
                    .ThenInclude(haspct => haspct.HinhAnhs)
            );

            var result = await Task.WhenAll(gioHangChiTiets.Select(async g =>
            {
                var activeDiscount = await GetActiveDiscount(g.SanPhamChiTiet);
                return new GioHangItemClientDTO
                {
                    id_gio_hang_chi_tiet = g.id_gio_hang_chi_tiet,
                    id_san_pham_chi_tiet = g.id_san_pham_chi_tiet,
                    ma_san_pham_chi_tiet = g.SanPhamChiTiet.ma_san_pham_chi_tiet,
                    ten_san_pham = g.SanPhamChiTiet.SanPham.ten_san_pham,
                    ten_mau_sac = g.SanPhamChiTiet.MauSac.ten_mau_sac,
                    ten_kich_co = g.SanPhamChiTiet.KichCo.ten_kich_co,
                    so_luong = g.so_luong,
                    gia_ban = g.SanPhamChiTiet.gia_ban,
                    gia_sau_giam = activeDiscount != null ? TinhGiaSauGiam(g.SanPhamChiTiet.gia_ban, activeDiscount) : null,
                    url_anh = g.SanPhamChiTiet.HinhAnhSanPhamChiTiets.FirstOrDefault()?.HinhAnhs?.url ?? g.SanPhamChiTiet.SanPham.anhMacDinh?.url,
                    trang_thai = g.trang_thai,
                    so_luong_ton = g.SanPhamChiTiet.so_luong,
                    id_giam_gia = activeDiscount?.id_giam_gia,
                    ten_giam_gia = activeDiscount?.ten_giam_gia,
                    kieu_giam_gia = activeDiscount?.kieu_giam_gia,
                    gia_tri_giam = activeDiscount?.gia_tri_giam,
                    thoi_gian_bat_dau = activeDiscount?.thoi_gian_bat_dau,
                    thoi_gian_ket_thuc = activeDiscount?.thoi_gian_ket_thuc
                };
            }));

            _cache.Set(cacheKey, result, TimeSpan.FromSeconds(CACHE_DURATION_SECONDS));
            return result;
        }

        public async Task<IEnumerable<GioHangItemClientDTO>> GetGioHangDaChonAsync(Guid idKhachHang)
        {
            var allItems = await GetGioHangByKhachHangAsync(idKhachHang);
            return allItems.Where(item => item.trang_thai);
        }

        private decimal TinhGiaSauGiam(decimal giaBan, GiamGia giamGia)
        {
            if (giamGia == null) return giaBan;

            decimal giaSauGiam = giaBan;
            if (giamGia.kieu_giam_gia == "PhanTram")
            {
                giaSauGiam = giaBan - (giaBan * (giamGia.gia_tri_giam / 100));
            }
            else if (giamGia.kieu_giam_gia == "SoTien")
            {
                giaSauGiam = giaBan - giamGia.gia_tri_giam;
            }

            return Math.Max(0, giaSauGiam);
        }

        public async Task<(bool success, string message)> ThemSanPhamVaoGioHangAsync(Guid idKhachHang, Guid idSanPhamChiTiet, int soLuong)
        {
            var lockKey = $"cart_operation_{idKhachHang}_{idSanPhamChiTiet}";
            var lockObj = GetLock(lockKey);

            try
            {
                await lockObj.WaitAsync();

                if (soLuong <= 0)
                    return (false, "Số lượng phải lớn hơn 0");

                if (soLuong > MAX_QUANTITY_PER_ITEM)
                    return (false, $"Số lượng không được vượt quá {MAX_QUANTITY_PER_ITEM}");

                var currentCartItems = await _repository.GetByConditionAsync(g => g.id_khach_hang == idKhachHang);
                if (currentCartItems.Count() >= MAX_ITEMS_PER_CART)
                    return (false, $"Giỏ hàng không thể chứa quá {MAX_ITEMS_PER_CART} sản phẩm");

                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdAsync(idSanPhamChiTiet);
                if (sanPhamChiTiet == null)
                    return (false, "Sản phẩm không tồn tại");

                if (sanPhamChiTiet.trang_thai != "HoatDong")
                    return (false, "Sản phẩm hiện không khả dụng");

                if (sanPhamChiTiet.so_luong < soLuong)
                    return (false, $"Số lượng tồn không đủ. Hiện chỉ còn {sanPhamChiTiet.so_luong} sản phẩm");

                var existingItem = currentCartItems.FirstOrDefault(g => g.id_san_pham_chi_tiet == idSanPhamChiTiet);
                if (existingItem != null)
                {
                    var newQuantity = existingItem.so_luong + soLuong;
                    if (newQuantity > MAX_QUANTITY_PER_ITEM)
                        return (false, $"Tổng số lượng không được vượt quá {MAX_QUANTITY_PER_ITEM}");

                    if (sanPhamChiTiet.so_luong < newQuantity)
                        return (false, $"Số lượng tồn không đủ. Hiện chỉ còn {sanPhamChiTiet.so_luong} sản phẩm");

                    existingItem.so_luong = newQuantity;
                    var updateResult = await _repository.UpdateAsync(existingItem);
                    if (updateResult)
                    {
                        _cache.Remove(GetCacheKey(idKhachHang));
                        return (true, "Cập nhật số lượng trong giỏ hàng thành công");
                    }
                    return (false, "Không thể cập nhật số lượng trong giỏ hàng");
                }

                var gioHangChiTiet = new GioHangChiTiet
                {
                    id_gio_hang_chi_tiet = Guid.NewGuid(),
                    id_khach_hang = idKhachHang,
                    id_san_pham_chi_tiet = idSanPhamChiTiet,
                    so_luong = soLuong,
                    trang_thai = true
                };

                var result = await _repository.CreateAsync(gioHangChiTiet);
                if (result)
                {
                    _cache.Remove(GetCacheKey(idKhachHang));
                    return (true, "Thêm sản phẩm vào giỏ hàng thành công");
                }
                return (false, "Không thể thêm sản phẩm vào giỏ hàng");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi thêm sản phẩm vào giỏ hàng: {ex.Message}");
            }
            finally
            {
                lockObj.Release();
            }
        }

        public async Task<(bool success, string message)> CapNhatSoLuongAsync(Guid idGioHangChiTiet, int soLuong)
        {
            var gioHangChiTiet = await _repository.GetByIdAsync(idGioHangChiTiet);
            if (gioHangChiTiet == null)
                return (false, "Không tìm thấy sản phẩm trong giỏ hàng");

            var lockKey = $"cart_operation_{gioHangChiTiet.id_khach_hang}_{gioHangChiTiet.id_san_pham_chi_tiet}";
            var lockObj = GetLock(lockKey);

            try
            {
                await lockObj.WaitAsync();

                if (soLuong < 0)
                    return (false, "Số lượng không được âm");

                if (soLuong > MAX_QUANTITY_PER_ITEM)
                    return (false, $"Số lượng không được vượt quá {MAX_QUANTITY_PER_ITEM}");

                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdAsync(gioHangChiTiet.id_san_pham_chi_tiet);
                if (sanPhamChiTiet == null)
                    return (false, "Sản phẩm không tồn tại");

                if (sanPhamChiTiet.trang_thai != "HoatDong")
                    return (false, "Sản phẩm hiện không khả dụng");

                if (sanPhamChiTiet.so_luong < soLuong)
                    return (false, $"Số lượng tồn không đủ. Hiện chỉ còn {sanPhamChiTiet.so_luong} sản phẩm");

                if (soLuong == 0)
                {
                    var deleteResult = await _repository.DeleteAsync(idGioHangChiTiet);
                    if (deleteResult)
                    {
                        _cache.Remove(GetCacheKey(gioHangChiTiet.id_khach_hang));
                        return (true, "Đã xóa sản phẩm khỏi giỏ hàng");
                    }
                    return (false, "Không thể xóa sản phẩm khỏi giỏ hàng");
                }

                gioHangChiTiet.so_luong = soLuong;
                var result = await _repository.UpdateAsync(gioHangChiTiet);
                if (result)
                {
                    _cache.Remove(GetCacheKey(gioHangChiTiet.id_khach_hang));
                    return (true, "Cập nhật số lượng thành công");
                }
                return (false, "Không thể cập nhật số lượng");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật số lượng: {ex.Message}");
            }
            finally
            {
                lockObj.Release();
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
                if (result)
                {
                    _cache.Remove(GetCacheKey(gioHangChiTiet.id_khach_hang));
                    return (true, "Xóa sản phẩm khỏi giỏ hàng thành công");
                }
                return (false, "Không thể xóa sản phẩm khỏi giỏ hàng");
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
                if (result)
                {
                    _cache.Remove(GetCacheKey(idKhachHang));
                    return (true, "Xóa giỏ hàng thành công");
                }
                return (false, "Không thể xóa giỏ hàng");
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
            var gioHangChiTiet = await _repository.GetByIdAsync(idGioHangChiTiet);
            if (gioHangChiTiet == null)
                return (false, "Không tìm thấy sản phẩm trong giỏ hàng");

            var lockKey = $"cart_operation_{gioHangChiTiet.id_khach_hang}_{gioHangChiTiet.id_san_pham_chi_tiet}";
            var lockObj = GetLock(lockKey);

            try
            {
                await lockObj.WaitAsync();

                var sanPhamChiTiet = await _sanPhamChiTietRepository.GetByIdAsync(gioHangChiTiet.id_san_pham_chi_tiet);
                if (sanPhamChiTiet == null)
                    return (false, "Sản phẩm không tồn tại");

                if (sanPhamChiTiet.trang_thai != "HoatDong")
                    return (false, "Sản phẩm hiện không khả dụng");

                if (trangThai && sanPhamChiTiet.so_luong < gioHangChiTiet.so_luong)
                    return (false, $"Số lượng tồn không đủ. Hiện chỉ còn {sanPhamChiTiet.so_luong} sản phẩm");

                gioHangChiTiet.trang_thai = trangThai;
                var result = await _repository.UpdateAsync(gioHangChiTiet);
                if (result)
                {
                    _cache.Remove(GetCacheKey(gioHangChiTiet.id_khach_hang));
                    return (true, "Cập nhật trạng thái thành công");
                }
                return (false, "Không thể cập nhật trạng thái");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật trạng thái: {ex.Message}");
            }
            finally
            {
                lockObj.Release();
            }
        }
    }
}