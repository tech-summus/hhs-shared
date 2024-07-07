using System.Diagnostics;
using System.Security.Claims;
using Hhs.Shared.Hosting.Models;
using HsnSoft.Base.AspNetCore.Logging;
using HsnSoft.Base.AspNetCore.Tracing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Hhs.Shared.Hosting.Middlewares;

public sealed class RequestResponseLoggerMiddleware : IMiddleware
{
    private readonly HostingSettings _settings;
    private readonly IRequestResponseLogger _logger;

    public RequestResponseLoggerMiddleware(IOptions<HostingSettings> settings, IRequestResponseLogger logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_settings?.IsEnabledRequestResponseLogger == false)
        {
            await next(context);
            return;
        }

        var watch = new Stopwatch();
        watch.Start();

        var reqStartTime = DateTime.UtcNow;
        var log = new RequestResponseLogModel();
        var request = context.Request;

        /*log*/
        log.LogId = Guid.NewGuid().ToString();
        log.TraceId = context.TraceIdentifier;
        log.CorrelationId = context.GetCorrelationId();
        log.Facility = RequestResponseLogFacility.HTTP_REQUEST_LOG.ToString();


        var ip = request.HttpContext.Connection.RemoteIpAddress;
        log.ClientInfo = new ClientInfoLogDetail
        {
            GatewayIp = ip?.ToString(),
            ClientOriginIp = null,
            ClientOriginHost = null,
            ClientLat = context.GetClientRequestLat(),
            ClientLong = context.GetClientRequestLong(),
            ClientVersion = context.GetClientVersion(),
            ClientUserId = Guid.Empty.ToString(),
            ClientUserRole = "anonymous",
            ClientUserAgent = null,
            ClientLanguage = "en"
        };

        if (request.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedClientIp))
        {
            log.ClientInfo.ClientOriginIp = forwardedClientIp.ToString();
        }

        if (request.HttpContext.Request.Headers.TryGetValue("Origin", out var originHost))
        {
            log.ClientInfo.ClientOriginHost = originHost.ToString();
        }

        if (string.IsNullOrWhiteSpace(log.ClientInfo.ClientOriginHost) || log.ClientInfo.ClientOriginHost == "null")
        {
            if (request.HttpContext.Request.Headers.TryGetValue("Referer", out var refererHost))
            {
                log.ClientInfo.ClientOriginHost = refererHost.ToString();
            }
        }

        if (string.IsNullOrWhiteSpace(log.ClientInfo.ClientOriginHost) || log.ClientInfo.ClientOriginHost == "null")
        {
            if (request.HttpContext.Request.Headers.TryGetValue("From", out var fromHost))
            {
                log.ClientInfo.ClientOriginHost = fromHost.ToString();
            }
        }

        if (request.HttpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent))
        {
            log.ClientInfo.ClientUserAgent = userAgent.ToString();
        }

        if (request.HttpContext.Request.Headers.TryGetValue("Accept-Language", out var acceptLanguage))
        {
            log.ClientInfo.ClientLanguage = acceptLanguage.ToString();
        }

        /*request*/
        log.RequestInfo = new RequestInfoLogDetail
        {
            RequestDateTimeUtc = reqStartTime,
            RequestMethod = request.Method,
            RequestPath = request.Path,
            RequestBody = await ReadBodyFromRequest(request),
            RequestScheme = request.Scheme,
            RequestHost = request.Host.ToString()
        };
        var requestQuery = request.QueryString.ToString();
        var requestHeaders = FormatHeaders(request.Headers);

        // Temporarily replace the HttpResponseStream,
        // which is a write-only stream, with a MemoryStream to capture
        // its value in-flight.
        var response = context.Response;
        var originalResponseBody = response.Body;
        var newResponseBody = new MemoryStream();
        response.Body = newResponseBody;

        // _logger.RequestResponseInfoLog(log);

        try
        {
            // Call the next middleware in the pipeline
            await next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
        }

        newResponseBody.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(newResponseBody).ReadToEndAsync();

        newResponseBody.Seek(0, SeekOrigin.Begin);
        await newResponseBody.CopyToAsync(originalResponseBody);
        await newResponseBody.DisposeAsync();

        watch.Stop();

        SetSessionUserInfo(request.HttpContext.User, ref log);

        /*response*/
        var responseHeader = FormatHeaders(response.Headers);
        log.ResponseInfo = new ResponseInfoLogDetail
        {
            ResponseStatus = response.StatusCode.ToString(),
            ResponseBody = responseBodyText,
            ResponseDateTimeUtc = DateTime.UtcNow
        };

        log.RequestResponseWorkingTime = $"{watch.ElapsedMilliseconds:0.####}ms";

        if (response.StatusCode >= 400)
        {
            log.Facility = RequestResponseLogFacility.HTTP_REQUEST_ERROR_LOG.ToString();
            log.RequestInfo.RequestHeaders = requestHeaders;
            log.RequestInfo.RequestQuery = requestQuery;
            log.ResponseInfo.ResponseHeaders = responseHeader;
            _logger.RequestResponseErrorLog(log);
        }
        else
        {
            log.Facility = RequestResponseLogFacility.HTTP_REQUEST_RESPONSE_LOG.ToString();

            //TODO: Remove after test
            log.RequestInfo.RequestHeaders = requestHeaders;
            log.RequestInfo.RequestQuery = requestQuery;
            log.ResponseInfo.ResponseHeaders = responseHeader;
            /////////////////////////

            _logger.RequestResponseInfoLog(log);
        }
    }

    private Dictionary<string, string> FormatHeaders(IHeaderDictionary headers)
    {
        var pairs = new Dictionary<string, string>();
        foreach (var header in headers)
        {
            if (header.Key.Equals("Authorization") && header.Value.ToString().StartsWith("Bearer "))
            {
                pairs.Add(header.Key, "Bearer --AccessToken--");
                continue;
            }

            pairs.Add(header.Key, header.Value);
        }

        return pairs;
    }

    private List<KeyValuePair<string, string>> FormatQueries(string queryString)
    {
        var pairs = new List<KeyValuePair<string, string>>();
        foreach (var query in queryString.TrimStart('?').Split("&"))
        {
            var items = query.Split("=");
            var key = items.Any() ? items[0] : string.Empty;
            var value = items.Length >= 2 ? items[1] : string.Empty;
            if (!string.IsNullOrEmpty(key))
            {
                pairs.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        return pairs;
    }

    private async Task<string> ReadBodyFromRequest(HttpRequest request)
    {
        // Ensure the request's body can be read multiple times
        // (for the next middlewares in the pipeline).
        request.EnableBuffering();
        using var streamReader = new StreamReader(request.Body, leaveOpen: true);
        var requestBody = await streamReader.ReadToEndAsync();
        // Reset the request's body stream position for
        // next middleware in the pipeline.
        request.Body.Position = 0;
        return requestBody;
    }

    private static void SetSessionUserInfo(ClaimsPrincipal principal, ref RequestResponseLogModel log)
    {
        var userIdOrNull = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdOrNull != null && !userIdOrNull.Value.IsNullOrWhiteSpace())
        {
            log.ClientInfo.ClientUserId = userIdOrNull.Value;
        }

        var roles = principal.Claims.Where(c => c.Type == ClaimTypes.Role).ToArray() ?? Array.Empty<Claim>();
        if (roles is { Length: > 0 })
        {
            log.ClientInfo.ClientUserRole = roles.Select(c => c.Value).Distinct().ToArray().JoinAsString(",");
        }
    }
}