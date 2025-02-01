namespace LifeManagers.Data.Backup;

public interface IPeriodicBackuper
{
    Task<DateTime?> GetLastBackupDateTimeAsync();

    /// <summary>
    /// Compares last backup time with current time. If difference is larger than specified period,
    /// performs backup.
    /// </summary>
    /// <returns> </returns>
    Task PerformBackupIfNecessaryAsync();
}