using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace LifeManagers.Data.Backup;

internal class BackupManager(IOptions<DataServicesOptions> options) : IBackupManager
{
    private readonly string _databaseFilePath = Path.Combine(options.Value.DataDirectoryPath, options.Value.DatabaseFileName);

    public void ReplaceDatabaseFile(string sourceDatabaseFilePath)
    {
        SqliteConnection.ClearAllPools();
        File.Copy(sourceDatabaseFilePath, _databaseFilePath, true);
    }

    public Stream CreateBackupStream()
    {
        SqliteConnection.ClearAllPools();
        return File.Open(_databaseFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}