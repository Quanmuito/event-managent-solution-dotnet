using MongoDB.Driver;
using MongoDB.Bson;
using EventManagementSolution.Api.Event.Dtos;
using EventManagementSolution.Api.Services;

namespace EventManagementSolution.Api.Event.V1;

public class EventService
{
    private readonly IMongoCollection<EventEntity> _eventCollection;

    public EventService(MongoDbService mongoDbService)
    {
        _eventCollection = mongoDbService.GetCollection<EventEntity>("Events");
    }

    public async Task<List<EventDto>> Search(string? data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return await FindAll();

        var keywords = data.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (keywords.Length == 0)
            return await FindAll();

        var filters = keywords.Select(word =>
            Builders<EventEntity>.Filter.Or(
                Builders<EventEntity>.Filter.Regex(e => e.Title, new BsonRegularExpression(word, "i")),
                Builders<EventEntity>.Filter.Regex(e => e.Details, new BsonRegularExpression(word, "i"))
            )
        );

        var combinedFilter = Builders<EventEntity>.Filter.Or(filters);
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

    public async Task<EventEntity> Create(CreateEventDto createDto)
    {
        var newEvent = EventEntity.GetEntityFromDto(createDto);
        await _eventCollection.InsertOneAsync(newEvent);
        return newEvent;
    }

    public async Task<EventEntity> Update(string id, UpdateEventDto updateDto)
    {
        var updates = new List<UpdateDefinition<EventEntity>>();

        if (updateDto.Title != null)
            updates.Add(Builders<EventEntity>.Update.Set(e => e.Title, updateDto.Title));

        if (updateDto.HostedBy != null)
            updates.Add(Builders<EventEntity>.Update.Set(e => e.HostedBy, updateDto.HostedBy));

        if (updateDto.IsPublic.HasValue)
            updates.Add(Builders<EventEntity>.Update.Set(e => e.IsPublic, updateDto.IsPublic.Value));

        if (updateDto.Details != null)
            updates.Add(Builders<EventEntity>.Update.Set(e => e.Details, updateDto.Details));

        if (updateDto.TimeStart.HasValue)
            updates.Add(Builders<EventEntity>.Update.Set(e => e.TimeStart, updateDto.TimeStart.Value));

        if (updateDto.TimeEnd.HasValue)
            updates.Add(Builders<EventEntity>.Update.Set(e => e.TimeEnd, updateDto.TimeEnd.Value));
        
        updates.Add(Builders<EventEntity>.Update.Set(e => e.UpdatedAt, DateTime.UtcNow));

        if (updates.Count == 0)
            throw new ArgumentException("No valid fields to update.");

        var updateDef = Builders<EventEntity>.Update.Combine(updates);

        var result = await _eventCollection.FindOneAndUpdateAsync(
            e => e.Id == id,
            updateDef,
            new FindOneAndUpdateOptions<EventEntity> { ReturnDocument = ReturnDocument.After }
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
