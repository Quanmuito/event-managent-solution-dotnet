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
}
