
namespace API.DbConects.DTO.HoaDonDTO
{
    
        public class Them_HoaDonDTO
        {
            public Guid ID_NhanVien { get; set; } // ID nhân viên (FK)
            public Guid ID_KhachHang { get; set; } // ID khách hàng (FK)
            public Guid? ID_KhuyenMai { get; set; } // ID khuyến mãi (FK, có thể null)
            public string MaHoaDon { get; set; } // Mã hóa đơn
            public Guid CreateBy { get; set; } // Người tạo
            public decimal TongTienDonHang { get; set; } // Tổng tiền đơn hàng
            public decimal SoTienKhuyenMai { get; set; } // Số tiền khuyến mãi
            public string GhiChu { get; set; } // Ghi chú
            public Guid ID_PhuongThucThanhToan { get; set; } // ID phương thức thanh toán (FK)
            public decimal TongTienPhaiThanhToan { get; set; } // Tổng tiền phải thanh toán
            public string TenKhachNhan { get; set; } // Tên khách nhận
            public string SoDienThoaiKhachNhan { get; set; } // SĐT khách nhận
            public string DiaChiNhan { get; set; } // Địa chỉ nhận
            public Guid ID_TrangThaiHoaDon { get; set; } // Trạng thái hóa đơn (FK)
        }

        public class Sua_HoaDonDTO
        {
            public decimal TongTienDonHang { get; set; }
            public decimal SoTienKhuyenMai { get; set; }
            public string GhiChu { get; set; }
            public decimal TongTienPhaiThanhToan { get; set; }
            public string TenKhachNhan { get; set; }
            public string SoDienThoaiKhachNhan { get; set; }
            public string DiaChiNhan { get; set; }
            public TrangThaiHoaDonDTO TrangThaiHoaDon { get; set; }
        }

        public enum TrangThaiHoaDonDTO
        {
            ChoThanhToan,
            DaThanhToan,
            DaHuy
        }
   

}
