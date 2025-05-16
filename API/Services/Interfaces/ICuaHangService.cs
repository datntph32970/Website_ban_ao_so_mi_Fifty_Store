using API.DbConects.Entities.Entities_Hoa_Don;

namespace API.Services.Interfaces
{
    public interface ICuaHangService : IBaseService<CuaHang>
    {
        Task<CuaHang> GetCuaHangFirstOrDefaultAsync();
        Task<CuaHang> UpdateCuaHangFirstOrDefaultAsync(CuaHang cuaHang);
    }
}