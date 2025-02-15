using API.DbConects.Entities.Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.San_Pham
{
    public class ChatLieu
    {
        [Key]
        public Guid id_chat_lieu { get; set; }
        public string ma_chat_lieu { get; set; }
        public string ten_chat_lieu { get; set; }
        public string trang_thai { get; set; }
        [ForeignKey("NguoiTao")]
        public Guid id_nguoi_tao { get; set; }
        public DateTime ngay_tao { get; set; }
        [ForeignKey("NguoiSua")]
        public Guid? id_nguoi_sua { get; set; }
        public DateTime? ngay_sua { get; set; }
        public virtual ICollection<SanPham>? SanPhams { get; set; }
        public virtual NhanVien? NguoiTao { get; set; }
        public virtual NhanVien? NguoiSua { get; set; }
    }
}
