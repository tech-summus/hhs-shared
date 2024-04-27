using System.Reflection;
using System.Text.Json.Serialization;
using Hhs.Shared.Hosting.Microservices.Filters;
using Hhs.Shared.Hosting.Microservices.Handlers;
using Hhs.Shared.Hosting.Microservices.Middlewares;
using Hhs.Shared.Hosting.Microservices.Models;
using HsnSoft.Base.Application.Dtos;
using HsnSoft.Base.AspNetCore.Security.Claims;
using HsnSoft.Base.AspNetCore.Tracing;
using HsnSoft.Base.EventBus;
using HsnSoft.Base.EventBus.Logging;
using HsnSoft.Base.EventBus.RabbitMQ;
using HsnSoft.Base.RabbitMQ;
using HsnSoft.Base.Security.Claims;
using HsnSoft.Base.Tracing;
using HsnSoft.Base.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Hhs.Shared.Hosting.Microservices;

public static class MicroserviceHostExtensions
{
    public static IServiceCollection ConfigureMicroserviceHost(this IServiceCollection services)
    {
        Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

        // Set paging limit value
        PagedLimitedResultRequestDto.MaxMaxResultCount = 1000000;

        // Set filter limit value
        SearchLimitedResultRequestDto.MaxMaxResultCount = 20;

        services.ConfigureSharedAspNetCoreHost();

        return services;
    }

    public static IServiceCollection AddMicroserviceMvc(this IServiceCollection services, IConfiguration configuration, Type type)
    {
        // Add our Config object so it can be injected
        services.Configure<MicroserviceSettings>(configuration.GetSection("MicroserviceSettings"));

        services.AddControllers(options =>
            {
                options.Filters.Add(typeof(RequestResponseActionFilterAttribute));
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            })
            // Added for functional tests
            .AddApplicationPart(type.Assembly)
            .AddJsonOptions(options =>
            {
                var microserviceSettings = new MicroserviceSettings();
                configuration.Bind("MicroserviceSettings", microserviceSettings);

                options.JsonSerializerOptions.WriteIndented = true;
                if (microserviceSettings.IgnoreNullValueForJsonResponse)
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                }
            });

        services.AddSingleton<IRequestResponseLogger, RequestResponseLogger>();
        services.AddScoped<IRequestResponseLogModelCreator, RequestResponseLogModelCreator>();
        services.AddScoped<RequestResponseLoggerMiddleware>();

        services.AddSingleton<IResponseExceptionHandler, ResponseExceptionHandler>();
        services.AddScoped<GlobalExceptionHandlerMiddleware>();

        return services;
    }

    public static IServiceCollection AddMicroserviceHealthChecks(this IServiceCollection services, IConfiguration configuration, string connectionStringKey)
    {
        var hcBuilder = services.AddHealthChecks();

        var healtCheckPrefix = connectionStringKey ?? string.Empty;
        healtCheckPrefix = healtCheckPrefix.ToLower().Replace("service", "");

        hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

        hcBuilder.AddNpgSql
        (
            configuration.GetConnectionString(connectionStringKey) ?? throw new InvalidOperationException(),
            healthQuery: "SELECT 1;",
            name: $"{healtCheckPrefix}-db-check",
            tags: new[] { "db" }
        );

        var rabbitMq = new RabbitMqConnectionSettings();
        configuration.Bind("RabbitMQ:Connection", rabbitMq);
        hcBuilder.AddRabbitMQ
        (
            //amqp://user:pass@host:10000/vhost	"user"	"pass"	"host"	10000	"vhost"
            $"amqp://{rabbitMq.UserName}:{rabbitMq.Password}@{rabbitMq.HostName}:{rabbitMq.Port}",
            name: $"{healtCheckPrefix}-rabbitmq-check",
            tags: new[] { "rabbitmq" }
        );

        return services;
    }

    public static IServiceCollection AddMicroserviceEventBus(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        services.AddRabbitMqEventBus(configuration);

        // Add All Event Handlers
        services.AddEventHandlers(assembly);

        return services;
    }

    private static void AddRabbitMqEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        // Add configuration objects
        services.Configure<RabbitMqConnectionSettings>(configuration.GetSection("RabbitMq:Connection"));
        services.Configure<RabbitMqEventBusConfig>(configuration.GetSection("RabbitMq:EventBus"));

        // Add event bus instances
        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentPrincipalAccessor, HttpContextCurrentPrincipalAccessor>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddSingleton<ITraceAccesor, HttpContextTraceAccessor>();
        services.AddSingleton<IEventBusLogger, DefaultEventBusLogger>();
        services.AddSingleton<IRabbitMqPersistentConnection, RabbitMqPersistentConnection>();
        services.AddSingleton<IEventBus, EventBusRabbitMq>();
    }

    private static void AddEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        var refType = typeof(IIntegrationEventHandler);
        var types = assembly.GetTypes()
            .Where(p => refType.IsAssignableFrom(p) && p is { IsInterface: false, IsAbstract: false });

        foreach (var type in types.ToList())
        {
            services.AddTransient(type);
        }
    }

    public static void UseEventBus(this IApplicationBuilder app, Assembly assembly)
    {
        var refType = typeof(IIntegrationEventHandler);
        var eventHandlerTypes = assembly.GetTypes()
            .Where(p => refType.IsAssignableFrom(p) && p is { IsInterface: false, IsAbstract: false }).ToList();

        if (eventHandlerTypes is not { Count: > 0 }) return;

        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        foreach (var eventHandlerType in eventHandlerTypes)
        {
            var eventType = eventHandlerType.GetInterfaces().First(x => x.IsGenericType).GenericTypeArguments[0];

            eventBus.Subscribe(eventType, eventHandlerType);
        }
    }
}