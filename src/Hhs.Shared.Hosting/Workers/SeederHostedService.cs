using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Hhs.Shared.Hosting.Workers;

public class SeederHostedService : IHostedService
{
    private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType!);
  
    private readonly IServiceScopeFactory _scopeFactory;

    public SeederHostedService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.Information("SeederHostedService started");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await LoadConfiguration(cancellationToken);
                Logger.Information($"SeederHostedService successfully completed - {DateTime.UtcNow:yyyyMMdd hh:mm:ss}");
                break;
            }
            catch (OperationCanceledException) { }

            Logger.Error($"SeederHostedService Failed - {DateTime.UtcNow:yyyyMMdd hh:mm:ss}");
            break;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.Information("SeederHostedService stopped");
        return Task.CompletedTask;
    }

    private async Task LoadConfiguration(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        await seeder.EnsureSeedDataAsync(scope.ServiceProvider);
    }
}