using LifeManagers.Data.Backup;
using LifeManagers.Data.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LifeManagers.Data;

public class DatabaseInitializer<T>(
        IDbContextFactory<T> contextFactory,
        ISeeder<T>? seeder,
        IPeriodicBackuper? periodicBackuper,
        IOptions<DataServicesOptions> options) : IDatabaseInitializer where T : AppDbContextBase
{
    private readonly IDbContextFactory<T> _contextFactory = contextFactory;
    private readonly ISeeder<T>? _seeder = seeder;
    private readonly IPeriodicBackuper? _periodicBackuper = periodicBackuper;
    private readonly IOptions<DataServicesOptions> _options = options;

    public event EventHandler<string> StepExecuting = delegate { };

    public async Task InitializeDatabaseAsync()
    {
        CreateDatabaseFileIfNotExist();

        using T context = await _contextFactory.CreateDbContextAsync();

        StepExecuting?.Invoke(this, "Migrating database");
        await context.PerformNecessaryMigrationsAsync();

        if (_seeder is not null)
        {
            StepExecuting?.Invoke(this, "Seeding database");
            await _seeder.SeedRequiredDataAsync();

            DataServicesOptions options = _options.Value;
            if (options.DebugMode)
                await _seeder.SeedDebugDataAsync();
        }

        if (_periodicBackuper is not null)
        {
            StepExecuting?.Invoke(this, "Performing periodic database backup");
            await _periodicBackuper.PerformBackupIfNecessaryAsync();
        }
    }

    private void CreateDatabaseFileIfNotExist()
    {
        DataServicesOptions options = _options.Value;
        string databasePath = Path.Combine(options.DataDirectoryPath, options.DatabaseFileName);

        if (File.Exists(databasePath))
            return;

        File.Create(databasePath).Close();
    }
}