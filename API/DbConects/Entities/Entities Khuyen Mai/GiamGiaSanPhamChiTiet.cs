using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Khuyen_Mai
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
        public Guid id_nguoi_tao { get; set; }
        public Guid id_nguoi_cap_nhat { get; set; }
        public virtual GiamGia GiamGia { get; set; }
        public virtual SanPhamChiTiet SanPhamChiTiet { get; set; }
        [ForeignKey("id_nguoi_tao")]
        public virtual NhanVien NguoiTao { get; set; }
        [ForeignKey("id_nguoi_cap_nhat")]
        public virtual NhanVien NguoiSua { get; set; }
    }
}
