using HsnSoft.Base.Logging;

namespace Hhs.Shared.Hosting.Microservices.Models;

public interface IRequestResponseLogger
{
    void Log(IRequestResponseLogModelCreator logCreator);
}

public sealed class RequestResponseLogger : IRequestResponseLogger
{
    private readonly IBaseLogger _logger;

    public RequestResponseLogger(IBaseLogger logger)
    {
        _logger = logger;
    }

    public void Log(IRequestResponseLogModelCreator logCreator)
    {
        _logger.LogInformation(logCreator.LogString());
    }
}