namespace Hhs.Shared.Hosting.Workers;

public sealed class DefaultSeeder : ISeeder
{
    public Task EnsureSeedDataAsync(IServiceProvider provider)
    {
        return Task.CompletedTask;
    }
}