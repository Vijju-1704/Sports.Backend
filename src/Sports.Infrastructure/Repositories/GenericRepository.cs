using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Sports.Domain.Interfaces;
using Sports.Infrastructure.Data;

namespace Sports.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext DbContext;
    private readonly DbSet<T> DbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        DbContext = context;
        DbSet = DbContext.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
    {
        return await DbSet.Where(expression).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
    }

    public void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    public IQueryable<T> GetQueryable()
    {
        return DbSet.AsQueryable();
    }
}
