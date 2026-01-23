using BookingService.Api.Messages;
using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Data.Repositories;
using DatabaseService;
using DatabaseService.Settings;
using EventService.Data.Repositories;
using NotificationService.Common.Messages;
using NotificationService.Common.Services;
using AspNet.Common.Extensions;
using AWSService.Extensions;
using Ems.Common.Extensions.Startup;
using Ems.Common.Http.ExceptionHandler;
using Asp.Versioning;

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

    services.AddCommonApiServices(apiVersion);
    services.AddExceptionHandler<GlobalExceptionHandler>();

    services.AddSingleton<MongoDbContext>();
    services.AddScoped<IBookingRepository, BookingRepository>();
    services.AddScoped<IQrCodeRepository, QrCodeRepository>();
    services.AddScoped<IEventRepository, EventRepository>();
    services.AddScoped<HandleBookingService>();

    services.AddAWSSES(configuration);

    services.AddSingleton<IEmailService, EmailService>();
    services.AddSingleton<IPhoneService, PhoneService>();

    services.AddTaskService<QrCodeTaskMessage, QrCodeTaskProcessor>();
    services.AddTaskService<EmailNotificationTaskMessage<BookingDto>, BookingEmailNotificationTaskProcessor>();
    services.AddTaskService<PhoneNotificationTaskMessage<BookingDto>, BookingPhoneNotificationTaskProcessor>();
}
