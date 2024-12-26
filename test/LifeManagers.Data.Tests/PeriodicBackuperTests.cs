using LifeManagers.Data.Backup;

using Microsoft.Extensions.Options;

namespace LifeManagers.Data.Tests;

[TestClass]
public class PeriodicBackuperTests
{
    private static readonly DataServicesOptions Options = new()
    {
        DataDirectoryPath = Path.GetTempPath(),
        BackupPeriod = TimeSpan.FromDays(1)
    };

    [TestInitialize]
    public void InitializeTests()
    {
        DeleteFiles(Options);
        CreateDummyDatabaseFile(Options);
    }

    [TestMethod]
    public async Task PerformBackupIfNecessary_CreatesCopyOfFile_ThereIsNoLastBackupFileAsync()
    {
        IOptions<DataServicesOptions> optionsService = Microsoft.Extensions.Options.Options.Create(Options);
        PeriodicBackuper backuper = new(new BackupManager(optionsService), optionsService);

        try
        {
            await backuper.PerformBackupIfNecessaryAsync();

            Assert.IsTrue(Directory.GetFiles(Path.Combine(Options.DataDirectoryPath, Options.BackupDirectory)).Length > 0);
        }
        finally
        {
            DeleteFiles(Options);
        }
    }

    [TestMethod]
    public async Task PerformBackupIfNecessary_CreatesCopyOfFile_ThereIsOldBackupAsync()
    {
        IOptions<DataServicesOptions> optionsService = Microsoft.Extensions.Options.Options.Create(Options);
        PeriodicBackuper backuper = new(new BackupManager(optionsService), optionsService);

        try
        {
            WriteLastBackupDate(Options, DateTime.Today - TimeSpan.FromDays(2));
            await backuper.PerformBackupIfNecessaryAsync();

            Assert.IsTrue(Directory.GetFiles(Path.Combine(Options.DataDirectoryPath, Options.BackupDirectory)).Length > 0);
        }
        finally
        {
            DeleteFiles(Options);
        }
    }

    private static void CreateDummyDatabaseFile(DataServicesOptions options)
    {
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
    }
}