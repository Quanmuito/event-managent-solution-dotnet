namespace DatabaseService.Tests.Helpers;

using MongoDB.Bson;
using DatabaseService.Tests.Models;

public static class TestDataBuilder
{
    public static TestEntity CreateTestEntity(string? id = null)
    {
        return new TestEntity
        {
            Id = id ?? ObjectId.GenerateNewId().ToString(),
            Title = "Test Title",
            Name = "Test Name"
        };
    }
}
