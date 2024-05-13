using HsnSoft.Base.AspNetCore;
using HsnSoft.Base.AspNetCore.Mvc.Services;
using HsnSoft.Base.AspNetCore.Serilog;
using HsnSoft.Base.AspNetCore.Serilog.Persistent;
using HsnSoft.Base.Logging;
using HsnSoft.Base.MultiTenancy;
using HsnSoft.Base.Timing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

    public static IServiceCollection AddCorsSettings(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env, string corsName)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(corsName, policy =>
            {
                if (!env.IsProduction() || string.IsNullOrWhiteSpace(corsName) || corsName == "default")
                {
                    policy
                        // .SetIsOriginAllowed((host) => true)
                        .SetIsOriginAllowed(isOriginAllowed: _ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
                else
                {
                    policy.WithOrigins( //new []{ "http://localhost:3000"}
                            configuration["App:CorsOrigins"]
                                ?.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                // .Select(o => o.Trim().RemovePostFix("/"))
                                .ToArray() ?? throw new InvalidOperationException()
                        )
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                }
            });
        });

        return services;
    }

    public static void UseCorsSettings(this IApplicationBuilder app, string corsName)
    {
        app.UseCors(corsName);
    }
}