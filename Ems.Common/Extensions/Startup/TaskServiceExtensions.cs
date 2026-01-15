namespace Ems.Common.Extensions.Startup;

using System.Threading.Channels;
using Ems.Common.Services.Tasks;
using Microsoft.Extensions.DependencyInjection;

public static class TaskServiceExtensions
{
    public static IServiceCollection AddTaskService<TMessage, TProcessor>(this IServiceCollection services, int capacity = 1000)
        where TProcessor : class, ITaskProcessor<TMessage>
    {
        var channel = Channel.CreateBounded<TMessage>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
        services.AddSingleton(channel.Reader);
        services.AddSingleton(channel.Writer);

        services.AddSingleton<ITaskQueue<TMessage>, TaskQueue<TMessage>>();
        services.AddScoped<ITaskProcessor<TMessage>, TProcessor>();
        services.AddHostedService<TaskService<TMessage>>();

        return services;
    }
}
