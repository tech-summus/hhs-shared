using System.Net;
using Hhs.Shared.Hosting.Microservices.Handlers;
using HsnSoft.Base.Communication;
using HsnSoft.Base.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Hhs.Shared.Hosting.Microservices.Filters;

[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequestResponseActionFilterAttribute : Attribute, IActionFilter
{
    private readonly IBaseLogger _logger;
    private readonly IResponseExceptionHandler _handler;
    private readonly IWebHostEnvironment _env;
    private readonly MicroserviceHostingSettings _settings;
    private readonly JsonSerializerSettings _serializerSettings;

    public RequestResponseActionFilterAttribute(IBaseLogger logger,
        IResponseExceptionHandler handler,
        IWebHostEnvironment env,
        IOptions<MicroserviceHostingSettings> settings,
        IStringLocalizerFactory factory)
    {
        _logger = logger;
        _handler = handler;
        _env = env;
        _settings = settings.Value;

        _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver() { NamingStrategy = new DefaultNamingStrategy() },
            // ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() },
            // ContractResolver = new DefaultContractResolver() { NamingStrategy = new KebabCaseNamingStrategy() },
            //ContractResolver = new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() },

            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,

            Formatting = Formatting.Indented
        };
        // _serializerSettings.Converters.Add(new BaseTimeSpanConverter());
        // _serializerSettings.Converters.Add(new ObjectIdConverter());
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Log etc.

        if (context.ModelState.IsValid) return;

        var messages = new List<string> { "InvalidModelStateErrorMessage" };

        if (!_env.IsHhsProduction())
        {
            var errorsInModelState = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage)).ToArray();

            messages.AddRange(from error in errorsInModelState from subError in error.Value select $"{error.Key}: {subError}");
        }

        context.Result = new ContentResult
        {
            Content = JsonConvert.SerializeObject(new BaseResponse { StatusCode = (int)HttpStatusCode.BadRequest, StatusMessages = messages }, _serializerSettings),
            StatusCode = (int)HttpStatusCode.BadRequest,
            ContentType = "application/json"
        };
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
            context.Result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(new BaseResponse { StatusCode = code, StatusMessages = messages }, _serializerSettings),
                StatusCode = code,
                ContentType = "application/json"
            };
        }
        else // Manipulate response data
        {
            if (context.Result is RedirectResult) return;

            if (!_settings.IsActiveResponseDataManipulation) return;

            var newContent = context.Result switch
            {
                ObjectResult or => JsonConvert.SerializeObject(
                    new BaseResponse<object>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        StatusMessages = new List<string> { _handler.GetStatusCodeDescription((int)HttpStatusCode.OK) },
                        Payload = or.Value
                    }
                    , _serializerSettings),
                EmptyResult => JsonConvert.SerializeObject(
                    new BaseResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        StatusMessages = new List<string> { _handler.GetStatusCodeDescription((int)HttpStatusCode.OK) }
                    }
                    , _serializerSettings),
                _ => JsonConvert.SerializeObject(string.Empty, _serializerSettings)
            };

            context.Result = new ContentResult
            {
                Content = newContent, StatusCode = (int)HttpStatusCode.OK, ContentType = "application/json"
            };
        }
    }
}