using System.Reflection;
using Hhs.Shared.Hosting.Microservices.Handlers;
using HsnSoft.Base.Communication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Hhs.Shared.Hosting.Microservices.Middlewares;

public sealed class GlobalExceptionHandlerMiddleware : IMiddleware
{
    private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType!);
    private readonly IResponseExceptionHandler _handler;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(IResponseExceptionHandler handler, IWebHostEnvironment env)
    {
        _handler = handler;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Temporarily replace the HttpResponseStream, 
        // which is a write-only stream, with a MemoryStream to capture 
        // its value in-flight.
        var response = context.Response;
        var originalResponseBody = response.Body;
        var newResponseBody = new MemoryStream();
        response.Body = newResponseBody;

        var hasException = false;
        try
        {
            // Call the next middleware in the pipeline
            await next(context);
        }
        catch (Exception exception)
        {
            hasException = true;

            #region When exception filter is disabled, will be run this code block!

            Logger.Error("GlobalExceptionHandlerMiddleware -> Error Message: {ErrorMessage}", exception.Message);
            var (code, messages) = _handler.Handle(exception, _env);
            response.StatusCode = code;
            await response.WriteAsJsonAsync(new DtoResponse(messages, code));

            #endregion
        }

        // When client has not access ActionLayer, will be response Json error response
        if (!hasException && response.ContentLength == null && response.StatusCode is >= 400 and < 600)
        {
            Logger.Error("GlobalExceptionHandlerMiddleware -> Client Access Error Status: {ErrorStatus} | {ErrorMessage}"
                , response.StatusCode.ToString()
                , _handler.GetStatusCodeDescription(response.StatusCode));

            if (response.Body.Length > 0 && response.StatusCode < 500)
            {
                #region When Controller -> ConfigureApiBehaviorOptions -> SuppressModelStateInvalidFilter => false, will be run this code block!

                if (!_env.IsProduction())
                {
                    response.Body.Seek(0, SeekOrigin.Begin);
                    var errorBodyText = await new StreamReader(response.Body).ReadToEndAsync();
                    Logger.Information($"{new string('-', 20)} GlobalExceptionHandlerMiddleware -> Old Body {new string('-', 20)}");
                    Logger.Information("{OldBody}", errorBodyText);
                    Logger.Information($"{new string('-', 20)} GlobalExceptionHandlerMiddleware -> Old Body {new string('-', 20)}");
                }

                // Reset response body
                newResponseBody = new MemoryStream();
                response.Body = newResponseBody;

                #endregion
            }

            await response.WriteAsJsonAsync(new DtoResponse(_handler.GetStatusCodeDescription(response.StatusCode), response.StatusCode));
        }

        if (!_env.IsProduction() && response.StatusCode >= 400)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(response.Body).ReadToEndAsync();
            Logger.Information($"{new string('-', 20)} GlobalExceptionHandlerMiddleware -> Response Body {new string('-', 20)}");
            Logger.Information("{ResponseBody}", responseBodyText);
            Logger.Information($"{new string('-', 20)} GlobalExceptionHandlerMiddleware -> Response Body {new string('-', 20)}");
        }

        newResponseBody.Seek(0, SeekOrigin.Begin);
        await newResponseBody.CopyToAsync(originalResponseBody);
        await newResponseBody.DisposeAsync();
    }
}