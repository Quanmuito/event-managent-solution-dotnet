using AuthService.Api.Services;
using AuthService.Api.Settings;
using AuthService.Data.Repositories;
using DatabaseService;
using DatabaseService.Settings;
using AspNet.Common.Extensions;
using Ems.Common.Extensions.Startup;
using Ems.Common.Http.ExceptionHandler;
using Asp.Versioning;

const string environmentVariablesPrefix = "AuthService_";
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
    services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));
    services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

    services.AddCommonApiServices(apiVersion);
    services.AddExceptionHandler<GlobalExceptionHandler>();

    services.AddSingleton<MongoDbContext>();
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IAuthRepository, AuthRepository>();
    services.AddScoped<IJwtTokenService, JwtTokenService>();
    services.AddScoped<HandleUserService>();
    services.AddScoped<HandleAuthService>();
}
