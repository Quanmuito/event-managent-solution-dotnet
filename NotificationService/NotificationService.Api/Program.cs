using AspNet.Common.Extensions;
using Ems.Common.Extensions.Startup;
using Ems.Common.Http.ExceptionHandler;
using Asp.Versioning;

const string environmentVariablesPrefix = "NotificationService_";
ApiVersion apiVersion = new(1, 0);
ILogger<Program>? logger = null;
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddEnvironmentVariables(prefix: environmentVariablesPrefix);
    ConfigureLogging(builder.Host);
    ConfigureServices(builder.Services, builder.Configuration);

    var app = builder.Build();
    app.UseExceptionHandler();
    app.MapCommonApiEndpoints();
    logger = app.Services.GetRequiredService<ILogger<Program>>();

    await app.RunAsync();
}
catch (Exception ex)
{
    logger?.LogError(ex, "An error occured during initialization");
}
finally
{
    logger?.LogInformation("Shutting down");
}

void ConfigureLogging(IHostBuilder builder)
{
    builder.ConfigureCustomLogging();
}

void ConfigureServices(IServiceCollection services, ConfigurationManager configuration)
{
    services.AddCommonApiServices(apiVersion);
    services.AddExceptionHandler<GlobalExceptionHandler>();
}
