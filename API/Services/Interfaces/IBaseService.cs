using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace API.Services.Interfaces
{
    public interface IBaseService<T> where T : class
    {
        // CRUD Operations
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(Guid id);
        Task<bool> CreateAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(Guid id);

        // Advanced Query Operations
        Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> expression);
        Task<List<T>> GetByConditionWithIncludeAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> expression);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> expression);
        Task<int> CountAsync(Expression<Func<T, bool>> expression = null);

        // Include Related Entities
        Task<List<T>> GetAllWithIncludeAsync(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);
        Task<T> GetByIdWithIncludeAsync(Guid id, params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);

        // Bulk Operations
        Task<bool> CreateRangeAsync(IEnumerable<T> entities);
        Task<bool> UpdateRangeAsync(IEnumerable<T> entities);
        Task<bool> DeleteRangeAsync(IEnumerable<T> entities);

        // Transaction Support
        Task<bool> ExecuteInTransactionAsync(Func<Task<bool>> operation);
    }
}