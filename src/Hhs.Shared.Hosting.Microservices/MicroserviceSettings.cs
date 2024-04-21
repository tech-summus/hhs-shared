using Hhs.Shared.Hosting.Microservices.Models;

namespace Hhs.Shared.Hosting.Microservices;

public sealed class MicroserviceSettings
{
    public bool IsActiveResponseDataManipulation { get; set; }

    public bool IgnoreNullValueForJsonResponse { get; set; }

    public RequestResponseLoggerOption RequestResponseLogger { get; set; } = new();
}