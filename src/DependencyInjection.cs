using LifeManagers.Data.Backup;
using LifeManagers.Data.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LifeManagers.Data;

public static class DependencyInjection
{
    public static IServiceCollection RegisterDataServices<T>(this IServiceCollection services, Action<DataServicesOptionsBuilder<T>> configureOptions) where T : AppDbContextBase
    {
        DataServicesOptionsBuilder<T> builder = new();
        configureOptions(builder);
        DataServicesOptions options = builder.Build();

        services.AddOptions<DataServicesOptions>()
            .Configure(opt => opt.CreateOptions(options));

        services.AddDbContextFactory<T>((sp, opt) =>
            {
                DataServicesOptions options = sp.GetRequiredService<IOptions<DataServicesOptions>>().Value;
                if (options.DebugMode)
                    opt.EnableSensitiveDataLogging();

                opt.UseSqlite($"Filename={Path.Combine(options.DataDirectoryPath, options.DatabaseFileName)}");
            });

        services.AddTransient<BackupManager>();

        if (options.BackupPeriod is not null)
            services.AddTransient<PeriodicBackuper>();

        if (options.SeederType is not null)
            services.AddTransient(typeof(ISeeder<T>), options.SeederType);

        services.AddTransient<DatabaseInitializer<T>>(sp => new(sp));

        return services;
    }
}