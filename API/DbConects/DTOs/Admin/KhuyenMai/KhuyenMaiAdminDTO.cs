using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.DbConects.DTOs.Admin.KhuyenMai
{
    public class KhuyenMaiAdminDTO
    {
        public Guid id_khuyen_mai { get; set; }
        public string ma_khuyen_mai { get; set; }
        public string ten_khuyen_mai { get; set; }
        public string mo_ta { get; set; }
        public string kieu_khuyen_mai { get; set; }
        public decimal gia_tri_giam { get; set; }
        public decimal gia_tri_don_hang_toi_thieu { get; set; }
        public decimal gia_tri_giam_toi_da { get; set; }
        public int so_luong_toi_da { get; set; }
        public int so_luong_da_su_dung { get; set; }
        public DateTime ngay_bat_dau { get; set; }
        public DateTime ngay_ket_thuc { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public string ten_nguoi_tao { get; set; }
        public DateTime? ngay_sua { get; set; }
        public string ten_nguoi_sua { get; set; }
    }

    public enum KieuKhuyenMai
    {
        PhanTram,
        SoTien
    }
    public class ThemKhuyenMaiAdminDTO
    {
        [Required(ErrorMessage = "Tên khuyến mãi không được để trống")]
        public string ten_khuyen_mai { get; set; }

        [Required(ErrorMessage = "Mô tả khuyến mãi không được để trống")]
        public string mo_ta { get; set; }
        public string? ma_khuyen_mai { get; set; }

        [Required(ErrorMessage = "Kiểu giảm giá không được để trống")]
        [RegularExpression("^(PhanTram|TienMat)$", ErrorMessage = "Kiểu giảm giá phải là 'PhanTram' hoặc 'TienMat'")]
        public string kieu_khuyen_mai { get; set; }

        [Required(ErrorMessage = "Giá trị giảm không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị giảm tối thiểu phải lớn hơn hoặc bằng 0")]
        public decimal gia_tri_giam { get; set; }

        [Required(ErrorMessage = "Giá trị đơn hàng tối thiểu không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu phải lớn hơn hoặc bằng 0")]
        public decimal gia_tri_don_hang_toi_thieu { get; set; }

        [Required(ErrorMessage = "Giá trị giảm tối đa không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị giảm tối đa phải lớn hơn hoặc bằng 0")]
        public decimal gia_tri_giam_toi_da { get; set; }

        [Required(ErrorMessage = "Số lượng tối đa không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng tối đa phải lớn hơn 0")]
        public int so_luong_toi_da { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        public DateTime thoi_gian_bat_dau { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        public DateTime thoi_gian_ket_thuc { get; set; }
    }

    public class SuaKhuyenMaiAdminDTO
    {
        [Required(ErrorMessage = "Tên khuyến mãi không được để trống")]
        public string ten_khuyen_mai { get; set; }

        [Required(ErrorMessage = "Mô tả khuyến mãi không được để trống")]
        public string mo_ta { get; set; }
        public string ma_khuyen_mai { get; set; }

        [Required(ErrorMessage = "Kiểu khuyến mãi không được để trống")]
        [RegularExpression("^(PhanTram|TienMat)$", ErrorMessage = "Kiểu khuyến mãi phải là 'PhanTram' hoặc 'TienMat'")]
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

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        public DateTime thoi_gian_bat_dau { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        public DateTime thoi_gian_ket_thuc { get; set; }

    }

    public class CapNhatTrangThaiKhuyenMaiDTO
    {
        [Required(ErrorMessage = "ID khuyến mãi không được để trống")]
        public Guid id_khuyen_mai { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(HoatDong|KhongHoatDong)$", ErrorMessage = "Trạng thái phải là 'HoatDong' hoặc 'KhongHoatDong'")]
        public string trang_thai { get; set; }
    }
}