using Microsoft.Extensions.Options;

namespace LifeManagers.Data.Backup;

internal class PeriodicBackuper(BackupManager backupManager, IOptions<DataServicesOptions> options)
{
    private readonly BackupManager _backupManager = backupManager;
    private readonly string _backupDirectory = Path.Combine(options.Value.DataDirectoryPath, options.Value.BackupDirectory);
    private readonly string _backupLastTimeFilePath = Path.Combine(options.Value.DataDirectoryPath, options.Value.LastBackupTimeFileName);
    private readonly TimeSpan _backupPeriod = options.Value.BackupPeriod!.Value;

    public async Task PerformBackupIfNecessaryAsync()
    {
        if (!await IsBackupNecessary())
            return;

        using (Stream backupStream = _backupManager.CreateBackupStream())
        {
            if (!Directory.Exists(_backupDirectory))
                Directory.CreateDirectory(_backupDirectory);

            using FileStream saveFileStream = File.Create(Path.Combine(_backupDirectory, $"{DateTime.Today.ToShortTimeString()}backup.db3"));
            await backupStream.CopyToAsync(saveFileStream);
        }

        await SaveBackupTime();
    }

    private async Task<bool> IsBackupNecessary()
    {
        if (!File.Exists(_backupLastTimeFilePath))
            return true;

        string fileText = await File.ReadAllTextAsync(_backupLastTimeFilePath);
        DateTime lastBackupTime = DateTime.Parse(fileText);

        return DateTime.Now >= lastBackupTime + _backupPeriod;
    }

    private Task SaveBackupTime() => File.WriteAllTextAsync(_backupLastTimeFilePath, DateTime.Now.ToString());
}