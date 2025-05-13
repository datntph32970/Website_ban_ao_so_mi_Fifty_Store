using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_San_Pham;

namespace API.Services.Interfaces
{
    public interface ISanPhamService : IBaseService<SanPham>
    {
        Task<List<SanPhamAdminDTO>> GetAllSanPhamAdminDTOAsync();
        Task<List<SanPhamAdminDTO>> GetByConditionWithIncludeAsync(Expression<Func<SanPham, bool>> condition, params Expression<Func<SanPham, object>>[] includes);
        Task<SanPhamAdminDTO> GetByIdSanPhamAdminDTOAsync(Guid id);
        Task<List<SanPham>> GetSanPhamByDanhMucAsync(Guid danhMucId);
        Task<List<SanPham>> GetSanPhamByThuongHieuAsync(Guid thuongHieuId);
        Task<List<SanPham>> GetSanPhamByKieuDangAsync(Guid kieuDangId);
        Task<List<SanPham>> GetSanPhamByChatLieuAsync(Guid chatLieuId);
        Task<List<SanPham>> GetSanPhamByXuatXuAsync(Guid xuatXuId);
        Task<SanPham> GetSanPhamWithDetailsAsync(Guid id);
        Task<bool> UpdateTrangThaiAsync(Guid id, string trangThai);
        Task<List<SanPham>> SearchSanPhamAsync(string keyword);
        Task<List<SanPham>> GetSanPhamByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<bool> RemoveInvalidDiscountsAsync();
        Task<bool> RemoveInvalidPromotionsAsync();
    }
}