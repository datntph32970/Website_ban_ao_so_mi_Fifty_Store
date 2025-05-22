using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_San_Pham
{
    public class HinhAnh
    {
        [Key]
        public Guid id_hinh_anh { get; set; }
        [Required(ErrorMessage = "Mã hình ảnh không được để trống")]
        public string ma_hinh_anh { get; set; }
        [Required(ErrorMessage = "URL không được để trống")]
        public string url { get; set; }
        [Required(ErrorMessage = "Mã người tạo không được để trống")]
        public Guid id_nguoi_tao { get; set; }
        [Required(ErrorMessage = "Ngày tạo không được để trống")]
        public DateTime ngay_tao { get; set; }
        public Guid? id_nguoi_sua { get; set; }
        public DateTime? ngay_sua { get; set; }
        public virtual ICollection<HinhAnhSanPhamChiTiet>? HinhAnhSanPhamChiTiets { get; set; }
        public virtual ICollection<HinhAnhHoanHang>? HinhAnhHoanHangs { get; set; }
        [ForeignKey("id_nguoi_tao")]
        public virtual NhanVien? NguoiTao { get; set; }
        [ForeignKey("id_nguoi_sua")]
        public virtual NhanVien? NguoiSua { get; set; }
        public virtual SanPham? SanPham { get; set; }
        public virtual CuaHang? CuaHang { get; set; }
    }
}
