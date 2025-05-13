using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Repositories.Interfaces;
using API.Services.Interfaces;
using API.Services.Validation;

namespace API.Services.Implementations
{
    public class KhachHangService : BaseService<KhachHang>, IKhachHangService
    {
        private readonly IKhachHangValidationService _validationService;

        public KhachHangService(
            IBaseRepository<KhachHang> repository,
            IKhachHangValidationService validationService)
            : base(repository)
        {
            _validationService = validationService;
        }

        public async Task<KhachHang> GetByMaKhachHangAsync(string maKhachHang)
        {
            return await _repository.GetFirstOrDefaultAsync(kh => kh.ma_khach_hang == maKhachHang);
        }

        public async Task<KhachHang> GetByEmailAsync(string email)
        {
            return await _repository.GetFirstOrDefaultAsync(kh => kh.email == email);
        }

        public async Task<KhachHang> GetBySoDienThoaiAsync(string soDienThoai)
        {
            return await _repository.GetFirstOrDefaultAsync(kh => kh.so_dien_thoai == soDienThoai);
        }

        public async Task<IEnumerable<KhachHang>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAllAsync();

            keyword = keyword.ToLower();
            var khachHangs = await GetAllAsync();
            return khachHangs.Where(kh =>
                kh.ma_khach_hang.ToLower().Contains(keyword) ||
                kh.ten_khach_hang.ToLower().Contains(keyword) ||
                kh.email.ToLower().Contains(keyword) ||
                kh.so_dien_thoai.Contains(keyword));
        }

        public async Task<ValidationResult> ValidateAsync(KhachHang entity)
        {
            return await _validationService.ValidateAsync(entity);
        }

        public async Task<bool> CreateAsync(KhachHang entity)
        {
            var validationResult = await ValidateAsync(entity);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            return await base.CreateAsync(entity);
        }

        public async Task<bool> UpdateAsync(KhachHang entity)
        {
            var validationResult = await ValidateAsync(entity);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            return await base.UpdateAsync(entity);
        }
    }

}