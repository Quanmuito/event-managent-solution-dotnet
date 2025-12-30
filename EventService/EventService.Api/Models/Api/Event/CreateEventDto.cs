namespace EventService.Api.Models.Api.Event;

public class CreateEventDto
{
    public required string Title { get; set; }
    public required string HostedBy { get; set; }
    
    //TODO: Why this is nullable, what is the purpose to have null value possible here?
    public bool? IsPublic { get; set; }
    public string? Details { get; set; }
    public DateTime TimeStart { get; set; }
    public DateTime TimeEnd { get; set; }
}