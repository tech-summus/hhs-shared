using HsnSoft.Base.Data;
using HsnSoft.Base.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hhs.Shared.Hosting.Microservices.Workers;

public class SeederHostedService : IHostedService
{
    private readonly IBaseLogger _logger;

    private readonly IServiceScopeFactory _scopeFactory;

    public SeederHostedService(IServiceScopeFactory scopeFactory, IBaseLogger logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SeederHostedService started");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await LoadConfiguration(cancellationToken);
                _logger.LogInformation($"SeederHostedService successfully completed - {DateTime.UtcNow:yyyyMMdd hh:mm:ss}");
                break;
            }
            catch (OperationCanceledException) { }

            _logger.LogError($"SeederHostedService Failed - {DateTime.UtcNow:yyyyMMdd hh:mm:ss}");
            break;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SeederHostedService stopped");
        return Task.CompletedTask;
    }

    private async Task LoadConfiguration(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IBasicDataSeeder>();
        await seeder.EnsureSeedDataAsync(cancellationToken);
    }
}