namespace AuthService.Data.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("phone")]
    [BsonIgnoreIfNull]
    public string? Phone { get; set; }

    [BsonElement("isVerified")]
    public bool IsVerified { get; set; } = false;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonIgnoreIfNull]
    public DateTime? DeletedAt { get; set; }

    [BsonElement("deletedBy")]
    [BsonIgnoreIfNull]
    public string? DeletedBy { get; set; }

    [BsonElement("deletedReason")]
    [BsonIgnoreIfNull]
    public string? DeletedReason { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonIgnoreIfNull]
    public DateTime? UpdatedAt { get; set; }
}
