using API.DbConects.Entities.Entities_San_Pham;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Khuyen_Mai
{
    public class GiamGia
    {
        [Key]
        public Guid id_giam_gia { get; set; }
        [Required(ErrorMessage = "Mã giảm giá không được để trống")]
        public string ma_giam_gia { get; set; }
        [Required(ErrorMessage = "Tên giảm giá không được để trống")]
        public string ten_giam_gia { get; set; }
        [Required(ErrorMessage = "Mô tả giảm giá không được để trống")]
        public string mo_ta { get; set; }
        [Required(ErrorMessage = "Loại giảm giá không được để trống")]
        [RegularExpression("^(PhanTram|SoTien)$", ErrorMessage = "Loại giảm giá phải là 'PhanTram' hoặc 'TienMat'")]
        public string kieu_giam_gia { get; set; }
        [Required(ErrorMessage = "Giá trị giảm giá không được để trống")]
        public decimal gia_tri_giam { get; set; }
        [Required(ErrorMessage = "Số lượng tối đa không được để trống")]
        public int so_luong_toi_da { get; set; }
        [Required(ErrorMessage = "Số lượng đã sử dụng không được để trống")]
        public int so_luong_da_su_dung { get; set; }
        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        public DateTime thoi_gian_bat_dau { get; set; }
        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        public DateTime thoi_gian_ket_thuc { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(HoatDong|KhongHoatDong)$", ErrorMessage = "Trạng thái phải là 'HoatDong' hoặc 'KhongHoatDong'")]
        public string trang_thai { get; set; }
        [Required(ErrorMessage = "Ngày tạo không được để trống")]
        public DateTime ngay_tao { get; set; }
        [Required(ErrorMessage = "Ngày cập nhật không được để trống")]
        public DateTime ngay_cap_nhat { get; set; }
        [Required(ErrorMessage = "Mã người tạo không được để trống")]
        public Guid id_nguoi_tao { get; set; }
        public Guid? id_nguoi_cap_nhat { get; set; }
        public virtual ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
        [ForeignKey("id_nguoi_tao")]
        public virtual NhanVien? NguoiTao { get; set; }
        [ForeignKey("id_nguoi_cap_nhat")]
        public virtual NhanVien? NguoiSua { get; set; }
    }
}
