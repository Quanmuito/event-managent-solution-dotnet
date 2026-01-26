namespace QueueService.Data.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Queue
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("eventId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string EventId { get; set; }

    [BsonElement("available")]
    public int Available { get; set; }

    [BsonElement("top")]
    public int Top { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonIgnoreIfNull]
    public DateTime? UpdatedAt { get; set; } = null;
}

