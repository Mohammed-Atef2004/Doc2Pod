using Domain.Interfaces.Repositories;
using Infrastructure.Presistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.Shared
{


    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual IQueryable<T> EntityQuery => _dbSet.AsNoTracking();

        public async Task<T?> GetByIdAsync(object id)
            => await _dbSet.FindAsync(id);

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate != null
                ? await _dbSet.CountAsync(predicate)
                : await _dbSet.CountAsync();
        }
    }
}