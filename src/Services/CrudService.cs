using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LifeManagers.Data.Services;

public abstract class CrudService<TEntity, TContext>(IDbContextFactory<TContext> contextFactory)
    where TContext : AppDbContextBase
    where TEntity : Entity
{
    protected IDbContextFactory<TContext> ContextFactory { get; } = contextFactory;

    /// <summary>
    /// Returns all entities of type <see cref="TEntity"/> from database
    /// </summary>
    /// <remarks>
    /// Entites are set up as NoTracking. Using <see cref="DbContext.SaveChanges()"/> won't persist
    /// them. Use <see cref="UpdateAsync(TEntity)"/> to save changes
    /// </remarks>
    // Returning IQueryable after disposing AppDbContext leads to access after disposal (IQuerayble
    // defers execution to later materialization)
    public async Task<List<TEntity>> GetAllAsync()
    {
        using TContext context = ContextFactory.CreateDbContext();
        return await context.Set<TEntity>().AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Returns entity properties without any navigation properties.
    /// </summary>
    /// <remarks> For navigation properties use <see cref="GetDetailsAsync"/> </remarks>
    public async Task<TEntity?> GetAsync(int id)
    {
        using TContext context = ContextFactory.CreateDbContext();
        return await context.Set<TEntity>().FindAsync(id);
    }

    /// <summary>
    /// Returns entity properties including necessary dependencies
    /// </summary>
    public abstract Task<TEntity?> GetDetailsAsync(int id);

    /// <summary>
    /// Adds <paramref name="entity"/> to context using <see cref="DbContext.Attach{TEntity}(TEntity)"/>
    /// </summary>
    /// <remarks>
    /// Calling <see cref="AddAsync(TEntity)"/> multiple times won't have any effect. In addition it
    /// won't add any dependent entities
    /// </remarks>
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