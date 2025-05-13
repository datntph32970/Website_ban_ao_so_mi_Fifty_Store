using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_San_Pham
{
    public class HinhAnhSanPhamChiTiet
    {
        [Key]
        public Guid id_hinh_anh_san_pham_chi_tiet { get; set; }
        [ForeignKey("HinhAnhs")]
        public Guid id_hinh_anh { get; set; }
        [ForeignKey("SanPhamChiTiets")]
        public Guid id_san_pham_chi_tiet { get; set; }
        public virtual HinhAnh HinhAnhs { get; set; }
        public virtual SanPhamChiTiet SanPhamChiTiets { get; set; }
    }
}
