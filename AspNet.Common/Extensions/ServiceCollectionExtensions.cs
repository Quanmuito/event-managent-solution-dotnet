namespace AspNet.Common.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Asp.Versioning;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonApiServices(this IServiceCollection services, ApiVersion defaultVersion)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddHealthChecks();
        services.AddApiVersioningWithDefaults(defaultVersion);
        services.AddProblemDetails();

        return services;
    }

    public static IServiceCollection AddApiVersioningWithDefaults(this IServiceCollection services, ApiVersion defaultVersion)
    {
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = defaultVersion;
        });

        return services;
    }
}
