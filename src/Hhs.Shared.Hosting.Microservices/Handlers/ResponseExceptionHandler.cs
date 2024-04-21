using System.Net;
using HsnSoft.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Hhs.Shared.Hosting.Microservices.Handlers;

internal sealed class ResponseExceptionHandler : IResponseExceptionHandler
{
    public (int code, List<string> messages) Handle(Exception ex, IHostEnvironment env)
    {
        var code = StatusCodes.Status500InternalServerError;
        var messages = new List<string>();

        if (ex is null) return (code, messages);

        switch (ex)
        {
            // Some logic to handle specific exceptions
            case BusinessException be:
            {
                code = StatusCodes.Status400BadRequest;
                messages.Add(!string.IsNullOrWhiteSpace(be.Message) ? be.Message : GetStatusCodeDescription(code));
                if (!string.IsNullOrWhiteSpace(be.ErrorCode)) messages.Add(be.ErrorCode);
                if (be.Data is { Count: > 0 })
                {
                    messages.AddRange(be.GetDictionaryDataList()
                        .Select(data => $"{data.Key}: {data.Value}"));
                }

                if (be.InnerException != null && !env.IsProduction())
                {
                    messages.AddRange(be.InnerException.GetMessages());
                }

                break;
            }
            case DomainException de:
            {
                code = StatusCodes.Status400BadRequest;
                messages.Add(!string.IsNullOrWhiteSpace(de.Message) ? de.Message : GetStatusCodeDescription(code));
                if (de.Data is { Count: > 0 })
                {
                    messages.AddRange(de.GetDictionaryDataList()
                        .Select(data => $"{data.Key}: {data.Value}"));
                }

                if (de.InnerException != null && !env.IsProduction())
                {
                    messages.AddRange(de.InnerException.GetMessages());
                }

                break;
            }
            case BaseHttpException he:
            {
                code = he.HttpStatusCode;
                messages.Add(!string.IsNullOrWhiteSpace(he.Message) ? he.Message : GetStatusCodeDescription(code));
                if (he.Data is { Count: > 0 })
                {
                    messages.AddRange(he.GetDictionaryDataList()
                        .Select(data => $"{data.Key}: {data.Value}"));
                }

                if (he.InnerException != null && !env.IsProduction())
                {
                    messages.AddRange(he.InnerException.GetMessages());
                }

                break;
            }
            default:
            {
                messages.Add(GetStatusCodeDescription(code));
                if (!env.IsProduction())
                {
                    if (!string.IsNullOrWhiteSpace(ex.Message)) messages.Add(ex.Message);
                    messages.AddRange(ex.InnerException.GetMessages());
                }

                break;
            }
        }


        return (code, messages);
    }

    public string GetStatusCodeDescription(int statusCode)
    {
        if (statusCode is < 200 or > 520) return string.Empty;

        return (HttpStatusCode)statusCode switch
        {
            HttpStatusCode.BadRequest => "InvalidModelStateErrorMessage",
            HttpStatusCode.Unauthorized => "UnauthorizedRequest",
            HttpStatusCode.Forbidden => "ForbiddenRequest",
            HttpStatusCode.RequestTimeout => "RequestTimeout",
            HttpStatusCode.UnsupportedMediaType => "UnsupportedRequestContentType",
            HttpStatusCode.OK => "SuccessRequest",
            _ => ((HttpStatusCode)statusCode).ToString()
        };
    }
}