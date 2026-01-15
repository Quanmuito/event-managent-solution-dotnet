namespace Ems.Common.Services.Tasks;

using System.Threading.Channels;

public class TaskQueue<TMessage>(ChannelWriter<TMessage> writer) : ITaskQueue<TMessage>
{

    public async ValueTask EnqueueAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        await writer.WriteAsync(message, cancellationToken);
    }
}
