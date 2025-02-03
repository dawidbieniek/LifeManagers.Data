using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LifeManagers.Data.Backup;

internal class BackupManager<T>(IOptions<DataServicesOptions> options, IDbContextFactory<T> contextFactory) : IBackupManager where T : AppDbContextBase
{
    private readonly string _databaseFilePath = Path.Combine(options.Value.DataDirectoryPath, options.Value.DatabaseFileName);
    private readonly IDbContextFactory<T> _contextFactory = contextFactory;

    public void ReplaceDatabaseFile(string sourceDatabaseFilePath)
    {
        SqliteConnection.ClearAllPools();
        File.Copy(sourceDatabaseFilePath, _databaseFilePath, true);
    }

    public void BackupDatabase(string backupPath)
    {
        using T context = _contextFactory.CreateDbContext();

        string? backupDir = Path.GetDirectoryName(backupPath);
        if (!string.IsNullOrEmpty(backupDir))
            Directory.CreateDirectory(backupDir);

        using SqliteConnection backupConnection = new($"Data Source={backupPath}");
        backupConnection.Open();

        using SqliteConnection sourceConnection = (SqliteConnection)context.Database.GetDbConnection();

        if (sourceConnection.State != System.Data.ConnectionState.Open)
            sourceConnection.Open();

        sourceConnection.BackupDatabase(backupConnection);
    }
}