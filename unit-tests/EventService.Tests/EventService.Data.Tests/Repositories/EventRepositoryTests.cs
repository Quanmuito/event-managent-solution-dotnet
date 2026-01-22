namespace EventService.Data.Tests.Repositories;

using DatabaseService;
using EventService.Data.Models;
using EventService.Data.Repositories;
using EventService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class EventRepositoryTests : RepositoryTestBase<Event, EventRepository>
{
    protected override string GetCollectionName()
    {
        return "Events";
    }

    protected override EventRepository CreateRepository(MongoDbContext mongoDbContext)
    {
        return new EventRepository(mongoDbContext);
    }

    protected override Event CreateEntity(string? id = null)
    {
        return TestDataBuilder.CreateEvent(id);
    }

    protected override string GetValidEntityId()
    {
        return "507f1f77bcf86cd799439011";
    }

    protected override string GetNonExistentEntityId()
    {
        return "507f1f77bcf86cd799439999";
    }

    protected override UpdateDefinition<Event> CreateUpdateDefinition(Event entity)
    {
        return Builders<Event>.Update.Set(e => e.Title, entity.Title);
    }

    protected override void AssertEntityMatches(Event actual, Event expected)
    {
        base.AssertEntityMatches(actual, expected);
        actual.Id.Should().Be(expected.Id);
        actual.Title.Should().Be(expected.Title);
    }

    protected override bool AssertEntityEquals(Event actual, Event expected)
    {
        return actual.Id == expected.Id && actual.Title == expected.Title;
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ShouldReturnAllEvents()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011"),
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439012")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, events);

        var result = await Repository.SearchAsync(null, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(events);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ShouldReturnAllEvents()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, events);

        var result = await Repository.SearchAsync("   ", CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_WithWhitespaceQuery_ShouldReturnAllEvents()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, events);

        var result = await Repository.SearchAsync("\t\n", CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_WithSingleKeyword_ShouldFilterByTitleOrDetails()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011", "Test Event", "Test details")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, events);

        var result = await Repository.SearchAsync("Test", CancellationToken.None);

        result.Should().HaveCount(1);
        MockCollection.Verify(x => x.FindAsync(
            It.IsAny<FilterDefinition<Event>>(),
            It.IsAny<FindOptions<Event, Event>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithMultipleKeywords_ShouldFilterByAnyKeyword()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011", "Event One", "Details"),
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439012", "Event Two", "More details")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, events);

        var result = await Repository.SearchAsync("One, Two", CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_WithCommaSeparatedKeywords_ShouldTrimAndFilter()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011", "Test Event", "Details")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, events);

        var result = await Repository.SearchAsync(" Test , Event ", CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyKeywordsAfterSplit_ShouldReturnAllEvents()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, events);

        var result = await Repository.SearchAsync(",,,", CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_WithSpecialCharacters_ShouldEscapeRegex()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011", "Test (Event)", "Details")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, events);

        var result = await Repository.SearchAsync("(Event)", CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_ShouldBeCaseInsensitive()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011", "Test Event", "Details")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, events);

        var result = await Repository.SearchAsync("TEST", CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
