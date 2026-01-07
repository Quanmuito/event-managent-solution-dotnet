namespace EventService.Tests.Helpers;
using EventService.Api.Models.Api.Event;
using EventService.Api.Models.Domain;

public static class TestDataBuilder
{
    public static CreateEventDto CreateValidCreateEventDto()
    {
        return new CreateEventDto
        {
            Title = "Test Event",
            HostedBy = "Test Host",
            IsPublic = true,
            Details = "Test event details",
            TimeStart = DateTime.UtcNow.AddDays(1),
            TimeEnd = DateTime.UtcNow.AddDays(2)
        };
    }

    public static CreateEventDto CreateInvalidCreateEventDto()
    {
        return new CreateEventDto
        {
            Title = "Test Event",
            HostedBy = "Test Host",
            IsPublic = true,
            Details = "Test event details",
            TimeStart = DateTime.UtcNow.AddDays(2),
            TimeEnd = DateTime.UtcNow.AddDays(1)
        };
    }

    public static UpdateEventDto CreateValidUpdateEventDto()
    {
        return new UpdateEventDto
        {
            Title = "Updated Event Title",
            HostedBy = "Updated Host",
            IsPublic = false,
            Details = "Updated details",
            TimeStart = DateTime.UtcNow.AddDays(3),
            TimeEnd = DateTime.UtcNow.AddDays(4)
        };
    }

    public static UpdateEventDto CreateInvalidUpdateEventDto()
    {
        return new UpdateEventDto
        {
            Title = "Updated Event Title",
            TimeStart = DateTime.UtcNow.AddDays(4),
            TimeEnd = DateTime.UtcNow.AddDays(3)
        };
    }

    public static Event CreateEvent(string? id = null)
    {
        return new Event
        {
            Id = id ?? "507f1f77bcf86cd799439011",
            Title = "Test Event",
            HostedBy = "Test Host",
            IsPublic = true,
            Details = "Test event details",
            TimeStart = DateTime.UtcNow.AddDays(1),
            TimeEnd = DateTime.UtcNow.AddDays(2),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    public static List<Event> CreateEventList(int count = 3)
    {
        var events = new List<Event>();
        for (int i = 0; i < count; i++)
        {
            events.Add(CreateEvent($"507f1f77bcf86cd79943901{i}"));
        }
        return events;
    }
}
