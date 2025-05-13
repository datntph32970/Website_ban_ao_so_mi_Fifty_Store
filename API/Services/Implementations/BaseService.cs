using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using API.Repositories.Interfaces;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace API.Services.Implementations
{
    public class BaseService<T> : IBaseService<T> where T : class
    {
        protected readonly IBaseRepository<T> _repository;

        public BaseService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<List<T>> GetAllAsync() => await _repository.GetAllAsync();
        public async Task<T> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);
        public async Task<bool> CreateAsync(T entity) => await _repository.CreateAsync(entity);
        public async Task<bool> UpdateAsync(T entity) => await _repository.UpdateAsync(entity);
        public async Task<bool> DeleteAsync(Guid id) => await _repository.DeleteAsync(id);
        public async Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> expression) => await _repository.GetByConditionAsync(expression);
        public async Task<List<T>> GetByConditionWithIncludeAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes) => await _repository.GetByConditionWithIncludeAsync(expression, includes);
        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> expression) => await _repository.GetFirstOrDefaultAsync(expression);
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression) => await _repository.ExistsAsync(expression);
        public async Task<int> CountAsync(Expression<Func<T, bool>> expression = null) => await _repository.CountAsync(expression);
        public async Task<List<T>> GetAllWithIncludeAsync(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes) => await _repository.GetAllWithIncludeAsync(includes);
        public async Task<T> GetByIdWithIncludeAsync(Guid id, params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes) => await _repository.GetByIdWithIncludeAsync(id, includes);
        public async Task<bool> CreateRangeAsync(IEnumerable<T> entities) => await _repository.CreateRangeAsync(entities);
        public async Task<bool> UpdateRangeAsync(IEnumerable<T> entities) => await _repository.UpdateRangeAsync(entities);
        public async Task<bool> DeleteRangeAsync(IEnumerable<T> entities) => await _repository.DeleteRangeAsync(entities);
        public async Task<bool> ExecuteInTransactionAsync(Func<Task<bool>> operation) => await _repository.ExecuteInTransactionAsync(operation);
    }
}