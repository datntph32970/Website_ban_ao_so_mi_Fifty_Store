using System.Linq.Expressions;
using API.DbConects.DTOs.Admin.KhuyenMai;
using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Khuyen_Mai;
using API.DbConects.Entities.Entities_San_Pham;
using API.Repositories.Interfaces;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.VisualBasic;

namespace API.Services.Implementations
{
    public class SanPhamService : BaseService<SanPham>, ISanPhamService
    {
        private readonly IBaseRepository<SanPham> _repository;
        private readonly IBaseRepository<GiamGia> _giamGiaRepository;
        private readonly IBaseRepository<HoaDon> _hoaDonRepository;
        private readonly IBaseRepository<KhuyenMai> _khuyenMaiRepository;
        private readonly IThongKeService _thongKeService;
        private readonly IBaseRepository<SanPhamChiTiet> _sanPhamChiTietRepository;
        public SanPhamService(IBaseRepository<SanPham> repository, IBaseRepository<SanPhamChiTiet> sanPhamChiTietRepository, IBaseRepository<GiamGia> giamGiaRepository, IBaseRepository<KhuyenMai> khuyenMaiRepository, IBaseRepository<HoaDon> hoaDonRepository, IThongKeService thongKeService) : base(repository)
        {
            _repository = repository;
            _sanPhamChiTietRepository = sanPhamChiTietRepository;
            _giamGiaRepository = giamGiaRepository;
            _khuyenMaiRepository = khuyenMaiRepository;
            _hoaDonRepository = hoaDonRepository;
            _thongKeService = thongKeService;
        }
        public async Task<List<SanPhamAdminDTO>> GetAllSanPhamAdminDTOAsync()
        {
            try
            {
                await RemoveInvalidDiscountsAsync();
                var result = await _repository.GetAllWithIncludeAsync(q => q.Include(s => s.DanhMuc)
                                                               .Include(s => s.ThuongHieu)
                                                               .Include(s => s.KieuDang)
                                                               .Include(s => s.ChatLieu)
                                                               .Include(s => s.XuatXu)
                                                               .Include(s => s.anhMacDinh)
                                                               .Include(s => s.SanPhamChiTiets)
                                                               .ThenInclude(spct => spct.GiamGia));

                var sanPhams = new List<SanPhamAdminDTO>();
                foreach (var s in result)
                {
                    var chiTiets = new List<SanPhamChiTietAdminDTO>();
                    foreach (var spct in s.SanPhamChiTiets)
                    {
                        var soLuongDaBan = await _thongKeService.TinhSoLuongSanPhamChiTietDaBan(spct.id_san_pham_chi_tiet);
                        chiTiets.Add(new SanPhamChiTietAdminDTO
                        {
                            id_san_pham_chi_tiet = spct.id_san_pham_chi_tiet,
                            ma_san_pham_chi_tiet = spct.ma_san_pham_chi_tiet,
                            so_luong = spct.so_luong,
                            so_luong_da_ban = soLuongDaBan,
                            gia_ban = spct.gia_ban,
                            gia_nhap = spct.gia_nhap,
                            trang_thai = spct.trang_thai,
                            ngay_tao = spct.ngay_tao,
                            ngay_sua = spct.ngay_sua,
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
                        });
                    }

                    var sanPham = new SanPhamAdminDTO
                    {
                        id_san_pham = s.id_san_pham,
                        ma_san_pham = s.ma_san_pham,
                        ten_san_pham = s.ten_san_pham,
                        mo_ta = s.mo_ta,
                        trang_thai = s.trang_thai,
                        ngay_tao = s.ngay_tao,
                        ngay_sua = s.ngay_sua,
                        url_anh_mac_dinh = s.anhMacDinh?.url,
                        thuongHieu = s.ThuongHieu != null ? new ThuongHieuAdminDTO
                        {
                            id_thuong_hieu = s.ThuongHieu.id_thuong_hieu,
                            ma_thuong_hieu = s.ThuongHieu.ma_thuong_hieu,
                            ten_thuong_hieu = s.ThuongHieu.ten_thuong_hieu
                        } : null,
                        danhMuc = s.DanhMuc != null ? new DanhMucAdminDTO
                        {
                            id_danh_muc = s.DanhMuc.id_danh_muc,
                            ma_danh_muc = s.DanhMuc.ma_danh_muc,
                            ten_danh_muc = s.DanhMuc.ten_danh_muc
                        } : null,
                        sanPhamChiTiets = chiTiets
                    };
                    sanPhams.Add(sanPham);
                }

                return sanPhams;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        //tạo hàm gỡ id giảm giá nếu bị ngừng hoạt động hoặc đã hết hạn hoặc hết số lượng

        public async Task<SanPhamAdminDTO> GetByIdSanPhamAdminDTOAsync(Guid id)
        {
            await RemoveInvalidDiscountsAsync();

            // Load all data upfront in a single query
            var result = await _repository.GetByIdWithIncludeAsync(id, q => q.Include(s => s.DanhMuc)
                                                               .Include(s => s.ThuongHieu)
                                                               .Include(s => s.ChatLieu)
                                                               .Include(s => s.KieuDang)
                                                               .Include(s => s.anhMacDinh)
                                                               .Include(s => s.XuatXu)
                                                               .Include(s => s.NguoiTao)
                                                               .Include(s => s.NguoiSua)
                                                               .Include(s => s.SanPhamChiTiets)
                                                                   .ThenInclude(spct => spct.HinhAnhSanPhamChiTiets)
                                                                       .ThenInclude(ha => ha.HinhAnhs)
                                                               .Include(s => s.SanPhamChiTiets)
                                                                   .ThenInclude(spct => spct.MauSac)
                                                               .Include(s => s.SanPhamChiTiets)
                                                                   .ThenInclude(spct => spct.KichCo)
                                                               .Include(s => s.SanPhamChiTiets)
                                                                   .ThenInclude(spct => spct.GiamGia));

            if (result == null) return null;

            // Pre-calculate số lượng đã bán for all SanPhamChiTiet
            var soLuongDaBanDict = new Dictionary<Guid, int>();
            foreach (var spct in result.SanPhamChiTiets)
            {
                var soLuongDaBan = await _thongKeService.TinhSoLuongSanPhamChiTietDaBan(spct.id_san_pham_chi_tiet);
                soLuongDaBanDict[spct.id_san_pham_chi_tiet] = soLuongDaBan;
            }

            // Create DTO with pre-calculated data
            SanPhamAdminDTO sanPhamDTO = new SanPhamAdminDTO
            {
                id_san_pham = result.id_san_pham,
                ma_san_pham = result.ma_san_pham,
                ten_san_pham = result.ten_san_pham,
                mo_ta = result.mo_ta,
                url_anh_mac_dinh = result.anhMacDinh?.url,
                trang_thai = result.trang_thai,
                ngay_tao = result.ngay_tao,
                ngay_sua = result.ngay_sua,
                ma_nguoi_tao = result.NguoiTao?.ma_nhan_vien,
                ma_nguoi_sua = result.NguoiSua?.ma_nhan_vien,
                ten_nguoi_tao = result.NguoiTao?.ten_nhan_vien,
                ten_nguoi_sua = result.NguoiSua?.ten_nhan_vien,
                thuongHieu = result.ThuongHieu != null ? new ThuongHieuAdminDTO
                {
                    id_thuong_hieu = result.ThuongHieu.id_thuong_hieu,
                    ma_thuong_hieu = result.ThuongHieu.ma_thuong_hieu,
                    ten_thuong_hieu = result.ThuongHieu.ten_thuong_hieu
                } : null,
                danhMuc = result.DanhMuc != null ? new DanhMucAdminDTO
                {
                    id_danh_muc = result.DanhMuc.id_danh_muc,
                    ma_danh_muc = result.DanhMuc.ma_danh_muc,
                    ten_danh_muc = result.DanhMuc.ten_danh_muc
                } : null,
                kieuDang = result.KieuDang != null ? new KieuDangAdminDTO
                {
                    id_kieu_dang = result.KieuDang.id_kieu_dang,
                    ma_kieu_dang = result.KieuDang.ma_kieu_dang,
                    ten_kieu_dang = result.KieuDang.ten_kieu_dang
                } : null,
                chatLieu = result.ChatLieu != null ? new ChatLieuAdminDTO
                {
                    id_chat_lieu = result.ChatLieu.id_chat_lieu,
                    ma_chat_lieu = result.ChatLieu.ma_chat_lieu,
                    ten_chat_lieu = result.ChatLieu.ten_chat_lieu
                } : null,
                xuatXu = result.XuatXu != null ? new XuatXuAdminDTO
                {
                    id_xuat_xu = result.XuatXu.id_xuat_xu,
                    ma_xuat_xu = result.XuatXu.ma_xuat_xu,
                    ten_xuat_xu = result.XuatXu.ten_xuat_xu
                } : null,
                sanPhamChiTiets = result.SanPhamChiTiets.Select(spct => new SanPhamChiTietAdminDTO
                {
                    id_san_pham_chi_tiet = spct.id_san_pham_chi_tiet,
                    ma_san_pham_chi_tiet = spct.ma_san_pham_chi_tiet,
                    so_luong = spct.so_luong,
                    so_luong_da_ban = soLuongDaBanDict[spct.id_san_pham_chi_tiet],
                    gia_ban = spct.gia_ban,
                    gia_nhap = spct.gia_nhap,
                    trang_thai = spct.trang_thai,
                    ngay_tao = spct.ngay_tao,
                    ngay_sua = spct.ngay_sua,
                    ma_nguoi_tao = spct.NguoiTao?.ma_nhan_vien,
                    ma_nguoi_sua = spct.NguoiSua?.ma_nhan_vien,
                    ten_nguoi_tao = spct.NguoiTao?.ten_nhan_vien,
                    ten_nguoi_sua = spct.NguoiSua?.ten_nhan_vien,
                    hinhAnhSanPhamChiTiets = spct.HinhAnhSanPhamChiTiets.Select(ha => new HinhAnhSanPhamChiTietAdminDTO
                    {
                        hinh_anh_urls = ha.HinhAnhs.url,
                        id_hinh_anh = ha.HinhAnhs.id_hinh_anh,
                    }).ToList(),
                    mauSac = new MauSacAdminDTO
                    {
                        id_mau_sac = spct.MauSac.id_mau_sac,
                        ma_mau_sac = spct.MauSac.ma_mau_sac,
                        ten_mau_sac = spct.MauSac.ten_mau_sac
                    },
                    kichCo = new KichCoAdminDTO
                    {
                        id_kich_co = spct.KichCo.id_kich_co,
                        ma_kich_co = spct.KichCo.ma_kich_co,
                        ten_kich_co = spct.KichCo.ten_kich_co
                    },
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
            };

            return sanPhamDTO;
        }

        public Task<List<SanPham>> GetSanPhamByChatLieuAsync(Guid chatLieuId)
        {
            throw new NotImplementedException();
        }

        public Task<List<SanPham>> GetSanPhamByDanhMucAsync(Guid danhMucId)
        {
            throw new NotImplementedException();
        }

        public Task<List<SanPham>> GetSanPhamByKieuDangAsync(Guid kieuDangId)
        {
            throw new NotImplementedException();
        }

        public Task<List<SanPham>> GetSanPhamByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            throw new NotImplementedException();
        }

        public Task<List<SanPham>> GetSanPhamByThuongHieuAsync(Guid thuongHieuId)
        {
            throw new NotImplementedException();
        }

        public Task<List<SanPham>> GetSanPhamByXuatXuAsync(Guid xuatXuId)
        {
            throw new NotImplementedException();
        }

        public Task<SanPham> GetSanPhamWithDetailsAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<SanPham>> SearchSanPhamAsync(string keyword)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateTrangThaiAsync(Guid id, string trangThai)
        {
            try
            {
                var sanPham = await _repository.GetByIdAsync(id);
                if (sanPham == null) return false;
                if (trangThai != "KhongHoatDong" && trangThai != "HoatDong")
                    return false;
                sanPham.trang_thai = trangThai;
                sanPham.ngay_sua = DateTime.Now;
                //cập nhật trạng thái của spct
                var spcts = await _sanPhamChiTietRepository.GetByConditionAsync(spct => spct.id_san_pham == id);
                foreach (var spct in spcts)
                {
                    if (trangThai == "KhongHoatDong")
                    {
                        spct.trang_thai = "KhongHoatDong";
                    }
                    else
                    {
                        spct.trang_thai = "HoatDong";
                    }
                    var updateSpct = await _sanPhamChiTietRepository.UpdateAsync(spct);
                    if (!updateSpct)
                    {
                        return false;
                    }
                }
                var result = await _repository.UpdateAsync(sanPham);
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<List<SanPhamAdminDTO>> GetByConditionWithIncludeAsync(Expression<Func<SanPham, bool>> condition, params Func<IQueryable<SanPham>, IIncludableQueryable<SanPham, object>>[] includes)
        {
            try
            {
                await RemoveInvalidDiscountsAsync();
                var query = await _repository.GetByConditionWithIncludeAsync(condition, includes);

                var sanPhams = new List<SanPhamAdminDTO>();
                foreach (var s in query)
                {
                    var chiTiets = new List<SanPhamChiTietAdminDTO>();
                    foreach (var spct in s.SanPhamChiTiets)
                    {
                        var soLuongDaBan = await _thongKeService.TinhSoLuongSanPhamChiTietDaBan(spct.id_san_pham_chi_tiet);
                        chiTiets.Add(new SanPhamChiTietAdminDTO
                        {
                            id_san_pham_chi_tiet = spct.id_san_pham_chi_tiet,
                            ma_san_pham_chi_tiet = spct.ma_san_pham_chi_tiet,
                            so_luong = spct.so_luong,
                            so_luong_da_ban = soLuongDaBan,
                            gia_ban = spct.gia_ban,
                            gia_nhap = spct.gia_nhap,
                            trang_thai = spct.trang_thai,
                            ngay_tao = spct.ngay_tao,
                            ngay_sua = spct.ngay_sua,
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
                        });
                    }

                    var sanPham = new SanPhamAdminDTO
                    {
                        id_san_pham = s.id_san_pham,
                        ma_san_pham = s.ma_san_pham,
                        ten_san_pham = s.ten_san_pham,
                        mo_ta = s.mo_ta,
                        trang_thai = s.trang_thai,
                        ngay_tao = s.ngay_tao,
                        ngay_sua = s.ngay_sua,
                        url_anh_mac_dinh = s.anhMacDinh?.url,
                        thuongHieu = s.ThuongHieu != null ? new ThuongHieuAdminDTO
                        {
                            id_thuong_hieu = s.ThuongHieu.id_thuong_hieu,
                            ma_thuong_hieu = s.ThuongHieu.ma_thuong_hieu,
                            ten_thuong_hieu = s.ThuongHieu.ten_thuong_hieu
                        } : null,
                        danhMuc = s.DanhMuc != null ? new DanhMucAdminDTO
                        {
                            id_danh_muc = s.DanhMuc.id_danh_muc,
                            ma_danh_muc = s.DanhMuc.ma_danh_muc,
                            ten_danh_muc = s.DanhMuc.ten_danh_muc
                        } : null,
                        sanPhamChiTiets = chiTiets
                    };
                    sanPhams.Add(sanPham);
                }

                return sanPhams;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<bool> RemoveInvalidDiscountsAsync()
        {
            try
            {
                var result = await _giamGiaRepository.ExecuteInTransactionAsync(async () =>
                {
                    var giamGias = await _giamGiaRepository.GetAllWithIncludeAsync(q => q.Include(gg => gg.SanPhamChiTiets));
                    foreach (var giamGia in giamGias)
                    {
                        if (giamGia.thoi_gian_ket_thuc < DateTime.Now || giamGia.so_luong_da_su_dung >= giamGia.so_luong_toi_da)
                        {
                            giamGia.trang_thai = "KhongHoatDong";
                            foreach (var spct in giamGia.SanPhamChiTiets)
                            {
                                spct.id_giam_gia = null;
                                var updatespct = await _sanPhamChiTietRepository.UpdateAsync(spct);
                                if (!updatespct)
                                {
                                    return false;
                                }
                            }
                            var result = await _giamGiaRepository.UpdateAsync(giamGia);
                            if (!result)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                });
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gỡ giảm giá không hợp lệ: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> RemoveInvalidPromotionsAsync()
        {
            try
            {
                var result = await _khuyenMaiRepository.ExecuteInTransactionAsync(async () =>
                {
                    var khuyenMais = await _khuyenMaiRepository.GetAllWithIncludeAsync(q => q.Include(km => km.HoaDons));
                    foreach (var khuyenMai in khuyenMais)
                    {
                        if (khuyenMai.thoi_gian_ket_thuc < DateTime.Now || khuyenMai.so_luong_da_su_dung >= khuyenMai.so_luong_toi_da)
                        {
                            khuyenMai.trang_thai = "KhongHoatDong";
                            foreach (var hoaDon in khuyenMai.HoaDons)
                            {
                                if (hoaDon.trang_thai_hoa_don == "ChoTaiQuay")
                                {
                                    hoaDon.id_khuyen_mai = null;
                                    hoaDon.so_tien_khuyen_mai = 0;
                                    hoaDon.tong_tien_phai_thanh_toan = hoaDon.tong_tien_don_hang;
                                    var updatehoaDon = await _hoaDonRepository.UpdateAsync(hoaDon);
                                    if (!updatehoaDon)
                                    {
                                        return false;
                                    }
                                }
                            }
                            var result = await _khuyenMaiRepository.UpdateAsync(khuyenMai);
                            if (!result)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                });
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gỡ khuyến mãi không hợp lệ: {ex.Message}");
                return false;
            }
        }
    }
}
