namespace LifeManagers.Data.Repositories;

public interface IRepositoryBase<TEntity, TContext>
    where TEntity : Entity
    where TContext : AppDbContextBase
{
    /// <summary>
    /// Returns all entities of repository type from database
    /// </summary>
    /// <remarks>
    /// Entites are set up as NoTracking. Using SaveChanes won't persist. Use <see
    /// cref="UpdateAsync(TEntity, bool)"/> to persist changes
    /// </remarks>
    Task<List<TEntity>> GetAllAsync();  // Returning IQueryable after disposing AppDbContext leads to access after disposal (IQuerayble defers execution to later materialization

    /// <summary>
    /// Returns entity properties without any dependencies.
    /// </summary>
    /// <remarks> For dependencies (navigation properties) use <see cref="GetDetailsAsync"/>&gt; </remarks>
    Task<TEntity?> GetAsync(int id);

    /// <summary>
    /// Returns entity properties includeing necessary dependencies
    /// </summary>
    Task<TEntity?> GetDetailsAsync(int id);

    /// <summary>
    /// Marks <paramref name="entity"/> as Added. In next SaveChanges it will be persisted in database.
    /// </summary>
    /// <remarks> Won't add any dependent entities </remarks>
    Task<TEntity> AddAsync(TEntity entity);

    Task UpdateAsync(TEntity entity);

    Task UpdateRangeAsync(IEnumerable<TEntity> entities);

    Task DeleteAsync(int id);

    Task ExecuteInTransactionAsync(Func<TContext, Task> action);
}