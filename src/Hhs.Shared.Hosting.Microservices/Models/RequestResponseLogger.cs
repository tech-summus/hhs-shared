using Microsoft.Extensions.Logging;

namespace Hhs.Shared.Hosting.Microservices.Models;

public interface IRequestResponseLogger
{
    void Log(IRequestResponseLogModelCreator logCreator);
}

public sealed class RequestResponseLogger : IRequestResponseLogger
{
    private readonly ILogger<RequestResponseLogger> _logger;

    public RequestResponseLogger(ILogger<RequestResponseLogger> logger)
    {
        _logger = logger;
    }

    public void Log(IRequestResponseLogModelCreator logCreator)
    {
        _logger.LogTrace(logCreator.LogString());
    }
}