using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics;
using Ems.Common.Extensions.Startup;
using BookingService.Api.Messages;
using DatabaseService;
using DatabaseService.Settings;
using BookingService.Api.Services;
using BookingService.Data.Repositories;
using EventService.Data.Repositories;

const string environmentVariablesPrefix = "BookingService_";
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
    services.AddScoped<IBookingRepository, BookingRepository>();
    services.AddScoped<IQrCodeRepository, QrCodeRepository>();
    services.AddScoped<IEventRepository, EventRepository>();
    services.AddScoped<HandleBookingService>();

    services.AddTaskService<QrCodeTaskMessage, QrCodeTaskProcessor>();
    services.AddTaskService<NotificationTaskMessage, NotificationTaskProcessor>();

    services.AddHealthChecks();

    services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = apiVersion;
    });
}

void ConfigureMiddleware(WebApplication app)
{
    app.UseExceptionHandler(appError =>
    {
        appError.Run(async context =>
        {
            var statusCode = StatusCodes.Status500InternalServerError;
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (contextFeature != null)
            {
                var errorMessage = !string.IsNullOrWhiteSpace(contextFeature.Error?.Message)
                    ? contextFeature.Error.Message
                    : "Oops! Something went wrong...";

                await context.Response.WriteAsJsonAsync(new
                {
                    StatusCode = statusCode,
                    Message = errorMessage
                });
            }
        });
    });
}

void ConfigureEndpoints(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseAuthorization();
    app.MapGet("/", () => "Hello World!");
    app.MapHealthChecks("/health");
    app.MapControllers();
}
