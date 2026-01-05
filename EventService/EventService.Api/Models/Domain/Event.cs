namespace EventService.Api.Models.Domain;
using EventService.Api.Models.Api.Event;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Event
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = null!;

    [BsonElement("hostedBy")]
    public string HostedBy { get; set; } = null!;

    [BsonElement("isPublic")]
    public bool IsPublic { get; set; } = false;

    [BsonElement("details")]
    [BsonIgnoreIfNull]
    public string? Details { get; set; }

    [BsonElement("timeStart")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime TimeStart { get; set; }

    [BsonElement("timeEnd")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime TimeEnd { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonIgnoreIfNull]
    public DateTime? UpdatedAt { get; set; } = null;

    public static Event GetEntityFromDto(CreateEventDto dto)
    {
        return new Event
        {
            Title = dto.Title,
            HostedBy = dto.HostedBy,
            IsPublic = dto.IsPublic,
            Details = dto.Details,
            TimeStart = dto.TimeStart,
            TimeEnd = dto.TimeEnd,
            CreatedAt = DateTime.UtcNow
        };
    }
}