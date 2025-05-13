using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using API.Services.Interfaces;
using API.DbConects.DTOs.Admin.HoaDon;
using API.DbConects.DTOs.Client.HoaDon;

namespace API.Services.Validation
{
    public class HoaDonAdminValidationService : IValidationService<ThemHoaDonAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemHoaDonAdminDTO dto)
        {
            var result = new ValidationResult();

            if (dto.id_khach_hang == Guid.Empty)
                result.Errors.Add("Khách hàng không được để trống");

            if (string.IsNullOrEmpty(dto.dia_chi))
                result.Errors.Add("Địa chỉ không được để trống");

            if (string.IsNullOrEmpty(dto.phuong_thuc_thanh_toan))
                result.Errors.Add("Phương thức thanh toán không được để trống");

            if (dto.hoaDonChiTiets == null || dto.hoaDonChiTiets.Count == 0)
                result.Errors.Add("Phải có ít nhất 1 sản phẩm trong hóa đơn");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class HoaDonChiTietAdminValidationService : IValidationService<ThemHoaDonChiTietAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemHoaDonChiTietAdminDTO dto)
        {
            var result = new ValidationResult();

            if (dto.id_san_pham_chi_tiet == Guid.Empty)
                result.Errors.Add("Sản phẩm chi tiết không được để trống");

            if (dto.so_luong <= 0)
                result.Errors.Add("Số lượng phải lớn hơn 0");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class HoaDonClientValidationService : IValidationService<ThemHoaDonClientDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemHoaDonClientDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.dia_chi))
                result.Errors.Add("Địa chỉ không được để trống");

            if (string.IsNullOrEmpty(dto.phuong_thuc_thanh_toan))
                result.Errors.Add("Phương thức thanh toán không được để trống");

            if (dto.HoaDonChiTiets == null || dto.HoaDonChiTiets.Count == 0)
                result.Errors.Add("Phải có ít nhất 1 sản phẩm trong hóa đơn");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class HoaDonChiTietClientValidationService : IValidationService<ThemHoaDonChiTietClientDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemHoaDonChiTietClientDTO dto)
        {
            var result = new ValidationResult();

            if (dto.id_san_pham_chi_tiet == Guid.Empty)
                result.Errors.Add("Sản phẩm chi tiết không được để trống");

            if (dto.so_luong <= 0)
                result.Errors.Add("Số lượng phải lớn hơn 0");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }
}