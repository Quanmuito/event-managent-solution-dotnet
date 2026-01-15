namespace BookingService.Data.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class QrCode
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string BookingId { get; set; }

    [BsonElement("qrCodeData")]
    public required byte[] QrCodeData { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
