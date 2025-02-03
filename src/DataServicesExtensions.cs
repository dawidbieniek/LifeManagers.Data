using LifeManagers.Data.Backup;
using LifeManagers.Data.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LifeManagers.Data;

public static class DataServicesExtensions
{
    /// <summary>
    /// Adds all necessary data library services
    /// </summary>
    public static IServiceCollection AddDataServices<T>(this IServiceCollection services, Action<DataServicesOptionsBuilder> configureOptions) where T : AppDbContextBase
    {
        DataServicesOptionsBuilder builder = new();
        configureOptions(builder);
        DataServicesOptions options = builder.Build();

        services.AddOptions<DataServicesOptions>()
                    .Configure(opt => opt.CreateOptions(options));

        services.AddDbContextFactory<T>(opt =>
        {
            DataServicesOptions opts = options;
            if (opts.DebugMode)
                opt.EnableSensitiveDataLogging();

            opt.UseSqlite($"Data Source={Path.Combine(opts.DataDirectoryPath, opts.DatabaseFileName)}");
        });

        services.AddTransient<IBackupManager, BackupManager<T>>();
        if (options.BackupPeriod.HasValue)
            services.AddTransient<IPeriodicBackuper, PeriodicBackuper>();

        if (options.SeederType != null)
            services.AddTransient(typeof(ISeeder<T>), options.SeederType);

        services.AddTransient<IDatabaseInitializer, DatabaseInitializer<T>>(sp =>
        {
            return new DatabaseInitializer<T>(
                sp.GetRequiredService<IDbContextFactory<T>>(),
                sp.GetService<ISeeder<T>>(),
                sp.GetService<IPeriodicBackuper>(),
                sp.GetRequiredService<IOptions<DataServicesOptions>>());
        });

        return services;
    }
}