namespace Hhs.Shared.Hosting.Microservices.Models;

public sealed class RequestResponseLoggerOption
{
    public bool IsEnabled { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
}