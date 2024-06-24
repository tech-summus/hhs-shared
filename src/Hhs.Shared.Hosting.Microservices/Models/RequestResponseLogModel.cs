using HsnSoft.Base.AspNetCore.Logging;

namespace Hhs.Shared.Hosting.Microservices.Models;

public sealed class RequestResponseLogModel : IRequestResponseLog
{
    public string LogId { get; set; } = Guid.NewGuid().ToString();
    public string TraceId { get; set; } /*HttpContext TraceIdentifier*/
    public string CorrelationId { get; set; } /*HttpContext CorrelationId*/
    public string Facility { get; set; }
    public string Node { get; set; } /*project name*/
    public ClientInfoLogDetail ClientInfo { get; set; }
    public RequestInfoLogDetail RequestInfo { get; set; }
    public ResponseInfoLogDetail ResponseInfo { get; set; }
    public string RequestResponseWorkingTime { get; set; }
    public string ExceptionMessage { get; set; }
    public string ExceptionStackTrace { get; set; }
}

public sealed class ClientInfoLogDetail
{
    public string ClientIp { get; set; }
    public string ClientLat { get; set; }
    public string ClientLong { get; set; }
    public string ClientVersion { get; set; }
    public string ClientUserId { get; set; }
    public string ClientUserRole { get; set; }
}

public sealed class RequestInfoLogDetail
{
    public DateTime? RequestDateTimeUtc { get; set; }
    public string RequestPath { get; set; }
    public string RequestQuery { get; set; }
    public List<KeyValuePair<string, string>> RequestQueries { get; set; }
    public string RequestMethod { get; set; }
    public string RequestScheme { get; set; }
    public string RequestHost { get; set; }
    public Dictionary<string, string> RequestHeaders { get; set; }
    public string RequestBody { get; set; }
    public string RequestContentType { get; set; }
}

public sealed class ResponseInfoLogDetail
{
    public DateTime? ResponseDateTimeUtc { get; set; }
    public string ResponseStatus { get; set; }
    public Dictionary<string, string> ResponseHeaders { get; set; }
    public string ResponseBody { get; set; }
    public string ResponseContentType { get; set; }
}