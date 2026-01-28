namespace EventService.Data.Models;

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

    [BsonElement("available")]
    public int Available { get; set; }

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
}
