using API.DbConects.DTOs.Admin.SanPham;
using static API.Constants.AppConstants;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Text.Json.Serialization;

namespace API.DbConects.DTOs.Admin.KhuyenMai
{
    public class GiamGiaAdminDTO
    {
        public Guid id_giam_gia { get; set; }
        public string ma_giam_gia { get; set; }
        public string ten_giam_gia { get; set; }
        public string mo_ta { get; set; }
        public string kieu_giam_gia { get; set; }
        public decimal gia_tri_giam { get; set; }
        public int so_luong_toi_da { get; set; }
        public int so_luong_da_su_dung { get; set; }
        public DateTime thoi_gian_bat_dau { get; set; }
        public DateTime thoi_gian_ket_thuc { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public string ten_nguoi_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string ten_nguoi_sua { get; set; }
        public List<SanPhamChiTietAdminDTO> sanPhamChiTiets { get; set; }
    }

    public class ThemGiamGiaAdminDTO : IValidatableObject
    {
        [Required(ErrorMessage = "Tên giảm giá không được để trống")]
        [StringLength(100, ErrorMessage = "Tên giảm giá không được vượt quá 100 ký tự")]
        public string ten_giam_gia { get; set; }
        public string? ma_giam_gia { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string mo_ta { get; set; }

        [Required(ErrorMessage = "Kiểu giảm giá không được để trống")]
        public string kieu_giam_gia { get; set; }

        [Required(ErrorMessage = "Giá trị giảm không được để trống")]
        public decimal gia_tri_giam { get; set; }

        [Required(ErrorMessage = "Số lượng tối đa không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng tối đa phải lớn hơn 0")]
        public int so_luong_toi_da { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        [DataType(DataType.DateTime, ErrorMessage = "Thời gian bắt đầu không hợp lệ")]
        public DateTime thoi_gian_bat_dau { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        [DataType(DataType.DateTime, ErrorMessage = "Thời gian kết thúc không hợp lệ")]
        public DateTime thoi_gian_ket_thuc { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string trang_thai { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (thoi_gian_bat_dau < DateTime.Now)
            {
                yield return new ValidationResult(
                    "Thời gian bắt đầu không được trong quá khứ",
                    new[] { nameof(thoi_gian_bat_dau) });
            }

            if (thoi_gian_bat_dau >= thoi_gian_ket_thuc)
            {
                yield return new ValidationResult(
                    "Thời gian kết thúc phải sau thời gian bắt đầu",
                    new[] { nameof(thoi_gian_ket_thuc) });
            }

            if (kieu_giam_gia == "PhanTram")
            {
                if (gia_tri_giam < 0 || gia_tri_giam > 100)
                {
                    yield return new ValidationResult(
                        "Giá trị giảm theo phần trăm phải từ 0 đến 100",
                        new[] { nameof(gia_tri_giam) });
                }
            }
            else if (kieu_giam_gia == "SoTien")
            {
                if (gia_tri_giam <= 0)
                {
                    yield return new ValidationResult(
                        "Giá trị giảm theo số tiền phải lớn hơn 0",
                        new[] { nameof(gia_tri_giam) });
                }
            }
        }
    }
    public class SuaGiamGiaAdminDTO
    {
        [Required(ErrorMessage = "Tên giảm giá không được để trống")]
        [StringLength(100, ErrorMessage = "Tên giảm giá không được vượt quá 100 ký tự")]
        public string ten_giam_gia { get; set; }
        [Required(ErrorMessage = "Mã giảm giá không được để trống")]
        public string ma_giam_gia { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string mo_ta { get; set; }

        [Required(ErrorMessage = "Kiểu giảm giá không được để trống")]
        public string kieu_giam_gia { get; set; }

        [Required(ErrorMessage = "Giá trị giảm không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị giảm phải lớn hơn hoặc bằng 0")]
        public decimal gia_tri_giam { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng tối đa phải lớn hơn 0")]
        public int so_luong_toi_da { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        [DataType(DataType.DateTime, ErrorMessage = "Thời gian bắt đầu không hợp lệ")]
        public DateTime thoi_gian_bat_dau { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        [DataType(DataType.DateTime, ErrorMessage = "Thời gian kết thúc không hợp lệ")]
        public DateTime thoi_gian_ket_thuc { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string trang_thai { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (thoi_gian_bat_dau < DateTime.Now)
            {
                yield return new ValidationResult(
                    "Thời gian bắt đầu không được trong quá khứ",
                    new[] { nameof(thoi_gian_bat_dau) });
            }

            if (thoi_gian_bat_dau >= thoi_gian_ket_thuc)
            {
                yield return new ValidationResult(
                    "Thời gian kết thúc phải sau thời gian bắt đầu",
                    new[] { nameof(thoi_gian_ket_thuc) });
            }

            if (kieu_giam_gia == "PhanTram")
            {
                if (gia_tri_giam < 0 || gia_tri_giam > 100)
                {
                    yield return new ValidationResult(
                        "Giá trị giảm theo phần trăm phải từ 0 đến 100",
                        new[] { nameof(gia_tri_giam) });
                }
            }
            else if (kieu_giam_gia == "SoTien")
            {
                if (gia_tri_giam <= 0)
                {
                    yield return new ValidationResult(
                        "Giá trị giảm theo số tiền phải lớn hơn 0",
                        new[] { nameof(gia_tri_giam) });
                }
            }
        }
    }

    public enum TrangThaiGiamGia
    {
        HoatDong,
        NgungHoatDong,
    }

    public enum KieuGiamGia
    {
        PhanTram,
        SoTien
    }
}