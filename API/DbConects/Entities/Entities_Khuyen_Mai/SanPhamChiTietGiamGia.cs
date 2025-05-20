using API.DbConects.Entities.Entities_San_Pham;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Khuyen_Mai
{
    public class SanPhamChiTietGiamGia
    {
        [Key]
        public Guid id { get; set; }

        [ForeignKey("SanPhamChiTiet")]
        public Guid id_san_pham_chi_tiet { get; set; }

        [ForeignKey("GiamGia")]
        public Guid id_giam_gia { get; set; }

        public virtual SanPhamChiTiet? SanPhamChiTiet { get; set; }
        public virtual GiamGia? GiamGia { get; set; }
    }
}