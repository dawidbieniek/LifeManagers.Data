using LifeManagers.Data.Backup;

using Microsoft.Extensions.Options;

namespace LifeManagers.Data.Tests;

[TestClass]
public class PeriodicBackuperTests
{
    [TestMethod]
    public async Task PerformBackupIfNecessary_CreatesCopyOfFile_ThereIsNoLastBackupFileAsync()
    {
        DataServicesOptions options = SetupTest();

        IOptions<DataServicesOptions> optionsService = Microsoft.Extensions.Options.Options.Create(options);
        PeriodicBackuper backuper = new(new BackupManager(optionsService), optionsService);

        try
        {
            await backuper.PerformBackupIfNecessaryAsync();

            Assert.IsTrue(Directory.GetFiles(Path.Combine(options.DataDirectoryPath, options.BackupDirectory)).Length > 0);
        }
        finally
        {
            DeleteFiles(options);
        }
    }

    [TestMethod]
    public async Task PerformBackupIfNecessary_CreatesCopyOfFile_ThereIsOldBackupAsync()
    {
        DataServicesOptions options = SetupTest();

        IOptions<DataServicesOptions> optionsService = Microsoft.Extensions.Options.Options.Create(options);
        PeriodicBackuper backuper = new(new BackupManager(optionsService), optionsService);

        try
        {
            WriteLastBackupDate(options, DateTime.Today - TimeSpan.FromDays(2));
            await backuper.PerformBackupIfNecessaryAsync();

            Assert.IsTrue(Directory.GetFiles(Path.Combine(options.DataDirectoryPath, options.BackupDirectory)).Length > 0);
        }
        finally
        {
            DeleteFiles(options);
        }
    }

    private static DataServicesOptions SetupTest()
    {
        DataServicesOptions options = new()
        {
            DataDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),
            BackupPeriod = TimeSpan.FromDays(1)
        };

        CreateDummyDatabaseFile(options);
        return options;
    }

    private static void CreateDummyDatabaseFile(DataServicesOptions options)
    {
        Directory.CreateDirectory(options.DataDirectoryPath);
        File.Create(Path.Combine(options.DataDirectoryPath, options.DatabaseFileName)).Close();
    }

    private static void WriteLastBackupDate(DataServicesOptions options, DateTime lastBackupDate) => File.WriteAllText(Path.Combine(options.DataDirectoryPath, options.LastBackupTimeFileName), lastBackupDate.ToString());

    private static void DeleteFiles(DataServicesOptions options)
    {
        string databaseFilePath = Path.Combine(options.DataDirectoryPath, options.DatabaseFileName);
        if (File.Exists(databaseFilePath))
            File.Delete(databaseFilePath);

        string lastBackupTimeFilePath = Path.Combine(options.DataDirectoryPath, options.LastBackupTimeFileName);
        if (File.Exists(lastBackupTimeFilePath))
            File.Delete(lastBackupTimeFilePath);

        string backupsDirectory = Path.Combine(options.DataDirectoryPath, options.BackupDirectory);
        if (Directory.Exists(backupsDirectory))
            Directory.Delete(backupsDirectory, true);

        if (Directory.Exists(options.DataDirectoryPath))
            Directory.Delete(options.DataDirectoryPath);
    }
}