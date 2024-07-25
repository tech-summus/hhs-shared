using System.Reflection;
using System.Text.Json.Serialization;
using Hhs.Shared.Hosting.Microservices.Filters;
using Hhs.Shared.Hosting.Microservices.Handlers;
using Hhs.Shared.Hosting.Microservices.Middlewares;
using Hhs.Shared.Hosting.Microservices.Workers;
using HsnSoft.Base.Application.Dtos;
using HsnSoft.Base.AspNetCore.Hosting.Loader;
using HsnSoft.Base.AspNetCore.Security.Claims;
using HsnSoft.Base.AspNetCore.Serilog.Persistent;
using HsnSoft.Base.AspNetCore.Tracing;
using HsnSoft.Base.Data;
using HsnSoft.Base.EventBus;
using HsnSoft.Base.EventBus.Logging;
using HsnSoft.Base.EventBus.RabbitMQ;
using HsnSoft.Base.EventBus.RabbitMQ.Configs;
using HsnSoft.Base.EventBus.RabbitMQ.Connection;
using HsnSoft.Base.EventBus.SubManagers;
using HsnSoft.Base.Security.Claims;
using HsnSoft.Base.Tracing;
using HsnSoft.Base.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace Hhs.Shared.Hosting.Microservices;

public static class MicroserviceHostExtensions
{
    public static IServiceCollection ConfigureMicroserviceHost(this IServiceCollection services, IConfiguration configuration)
    {
        Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

        // Set paging limit value
        PagedLimitedResultRequestDto.MaxMaxResultCount = 1000000;

        // Set filter limit value
        SearchLimitedResultRequestDto.MaxMaxResultCount = 20;

        services.ConfigureSharedAspNetCoreHost(configuration);

        // Loader functionality
        services.AddTransient<IBasicLoader, DefaultBasicLoader>();
        services.AddHostedService<LoaderHostedService>();

        // Seeder functionality
        services.AddTransient<IBasicDataSeeder, DefaultBasicDataSeeder>();
        services.AddHostedService<DataSeederHostedService>();

        return services;
    }

    public static IServiceCollection AddAdvancedController(this IServiceCollection services, IConfiguration configuration, Type type)
    {
        services.Configure<MicroserviceHostingSettings>(configuration.GetSection("HostingSettings"));

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
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                var hostingSettings = new MicroserviceHostingSettings();
                configuration.Bind("HostingSettings", hostingSettings);

                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                if (hostingSettings.IgnoreNullValueForJsonResponse)
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                }
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

        services.AddSingleton<IResponseExceptionHandler, ResponseExceptionHandler>();
        services.AddScoped<GlobalExceptionHandlerMiddleware>();

        return services;
    }

    public static IServiceCollection AddMicroserviceHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        // var healtCheckPrefix = connectionStringKey ?? string.Empty;
        // healtCheckPrefix = healtCheckPrefix.ToLower().Replace("service", "");

        hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

        // hcBuilder.AddNpgSql
        // (
        //     configuration.GetConnectionString(connectionStringKey) ?? throw new InvalidOperationException(),
        //     healthQuery: "SELECT 1;",
        //     name: $"{healtCheckPrefix}-db-check",
        //     tags: new[] { "db" }
        // );

        // var rabbitMq = new RabbitMqConnectionSettings();
        // configuration.Bind("RabbitMQ:Connection", rabbitMq);
        // hcBuilder.AddRabbitMQ
        // (
        //     //amqp://user:pass@host:10000/vhost	"user"	"pass"	"host"	10000	"vhost"
        //     $"amqp://{rabbitMq.UserName}:{rabbitMq.Password}@{rabbitMq.HostName}:{rabbitMq.Port}",
        //     name: $"{healtCheckPrefix}-rabbitmq-check",
        //     tags: new[] { "rabbitmq" }
        // );

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
        services.AddSingleton<IEventBusLogger, EventBusLogger>();
        services.AddSingleton<IRabbitMqPersistentConnection, RabbitMqPersistentConnection>();
        services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionManager>();
        services.AddSingleton<IEventBus, EventBusRabbitMq>(sp => new EventBusRabbitMq(sp));
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