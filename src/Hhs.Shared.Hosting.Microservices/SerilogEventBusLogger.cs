using HsnSoft.Base.AspNetCore.Serilog;
using HsnSoft.Base.EventBus.Logging;
using Serilog.Events;

namespace Hhs.Shared.Hosting.Microservices;

public sealed class SerilogEventBusLogger : SerilogBaseLogger, IEventBusLogger
{
    public void EventBusInfoLog<T>(T t) where T : IEventBusLog => Write(LogEventLevel.Verbose, t);

    public void EventBusErrorLog<T>(T t) where T : IEventBusLog => Write(LogEventLevel.Fatal, t);

    private void Write<T>(LogEventLevel logLevel, T log) => BaseLogger.Write(logLevel, "{@Log}", log);
}