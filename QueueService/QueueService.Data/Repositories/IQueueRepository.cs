namespace QueueService.Data.Repositories;

using QueueService.Data.Models;
using DatabaseService.Repositories;

public interface IQueueRepository : IRepository<Queue>
{
    Task<Queue> GetByEventIdAsync(string eventId, CancellationToken cancellationToken);
    Task<bool> BookingRegisteredAsync(string eventId, CancellationToken cancellationToken);
    Task<bool> EnqueueAsync(string eventId, CancellationToken cancellationToken);
    Task<bool> DequeueAsync(string eventId, CancellationToken cancellationToken);
    Task<bool> NextAsync(string eventId, CancellationToken cancellationToken);
}
