namespace EventService.Data.Tests;
using EventService.Data;
using EventService.Data.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;

public class MongoDbContextTests
{
    [Fact]
    public void GetCollection_ShouldReturnCollectionWithCorrectName()
    {
        var settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "test-db"
        };
        var options = Options.Create(settings);
        var context = new MongoDbContext(options);

        var collection = context.GetCollection<TestDocument>("TestCollection");

        collection.Should().NotBeNull();
        collection.CollectionNamespace.CollectionName.Should().Be("TestCollection");
    }

    [Fact]
    public void GetCollection_ShouldReturnCollectionWithCorrectType()
    {
        var settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "test-db"
        };
        var options = Options.Create(settings);
        var context = new MongoDbContext(options);

        var collection = context.GetCollection<TestDocument>("TestCollection");

        collection.Should().NotBeNull();
        collection.DocumentSerializer.ValueType.Should().Be(typeof(TestDocument));
    }

    private class TestDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
