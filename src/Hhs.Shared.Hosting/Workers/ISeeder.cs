namespace Hhs.Shared.Hosting.Workers;

public interface ISeeder
{
    Task EnsureSeedDataAsync(IServiceProvider provider);
}