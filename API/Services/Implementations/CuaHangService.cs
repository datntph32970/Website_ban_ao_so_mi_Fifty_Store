using API.DbConects.Entities.Entities_Hoa_Don;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services.Implementations
{
    public class CuaHangService : BaseService<CuaHang>, ICuaHangService
    {
        private readonly IBaseRepository<CuaHang> _cuaHangRepository;
        public CuaHangService(IBaseRepository<CuaHang> cuaHangRepository) : base(cuaHangRepository)
        {
            _cuaHangRepository = cuaHangRepository;
        }

        public async Task<CuaHang> GetCuaHangFirstOrDefaultAsync()
        {
            var cuaHang = await _cuaHangRepository.GetFirstOrDefaultAsync(x => x.id_cua_hang != Guid.Empty);
            return cuaHang;
        }

        public async Task<CuaHang> UpdateCuaHangFirstOrDefaultAsync(CuaHang cuaHang)
        {
            var cuaHangFirstOrDefault = await _cuaHangRepository.GetFirstOrDefaultAsync(x => x.id_cua_hang != Guid.Empty);
            if (cuaHangFirstOrDefault == null)
            {
                return null;
            }
            cuaHangFirstOrDefault.id_nguoi_sua = cuaHang.id_nguoi_sua;
            cuaHangFirstOrDefault.ten_cua_hang = cuaHang.ten_cua_hang;
            cuaHangFirstOrDefault.website = cuaHang.website;
            cuaHangFirstOrDefault.email = cuaHang.email;
            cuaHangFirstOrDefault.sdt = cuaHang.sdt;
            cuaHangFirstOrDefault.dia_chi = cuaHang.dia_chi;
            cuaHangFirstOrDefault.mo_ta = cuaHang.mo_ta;
            await _cuaHangRepository.UpdateAsync(cuaHangFirstOrDefault);
            return cuaHangFirstOrDefault;
        }


    }
}