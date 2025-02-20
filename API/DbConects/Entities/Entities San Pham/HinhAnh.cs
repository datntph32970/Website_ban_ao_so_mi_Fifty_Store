using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_San_Pham
{
    public class HinhAnh
    {
        [Key]
        public Guid id_hinh_anh { get; set; }
        public string ma_hinh_anh { get; set; }
        public string ten_hinh_anh { get; set; }
        public string url { get; set; }
        public string trang_thai { get; set; }
        [ForeignKey("SanPhamChiTiets")]
        public Guid id_san_pham_chi_tiet { get; set; }
        public Guid id_nguoi_tao { get; set; }
        public DateTime ngay_tao { get; set; }
        public Guid? id_nguoi_sua { get; set; }
        public DateTime? ngay_sua { get; set; }
        public virtual SanPhamChiTiet? SanPhamChiTiets { get; set; }
        [ForeignKey("id_nguoi_tao")]
        public virtual NhanVien? NguoiTao { get; set; }
        [ForeignKey("id_nguoi_sua")]
        public virtual NhanVien? NguoiSua { get; set; }
    }
}
