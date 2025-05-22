using API.DbConects.Entities.Entities_Hoa_Don;

namespace API.DbConects.DTOs.Admin.HoaDon
{
    // API/DbConects/DTOs/HoaDon/YeuCauHoanHangDTO.cs
    public class TaoYeuCauHoanHangDTO
    {
        public Guid IdHoaDon { get; set; }
        public string LyDoHoanHang { get; set; }
        public string MoTaChiTiet { get; set; }
        public List<string> HinhAnhBase64 { get; set; }
        public List<ChiTietHoanHangDTO> ChiTietHoanHang { get; set; }
    }

    public class ChiTietHoanHangDTO
    {
        public Guid id_san_pham_chi_tiet { get; set; }
        public int SoLuong { get; set; }
        public string LyDo { get; set; }
    }

    public class YeuCauHoanHangDTO
    {
        public Guid Id { get; set; }
        public string MaHoaDon { get; set; }
        public string TenKhachHang { get; set; }
        public string LyDoHoanHang { get; set; }
        public string MoTaChiTiet { get; set; }
        public List<string> HinhAnh { get; set; }
        public int TongSoLuong => ChiTietHoanHang?.Sum(ct => ct.SoLuong) ?? 0;
        public decimal SoTienHoan { get; set; }
        public TrangThaiHoanHang TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string GhiChu { get; set; }
        public List<ChiTietHoanHangDTO> ChiTietHoanHang { get; set; }
    }

    public class CapNhatTrangThaiHoanHangDTO
    {
        public TrangThaiHoanHang TrangThai { get; set; }
        public string GhiChu { get; set; }
    }
    public class ThamSoPhanTrangYeuCauHoanHangDTO
    {
        public int trang_hien_tai { get; set; }
        public int so_phan_tu_tren_trang { get; set; }
        public string? tim_kiem { get; set; }
        public string? trang_thai { get; set; }
        public string? loai_hoa_don { get; set; }
        public string? id_phuong_thuc_thanh_toan { get; set; }
        public string? ngay_tao_tu { get; set; }
        public string? ngay_tao_den { get; set; }
    }
    public class PhanTrangYeuCauHoanHangDTO
    {
        public int trang_hien_tai { get; set; }
        public int so_phan_tu_tren_trang { get; set; }
        public int tong_so_trang { get; set; }
        public int tong_so_phan_tu { get; set; }
        public List<YeuCauHoanHangDTO> danh_sach { get; set; }
    }
}