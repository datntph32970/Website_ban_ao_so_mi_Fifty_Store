using API.DbConects.Entities.Entities_Hoa_Don;
using API.Repositories.Interfaces;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Implementations
{
    public class GioHangService : BaseService<GioHangChiTiet>, IGioHangService
    {
        public GioHangService(IBaseRepository<GioHangChiTiet> repository) : base(repository)
        {
        }

        public async Task<IEnumerable<GioHangChiTiet>> GetGioHangByKhachHangAsync(Guid idKhachHang)
        {
            return await _repository.GetByConditionAsync(g => g.id_khach_hang == idKhachHang);
        }

        public async Task<bool> ThemSanPhamVaoGioHangAsync(Guid idKhachHang, Guid idSanPhamChiTiet, int soLuong)
        {
            var gioHangChiTiet = new GioHangChiTiet
            {
                id_gio_hang_chi_tiet = Guid.NewGuid(),
                id_khach_hang = idKhachHang,
                id_san_pham_chi_tiet = idSanPhamChiTiet,
                so_luong = soLuong
            };

            return await _repository.CreateAsync(gioHangChiTiet);
        }

        public async Task<bool> CapNhatSoLuongAsync(Guid idGioHangChiTiet, int soLuong)
        {
            var gioHangChiTiet = await _repository.GetByIdAsync(idGioHangChiTiet);
            if (gioHangChiTiet == null) return false;

            gioHangChiTiet.so_luong = soLuong;
            return await _repository.UpdateAsync(gioHangChiTiet);
        }

        public async Task<bool> XoaSanPhamKhoiGioHangAsync(Guid idGioHangChiTiet)
        {
            return await _repository.DeleteAsync(idGioHangChiTiet);
        }

        public async Task<bool> XoaGioHangAsync(Guid idKhachHang)
        {
            var gioHangChiTiets = await _repository.GetByConditionAsync(g => g.id_khach_hang == idKhachHang);
            return await _repository.DeleteRangeAsync(gioHangChiTiets);
        }
    }
}