using Asp.Versioning;
using Ems.Common.Extensions.Startup;
using EventService.Api.Services;
using EventService.Data;
using EventService.Data.Settings;
using Serilog;
using ILogger = Serilog.ILogger;

const string environmentVariablesPrefix = "EventService_";
ApiVersion apiVersion = new(1, 0);
ILogger<Program>? logger = null;
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddEnvironmentVariables(prefix: environmentVariablesPrefix);
    ConfigureLogging(builder.Host);
    ConfigureServices(builder.Services, builder.Configuration);
    
    var app = builder.Build();
    _ = app.Services.GetRequiredService<ILogger<Program>>();
    ConfigureMiddleware(app);
    ConfigureEndpoints(app);

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
    
    services.AddControllers();
    services.AddOpenApi();
    
    services.AddEndpointsApiExplorer();
    services.AddSingleton<MongoDbContext>();
    
    // Add API versioning
    services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = apiVersion;
    });
}

void ConfigureMiddleware(WebApplication app)
{
    //TODO: Add at least global error handling middleware
}

void ConfigureEndpoints(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseAuthorization();
    app.MapGet("/", () => "Hello World!");
    app.MapControllers();
}