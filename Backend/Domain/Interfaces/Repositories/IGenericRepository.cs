using System.Linq.Expressions;

namespace Domain.Interfaces.Repositories;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> EntityQuery { get; }

    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<T?> GetByIdAsync(object id);

    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
