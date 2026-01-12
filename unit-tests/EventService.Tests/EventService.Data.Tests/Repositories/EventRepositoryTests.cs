namespace EventService.Data.Tests.Repositories;

using DatabaseService;
using DatabaseService.Settings;
using EventService.Data.Models;
using EventService.Data.Repositories;
using TestUtilities.Helpers;
using EventService.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;

public class EventRepositoryTests
{
    private readonly Mock<MongoDbContext> _mockMongoDbContext;
    private readonly Mock<IMongoCollection<Event>> _mockCollection;
    private readonly EventRepository _repository;

    public EventRepositoryTests()
    {
        var mockOptions = new Mock<IOptions<MongoDbSettings>>();
        mockOptions.Setup(x => x.Value).Returns(new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "test-db"
        });
        _mockMongoDbContext = new Mock<MongoDbContext>(mockOptions.Object) { CallBase = true };
        _mockCollection = new Mock<IMongoCollection<Event>>(MockBehavior.Loose);
        _mockMongoDbContext.Setup(x => x.GetCollection<Event>("Events")).Returns(_mockCollection.Object);
        _repository = new EventRepository(_mockMongoDbContext.Object);
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ShouldReturnAllEvents()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011"),
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439012")
        };
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, events);

        var result = await _repository.SearchAsync(null, CancellationToken.None);

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
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, events);

        var result = await _repository.SearchAsync("   ", CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_WithWhitespaceQuery_ShouldReturnAllEvents()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011")
        };
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, events);

        var result = await _repository.SearchAsync("\t\n", CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_WithSingleKeyword_ShouldFilterByTitleOrDetails()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011", "Test Event", "Test details")
        };
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, events);

        var result = await _repository.SearchAsync("Test", CancellationToken.None);

        result.Should().HaveCount(1);
        _mockCollection.Verify(x => x.FindAsync(
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
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, events);

        var result = await _repository.SearchAsync("One, Two", CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_WithCommaSeparatedKeywords_ShouldTrimAndFilter()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011", "Test Event", "Details")
        };
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, events);

        var result = await _repository.SearchAsync(" Test , Event ", CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyKeywordsAfterSplit_ShouldReturnAllEvents()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011")
        };
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, events);

        var result = await _repository.SearchAsync(",,,", CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_WithSpecialCharacters_ShouldEscapeRegex()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011", "Test (Event)", "Details")
        };
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, events);

        var result = await _repository.SearchAsync("(Event)", CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_ShouldBeCaseInsensitive()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011", "Test Event", "Details")
        };
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, events);

        var result = await _repository.SearchAsync("TEST", CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
