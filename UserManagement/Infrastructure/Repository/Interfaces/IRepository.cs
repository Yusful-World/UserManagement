using System.Linq.Expressions;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Repository.Interfaces
{
    public interface IRepository<T> where T : class, IEntityBase
    {
        Task<T> GetAsync(Guid id);
        Task<T> GetBySpec(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includeProperties);
        Task<IEnumerable<T>> GetAllBySpec(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);

        IQueryable<T> GetQueryableBySpec(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T> DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task<bool> Exists(Guid id);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task SaveChangesAsync(CancellationToken cancellationToken);
        Task<List<User>> SearchUsersAsync(string keyword, int page, int pageSize);
    }
}
