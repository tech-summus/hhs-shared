using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Hhs.Shared.Hosting;

public static class SwaggerConfigurationHelper
{
    public static void Configure(IServiceCollection services, string apiTitle, string apiVersion = "v1", string apiName = "v1")
    {
        services.AddBaseSwaggerGen(options =>
        {
            options.SwaggerDoc(apiName, new OpenApiInfo { Title = apiTitle, Version = apiVersion });
            options.DocInclusionPredicate((docName, description) => true);
            options.CustomSchemaIds(type => type.FullName);
        });
    }

    public static void ConfigureWithBearer(IServiceCollection services, string tokenDescription,
        string apiTitle, string apiVersion = "v1", string apiName = "v1")
    {
        services.AddBaseSwaggerGenWithBearer(
            tokenDescription: tokenDescription,
            options =>
            {
                options.SwaggerDoc(apiName, new OpenApiInfo { Title = apiTitle, Version = apiVersion });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
    }
}