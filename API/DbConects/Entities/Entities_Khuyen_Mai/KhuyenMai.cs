using API.DbConects.Entities.Entities_Hoa_Don;
using API.DbConects.Entities.Entities_Tai_Khoan;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DbConects.Entities.Entities_Khuyen_Mai
{
    public class KhuyenMai
    {
        [Key]
        public Guid id_khuyen_mai { get; set; }
        [Required(ErrorMessage = "Mã khuyến mãi không được để trống")]
        public string ma_khuyen_mai { get; set; }
        [Required(ErrorMessage = "Tên khuyến mãi không được để trống")]
        public string ten_khuyen_mai { get; set; }
        [Required(ErrorMessage = "Mô tả khuyến mãi không được để trống")]
        public string mo_ta { get; set; }
        [Required(ErrorMessage = "Kiểu giảm giá không được để trống")]
        [RegularExpression("^(PhanTram|TienMat)$", ErrorMessage = "Kiểu giảm giá không hợp lệ")]
        public string kieu_khuyen_mai { get; set; }
        [Required(ErrorMessage = "Giá trị giảm không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị giảm phải lớn hơn hoặc bằng 0")]
        public decimal gia_tri_giam { get; set; }
        [Required(ErrorMessage = "Giá trị giảm tối đa không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị giảm tối đa phải lớn hơn hoặc bằng 0")]
        public decimal gia_tri_giam_toi_da { get; set; }
        [Required(ErrorMessage = "Giá trị đơn hàng tối thiểu không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu phải lớn hơn hoặc bằng 0")]
        public decimal gia_tri_don_hang_toi_thieu { get; set; }

        [Required(ErrorMessage = "Số lượng tối đa không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng tối đa phải lớn hơn 0")]
        public int so_luong_toi_da { get; set; }
        [Required(ErrorMessage = "Số lượng đã sử dụng không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng đã sử dụng phải lớn hơn hoặc bằng 0")]
        public int so_luong_da_su_dung { get; set; }
        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        public DateTime thoi_gian_bat_dau { get; set; }
        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        [CustomValidation(typeof(KhuyenMai), "ValidateThoiGianKetThuc")]
        public DateTime thoi_gian_ket_thuc { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(HoatDong|KhongHoatDong)$", ErrorMessage = "Trạng thái phải là 'HoatDong' hoặc 'KhongHoatDong'")]
        public string trang_thai { get; set; }
        [Required(ErrorMessage = "Ngày tạo không được để trống")]
        public DateTime ngay_tao { get; set; }
        [Required(ErrorMessage = "Mã người tạo không được để trống")]
        public Guid id_nguoi_tao { get; set; }
        public Guid? id_nguoi_sua { get; set; }
        public DateTime? ngay_sua { get; set; }
        [ForeignKey("id_nguoi_tao")]
        public virtual NhanVien? NguoiTao { get; set; }
        [ForeignKey("id_nguoi_sua")]
        public virtual NhanVien? NguoiSua { get; set; }
        public virtual ICollection<HoaDon>? HoaDons { get; set; }

        public static ValidationResult ValidateThoiGianKetThuc(DateTime thoiGianKetThuc, ValidationContext context)
        {
            var khuyenMai = (KhuyenMai)context.ObjectInstance;
            if (thoiGianKetThuc <= khuyenMai.thoi_gian_bat_dau)
            {
                return new ValidationResult("Thời gian kết thúc phải lớn hơn thời gian bắt đầu");
            }
            return ValidationResult.Success;
        }
    }
}
