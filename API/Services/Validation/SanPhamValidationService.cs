using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using API.Services.Interfaces;
using API.DbConects.DTOs.Admin.SanPham;

namespace API.Services.Validation
{
    public class SanPhamValidationService : IValidationService<ThemSanPhamAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemSanPhamAdminDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.ten_san_pham))
                result.Errors.Add("Tên sản phẩm không được để trống");

            if (string.IsNullOrEmpty(dto.mo_ta))
                result.Errors.Add("Mô tả sản phẩm không được để trống");

            if (string.IsNullOrEmpty(dto.id_thuong_hieu))
                result.Errors.Add("Thương hiệu không được để trống");

            if (string.IsNullOrEmpty(dto.id_kieu_dang))
                result.Errors.Add("Kiểu dáng không được để trống");

            if (string.IsNullOrEmpty(dto.id_chat_lieu))
                result.Errors.Add("Chất liệu không được để trống");

            if (string.IsNullOrEmpty(dto.id_xuat_xu))
                result.Errors.Add("Xuất xứ không được để trống");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class SanPhamChiTietValidationService : IValidationService<ThemSanPhamChiTietAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemSanPhamChiTietAdminDTO dto)
        {
            var result = new ValidationResult();

            if (dto.id_san_pham == Guid.Empty)
                result.Errors.Add("Sản phẩm không được để trống");

            if (dto.id_mau_sac == Guid.Empty)
                result.Errors.Add("Màu sắc không được để trống");

            if (dto.id_kich_co == Guid.Empty)
                result.Errors.Add("Kích cỡ không được để trống");

            if (dto.so_luong <= 0)
                result.Errors.Add("Số lượng phải lớn hơn 0");

            if (dto.gia_ban <= 0)
                result.Errors.Add("Giá bán phải lớn hơn 0");

            if (dto.gia_nhap <= 0)
                result.Errors.Add("Giá gốc phải lớn hơn 0");

            if (dto.gia_ban < dto.gia_nhap)
                result.Errors.Add("Giá bán không được nhỏ hơn giá gốc");

            if (dto.them_hinh_anh_spcts == null || dto.them_hinh_anh_spcts.Count == 0)
                result.Errors.Add("Phải có ít nhất 1 hình ảnh");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class ThuongHieuValidationService : IValidationService<ThemThuongHieuAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemThuongHieuAdminDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.ten_thuong_hieu))
                result.Errors.Add("Tên thương hiệu không được để trống");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class KieuDangValidationService : IValidationService<ThemKieuDangAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemKieuDangAdminDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.ten_kieu_dang))
                result.Errors.Add("Tên kiểu dáng không được để trống");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class ChatLieuValidationService : IValidationService<ThemChatLieuAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemChatLieuAdminDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.ten_chat_lieu))
                result.Errors.Add("Tên chất liệu không được để trống");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class XuatXuValidationService : IValidationService<ThemXuatXuAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemXuatXuAdminDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.ten_xuat_xu))
                result.Errors.Add("Tên xuất xứ không được để trống");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class MauSacValidationService : IValidationService<ThemMauSacAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemMauSacAdminDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.ten_mau_sac))
                result.Errors.Add("Tên màu sắc không được để trống");

            if (string.IsNullOrEmpty(dto.mo_ta))
                result.Errors.Add("Mã màu không được để trống");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }

    public class KichCoValidationService : IValidationService<ThemKichCoAdminDTO>
    {
        public async Task<ValidationResult> ValidateAsync(ThemKichCoAdminDTO dto)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(dto.ten_kich_co))
                result.Errors.Add("Tên kích cỡ không được để trống");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }
}