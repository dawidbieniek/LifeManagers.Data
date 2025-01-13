using LifeManagers.Data.Backup;
using LifeManagers.Data.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LifeManagers.Data;

public static class DependencyInjection
{
    private static bool _optionsAdded = false;

    /// <summary>
    /// Registers all data library services
    /// </summary>
    public static IServiceCollection RegisterDataServices<T>(this IServiceCollection services, Action<DataServicesOptionsBuilder> configureOptions) where T : AppDbContextBase
    {
        DataServicesOptions options = CreateOptions(configureOptions);

        AddOptions(services, options);

        services.AddDbContextFactory<T>((sp, opt) =>
        {
            DataServicesOptions options = sp.GetRequiredService<IOptions<DataServicesOptions>>().Value;
            if (options.DebugMode)
                opt.EnableSensitiveDataLogging();

            opt.UseSqlite($"Filename={Path.Combine(options.DataDirectoryPath, options.DatabaseFileName)}");
        });

        services.AddDataBackupServices(options);

        if (options.SeederType is not null)
            services.AddTransient(typeof(ISeeder<T>), options.SeederType);

        services.AddTransient<DatabaseInitializer<T>>(sp => new(sp));

        return services;
    }

    public static IServiceCollection RegisterDataBackupServices(this IServiceCollection services, Action<DataServicesOptionsBuilder> configureOptions)
    {
        DataServicesOptionsBuilder builder = new();
        configureOptions(builder);
        DataServicesOptions options = builder.Build();

        AddDataBackupServices(services, options);

        return services;
    }

    private static void AddOptions(IServiceCollection services, DataServicesOptions options)
    {
        if (_optionsAdded)
            return;

        services.AddOptions<DataServicesOptions>()
            .Configure(opt => opt.CreateOptions(options));

        _optionsAdded = true;
    }

    private static DataServicesOptions CreateOptions(Action<DataServicesOptionsBuilder> configureOptions)
    {
        DataServicesOptionsBuilder builder = new();
        configureOptions(builder);
        DataServicesOptions options = builder.Build();
        return options;
    }

    private static IServiceCollection AddDataBackupServices(this IServiceCollection services, DataServicesOptions options)
    {
        AddOptions(services, options);

        services.AddTransient<BackupManager>();

        if (options.BackupPeriod is not null)
            services.AddTransient<PeriodicBackuper>();

        return services;
    }
}