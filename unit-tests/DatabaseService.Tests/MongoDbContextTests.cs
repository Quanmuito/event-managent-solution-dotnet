namespace DatabaseService.Tests;

using DatabaseService;
using DatabaseService.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

public class MongoDbContextTests
{
    private static MongoDbContext CreateMongoDbContext()
    {
        var settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "test-db"
        };
        var options = Options.Create(settings);
        return new MongoDbContext(options);
    }

    [Fact]
    public void GetCollection_ShouldReturnCollectionWithCorrectName()
    {
        var context = CreateMongoDbContext();

        var collection = context.GetCollection<TestDocument>("TestCollection");

        collection.Should().NotBeNull();
        collection.CollectionNamespace.CollectionName.Should().Be("TestCollection");
    }

    [Fact]
    public void GetCollection_ShouldReturnCollectionWithCorrectType()
    {
        var context = CreateMongoDbContext();

        var collection = context.GetCollection<TestDocument>("TestCollection");

        collection.Should().NotBeNull();
        collection.DocumentSerializer.ValueType.Should().Be<TestDocument>();
    }

    [Fact]
    public void GetCollection_ShouldReturnDifferentCollectionsForDifferentNames()
    {
        var context = CreateMongoDbContext();

        var collection1 = context.GetCollection<TestDocument>("Collection1");
        var collection2 = context.GetCollection<TestDocument>("Collection2");

        collection1.CollectionNamespace.CollectionName.Should().Be("Collection1");
        collection2.CollectionNamespace.CollectionName.Should().Be("Collection2");
        collection1.CollectionNamespace.CollectionName.Should().NotBe(collection2.CollectionNamespace.CollectionName);
    }

    private class TestDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
