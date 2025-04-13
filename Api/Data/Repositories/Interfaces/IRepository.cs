using System.Linq.Expressions;

namespace backend.Api.Data.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        
        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);
            
        Task<T> GetByIdAsync(string id);
        
        Task<T> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> filter = null,
            string includeProperties = "");
            
        Task AddAsync(T entity);
        
        Task UpdateAsync(T entity);
        
        Task RemoveAsync(string id);
        
        Task RemoveAsync(T entity);
        
        Task<int> CountAsync(Expression<Func<T, bool>> filter = null);
    }
}