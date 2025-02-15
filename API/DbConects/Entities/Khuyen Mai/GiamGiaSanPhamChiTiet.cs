using API.DbConects.Entities.Hoa_Don;
using API.DbConects.Entities.San_Pham;
using API.DbConects.Entities.Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Khuyen_Mai
{
    public class GiamGiaSanPhamChiTiet
    {
        [Key]
        public Guid id_giam_gia_san_pham_chi_tiet { get; set; }
        [ForeignKey("GiamGia")]
        public Guid id_giam_gia { get; set; }
        [ForeignKey("SanPhamChiTiet")]
        public Guid id_san_pham_chi_tiet { get; set; }
        public DateTime ngay_tao { get; set; }
        public DateTime ngay_cap_nhat { get; set; }
        [ForeignKey("NguoiTao")]
        public Guid id_nguoi_tao { get; set; }
        [ForeignKey("NguoiSua")]
        public Guid id_nguoi_cap_nhat { get; set; }
        public virtual GiamGia GiamGia { get; set; }
        public virtual SanPhamChiTiet SanPhamChiTiet { get; set; }
        public virtual NhanVien NguoiTao { get; set; }
        public virtual NhanVien NguoiSua { get; set; }
    }
}
