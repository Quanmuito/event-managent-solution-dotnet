namespace EventService.Api.Services;

using EventService.Api.Models;
using EventService.Data.Models;
using EventService.Data.Repositories;
using MongoDB.Driver;

public class HandleEventService(IEventRepository eventRepository)
{
    public async Task<List<EventDto>> Search(string? data, CancellationToken cancellationToken)
    {
        var events = await eventRepository.SearchAsync(data, cancellationToken);
        return [.. events.Select(e => new EventDto(e))];
    }

    public async Task<EventDto> GetById(string id, CancellationToken cancellationToken)
    {
        var eventEntity = await eventRepository.GetByIdAsync(id, cancellationToken);
        return new EventDto(eventEntity);
    }

    public async Task<Event> Create(CreateEventDto createDto, CancellationToken cancellationToken)
    {
        var newEvent = new Event
        {
            Title = createDto.Title,
            HostedBy = createDto.HostedBy,
            IsPublic = createDto.IsPublic,
            Details = createDto.Details,
            Available = createDto.Available,
            TimeStart = createDto.TimeStart,
            TimeEnd = createDto.TimeEnd,
            CreatedAt = DateTime.UtcNow
        };
        return await eventRepository.CreateAsync(newEvent, cancellationToken);
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

        if (updateDto.Available.HasValue)
            updates.Add(Builders<Event>.Update.Set(e => e.Available, updateDto.Available.Value));

        if (updateDto.TimeStart.HasValue)
            updates.Add(Builders<Event>.Update.Set(e => e.TimeStart, updateDto.TimeStart.Value));

        if (updateDto.TimeEnd.HasValue)
            updates.Add(Builders<Event>.Update.Set(e => e.TimeEnd, updateDto.TimeEnd.Value));

        if (updates.Count == 0)
            throw new ArgumentException("No valid fields to update.");

        updates.Add(Builders<Event>.Update.Set(e => e.UpdatedAt, DateTime.UtcNow));

        var updateDef = Builders<Event>.Update.Combine(updates);

        var result = await eventRepository.UpdateAsync(id, updateDef, cancellationToken);
        return result;
    }

    public async Task<bool> Delete(string id, CancellationToken cancellationToken)
    {
        return await eventRepository.DeleteAsync(id, cancellationToken);
    }
}
