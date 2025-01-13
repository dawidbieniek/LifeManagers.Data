using LifeManagers.Data.Backup;

using Microsoft.Extensions.DependencyInjection;

namespace LifeManagers.Data.Tests;

[TestClass]
public class DependencyInejctionTests
{
    [TestMethod]
    public void AddDataBackupServices_RegisteresBackupManagerService()
    {
        IServiceCollection services = new ServiceCollection();
        services.RegisterDataBackupServices(opt => opt.WithDataDirectoryPath("somepath"));

        // Act
        BackupManager backupManager = services.BuildServiceProvider().GetRequiredService<BackupManager>();
    }

    [TestMethod]
    public void AddDataBackupServices_RegisteresPeriodicBackuperService()
    {
        IServiceCollection services = new ServiceCollection();
        services.RegisterDataBackupServices(opt =>
        {
            opt.WithDataDirectoryPath("somepath");
            opt.EnablePeriodicBackups(TimeSpan.FromDays(365));
        });

        // Act
        PeriodicBackuper backuper = services.BuildServiceProvider().GetRequiredService<PeriodicBackuper>();
    }
}