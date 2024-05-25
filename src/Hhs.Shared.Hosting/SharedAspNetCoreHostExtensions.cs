using HsnSoft.Base.AspNetCore;
using HsnSoft.Base.AspNetCore.Mvc.Services;
using HsnSoft.Base.AspNetCore.Serilog;
using HsnSoft.Base.AspNetCore.Serilog.Persistent;
using HsnSoft.Base.Logging;
using HsnSoft.Base.MultiTenancy;
using HsnSoft.Base.Timing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Hhs.Shared.Hosting;

public static class SharedAspNetCoreHostExtensions
{
    // All hostings
    public static IServiceCollection ConfigureSharedHost(this IServiceCollection services)
    {
        services.AddOptions();
        services.AddEndpointsApiExplorer();

        services.AddHttpContextAccessor();
        services.AddScoped<IActionContextAccessor, ActionContextAccessor>();

        services.AddSingleton<IBaseLogger, BaseLogger>();
        services.AddSingleton<IPersistentLogger, FrameworkLogger>();

        return services;
    }

    // Microservices and Mvc Apps Base
    public static IServiceCollection ConfigureSharedAspNetCoreHost(this IServiceCollection services)
    {
        services.ConfigureSharedHost();

        services.AddBaseAspNetCoreContextCollection();
        services.AddBaseAspNetCoreJsonLocalization();
        services.AddBaseMultiTenancyServiceCollection();
        services.AddBaseTimingServiceCollection();

        services.AddControllers();

        return services;
    }

    public static IServiceCollection AddMvcRazorRender(this IServiceCollection services)
    {
        services.AddScoped<IRazorRenderService, RazorRenderService>();

        return services;
    }

    public static IServiceCollection AddCorsSettings(this IServiceCollection services, string corsName)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(corsName, policy =>
            {
                policy.SetIsOriginAllowed(isOriginAllowed: _ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static void UseCorsSettings(this IApplicationBuilder app, string corsName)
    {
        app.UseCors(corsName);
    }
}