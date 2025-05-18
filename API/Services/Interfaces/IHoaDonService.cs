using API.DbConects.DTOs.Admin.HoaDon;
using API.DbConects.DTOs.Client.HoaDon;
using API.DbConects.Entities.Entities_Hoa_Don;
using API.Services.Implementations;
namespace API.Services.Interfaces
{
    public interface IHoaDonService : IBaseService<HoaDon>
    {
        Task<List<HoaDonAdminDTO>> GetAllHoaDonAdminDTOAsync();
        Task<HoaDonAdminDTO> GetByIdHoaDonAdminDTOAsync(Guid id);
        Task<List<HoaDonAdminDTO>> GetHoaDonBySanPhamChiTietAsync(Guid sanPhamChiTietId);
        Task<(bool, string)> ThemHoaDonBanTaiQuayMoiAsync(Guid id_nhan_vien_xu_ly);
        Task<(bool, string)> XoaHoaDon(Guid id_hoa_don);
        Task<(bool, string)> XoaHoaDonChiTiet(Guid id_hoa_don_chi_tiet);
        Task<(bool success, string message)> ThemHoaDonChiTiet(Guid id_hoa_don, Guid id_san_pham_chi_tiet, int so_luong, string ghi_chu);
        Task<(bool success, string message)> CapNhatHoaDonChiTiet(Guid id_hoa_don, Guid id_san_pham_chi_tiet, int so_luong, string ghi_chu);
        Task<HoaDonAdminDTO> GetHoaDonBanTaiQuayByIdAsync(Guid id_hoa_don, Guid id_nhan_vien_xu_ly);
        Task<(decimal tongTienSauKhuyenMai, decimal giaTriKhuyenMai)> CapNhatTongTienVaGiaTriKhuyenMai(Guid id_hoa_don);
        Task<(bool success, string message)> ThanhToanHoaDonChoTaiQuay(Guid id_hoa_don);
        Task<(bool success, string message, Guid id_hoa_don)> TaoHoaDonOnlineTrangThaiChuaThanhToan(Guid id_khach_hang, decimal phi_van_chuyen);
        Task<(bool success, string message)> CapNhatHoaDonOnline(
            Guid idHoaDon,
            string? IddiaChiNhanHang,
            string? ghiChu,
            string? idKhuyenMai,
            string? idPhuongThucThanhToan,
            decimal phi_van_chuyen
        );
        Task<(bool success, string message)> XoaHoaDonChuaThanhToanQuaHan();
        // Thêm các phương thức mới cho luồng trạng thái
        Task<(bool success, string message)> HuyDonHangAsync(Guid idHoaDon, string lyDo, bool isKhachHangHuy = true, Guid? id_nhan_vien_xu_ly = null);
        Task<(bool success, string message)> HoanTienVNPayAsync(Guid idHoaDon);
        Task<(bool success, string message)> DanhDauHetHangAsync(Guid idHoaDon, string ghiChu, Guid id_nhan_vien_xu_ly);
        Task<(bool success, string message)> GuiEmailCapNhatTrangThaiAsync(Guid idHoaDon, string trangThai);
        Task<(bool success, string message)> XacNhanDonHangAsync(Guid idHoaDon, Guid id_nhan_vien_xu_ly);
        Task<(bool success, string message)> CapNhatTrangThaiDonHangAsync(Guid idHoaDon, string trangThai, Guid id_nhan_vien_xu_ly);

    }
}