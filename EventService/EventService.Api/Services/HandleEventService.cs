namespace EventService.Api.Services;
using System.Text.RegularExpressions;
using Data;
using Models.Api.Event;
using Models.Domain;
using MongoDB.Bson;
using MongoDB.Driver;

public class HandleEventService(MongoDbContext mongoDbService)
{
    private readonly IMongoCollection<Event> _eventCollection = mongoDbService.GetCollection<Event>("Events");

    //TODO: (for sometime later) think about how many events will you store, are they all going to be retrieved at once?
    // How about pagination? also probably sorting can be useful if your client wants to have it
    public async Task<List<EventDto>> Search(string? data, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(data))
            return await FindAll(cancellationToken);

        var keywords = data.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (keywords.Length == 0)
            return await FindAll(cancellationToken);

        var filters = keywords.Select(word =>
        {
            var escapedWord = Regex.Escape(word);
            return Builders<Event>.Filter.Or(
                Builders<Event>.Filter.Regex(e => e.Title, new BsonRegularExpression(escapedWord, "i")),
                Builders<Event>.Filter.Regex(e => e.Details, new BsonRegularExpression(escapedWord, "i"))
            );
        });

        var combinedFilter = Builders<Event>.Filter.Or(filters);
        var events = await _eventCollection.Find(combinedFilter).ToListAsync(cancellationToken);

        return [.. events.Select(e => new EventDto(e))];
    }

    public async Task<EventDto> GetById(string id, CancellationToken cancellationToken)
    {
        var eventEntity = await _eventCollection
            .Find(e => e.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return new EventDto(eventEntity);
    }

    public async Task<Event> Create(CreateEventDto createDto, CancellationToken cancellationToken)
    {
        var newEvent = Event.GetEntityFromDto(createDto);
        await _eventCollection.InsertOneAsync(newEvent, cancellationToken: cancellationToken);
        return newEvent;
    }

    public async Task<Event> Update(string id, UpdateEventDto updateDto, CancellationToken cancellationToken)
    {

        var updates = new List<UpdateDefinition<Event>>();

        if (updateDto.Title != null)
            updates.Add(Builders<Event>.Update.Set(e => e.Title, updateDto.Title));

        if (updateDto.HostedBy != null)
            updates.Add(Builders<Event>.Update.Set(e => e.HostedBy, updateDto.HostedBy));

        if (updateDto.IsPublic.HasValue)
            updates.Add(Builders<Event>.Update.Set(e => e.IsPublic, updateDto.IsPublic.Value));

        if (updateDto.Details != null)
            updates.Add(Builders<Event>.Update.Set(e => e.Details, updateDto.Details));

        if (updateDto.TimeStart.HasValue)
            updates.Add(Builders<Event>.Update.Set(e => e.TimeStart, updateDto.TimeStart.Value));

        if (updateDto.TimeEnd.HasValue)
            updates.Add(Builders<Event>.Update.Set(e => e.TimeEnd, updateDto.TimeEnd.Value));

        updates.Add(Builders<Event>.Update.Set(e => e.UpdatedAt, DateTime.UtcNow));

        if (updates.Count == 1)
            throw new ArgumentException("No valid fields to update.");

        var updateDef = Builders<Event>.Update.Combine(updates);

        var result = await _eventCollection.FindOneAndUpdateAsync(
            e => e.Id == id,
            updateDef,
            new FindOneAndUpdateOptions<Event> { ReturnDocument = ReturnDocument.After },
            cancellationToken
        );

        return result;
    }

    public async Task<DeleteResult> Delete(string id, CancellationToken cancellationToken)
    {
        return await _eventCollection.DeleteOneAsync(e => e.Id == id, cancellationToken);
    }

    private async Task<List<EventDto>> FindAll(CancellationToken cancellationToken)
    {
        var events = await _eventCollection.Find(_ => true).ToListAsync(cancellationToken);
        return [.. events.Select(e => new EventDto(e))];
    }
}
