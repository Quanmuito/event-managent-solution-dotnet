namespace AspNet.Common.Tests.Helpers;

using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public static class AspNetCommonTestHelper
{
    public static ServiceCollection CreateServiceCollectionWithApiVersion(ApiVersion version)
    {
        return new ServiceCollection();
    }

    public static ServiceProvider BuildServiceProvider(IServiceCollection services)
    {
        return services.BuildServiceProvider();
    }

    public static T? GetService<T>(IServiceProvider provider) where T : class
    {
        return provider.GetService<T>();
    }

    public static WebApplicationBuilder CreateWebApplicationBuilder(string environmentName)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = environmentName;
        return builder;
    }

    public static WebApplication BuildWebApplication(WebApplicationBuilder builder)
    {
        return builder.Build();
    }
}
