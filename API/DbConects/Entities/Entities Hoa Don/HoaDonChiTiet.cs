using API.DbConects.Entities.Entities_San_Pham;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Hoa_Don
{
    public class HoaDonChiTiet
    {
        [Key]
        public Guid id_hoa_don_chi_tiet { get; set; }
        public string ma_hoa_don_chi_tiet { get; set; }
        [ForeignKey("HoaDon")]
        public Guid id_hoa_don { get; set; }
        [ForeignKey("SanPhamChiTiets")]
        public Guid id_san_pham_chi_tiet { get; set; }
        public int so_luong { get; set; }
        public decimal don_gia { get; set; }
        public decimal thanh_tien { get; set; }
        public string ghi_chu { get; set; }
        public bool trang_thai { get; set; }
        public virtual HoaDon HoaDon { get; set; }
        public virtual SanPhamChiTiet SanPhamChiTiets { get; set; }
    }
}
