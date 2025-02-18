using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Tai_Khoan
{
    public class DiaChi
    {
        [Key]
        public Guid id_dia_chi { get; set; }
        [ForeignKey("KhachHang")]
        public Guid id_khach_hang { get; set; }
        public string tinh { get; set; }
        public string huyen { get; set; }
        public string xa { get; set; }
        public string dia_chi_mac_dinh { get; set; }
        public string ngay_tao{ get; set; }
        public string ngay_sua{ get; set; }
        public virtual KhachHang? KhachHang { get; set; }
    }
}
