using Hhs.Shared.Hosting.Workers;
using HsnSoft.Base.AspNetCore;
using HsnSoft.Base.AspNetCore.Mvc.Services;
using HsnSoft.Base.Auditing;
using HsnSoft.Base.Data;
using HsnSoft.Base.MultiTenancy;
using HsnSoft.Base.Security;
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
    public static IServiceCollection ConfigureSharedHost(this IServiceCollection services)
    {
        services.AddOptions();

        // Seeder functionality
        services.AddScoped<ISeeder, DefaultSeeder>();
        services.AddHostedService<SeederHostedService>();

        return services;
    }

    public static IServiceCollection ConfigureSharedAspNetCoreHost(this IServiceCollection services)
    {
        services.ConfigureSharedHost();

        services.AddBaseMultiTenancyServiceCollection();
        services.AddBaseSecurityServiceCollection();
        services.AddBaseTimingServiceCollection();
        services.AddBaseAuditingServiceCollection();
        services.AddBaseDataServiceCollection();
        services.AddBaseAspNetCoreServiceCollection();
        services.AddBaseAspNetCoreJsonLocalization();

        services.AddEndpointsApiExplorer();

        services.AddHttpContextAccessor();
        services.AddScoped<IActionContextAccessor, ActionContextAccessor>();
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