namespace BookingService.Data.Models;

using System.ComponentModel.DataAnnotations;
using BookingService.Data.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Booking
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [AllowedValues(
        BookingStatus.Registered,
        BookingStatus.Canceled,
        BookingStatus.QueueEnrolled,
        BookingStatus.QueuePending,
        BookingStatus.QueueConfirmed)]
    public required string Status { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonIgnoreIfNull]
    public DateTime? UpdatedAt { get; set; } = null;
}