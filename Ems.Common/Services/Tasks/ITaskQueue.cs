namespace Ems.Common.Services.Tasks;

public interface ITaskQueue<in TMessage>
{
    ValueTask EnqueueAsync(TMessage message, CancellationToken cancellationToken = default);
}
