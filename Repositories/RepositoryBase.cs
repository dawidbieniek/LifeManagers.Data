using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LifeManagers.Data.Repositories;

public abstract class RepositoryBase<TEntity, TContext>(IDbContextFactory<TContext> contextFactory) : IRepositoryBase<TEntity, TContext>
    where TEntity : Entity
    where TContext : AppDbContextBase
{
    protected IDbContextFactory<TContext> ContextFactory { get; private init; } = contextFactory;

    public async Task<List<TEntity>> GetAllAsync()
    {
        using TContext context = ContextFactory.CreateDbContext();
        return await context.Set<TEntity>().AsNoTracking().ToListAsync();
    }

    public async Task<TEntity?> GetAsync(int id)
    {
        using TContext context = ContextFactory.CreateDbContext();
        return await context.Set<TEntity>().FindAsync(id);
    }

    public abstract Task<TEntity?> GetDetailsAsync(int id);

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        using TContext context = ContextFactory.CreateDbContext();
        context.Set<TEntity>().Attach(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(TEntity entity)
    {
        using TContext context = ContextFactory.CreateDbContext();
        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        using TContext context = ContextFactory.CreateDbContext();
        context.UpdateRange(entities);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using TContext context = ContextFactory.CreateDbContext();
        TEntity? entity = await context.Set<TEntity>().FindAsync(id);
        if (entity is null)
            return;

        context.Set<TEntity>().Remove(entity);
        await context.SaveChangesAsync();
    }

    public async Task ExecuteInTransactionAsync(Func<TContext, Task> action)
    {
        using TContext context = ContextFactory.CreateDbContext();
        using IDbContextTransaction transaction = context.Database.BeginTransaction();
        try
        {
            await action(context);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}