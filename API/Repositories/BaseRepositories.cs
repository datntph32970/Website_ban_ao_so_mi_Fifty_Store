
using API.DbConects;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class BaseRepositories<T> : IBaseRepositories<T> where T : class
    {
        private readonly AppDbContext _context;
        private DbSet<T> _entities;
        public BaseRepositories(AppDbContext context)
        {
            _context = context;
            _entities = context.Set<T>();
        }

        public async Task<ICollection<T>> GetAll()
        {
            return await _entities.ToListAsync();
        }
        public async Task<T> GetById(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            return await _entities.FindAsync(id);
        }
        public async Task<bool> Add(T entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await _entities.AddAsync(entity);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }
        public async Task<bool> Update(T entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                _entities.Update(entity);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> Delete(Guid id)
        {
            try
            {
                if (id == null)
                {
                    throw new ArgumentNullException(nameof(id));
                }
                var entity = await _entities.FindAsync(id);
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                _entities.Remove(entity);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
