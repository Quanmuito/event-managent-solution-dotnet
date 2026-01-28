namespace EventService.Data.Repositories;

using System.Text.RegularExpressions;
using EventService.Data.Models;
using DatabaseService;
using DatabaseService.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

public class EventRepository(MongoDbContext mongoDbContext) : Repository<Event>(mongoDbContext, "Events"), IEventRepository
{
    public async Task<List<Event>> SearchAsync(string? query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync(cancellationToken);

        var keywords = query.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (keywords.Length == 0)
            return await GetAllAsync(cancellationToken);

        var filters = keywords.Select(word =>
        {
            var escapedWord = Regex.Escape(word);
            return Builders<Event>.Filter.Or(
                Builders<Event>.Filter.Regex(e => e.Title, new BsonRegularExpression(escapedWord, "i")),
                Builders<Event>.Filter.Regex(e => e.Details, new BsonRegularExpression(escapedWord, "i"))
            );
        });

        var combinedFilter = Builders<Event>.Filter.Or(filters);
        return await Collection.Find(combinedFilter).ToListAsync(cancellationToken);
    }

    public async Task<Event> OnBookingRegisteredAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<Event>.Filter.And(
            Builders<Event>.Filter.Eq(e => e.Id, id),
            Builders<Event>.Filter.Gt(e => e.Available, 0)
        );

        var update = Builders<Event>.Update
            .Inc(e => e.Available, -1)
            .Set(e => e.UpdatedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<Event>
        {
            ReturnDocument = ReturnDocument.After
        };

        var result = await Collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);

        return result ?? throw new InvalidOperationException($"Cannot decrement availability for event '{id}': No available seats or event not found.");
    }

    public async Task<Event> OnBookingCancelledAsync(string id, CancellationToken cancellationToken)
    {
        var _event = await GetByIdAsync(id, cancellationToken);
        var updateDefinition = Builders<Event>.Update
            .Inc(q => q.Available, 1)
            .Set(q => q.UpdatedAt, DateTime.UtcNow);

        return await UpdateAsync(id, updateDefinition, cancellationToken);
    }
}
