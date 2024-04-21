using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Hhs.Shared.Hosting;

public static class SerilogConfigurationHelper
{
    public static ILogger Configure(string applicationName)
    {
        ILogger logger = new LoggerConfiguration()
#if DEBUG
            // .MinimumLevel.Debug()
            .MinimumLevel.Verbose()
#else
                .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", $"{applicationName}")
            .WriteTo.Async(c => c.File("Logs/logs.txt")) // All logs , Verbose,Debug,Information, Warning, Error, Fatal
            .WriteTo.Async(c => c.Console // All logs , Verbose,Debug,Information, Warning, Error, Fatal
            (
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                theme: AnsiConsoleTheme.Code
            ))
            .CreateLogger();

        return logger;
    }
}