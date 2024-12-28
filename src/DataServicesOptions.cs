using System.ComponentModel.DataAnnotations;

using LifeManagers.Data.Seeding;

namespace LifeManagers.Data;

public sealed class DataServicesOptions
{
    internal const string DefaultDatabaseFileName = "data.db3";
    internal const string DefaultBackupDirectory = "backups";
    internal const string DefaultLastBackupTimeFileName = "lastBackup.txt";

    [Required]
    [MinLength(1)]
    public string DataDirectoryPath { get; set; } = null!;
    public string DatabaseFileName { get; set; } = DefaultDatabaseFileName;
    public TimeSpan? BackupPeriod { get; set; } = null;
    public string BackupDirectory { get; set; } = DefaultBackupDirectory;
    public string LastBackupTimeFileName { get; set; } = DefaultLastBackupTimeFileName;
    public Type? SeederType { get; set; } = null;
    public bool DebugMode { get; set; } = false;

    internal void CreateOptions(DataServicesOptions builtOptions)
    {
        Type? type = GetType();

        foreach (var property in type.GetProperties())
        {
            if (property != null && property.CanWrite)
                property.SetValue(this, property.GetValue(builtOptions));
        }
    }
}

public class DataServicesOptionsBuilder<T> where T : AppDbContextBase
{
    private readonly DataServicesOptions _options = new();

    public DataServicesOptionsBuilder<T> WithDataDirectoryPath(string dataDirectoryPath, string databaseFileName = DataServicesOptions.DefaultDatabaseFileName)
    {
        _options.DataDirectoryPath = dataDirectoryPath;
        _options.DatabaseFileName = databaseFileName;
        return this;
    }

    public DataServicesOptionsBuilder<T> EnablePeriodicBackups(TimeSpan period,
        string backupDirectory = DataServicesOptions.DefaultBackupDirectory,
        string lastBackupFileName = DataServicesOptions.DefaultBackupDirectory)
    {
        _options.BackupPeriod = period;
        _options.BackupDirectory = backupDirectory;
        _options.LastBackupTimeFileName = lastBackupFileName;
        return this;
    }

    public DataServicesOptionsBuilder<T> WithSeeder<TSeeder>() where TSeeder : ISeeder<T>
    {
        _options.SeederType = typeof(TSeeder);

        return this;
    }

    public DataServicesOptionsBuilder<T> UsingDebugMode(bool useDebugMode = true)
    {
        _options.DebugMode = useDebugMode;

        return this;
    }

    public DataServicesOptions Build()
    {
        List<ValidationResult> validationResults = [];

        return !Validator.TryValidateObject(_options, new(_options), validationResults)
            ? throw new ValidationException($"Invalid data services options: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}")
            : _options;
    }
}