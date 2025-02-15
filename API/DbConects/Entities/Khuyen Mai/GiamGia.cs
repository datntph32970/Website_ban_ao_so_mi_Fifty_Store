using API.DbConects.Entities.Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Khuyen_Mai
{
    public class GiamGia
    {
        [Key]
        public Guid id_giam_gia { get; set; }
        public string ten_giam_gia { get; set; }
        public string mo_ta { get; set; }
        public string loai_giam_gia { get; set; }
        public DateTime thoi_gian_bat_dau { get; set; }
        public DateTime thoi_gian_ket_thuc { get; set; }
        public string trang_thai {  get; set; }
        public DateTime ngay_tao { get; set; }
        public DateTime ngay_cap_nhat { get; set; }
        [ForeignKey("NguoiTao")]
        public Guid id_nguoi_tao { get; set; }
        [ForeignKey("NguoiSua")]
        public Guid id_nguoi_cap_nhat { get; set; }
        public virtual ICollection<GiamGiaSanPhamChiTiet> GiamGiaSanPhamChiTiets { get; set; }
        public virtual NhanVien? NguoiTao { get; set; }
        public virtual NhanVien? NguoiSua { get; set; }
    }
}
