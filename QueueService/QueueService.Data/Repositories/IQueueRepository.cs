namespace QueueService.Data.Repositories;

using QueueService.Data.Models;
using DatabaseService.Repositories;

public interface IQueueRepository : IRepository<Queue>
{
    Task<Queue> GetByEventIdAsync(string eventId, CancellationToken cancellationToken);
}
