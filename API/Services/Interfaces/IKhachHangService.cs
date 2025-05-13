using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DbConects.Entities.Entities_Tai_Khoan;
using API.Services.Interfaces;

namespace API.Services.Interfaces
{
    public interface IKhachHangService : IBaseService<KhachHang>
    {
        Task<KhachHang> GetByMaKhachHangAsync(string maKhachHang);
        Task<KhachHang> GetByEmailAsync(string email);
        Task<KhachHang> GetBySoDienThoaiAsync(string soDienThoai);
        Task<IEnumerable<KhachHang>> SearchAsync(string keyword);
        Task<ValidationResult> ValidateAsync(KhachHang entity);
    }
}