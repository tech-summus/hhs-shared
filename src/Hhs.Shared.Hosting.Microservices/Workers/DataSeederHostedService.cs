using HsnSoft.Base.Data;
using HsnSoft.Base.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hhs.Shared.Hosting.Microservices.Workers;

public class DataSeederHostedService : IHostedService
{
    private readonly IBaseLogger _logger;

    private readonly IServiceScopeFactory _scopeFactory;

    public DataSeederHostedService(IServiceScopeFactory scopeFactory, IBaseLogger logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DataSeederHostedService | Started");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await SeedOperation(cancellationToken);
                _logger.LogInformation($"DataSeederHostedService | Successfully completed - {DateTime.UtcNow:yyyyMMdd hh:mm:ss}");
                break;
            }
            catch (OperationCanceledException) { }

            _logger.LogError($"DataSeederHostedService | Failed - {DateTime.UtcNow:yyyyMMdd hh:mm:ss}");
            break;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DataSeederHostedService | Stopped");
        return Task.CompletedTask;
    }

    private async Task SeedOperation(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IBasicDataSeeder>();
        await seeder.EnsureSeedDataAsync(cancellationToken);
    }
}