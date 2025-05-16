using API.DbConects.DTOs.Admin.HoaDon;
using API.DbConects.Entities.Entities_Hoa_Don;

namespace API.Services.Interfaces
{
    public interface IHoaDonService : IBaseService<HoaDon>
    {
        Task<List<HoaDonAdminDTO>> GetAllHoaDonAdminDTOAsync();
        Task<HoaDonAdminDTO> GetByIdHoaDonAdminDTOAsync(Guid id);
        Task<List<HoaDonAdminDTO>> GetHoaDonBySanPhamChiTietAsync(Guid sanPhamChiTietId);
        Task<(bool, string)> ThemHoaDonBanTaiQuayMoiAsync(Guid id_nguoi_tao);
        Task<(bool, string)> XoaHoaDon(Guid id_hoa_don);
        Task<(bool, string)> XoaHoaDonChiTiet(Guid id_hoa_don_chi_tiet);
        Task<(bool success, string message)> CapNhatHoaDonChiTiet(Guid id_hoa_don, Guid id_san_pham_chi_tiet, int so_luong, string ghi_chu);
        Task<(decimal tongTienSauKhuyenMai, decimal giaTriKhuyenMai)> CapNhatTongTienVaGiaTriKhuyenMai(Guid id_hoa_don);
        Task<HoaDonAdminDTO> GetHoaDonBanTaiQuayByIdAsync(Guid id_hoa_don, Guid id_nguoi_tao);
        Task<(bool success, string message)> ThemHoaDonChiTiet(Guid id_hoa_don, Guid id_san_pham_chi_tiet, int so_luong, string ghi_chu);
        Task<(bool success, string message)> ThanhToanHoaDonChoTaiQuay(Guid id_hoa_don);
    }
}