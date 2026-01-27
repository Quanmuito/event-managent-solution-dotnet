namespace AspNet.Common.Tests.Helpers;

using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

public static class AspNetCommonTestHelper
{
    public static ServiceCollection CreateServiceCollectionWithApiVersion(ApiVersion version)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());
        return services;
    }

    private class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "TestApplication";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(Directory.GetCurrentDirectory());
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
