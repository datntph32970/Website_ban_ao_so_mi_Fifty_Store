using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using API.Services.Interfaces;
using API.DbConects.DTOs.Admin.TaiKhoan;
using API.DbConects.DTOs.Client.TaiKhoan;

namespace API.Services.Validation
{
    public class TaiKhoanAdminValidationService : IValidationService<ThemTaiKhoanAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemTaiKhoanAdminDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.ten_dang_nhap))
                result.Errors.Add("Tên đăng nhập không được để trống");

            if (string.IsNullOrEmpty(dto.mat_khau))
                result.Errors.Add("Mật khẩu không được để trống");

            if (string.IsNullOrEmpty(dto.ho_ten))
                result.Errors.Add("Họ tên không được để trống");

            if (string.IsNullOrEmpty(dto.so_dien_thoai))
                result.Errors.Add("Số điện thoại không được để trống");
            else if (!Regex.IsMatch(dto.so_dien_thoai, @"^[0-9]{10}$"))
                result.Errors.Add("Số điện thoại không hợp lệ");

            if (string.IsNullOrEmpty(dto.email))
                result.Errors.Add("Email không được để trống");
            else if (!Regex.IsMatch(dto.email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                result.Errors.Add("Email không hợp lệ");

            if (string.IsNullOrEmpty(dto.dia_chi))
                result.Errors.Add("Địa chỉ không được để trống");

            if (string.IsNullOrEmpty(dto.vai_tro))
                result.Errors.Add("Vai trò không được để trống");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class TaiKhoanClientValidationService : IValidationService<DangKyClientDTO>
    {
        public async Task<ValidationResult> ValidateAsync(DangKyClientDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.ten_dang_nhap))
                result.Errors.Add("Tên đăng nhập không được để trống");

            else if (Regex.IsMatch(dto.ten_dang_nhap, @"\s"))
                result.Errors.Add("Tên đăng nhập không được chứa khoảng trắng");
            else if (!Regex.IsMatch(dto.ten_dang_nhap, @"^[a-zA-Z0-9]+$"))
                result.Errors.Add("Tên đăng nhập không được chứa ký tự đặc biệt");

            if (string.IsNullOrEmpty(dto.mat_khau))
                result.Errors.Add("Mật khẩu không được để trống");
            else if (dto.mat_khau.Length < 6)
                result.Errors.Add("Mật khẩu phải có ít nhất 6 ký tự");

            if (string.IsNullOrEmpty(dto.xac_nhan_mat_khau))
                result.Errors.Add("Xác nhận mật khẩu không được để trống");
            else if (dto.mat_khau != dto.xac_nhan_mat_khau)
                result.Errors.Add("Xác nhận mật khẩu không khớp");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class DangNhapValidationService : IValidationService<DangNhapClientDTO>
    {
        public async Task<ValidationResult> ValidateAsync(DangNhapClientDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.ten_dang_nhap))
                result.Errors.Add("Tên đăng nhập không được để trống");

            if (string.IsNullOrEmpty(dto.mat_khau))
                result.Errors.Add("Mật khẩu không được để trống");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class SuaThongTinValidationService : IValidationService<SuaThongTinClientDTO>
    {
        public async Task<ValidationResult> ValidateAsync(SuaThongTinClientDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.ho_ten))
                result.Errors.Add("Họ tên không được để trống");

            if (string.IsNullOrEmpty(dto.so_dien_thoai))
                result.Errors.Add("Số điện thoại không được để trống");
            else if (!Regex.IsMatch(dto.so_dien_thoai, @"^[0-9]{10}$"))
                result.Errors.Add("Số điện thoại không hợp lệ");

            if (string.IsNullOrEmpty(dto.email))
                result.Errors.Add("Email không được để trống");
            else if (!Regex.IsMatch(dto.email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                result.Errors.Add("Email không hợp lệ");

            if (string.IsNullOrEmpty(dto.dia_chi))
                result.Errors.Add("Địa chỉ không được để trống");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class DoiMatKhauValidationService : IValidationService<DoiMatKhauClientDTO>
    {
        public async Task<ValidationResult> ValidateAsync(DoiMatKhauClientDTO dto)
        {
            var result = new ValidationResult();
            if (string.IsNullOrEmpty(dto.ten_dang_nhap))
                result.Errors.Add("Tên đăng nhập không được để trống");

            if (string.IsNullOrEmpty(dto.mat_khau_cu))
                result.Errors.Add("Mật khẩu cũ không được để trống");

            if (string.IsNullOrEmpty(dto.mat_khau_moi))
                result.Errors.Add("Mật khẩu mới không được để trống");
            else if (dto.mat_khau_moi.Length < 6)
                result.Errors.Add("Mật khẩu mới phải có ít nhất 6 ký tự");

            if (string.IsNullOrEmpty(dto.xac_nhan_mat_khau_moi))
                result.Errors.Add("Xác nhận mật khẩu mới không được để trống");
            else if (dto.mat_khau_moi != dto.xac_nhan_mat_khau_moi)
                result.Errors.Add("Xác nhận mật khẩu mới không khớp");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }
}