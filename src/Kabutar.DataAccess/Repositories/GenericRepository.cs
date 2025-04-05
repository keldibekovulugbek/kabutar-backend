using Kabutar.DataAccess.Interfaces;
using Kabutar.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Kabutar.DataAccess.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : Auditable, new()
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = true)
    {
        var query = _dbSet.Where(x => !x.IsDeleted);

        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(long id, bool asNoTracking = true)
    {
        var query = _dbSet.Where(x => x.Id == id && !x.IsDeleted);

        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true)
    {
        var query = _dbSet.Where(x => !x.IsDeleted).Where(predicate);

        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        entity.Created = DateTime.UtcNow;
        entity.Updated = DateTime.UtcNow;
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync(); 
    }

    public async Task UpdateAsync(T entity)
    {
        entity.Updated = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(); 
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is null || entity.IsDeleted) return;

        entity.IsDeleted = true;
        entity.Updated = DateTime.UtcNow;

        _dbSet.Update(entity);
        await _context.SaveChangesAsync(); 
    }

    public async Task DeleteAsync(T entity)
    {
        if (entity.IsDeleted) return;

        entity.IsDeleted = true;
        entity.Updated = DateTime.UtcNow;

        _dbSet.Update(entity);
        await _context.SaveChangesAsync(); 
    }

}
