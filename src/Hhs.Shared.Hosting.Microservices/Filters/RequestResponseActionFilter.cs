using System.Net;
using System.Text.Json;
using Hhs.Shared.Hosting.Microservices.Handlers;
using HsnSoft.Base.Communication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hhs.Shared.Hosting.Microservices.Filters;

[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequestResponseActionFilterAttribute : Attribute, IActionFilter
{
    private readonly ILogger<RequestResponseActionFilterAttribute> _logger;
    private readonly IResponseExceptionHandler _handler;
    private readonly IWebHostEnvironment _env;
    private readonly MicroserviceSettings _settings;

    public RequestResponseActionFilterAttribute(ILogger<RequestResponseActionFilterAttribute> logger,
        IResponseExceptionHandler handler,
        IWebHostEnvironment env,
        IOptions<MicroserviceSettings> settings,
        IStringLocalizerFactory factory)
    {
        _logger = logger;
        _handler = handler;
        _env = env;
        _settings = settings.Value;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Log etc.

        if (context.ModelState.IsValid) return;

        var messages = new List<string> { "InvalidModelStateErrorMessage" };

        if (!_env.IsProduction())
        {
            var errorsInModelState = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage)).ToArray();

            messages.AddRange(from error in errorsInModelState from subError in error.Value select $"{error.Key}: {subError}");
        }

        context.Result = new ContentResult { Content = new DtoResponse(messages, (int)HttpStatusCode.BadRequest).ToJsonString(), StatusCode = (int)HttpStatusCode.BadRequest, ContentType = "application/json" };
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Custom exception filter
        if (context.Exception != null)
        {
            _logger.LogError("Request Response Exception Filter -> Error Message: {ErrorMessage}", context.Exception.Message);

            var (code, messages) = _handler.Handle(context.Exception, _env);

            context.Exception = null!;
            context.ExceptionDispatchInfo = null!;
            context.ExceptionHandled = true;
            context.Result = new ContentResult { Content = new DtoResponse(messages, code).ToJsonString(), StatusCode = code, ContentType = "application/json" };
        }
        else // Manipulate response data
        {
            if (context.Result is RedirectResult) return;

            if (!_settings.IsActiveResponseDataManipulation) return;

            var newContent = context.Result switch
            {
                ObjectResult or => JsonSerializer.Serialize(new DtoResponse<object>(payload: or.Value, message: _handler.GetStatusCodeDescription((int)HttpStatusCode.OK), code: (int)HttpStatusCode.OK)),
                EmptyResult => JsonSerializer.Serialize(new DtoResponse(message: _handler.GetStatusCodeDescription((int)HttpStatusCode.OK), code: (int)HttpStatusCode.OK)),
                _ => JsonSerializer.Serialize(string.Empty)
            };

            context.Result = new ContentResult
            {
                Content = newContent, StatusCode = (int)HttpStatusCode.OK, ContentType = "application/json"
            };
        }
    }
}