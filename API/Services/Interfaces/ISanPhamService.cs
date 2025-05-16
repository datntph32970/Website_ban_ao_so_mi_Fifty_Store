using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using API.DbConects.DTOs.Admin.SanPham;
using API.DbConects.Entities.Entities_San_Pham;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace API.Services.Interfaces
{
    public interface ISanPhamService : IBaseService<SanPham>
    {
        Task<List<SanPhamAdminDTO>> GetAllSanPhamAdminDTOAsync();
        Task<List<SanPhamAdminDTO>> GetByConditionWithIncludeAsync(Expression<Func<SanPham, bool>> condition, params Func<IQueryable<SanPham>, IIncludableQueryable<SanPham, object>>[] includes);
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