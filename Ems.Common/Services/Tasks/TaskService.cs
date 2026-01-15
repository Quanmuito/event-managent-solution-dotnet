namespace Ems.Common.Services.Tasks;

using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TaskService<TMessage>(
    ChannelReader<TMessage> reader,
    IServiceProvider serviceProvider,
    ILogger<TaskService<TMessage>> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<ITaskProcessor<TMessage>>();
                await processor.ProcessAsync(message, stoppingToken);
                logger.LogInformation("Successfully processed background task of type {MessageType}", typeof(TMessage).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing background task of type {MessageType}", typeof(TMessage).Name);
            }
        }
    }
}
