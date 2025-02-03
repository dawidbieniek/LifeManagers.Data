namespace LifeManagers.Data.Backup;

public interface IBackupManager
{
    /// <summary>
    /// Overrides current database file with one specified at <paramref name="sourceDatabaseFilePath"/>
    /// </summary>
    void ReplaceDatabaseFile(string sourceDatabaseFilePath);

    /// <summary>
    /// Creates copy of current database file in <paramref name="backupPath"/>
    /// </summary>
    void BackupDatabase(string backupPath);
}