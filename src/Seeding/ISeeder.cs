namespace LifeManagers.Data.Seeding;

public interface ISeeder<T> where T : AppDbContextBase
{
    public Task SeedRequiredDataAsync();

    public Task SeedDebugDataAsync();
}