namespace EventManagementSolution.Api.Event.Dtos;

public class EventDto(EventEntity eventEntity)
{
    public string Id { get; set; } = eventEntity.Id!;
    public string Title { get; set; } = eventEntity.Title;
    public string HostedBy { get; set; } = eventEntity.HostedBy;
    public bool IsPublic { get; set; } = eventEntity.IsPublic;
    public string? Details { get; set; } = eventEntity.Details;
    public DateTime TimeStart { get; set; } = eventEntity.TimeStart;
    public DateTime TimeEnd { get; set; } = eventEntity.TimeEnd;
    public DateTime CreatedAt { get; set; } = eventEntity.CreatedAt;
    public DateTime? UpdatedAt { get; set; } = eventEntity.UpdatedAt;
}