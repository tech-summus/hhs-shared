using System.Diagnostics;
using Hhs.Shared.Helper.Models;
using JetBrains.Annotations;

namespace Hhs.Shared.Helper.Utils;

public static class LogHelper
{
    public static FrameworkLogModel Generate([NotNull] string message, [CanBeNull] string reference = null, [CanBeNull] string facility = null, [CanBeNull] string correlationId = null, Exception exception = null)
    {
        var result = new FrameworkLogModel
        {
            CorrelationId = correlationId,
            Facility = facility,
            Description = message,
            Reference = reference,
            StackTrace = null
        };

        if (exception == null) return result;

        var stackFrame = (new StackTrace(exception, true)).GetFrame(0);
        result.StackTrace = new StackTraceLogDetail
        {
            StackFileName = stackFrame?.GetFileName(),
            StackMethodName = stackFrame?.GetMethod()?.Name,
            StackLineNumber = stackFrame?.GetFileLineNumber() ?? 0
        };

        return result;
    }
}