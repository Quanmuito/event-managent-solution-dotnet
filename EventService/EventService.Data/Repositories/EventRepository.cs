namespace EventService.Data.Repositories;

using System.Text.RegularExpressions;
using EventService.Data.Models;
using EventService.Data;
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
}
