namespace QueueService.Data.Repositories;

using QueueService.Data.Models;
using DatabaseService;
using DatabaseService.Repositories;
using MongoDB.Driver;

public class QueueRepository(MongoDbContext mongoDbContext) : Repository<Queue>(mongoDbContext, "Queues"), IQueueRepository
{
    public async Task<Queue> GetByEventIdAsync(string eventId, CancellationToken cancellationToken)
    {
        var filter = Builders<Queue>.Filter.Eq(q => q.EventId, eventId);
        var result = await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Queue with EventId '{eventId}' was not found.");

        return result;
    }

    public async Task<bool> BookingRegisteredAsync(string eventId, CancellationToken cancellationToken)
    {
        var queue = await GetByEventIdAsync(eventId, cancellationToken);
        var updateDefinition = Builders<Queue>.Update
            .Inc(q => q.Available, -1)
            .Set(q => q.UpdatedAt, DateTime.UtcNow);

        await UpdateAsync(queue.Id!, updateDefinition, cancellationToken);
        return true;
    }

    public async Task<bool> EnqueueAsync(string eventId, CancellationToken cancellationToken)
    {
        var queue = await GetByEventIdAsync(eventId, cancellationToken);
        var updateDefinition = Builders<Queue>.Update
            .Inc(q => q.Length, 1)
            .Set(q => q.UpdatedAt, DateTime.UtcNow);

        await UpdateAsync(queue.Id!, updateDefinition, cancellationToken);
        return true;
    }

    public async Task<bool> DequeueAsync(string eventId, CancellationToken cancellationToken)
    {
        var queue = await GetByEventIdAsync(eventId, cancellationToken);
        if (queue.Length == 0) return false;

        var updateDefinition = Builders<Queue>.Update
            .Inc(q => q.Length, -1)
            .Set(q => q.UpdatedAt, DateTime.UtcNow);

        await UpdateAsync(queue.Id!, updateDefinition, cancellationToken);
        return true;
    }

    public async Task<bool> NextAsync(string eventId, CancellationToken cancellationToken)
    {
        var queue = await GetByEventIdAsync(eventId, cancellationToken);
        if (queue.Position == queue.Length - 1) return false;

        var updateDefinition = Builders<Queue>.Update
            .Inc(q => q.Position, 1)
            .Set(q => q.UpdatedAt, DateTime.UtcNow);

        await UpdateAsync(queue.Id!, updateDefinition, cancellationToken);
        return true;
    }
}
