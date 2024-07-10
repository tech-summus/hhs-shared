using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Hhs.Shared.Hosting.Middlewares;

public sealed class SearchEngineAgentMiddleware : IMiddleware
{
    private readonly IWebHostEnvironment _env;

    public SearchEngineAgentMiddleware(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Path.StartsWithSegments("/robots.txt"))
        {
            await next(context);
            return;
        }

        var robotsTxtPath = Path.Combine(_env.ContentRootPath, "robots.txt");
        var output = "User-agent: *  \nDisallow: /";
        if (File.Exists(robotsTxtPath))
        {
            output = await File.ReadAllTextAsync(robotsTxtPath);
        }

        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(output);
    }
}