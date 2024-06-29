using HsnSoft.Base.Logging;

namespace Hhs.Shared.Helper.Models;

public sealed class FrameworkLogModel : IPersistentLog
{
    public string LogId { get; set; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; set; } /*HttpContext CorrelationId*/
    public string Facility { get; set; }
    public string Description { get; set; }
    public string Reference { get; set; }
    public StackTraceLogDetail StackTrace { get; set; }
}

public sealed class StackTraceLogDetail
{
    public string StackFileName { get; set; }
    public string StackMethodName { get; set; }
    public int StackLineNumber { get; set; }
}