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

    [BsonRepresentation(BsonType.ObjectId)]
    public required string EventId { get; set; }

    [AllowedValues(
        BookingStatus.Registered,
        BookingStatus.Canceled,
        BookingStatus.QueueEnrolled,
        BookingStatus.QueuePending)]
    public required string Status { get; set; }

    [BsonElement("name")]
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    [BsonElement("email")]
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [BsonElement("phone")]
    [Required]
    [StringLength(20)]
    public required string Phone { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonIgnoreIfNull]
    public DateTime? UpdatedAt { get; set; } = null;
}