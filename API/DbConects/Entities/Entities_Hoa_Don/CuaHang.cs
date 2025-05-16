using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Hoa_Don
{
    public class CuaHang
    {
        [Key]
        public Guid id_cua_hang { get; set; }
        public string ten_cua_hang { get; set; }
        public string website { get; set; }
        public string email { get; set; }
        public string sdt { get; set; }
        public string dia_chi { get; set; }
        public string mo_ta { get; set; }
        public Guid? id_hinh_anh { get; set; }
        public Guid id_nguoi_sua { get; set; }
        public virtual ICollection<HoaDon>? HoaDons { get; set; }
        [ForeignKey("id_nguoi_sua")]
        public virtual NhanVien? NguoiSua { get; set; }
        [ForeignKey("id_hinh_anh")]
        public virtual HinhAnh? HinhAnh { get; set; }

    }
}