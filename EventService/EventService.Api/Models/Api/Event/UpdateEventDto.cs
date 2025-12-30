namespace EventService.Api.Models.Api.Event;

public class UpdateEventDto
{
    //TODO: Do you wanna use nullables here to allow partial updates?
    // Then you might get an issue of not understanding what exactly is being updated, e.g. if you have null value
    // on nullable property, so it will be updated to null
    public string? Title { get; set; }
    public string? HostedBy { get; set; }
    public bool? IsPublic { get; set; }
    public string? Details { get; set; }
    public DateTime? TimeStart { get; set; }
    public DateTime? TimeEnd { get; set; }
}