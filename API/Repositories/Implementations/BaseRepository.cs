using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using API.DbConects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using API.Repositories.Interfaces;

namespace API.Repositories.Implementations
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            try
            {
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message, ex.Message);
                return null;
            }
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<bool> CreateAsync(T entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message, ex.Message);
                return false;
            }
        }


        public virtual async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                _dbSet.Update(entity);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
           
        }

        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity == null) return false;
                _dbSet.Remove(entity);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public virtual async Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.Where(expression).ToListAsync();
        }

        public virtual async Task<List<T>> GetByConditionWithIncludeAsync(Expression<Func<T, bool>> expression, params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = include(query);
            }
            return await query.Where(expression).ToListAsync();
        }

        public virtual async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.FirstOrDefaultAsync(expression);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.AnyAsync(expression);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> expression = null)
        {
            if (expression == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(expression);
        }

        public virtual async Task<List<T>> GetAllWithIncludeAsync(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            try
            {
                IQueryable<T> query = _dbSet;
                foreach (var include in includes)
                {
                    query = include(query);
                }
                return await query.ToListAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public virtual async Task<T> GetByIdWithIncludeAsync(Guid id, params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = include(query);
            }
            var primaryKeyName = GetPrimaryKeyName(); // Lấy tên khóa chính
            return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, primaryKeyName) == id);
        }
        private string GetPrimaryKeyName()
        {
            var entityType = _context.Model.FindEntityType(typeof(T));
            var primaryKey = entityType.FindPrimaryKey();
            return primaryKey.Properties.First().Name; // Lấy tên thuộc tính khóa chính
        }

        public virtual async Task<bool> CreateRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                await _dbSet.AddRangeAsync(entities);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public virtual async Task<bool> UpdateRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                _dbSet.UpdateRange(entities);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public virtual async Task<bool> DeleteRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                _dbSet.RemoveRange(entities);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public virtual async Task<bool> ExecuteInTransactionAsync(Func<Task<bool>> operation)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await operation();
                if (result)
                {
                    await transaction.CommitAsync();
                    return true;
                }
                await transaction.RollbackAsync();
                return false;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}