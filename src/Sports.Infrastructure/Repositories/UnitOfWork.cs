using System.Collections;
using Sports.Domain.Interfaces;
using Sports.Infrastructure.Data;

namespace Sports.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext DbContext;
    private Hashtable? Repositories;

    public UnitOfWork(ApplicationDbContext context)
    {
        DbContext = context;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        if (Repositories == null)
            Repositories = new Hashtable();

        var type = typeof(T).Name;

        if (!Repositories.ContainsKey(type))
        {
            var repositoryType = typeof(GenericRepository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), DbContext);
            Repositories.Add(type, repositoryInstance);
        }

        return (IGenericRepository<T>)Repositories[type]!;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await DbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }
}
