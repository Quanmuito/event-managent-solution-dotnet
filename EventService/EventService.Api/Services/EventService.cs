namespace EventService.Api.Services;

using Data;
using Models.Api.Event;
using Models.Domain;
using MongoDB.Bson;
using MongoDB.Driver;

public class EventService(MongoDbContext mongoDbService)
{
    private readonly IMongoCollection<Event> _eventCollection = mongoDbService.GetCollection<Event>("Events");

    //TODO: (for sometime later) think about how many events will you store, are they all going to be retrieved at once?
    // How about pagination? also probably sorting can be useful if your client wants to have it
    public async Task<List<EventDto>> Search(string? data)
    {
        //TODO: For any user-input you get, you want to sanitize it, in this case to prevent Regex injection
        // see Regex.Escape(data);
        
        if (string.IsNullOrWhiteSpace(data))
            return await FindAll();

        var keywords = data.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (keywords.Length == 0)
            return await FindAll();

        var filters = keywords.Select(word =>
            Builders<Event>.Filter.Or(
                Builders<Event>.Filter.Regex(e => e.Title, new BsonRegularExpression(word, "i")),
                Builders<Event>.Filter.Regex(e => e.Details, new BsonRegularExpression(word, "i"))
            )
        );

        var combinedFilter = Builders<Event>.Filter.Or(filters);
        var events = await _eventCollection.Find(combinedFilter).ToListAsync();

        return events.Select(e => new EventDto(e)).ToList();
    }

    public async Task<EventDto> GetById(string id)
    {
        var eventEntity = await _eventCollection
            .Find(e => e.Id == id)
            .FirstOrDefaultAsync();

        return new EventDto(eventEntity);
    }

    public async Task<Event> Create(CreateEventDto createDto)
    {
        var newEvent = Event.GetEntityFromDto(createDto);
        await _eventCollection.InsertOneAsync(newEvent);
        return newEvent;
    }

    public async Task<Event> Update(string id, UpdateEventDto updateDto)
    {
        //TODO: not only here, but also in other places, you might want to validate properties,
        // e.g. title cannot be too short or too long, and other properties as well
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

        //TODO: if you throw such exceptions you probably might have a logic to handle it correctly, otherwise you will
        // get 500 from a controller, but should be a bad request (after a validation)
        if (updates.Count == 0)
            throw new ArgumentException("No valid fields to update.");

        var updateDef = Builders<Event>.Update.Combine(updates);

        var result = await _eventCollection.FindOneAndUpdateAsync(
            e => e.Id == id,
            updateDef,
            new FindOneAndUpdateOptions<Event> { ReturnDocument = ReturnDocument.After }
        );

        return result;
    }

    public async Task<DeleteResult> Delete(string id)
    {
        return await _eventCollection.DeleteOneAsync(e => e.Id == id);
    }

    private async Task<List<EventDto>> FindAll()
    {
        var events = await _eventCollection.Find(_ => true).ToListAsync();
        return events.Select(e => new EventDto(e)).ToList();
    }
}
