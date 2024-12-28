using System.Reflection;

using Microsoft.EntityFrameworkCore;

namespace LifeManagers.Data;

public class AppDbContextBase(DbContextOptions options) : DbContext(options)
{
    public async Task PerformNecessaryMigrationsAsync()
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