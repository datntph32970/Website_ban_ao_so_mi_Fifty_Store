using System.ComponentModel.DataAnnotations;

namespace API.DbConects.Entities.Hoa_Don
{
    public class TrangThaiHoaDon
    {
        [Key]
        public Guid id_trang_thai_hoa_don { get; set; }
        public string ten_trang_thai_hoa_don { get; set; }
        public string ma_trang_thai_hoa_don { get; set; }
        public string mo_ta { get; set; }
        public bool trang_thai { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}
