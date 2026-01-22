namespace DatabaseService.Tests;

using DatabaseService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class MongoDbContextTests
{
    [Fact]
    public void GetCollection_ShouldReturnCollectionWithCorrectName()
    {
        var context = TestDataBuilder.CreateMongoDbContext();

        var collection = context.GetCollection<TestDocument>("TestCollection");

        collection.Should().NotBeNull();
        collection.CollectionNamespace.CollectionName.Should().Be("TestCollection");
    }

    [Fact]
    public void GetCollection_ShouldReturnCollectionWithCorrectType()
    {
        var context = TestDataBuilder.CreateMongoDbContext();

        var collection = context.GetCollection<TestDocument>("TestCollection");

        collection.Should().NotBeNull();
        collection.DocumentSerializer.ValueType.Should().Be<TestDocument>();
    }

    [Fact]
    public void GetCollection_ShouldReturnDifferentCollectionsForDifferentNames()
    {
        var context = TestDataBuilder.CreateMongoDbContext();

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
