using System.Reflection;

using Microsoft.EntityFrameworkCore;

namespace LifeManagers.Data;

public class AppDbContextBase(DbContextOptions options)
#if DEBUG
    : DbContext(new DbContextOptionsBuilder(options).EnableSensitiveDataLogging().Options)  // Display more info on exceptions
#else
    : DbContext(options)
#endif
{
    public async Task PerformNecessaryMigrations()
    {
        await Database.MigrateAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ApplyAllConfigurationsFromCurrentAssembly(modelBuilder);
    }

    private static void ApplyAllConfigurationsFromCurrentAssembly(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}