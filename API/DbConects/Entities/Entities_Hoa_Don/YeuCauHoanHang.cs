using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Hoa_Don
{
    public class YeuCauHoanHang
    {
        public Guid Id { get; set; }
        public Guid IdHoaDon { get; set; }
        public Guid IdKhachHang { get; set; }
        public string LyDoHoanHang { get; set; }
        public string MoTaChiTiet { get; set; }
        public decimal SoTienHoan { get; set; }
        public TrangThaiHoanHang TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string GhiChu { get; set; }

        // Navigation properties
        public HoaDon HoaDon { get; set; }
        public KhachHang KhachHang { get; set; }
        public ICollection<ChiTietHoanHang> ChiTietHoanHangs { get; set; }
        public ICollection<HinhAnhHoanHang> HinhAnhHoanHangs { get; set; }
    }
    public class HinhAnhHoanHang
    {
        public Guid Id { get; set; }
        public Guid IdYeuCauHoanHang { get; set; }
        public Guid idHinhAnh { get; set; }

        // Navigation properties
        public YeuCauHoanHang YeuCauHoanHang { get; set; }
        public HinhAnh hinhAnh { get; set; }
    }
    public class ChiTietHoanHang
    {
        public Guid Id { get; set; }
        public Guid IdYeuCauHoanHang { get; set; }
        public Guid id_san_pham_chi_tiet { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public string LyDo { get; set; }

        // Navigation properties
        public YeuCauHoanHang YeuCauHoanHang { get; set; }
        public SanPhamChiTiet SanPhamChiTiet { get; set; }
    }
    public enum TrangThaiHoanHang
    {
        ChoXacNhan,      // Chờ xác nhận từ admin
        DaXacNhan,       // Admin đã xác nhận
        DangXuLy,        // Đang xử lý hoàn hàng
        HoanThanh,       // Đã hoàn thành
        TuChoi,          // Yêu cầu bị từ chối
        HuyBo            // Khách hàng hủy yêu cầu
    }
}