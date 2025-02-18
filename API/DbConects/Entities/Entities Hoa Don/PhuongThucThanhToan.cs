using System.ComponentModel.DataAnnotations;

namespace API.DbConects.Entities.Entities_Hoa_Don
{
    public class PhuongThucThanhToan
    {
        [Key]
        public Guid id_phuong_thuc_thanh_toan { get; set; }
        public string ten_phuong_thuc_thanh_toan { get; set; }
        public string ma_phuong_thuc_thanh_toan { get; set; }
        public string mo_ta { get; set; }
        public bool trang_thai { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}
