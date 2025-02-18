﻿using Microsoft.Extensions.Options;

namespace LifeManagers.Data.Backup;

internal class PeriodicBackuper(IBackupManager backupManager, IOptions<DataServicesOptions> options) : IPeriodicBackuper
{
    private readonly IBackupManager _backupManager = backupManager;
    private readonly string _backupDirectory = Path.Combine(options.Value.DataDirectoryPath, options.Value.BackupDirectory);
    private readonly string _backupLastTimeFilePath = Path.Combine(options.Value.DataDirectoryPath, options.Value.LastBackupTimeFileName);
    private readonly TimeSpan _backupPeriod = options.Value.BackupPeriod!.Value;

    public async Task<DateTime?> GetLastBackupDateTimeAsync()
    {
        if (!File.Exists(_backupLastTimeFilePath))
            return null;

        string fileText = await File.ReadAllTextAsync(_backupLastTimeFilePath);
        return DateTime.Parse(fileText);
    }

    public async Task PerformBackupIfNecessaryAsync()
    {
        if (!await IsBackupNecessary())
            return;

        _backupManager.BackupDatabase(Path.Combine(_backupDirectory, $"{DateTime.Today.ToShortDateString()}-backup.db3"));
        await SaveBackupTimeAsync();
    }

    private async Task<bool> IsBackupNecessary() => !File.Exists(_backupLastTimeFilePath) || DateTime.Now >= await GetLastBackupDateTimeAsync()! + _backupPeriod;

    private Task SaveBackupTimeAsync() => File.WriteAllTextAsync(_backupLastTimeFilePath, DateTime.Now.ToString());
}