using API.DbConects.Entities.Khuyen_Mai;
using API.DbConects.Entities.Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Hoa_Don
{
    public class HoaDon
    {
        [Key]
        public Guid id_hoa_don { get; set; }
        public string ma_hoa_don { get; set; }
        public DateTime ngay_tao { get; set; }
        public decimal tong_tien_don_hang { get; set; }
        public decimal so_tien_khuyen_mai { get; set; }
        public string ghi_chu { get; set; }
        public decimal tong_tien_phai_thanh_toan { get; set; }
        public string ten_khach_hang { get; set; }
        public string ten_nhan_vien { get; set; }
        public string sdt_khach_hang { get; set; }
        public string dia_chi_nhan_hang { get; set; }
        //
        [ForeignKey("KhachHang")]
        public Guid id_khach_hang { get; set; }
        [ForeignKey("NhanVien")]
        public Guid? id_nhan_vien { get; set; }
        [ForeignKey("KhuyenMai")]
        public Guid id_khuyen_mai { get; set; }
        [ForeignKey("PhuongThucThanhToan")]
        public Guid id_phuong_thuc_thanh_toan { get; set; }
        [ForeignKey("TrangThaiHoaDon")]
        public Guid id_trang_thai_hoa_don { get; set; }
        //
        public virtual KhachHang KhachHang { get; set; }
        public virtual NhanVien NhanVien { get; set; }
        public virtual KhuyenMai KhuyenMai { get; set; }
        public virtual PhuongThucThanhToan PhuongThucThanhToan { get; set; }
        public virtual TrangThaiHoaDon TrangThaiHoaDon { get; set; }
        public virtual ICollection<HoaDonChiTiet> HoaDonChiTiets { get; set; }


    }
}
