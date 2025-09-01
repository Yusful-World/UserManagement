using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UserManagement.Data;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Repository.Interfaces;

namespace UserManagement.Infrastructure.Repository
{
    public class Repository<T> : IRepository<T> where T : class, IEntityBase
    {
        private readonly AppDbContext _context;

        public Repository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return _context.Set<T>().AnyAsync(predicate, cancellationToken: cancellationToken);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Set<T>().CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().CountAsync(predicate);
        }

        public Task<T> DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            return Task.FromResult(entity);
        }

        public async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
            await Task.CompletedTask;
        }

        public async Task<bool> Exists(Guid id)
        {
            return await _context.Set<T>().AnyAsync(e => e.Id == id);
        }

        public async Task<T> GetAsync(Guid id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includeProperties)
        {
            var entities = _context.Set<T>().AsNoTracking();
            foreach (var includeProperty in includeProperties)
            {
                entities = entities.Include(includeProperty);
            }

            return await entities.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllBySpec(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            var entities = _context.Set<T>().Where(predicate).AsNoTracking();
            foreach (var includeProperty in includeProperties)
            {
                entities = entities.Include(includeProperty);
            }

            return await entities.ToListAsync();
        }


        public IQueryable<T> GetQueryableBySpec(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().AsNoTracking().Where(predicate);
        }
        public async Task<T> GetBySpec(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            var entities = _context.Set<T>().Where(predicate).AsNoTracking();
            foreach (var includeProperty in includeProperties)
            {
                entities = entities.Include(includeProperty);
            }

            return await entities.FirstOrDefaultAsync();
        }

        public Task UpdateAsync(T entity)
        {
            var updated = _context.Set<T>().Update(entity);
            return Task.FromResult(updated);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<User>> SearchUsersAsync(string keyword, int page, int pageSize)
        {
            return await _context.Users
                .Where(u => EF.Functions.ILike(u.FirstName, $"%{keyword}%") ||
                    EF.Functions.ILike(u.LastName, $"%{keyword}%") ||
                    EF.Functions.ILike(u.Email, $"%{keyword}%"))
                .OrderBy(u => u.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
