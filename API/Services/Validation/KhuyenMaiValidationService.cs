using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using API.Services.Interfaces;
using API.DbConects.DTOs.Admin.KhuyenMai;

namespace API.Services.Validation
{
    public class KhuyenMaiValidationService : IValidationService<ThemKhuyenMaiAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemKhuyenMaiAdminDTO dto)
        {
            var result = new ValidationResult();
            if (string.IsNullOrEmpty(dto.ten_khuyen_mai))
                result.Errors.Add("Tên khuyến mãi không được để trống");

            if (string.IsNullOrEmpty(dto.mo_ta))
                result.Errors.Add("Mô tả không được để trống");

            if (dto.gia_tri_giam > decimal.MaxValue || dto.gia_tri_don_hang_toi_thieu > decimal.MaxValue || dto.gia_tri_giam_toi_da > decimal.MaxValue || dto.so_luong_toi_da > int.MaxValue)
                result.Errors.Add("Giá trị khuyến mãi không hợp lệ");

            if (dto.kieu_khuyen_mai == KieuKhuyenMai.PhanTram.ToString())
            {
                if (dto.gia_tri_giam > 100 || dto.gia_tri_giam < 0)
                    result.Errors.Add("Giá trị khuyến mãi phải nằm trong khoảng 0% đến 100%");

                if (dto.gia_tri_don_hang_toi_thieu < 0)
                    result.Errors.Add("Giá trị đơn hàng tối thiểu phải lớn hơn 0");

                if (dto.gia_tri_giam_toi_da < 0)
                    result.Errors.Add("Giá trị khuyến mãi tối đa phải lớn hơn 0");

                if (dto.so_luong_toi_da < 0)
                    result.Errors.Add("Số lượng khuyến mãi tối đa phải lớn hơn 0");
            }

            if (dto.kieu_khuyen_mai == KieuKhuyenMai.SoTien.ToString())
            {
                if (dto.gia_tri_giam < 0)
                    result.Errors.Add("Giá trị khuyến mãi phải lớn hơn 0");

                if (dto.gia_tri_don_hang_toi_thieu < 0)
                    result.Errors.Add("Giá trị đơn hàng tối thiểu phải lớn hơn 0");

                if (dto.so_luong_toi_da < 0)
                    result.Errors.Add("Số lượng khuyến mãi tối đa phải lớn hơn 0");
            }
            if (dto.thoi_gian_bat_dau > dto.thoi_gian_ket_thuc)
                result.Errors.Add("Thời gian bắt đầu không được lớn hơn thời gian kết thúc");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class SanPhamKhuyenMaiValidationService : IValidationService<GiamGiaAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(GiamGiaAdminDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.ten_giam_gia))
                result.Errors.Add("Tên khuyến mãi không được để trống");

            if (string.IsNullOrEmpty(dto.mo_ta))
                result.Errors.Add("Mô tả không được để trống");

            if (dto.gia_tri_giam > decimal.MaxValue)
                result.Errors.Add("Giá trị khuyến mãi không hợp lệ");

            if (dto.kieu_giam_gia == "PhanTram")
            {
                if (dto.gia_tri_giam > 100 || dto.gia_tri_giam < 0)
                    result.Errors.Add("Giá trị giảm giá phải nằm trong khoảng 0% đến 100%");
            }

            if (dto.kieu_giam_gia == "SoTien")
            {
                if (dto.gia_tri_giam < 0)
                    result.Errors.Add("Giá trị khuyến mãi phải lớn hơn 0");
            }
            if (dto.thoi_gian_bat_dau > dto.thoi_gian_bat_dau)
                result.Errors.Add("Ngày bắt đầu không được lớn hơn ngày kết thúc");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }
}