using Hhs.Shared.Hosting.Microservices.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Hhs.Shared.Hosting.Microservices.Middlewares;

public sealed class RequestResponseLoggerMiddleware : IMiddleware
{
    private readonly IRequestResponseLogModelCreator _logCreator;
    private readonly RequestResponseLoggerOption _options;
    private readonly IRequestResponseLogger _logger;

    public RequestResponseLoggerMiddleware(IRequestResponseLogModelCreator logCreator,
        IOptions<MicroserviceSettings> settings,
        IRequestResponseLogger logger)
    {
        _logCreator = logCreator;
        _options = settings.Value.RequestResponseLogger;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_options is not { IsEnabled: true })
        {
            await next(context);
            return;
        }

        var log = _logCreator.LogModel;
        log.RequestDateTimeUtc = DateTime.UtcNow;
        var request = context.Request;

        /*log*/
        log.LogId = Guid.NewGuid().ToString();
        log.TraceId = context.TraceIdentifier;
        var ip = request.HttpContext.Connection.RemoteIpAddress;
        log.ClientIp = ip?.ToString();
        log.Node = _options.Name;

        /*request*/
        log.RequestMethod = request.Method;
        log.RequestPath = request.Path;
        log.RequestQuery = request.QueryString.ToString();
        log.RequestQueries = FormatQueries(request.QueryString.ToString());
        log.RequestHeaders = FormatHeaders(request.Headers);
        log.RequestBody = await ReadBodyFromRequest(request);
        log.RequestScheme = request.Scheme;
        log.RequestHost = request.Host.ToString();
        log.RequestContentType = request.ContentType;

        // Temporarily replace the HttpResponseStream, 
        // which is a write-only stream, with a MemoryStream to capture 
        // its value in-flight.
        var response = context.Response;
        var originalResponseBody = response.Body;
        var newResponseBody = new MemoryStream();
        response.Body = newResponseBody;

        try
        {
            // Call the next middleware in the pipeline
            await next(context);
        }
        catch (Exception exception)
        {
            LogError(log, exception);
        }

        newResponseBody.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(newResponseBody).ReadToEndAsync();

        newResponseBody.Seek(0, SeekOrigin.Begin);
        await newResponseBody.CopyToAsync(originalResponseBody);
        await newResponseBody.DisposeAsync();

        /*response*/
        log.ResponseContentType = response.ContentType;
        log.ResponseStatus = response.StatusCode.ToString();
        log.ResponseHeaders = FormatHeaders(response.Headers);
        log.ResponseBody = responseBodyText;
        log.ResponseDateTimeUtc = DateTime.UtcNow;

        /*exception: but was managed at app.UseExceptionHandler() or by any middleware*/
        var contextFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (contextFeature != null)
        {
            var exception = contextFeature.Error;
            LogError(log, exception);
        }

        //var jsonString = logCreator.LogString(); /*log json*/
        _logger.Log(_logCreator);
    }

    private void LogError(RequestResponseLogModel log, Exception exception)
    {
        log.ExceptionMessage = exception.Message;
        log.ExceptionStackTrace = exception.StackTrace;
    }

    private Dictionary<string, string> FormatHeaders(IHeaderDictionary headers)
    {
        var pairs = new Dictionary<string, string>();
        foreach (var header in headers)
        {
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
}