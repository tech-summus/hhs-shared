using HsnSoft.Base.Logging;

namespace Hhs.Shared.Hosting;

public sealed record HhsLogModel(
    string LogId,
    DateTime LogTimeUtc,
    string LogMessage
) : IPersistentLog
{
    public string LogId { get; } = LogId;

    public DateTime LogTimeUtc { get; } = LogTimeUtc;

    public string LogMessage { get; } = LogMessage;
}