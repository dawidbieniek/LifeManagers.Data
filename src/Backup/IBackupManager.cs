namespace LifeManagers.Data.Backup;

public interface IBackupManager
{
    /// <summary>
    /// Overrides current database file with one specified at <paramref name="sourceDatabaseFilePath"/>
    /// </summary>
    void ReplaceDatabaseFile(string sourceDatabaseFilePath);

    /// <summary>
    /// Creates stream for reading of database file. Should be used for copying the database.
    /// </summary>
    Stream CreateBackupStream();
}