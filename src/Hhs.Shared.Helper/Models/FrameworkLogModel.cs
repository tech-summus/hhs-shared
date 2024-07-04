using HsnSoft.Base.Logging;
using JetBrains.Annotations;

namespace Hhs.Shared.Helper.Models;

public sealed class FrameworkLogModel : IPersistentLog
{
    [NotNull]
    public string LogId { get; set; } = Guid.NewGuid().ToString();

    [CanBeNull]
    public string CorrelationId { get; set; } /*HttpContext CorrelationId*/

    [NotNull]
    public string Facility { get; set; }

    [CanBeNull]
    public string Description { get; set; }

    public object Reference { get; set; }
    public StackTraceLogDetail StackTrace { get; set; }
}

public sealed class StackTraceLogDetail
{
    [CanBeNull]
    public string StackFileName { get; set; }

    [CanBeNull]
    public string StackMethodName { get; set; }
    public int StackLineNumber { get; set; }
}