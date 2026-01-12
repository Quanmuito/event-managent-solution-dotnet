namespace DatabaseService.Tests.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class TestEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
}
