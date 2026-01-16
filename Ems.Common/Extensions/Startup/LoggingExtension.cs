namespace Ems.Common.Extensions.Startup;

using Microsoft.Extensions.Hosting;
using Serilog;

public static class LoggingExtension
{
    public static IHostBuilder ConfigureCustomLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, _, configuration) =>
        {
            configuration
                .WriteTo.Console();

            var isDev = context.HostingEnvironment.IsDevelopment();
            if (isDev)
            {
                configuration.MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Information);
            }
            else
            {
                configuration.MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning);
            }
        });
    }
}