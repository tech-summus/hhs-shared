using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MMLib.Ocelot.Provider.AppConfiguration;
using Ocelot.DependencyInjection;

namespace Hhs.Shared.Hosting.Gateways;

public static class GatewayHostRegistration
{
    public static IServiceCollection ConfigureGatewayHost(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.ConfigureSharedHost();

        var ocelotBuilder = services.AddOcelot(configuration)
            .AddAppConfiguration(); //mmlib configuration service address discover
        //.AddConsul(); //service discovery
        //.AddPolly();  //response time management
        //.AddCacheManager(settings => settings.WithDictionaryHandle()); //cache management

        if (!env.IsHhsProduction())
        {
            // ocelotBuilder.AddDelegatingHandler<BaseRemoveCsrfCookieHandler>(true);
        }

        return services;
    }
}