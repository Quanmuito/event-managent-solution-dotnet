namespace EventService.Api.Models;

using EventService.Data.Models;

public class EventDto(Event @event)
{
    public string Id { get; set; } = @event.Id!;
    public string Title { get; set; } = @event.Title;
    public string HostedBy { get; set; } = @event.HostedBy;
    public bool IsPublic { get; set; } = @event.IsPublic;
    public string? Details { get; set; } = @event.Details;
    public int Available { get; set; } = @event.Available;
    public DateTime TimeStart { get; set; } = @event.TimeStart;
    public DateTime TimeEnd { get; set; } = @event.TimeEnd;
    public DateTime CreatedAt { get; set; } = @event.CreatedAt;
    public DateTime? UpdatedAt { get; set; } = @event.UpdatedAt;
}
