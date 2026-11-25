
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models.Interfaces;
using Portfolio.Services.Interfaces;

namespace Portfolio.Services;

public abstract class CrudService<T> : ICrudService<T>
    where T : class, IEntity
{
    protected readonly AppDbContext _context;

    protected CrudService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _context.Set<T>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
    await _context.Set<T>().AddAsync(entity);

    await _context.SaveChangesAsync();

    return entity;
    }
    
    public virtual async Task<T?> UpdateAsync(int id, T entity)
    {
    T? existingEntity = await _context.Set<T>()
        .FindAsync(id);

    if (existingEntity == null)
    {
        return null;
    }

    entity.Id = id;

    _context.Entry(existingEntity)
        .CurrentValues.SetValues(entity);

    await _context.SaveChangesAsync();

    return existingEntity;
    }


    
    public virtual async Task<bool> DeleteAsync(int id)
    {
         T? entity = await _context.Set<T>()
        .FindAsync(id);
    
        if (entity == null)
        {
          return false;
        }

        _context.Set<T>().Remove(entity);

        await _context.SaveChangesAsync();

        return true;
    }

}
