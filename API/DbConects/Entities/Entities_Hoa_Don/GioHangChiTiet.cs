using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Hoa_Don
{
    public class GioHangChiTiet
    {
        [Key]
        public Guid id_gio_hang_chi_tiet { get; set; }
        [Required(ErrorMessage = "Số lượng không được để trống")]
        public int so_luong { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public bool trang_thai { get; set; }
        //
        [ForeignKey("KhachHang")]
        public Guid id_khach_hang { get; set; }
        [ForeignKey("SanPhamChiTiet")]
        public Guid id_san_pham_chi_tiet { get; set; }
        //
        public virtual SanPhamChiTiet SanPhamChiTiet { get; set; }
        public virtual KhachHang KhachHang { get; set; }
    }
}
