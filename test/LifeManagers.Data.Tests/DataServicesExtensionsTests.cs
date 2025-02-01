using LifeManagers.Data.Backup;
using LifeManagers.Data.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LifeManagers.Data.Tests;

[TestClass]
public class DataServicesExtensionsTests
{
    [TestMethod]
    [DataRow(typeof(IDbContextFactory<AppDbContextBase>))]
    [DataRow(typeof(IBackupManager))]
    [DataRow(typeof(IOptions<DataServicesOptions>))]
    [DataRow(typeof(IDatabaseInitializer))]
    public void AddDataServices_RegistersService(Type serviceType)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDataServices<AppDbContextBase>(opt => opt.WithDataDirectoryPath(Path.GetTempFileName()));

        // Act
        services.BuildServiceProvider().GetRequiredService(serviceType);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [DataRow(typeof(IPeriodicBackuper))]
    [DataRow(typeof(ISeeder<AppDbContextBase>))]
    public void AddDataServices_DoesntRegisterService_WhenNoAppropriateOptionSpecified(Type serviceType)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDataServices<AppDbContextBase>(opt => opt.WithDataDirectoryPath(Path.GetTempFileName()));

        // Act
        services.BuildServiceProvider().GetRequiredService(serviceType);
    }

    [TestMethod]
    public void AddDataBackupServices_RegistersBackupManagerService()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddBackupServices(opt => opt.WithDataDirectoryPath(Path.GetTempFileName()));

        // Act
        services.BuildServiceProvider().GetRequiredService<IBackupManager>();
    }

    [TestMethod]
    public void AddDataBackupServices_RegistersPeriodicBackuperService()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddBackupServices(opt =>
        {
            opt.WithDataDirectoryPath(Path.GetTempFileName());
            opt.EnablePeriodicBackups(TimeSpan.FromDays(365));
        });

        // Act
        services.BuildServiceProvider().GetRequiredService<IPeriodicBackuper>();
    }
}