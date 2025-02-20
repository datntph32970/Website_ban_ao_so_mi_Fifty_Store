using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_San_Pham
{
    public class SanPham
    {
        [Key]
        public Guid id_san_pham { get; set; }
        public string ma_san_pham { get; set; }
        public string ten_san_pham { get; set; }
        public string mo_ta { get; set; }
        public string trang_thai { get; set; }
        public Guid id_nguoi_tao { get; set; }
        public DateTime ngay_tao { get; set; }
        public Guid? id_nguoi_sua { get; set; }
        public DateTime? ngay_sua { get; set; }

        [ForeignKey("ChatLieu")]
        public Guid id_chat_lieu { get; set; }

        [ForeignKey("KieuDang")]
        public Guid id_kieu_dang { get; set; }

        [ForeignKey("ThuongHieu")]
        public Guid id_thuong_hieu { get; set; }

        [ForeignKey("XuatXu")]
        public Guid id_xuat_xu { get; set; }

        public virtual ChatLieu? ChatLieu { get; set; }
        public virtual KieuDang? KieuDang { get; set; }
        public virtual ThuongHieu? ThuongHieu { get; set; }
        public virtual XuatXu? XuatXu { get; set; }
        [ForeignKey("id_nguoi_tao")]
        public virtual NhanVien? NguoiTao { get; set; }
        [ForeignKey("id_nguoi_sua")]
        public virtual NhanVien? NguoiSua { get; set; }
        public virtual ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}
