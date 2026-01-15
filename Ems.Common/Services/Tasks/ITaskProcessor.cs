namespace Ems.Common.Services.Tasks;

public interface ITaskProcessor<in TMessage>
{
    Task ProcessAsync(TMessage message, CancellationToken cancellationToken);
}
