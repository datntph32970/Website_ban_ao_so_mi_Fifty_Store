using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Khuyen_Mai
{
    public class KhuyenMai
    {
        [Key]
        public Guid id_khuyen_mai { get; set; }
        public string ma_khuyen_mai { get; set; }
        public string ten_khuyen_mai { get; set; }
        public string mo_ta { get; set; }
        public string kieu_giam_gia { get; set; }
        public decimal gia_tri_giam { get; set; }
        public decimal gia_tri_giam_toi_thieu { get; set; }
        public decimal gia_tri_giam_toi_da { get; set; }
        public int so_luong_toi_da { get; set; }
        public int so_luong_da_su_dung { get; set; }
        public DateTime thoi_gian_bat_dau { get; set; }
        public DateTime thoi_gian_ket_thuc { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public Guid id_nguoi_tao { get; set; }
        public DateTime ngay_sua { get; set; }
        public Guid? id_nguoi_sua { get; set; }
        [ForeignKey("id_nguoi_tao")]
        public virtual NhanVien? NguoiSua { get; set; }
        [ForeignKey("id_nguoi_sua")]
        public virtual NhanVien NguoiTao { get; set; }
        public virtual ICollection<HoaDon>? HoaDons { get; set; }
    }
}
