namespace API.Repositories
{
    public interface IBaseRepositories<T> where T : class
    {
        Task<ICollection<T>> GetAll();
        Task<T> GetById(Guid id);
        Task<bool> Add(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(Guid id);
    }
}
