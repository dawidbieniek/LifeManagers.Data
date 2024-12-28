using LifeManagers.Data.Backup;
using LifeManagers.Data.Seeding;

using SQLitePCL;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LifeManagers.Data;

public class DatabaseInitializer<T>(IServiceProvider serviceProvider) where T : AppDbContextBase
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public event EventHandler<string> StepExecuting = delegate { };

    public async Task InitializeDatabaseAsync()
    {
        CreateDatabaseFileIfNotExist();

        using T context = GetContext();

        StepExecuting?.Invoke(this, "Migrating database");
        await context.PerformNecessaryMigrationsAsync();

        ISeeder<T>? seeder = _serviceProvider.GetService<ISeeder<T>>();
        if (seeder is not null)
        {
            StepExecuting?.Invoke(this, "Seeding database");
            await seeder.SeedRequiredDataAsync();

            DataServicesOptions options = _serviceProvider.GetRequiredService<IOptions<DataServicesOptions>>().Value;
            if (options.DebugMode)
                await seeder.SeedDebugDataAsync();
        }

        PeriodicBackuper? backuper = _serviceProvider.GetService<PeriodicBackuper>();
        if (backuper is not null)
        {
            StepExecuting?.Invoke(this, "Performing periodic database backup");
            await backuper.PerformBackupIfNecessaryAsync();
        }
    }

    private void CreateDatabaseFileIfNotExist()
    {
        DataServicesOptions options = _serviceProvider.GetRequiredService<IOptions<DataServicesOptions>>().Value;
        string databasePath = Path.Combine(options.DataDirectoryPath, options.DatabaseFileName);

        if (File.Exists(databasePath))
            return;

        File.Create(databasePath).Close();
    }

    private T GetContext()
    {
        T context;

        IDbContextFactory<T>? contextFactory = _serviceProvider.GetService<IDbContextFactory<T>>();
        if (contextFactory is null)
            context = _serviceProvider.GetService<T>() ?? throw new InvalidOperationException($"No context or factory is registered for type {typeof(T)}");
        else
            context = contextFactory.CreateDbContext();

        return context;
    }
}