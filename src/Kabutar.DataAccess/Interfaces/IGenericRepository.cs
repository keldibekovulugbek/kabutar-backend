using System.Linq.Expressions;
using Kabutar.Domain.Common;

namespace Kabutar.DataAccess.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = true);
    Task<T?> GetByIdAsync(long id, bool asNoTracking = true);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true);

    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(long id);
    Task DeleteAsync(T entity);
}
