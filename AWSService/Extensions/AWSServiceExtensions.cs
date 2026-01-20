namespace AWSService.Extensions;

using AWSService.Services;
using AWSService.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class AWSServiceExtensions
{
    public static IServiceCollection AddAWSSES(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "AwsSes")
    {
        services.Configure<AWSSESSettings>(configuration.GetSection(configSection));
        services.AddOptions<AWSSESSettings>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IAWSSESClientFactory, AWSSESClientFactory>();
        services.AddSingleton(sp =>
            sp.GetRequiredService<IAWSSESClientFactory>().CreateClient());

        return services;
    }
}
