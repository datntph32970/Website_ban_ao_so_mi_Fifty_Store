using API.DbConects.Entities.Entities_Hoa_Don;

namespace API.Services.Interfaces
{
    public interface IGioHangService : IBaseService<GioHangChiTiet>
    {
        Task<IEnumerable<GioHangChiTiet>> GetGioHangByKhachHangAsync(Guid idKhachHang);
        Task<bool> ThemSanPhamVaoGioHangAsync(Guid idKhachHang, Guid idSanPhamChiTiet, int soLuong);
        Task<bool> CapNhatSoLuongAsync(Guid idGioHangChiTiet, int soLuong);
        Task<bool> XoaSanPhamKhoiGioHangAsync(Guid idGioHangChiTiet);
        Task<bool> XoaGioHangAsync(Guid idKhachHang);
    }
}