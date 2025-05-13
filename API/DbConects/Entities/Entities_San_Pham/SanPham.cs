using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_San_Pham
{
    public class SanPham
    {
        [Key]
        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        public Guid id_san_pham { get; set; }
        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        public string ma_san_pham { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string ten_san_pham { get; set; }
        [Required(ErrorMessage = "Mô tả sản phẩm không được để trống")]
        public string mo_ta { get; set; }
        [Required(ErrorMessage = "Trạng thái sản phẩm không được để trống")]
        public string trang_thai { get; set; }
        [Required(ErrorMessage = "Mã người tạo không được để trống")]
        public Guid id_nguoi_tao { get; set; }
        [Required(ErrorMessage = "Ngày tạo không được để trống")]
        public DateTime ngay_tao { get; set; }
        public Guid? id_nguoi_sua { get; set; }
        [ForeignKey("anhMacDinh")]
        public Guid? id_anh_mac_dinh { get; set; }
        public DateTime? ngay_sua { get; set; }
        [ForeignKey("ChatLieu")]
        [Required(ErrorMessage = "Mã chất liệu không được để trống")]
        public Guid id_chat_lieu { get; set; }
        [ForeignKey("KieuDang")]
        [Required(ErrorMessage = "Mã kiểu dáng không được để trống")]
        public Guid id_kieu_dang { get; set; }
        [ForeignKey("ThuongHieu")]
        [Required(ErrorMessage = "Mã thương hiệu không được để trống")]
        public Guid id_thuong_hieu { get; set; }
        [ForeignKey("XuatXu")]
        [Required(ErrorMessage = "Mã xuất xứ không được để trống")]
        public Guid id_xuat_xu { get; set; }
        [ForeignKey("DanhMuc")]
        [Required(ErrorMessage = "Mã danh mục không được để trống")]
        public Guid id_danh_muc { get; set; }

        public virtual ChatLieu? ChatLieu { get; set; }
        public virtual KieuDang? KieuDang { get; set; }
        public virtual HinhAnh? anhMacDinh { get; set; }
        public virtual DanhMuc? DanhMuc { get; set; }
        public virtual ThuongHieu? ThuongHieu { get; set; }
        public virtual XuatXu? XuatXu { get; set; }
        [ForeignKey("id_nguoi_tao")]
        public virtual NhanVien? NguoiTao { get; set; }
        [ForeignKey("id_nguoi_sua")]
        public virtual NhanVien? NguoiSua { get; set; }
        public virtual ICollection<SanPhamChiTiet>? SanPhamChiTiets { get; set; }
    }
}
