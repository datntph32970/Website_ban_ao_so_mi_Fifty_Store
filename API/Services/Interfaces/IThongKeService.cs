using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;

namespace API.Services.Interfaces
{
    public interface IThongKeService
    {
        Task<decimal> TinhTongDoanhThuTheoThang(int thang, int nam);
        Task<decimal> TinhTongDoanhThuTheoNam(int nam);
        Task<decimal> TinhTongDoanhThuTheoNgay(DateOnly ngay);
        Task<decimal> TinhTongDoanhThuTheoTuan(int tuan, int nam);
        Task<int> TinhTongDonHangTheoThang(int thang, int nam);
        Task<int> TinhTongDonHangTheoNam(int nam);
        Task<int> TinhTongDonHangTheoTuan(int tuan, int nam);
        Task<int> TinhTongDonHangTheoNgay(DateOnly ngay);
        Task<int> TinhTongNhanVienTheoThang(int thang, int nam);
        Task<int> TinhTongNhanVienTheoNam(int nam);
        Task<List<(NhanVien, decimal)>> LayDanhSachNhanVienCoDoanhThuCaoNhatTheoThang(int thang, int nam);
        Task<List<(NhanVien, decimal)>> LayDanhSachNhanVienCoDoanhThuCaoNhatTheoNam(int nam);
        Task<int> TinhTongSanPhamMoiTheoThang(int thang, int nam);
        Task<int> TinhTongSanPhamMoiTheoNam(int nam);
        Task<int> TinhTongSanPhamMoiTheoTuan(int tuan, int nam);
        Task<int> TinhTongSanPhamDaBanTheoTuan(int tuan, int nam);
        Task<int> TinhTongSanPhamDaBanTheoThang(int thang, int nam);
        Task<int> TinhTongSanPhamDaBanTheoNam(int nam);
        Task<int> TinhTongSanPhamDaBanTheoNgay(DateOnly ngay);
        Task<List<SanPham>> LaySanPhamBanChayNhatTheoThang(int thang, int nam);
        Task<List<SanPham>> LaySanPhamBanChayNhatTheoNam(int nam);
        Task<List<SanPham>> LaySanPhamBanChayNhatTheoNgay(DateOnly ngay);
        Task<List<SanPham>> LaySanPhamBanChayNhatTheoTuan(int tuan, int nam);
        Task<int> TinhSoLuongSanPhamChiTietDaBan(Guid id_san_pham_chi_tiet);
        Task<int> TinhSoLuongSanPhamDaBan(Guid id_san_pham);
        DateTime GetStartDateOfWeek(int week, int year);
    }
}