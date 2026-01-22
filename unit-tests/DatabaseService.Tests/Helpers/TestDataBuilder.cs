namespace DatabaseService.Tests.Helpers;

using DatabaseService;
using DatabaseService.Settings;
using MongoDB.Bson;
using DatabaseService.Tests.Models;
using Microsoft.Extensions.Options;

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

    public static MongoDbContext CreateMongoDbContext()
    {
        var settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "test-db"
        };
        var options = Options.Create(settings);
        return new MongoDbContext(options);
    }
}
