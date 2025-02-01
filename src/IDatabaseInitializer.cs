namespace LifeManagers.Data;

public interface IDatabaseInitializer
{
    event EventHandler<string> StepExecuting;

    Task InitializeDatabaseAsync();
}