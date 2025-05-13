using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services.Validation
{
    public interface IKhachHangValidationService : IValidationService<KhachHang>
    {
        Task<ValidationResult> ValidateEmailAsync(string email);
        Task<ValidationResult> ValidatePhoneAsync(string phone);
        Task<ValidationResult> ValidateMaKhachHangAsync(string maKhachHang, Guid? idKhachHang = null);
    }

    public class KhachHangValidationService : IKhachHangValidationService
    {
        private readonly IBaseRepository<KhachHang> _khachHangRepository;

        public KhachHangValidationService(IBaseRepository<KhachHang> khachHangRepository)
        {
            _khachHangRepository = khachHangRepository;
        }

        public async Task<ValidationResult> ValidateAsync(KhachHang entity)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate tên khách hàng
            if (string.IsNullOrWhiteSpace(entity.ten_khach_hang))
                result.Errors.Add("Tên khách hàng không được để trống");
            else if (entity.ten_khach_hang.Length > 100)
                result.Errors.Add("Tên khách hàng không được vượt quá 100 ký tự");

            // Validate ngày sinh
            if (entity.ngay_sinh > DateTime.Now)
                result.Errors.Add("Ngày sinh không thể lớn hơn ngày hiện tại");

            // Validate số điện thoại
            if (!string.IsNullOrWhiteSpace(entity.so_dien_thoai))
            {
                var phoneResult = await ValidatePhoneAsync(entity.so_dien_thoai);
                if (!phoneResult.IsValid)
                    result.Errors.AddRange(phoneResult.Errors);
            }

            // Validate email
            if (!string.IsNullOrWhiteSpace(entity.email))
            {
                var emailResult = await ValidateEmailAsync(entity.email);
                if (!emailResult.IsValid)
                    result.Errors.AddRange(emailResult.Errors);
            }

            // Validate mã khách hàng
            if (!string.IsNullOrWhiteSpace(entity.ma_khach_hang))
            {
                var maResult = await ValidateMaKhachHangAsync(entity.ma_khach_hang, entity.id_khach_hang);
                if (!maResult.IsValid)
                    result.Errors.AddRange(maResult.Errors);
            }

            // Validate giới tính
            if (string.IsNullOrWhiteSpace(entity.gioi_tinh))
                result.Errors.Add("Giới tính không được để trống");
            else if (!new[] { "Nam", "Nữ", "Khác" }.Contains(entity.gioi_tinh))
                result.Errors.Add("Giới tính không hợp lệ");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        public async Task<ValidationResult> ValidateEmailAsync(string email)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(email))
                return result; // Email không bắt buộc

            // Kiểm tra định dạng email
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(email))
                result.Errors.Add("Email không hợp lệ");

            // Kiểm tra email đã tồn tại chưa
            var existingKhachHang = await _khachHangRepository.GetFirstOrDefaultAsync(kh => kh.email == email);
            if (existingKhachHang != null)
                result.Errors.Add("Email đã được sử dụng bởi khách hàng khác");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        public async Task<ValidationResult> ValidatePhoneAsync(string phone)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(phone))
                return result; // Số điện thoại không bắt buộc

            // Kiểm tra định dạng số điện thoại
            var phoneRegex = new Regex(@"^(\+84|0)\d{9,10}$");
            if (!phoneRegex.IsMatch(phone))
                result.Errors.Add("Số điện thoại không hợp lệ");

            // Kiểm tra số điện thoại đã tồn tại chưa
            var existingKhachHang = await _khachHangRepository.GetFirstOrDefaultAsync(kh => kh.so_dien_thoai == phone);
            if (existingKhachHang != null)
                result.Errors.Add("Số điện thoại đã được sử dụng bởi khách hàng khác");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        public async Task<ValidationResult> ValidateMaKhachHangAsync(string maKhachHang, Guid? idKhachHang = null)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(maKhachHang))
            {
                result.Errors.Add("Mã khách hàng không được để trống");
                result.IsValid = false;
                return result;
            }

            // Kiểm tra định dạng mã khách hàng
            var maRegex = new Regex(@"^KH\d{6}$");
            if (!maRegex.IsMatch(maKhachHang))
                result.Errors.Add("Mã khách hàng phải có định dạng KH + 6 chữ số");

            // Kiểm tra mã khách hàng đã tồn tại chưa
            var existingKhachHang = await _khachHangRepository.GetFirstOrDefaultAsync(kh => kh.ma_khach_hang == maKhachHang);
            if (existingKhachHang != null && existingKhachHang.id_khach_hang != idKhachHang)
                result.Errors.Add("Mã khách hàng đã tồn tại");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }
}