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

    [Fact]
    public async Task OnBookingRegisteredAsync_WithAvailableSeats_ShouldDecrementAvailability()
    {
        var eventId = GetValidEntityId();
        var eventEntity = TestDataBuilder.CreateEvent(eventId);
        eventEntity.Available = 10;
        var updatedEvent = TestDataBuilder.CreateEvent(eventId);
        updatedEvent.Available = 9;
        updatedEvent.UpdatedAt = DateTime.UtcNow;

        MockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<UpdateDefinition<Event>>(),
                It.IsAny<FindOneAndUpdateOptions<Event>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEvent);

        var result = await Repository.OnBookingRegisteredAsync(eventId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Available.Should().Be(9);
        result.UpdatedAt.Should().NotBeNull();
        MockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<Event>>(),
            It.IsAny<UpdateDefinition<Event>>(),
            It.IsAny<FindOneAndUpdateOptions<Event>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnBookingRegisteredAsync_WithNoAvailableSeats_ShouldThrowInvalidOperationException()
    {
        var eventId = GetValidEntityId();

#pragma warning disable CS8620
        MockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<UpdateDefinition<Event>>(),
                It.IsAny<FindOneAndUpdateOptions<Event>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);
#pragma warning restore CS8620

        var act = async () => await Repository.OnBookingRegisteredAsync(eventId, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Cannot decrement availability for event '{eventId}': No available seats or event not found.");
    }

    [Fact]
    public async Task OnBookingRegisteredAsync_WithNonExistentEvent_ShouldThrowInvalidOperationException()
    {
        var eventId = GetNonExistentEntityId();

#pragma warning disable CS8620
        MockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<UpdateDefinition<Event>>(),
                It.IsAny<FindOneAndUpdateOptions<Event>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);
#pragma warning restore CS8620

        var act = async () => await Repository.OnBookingRegisteredAsync(eventId, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Cannot decrement availability for event '{eventId}': No available seats or event not found.");
    }

    [Fact]
    public async Task OnBookingCancelledAsync_WithValidId_ShouldIncrementAvailability()
    {
        var eventId = GetValidEntityId();
        var eventEntity = TestDataBuilder.CreateEvent(eventId);
        eventEntity.Available = 5;
        var updatedEvent = TestDataBuilder.CreateEvent(eventId);
        updatedEvent.Available = 6;
        updatedEvent.UpdatedAt = DateTime.UtcNow;

        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(MockCollection, eventEntity);
        MockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<UpdateDefinition<Event>>(),
                It.IsAny<FindOneAndUpdateOptions<Event>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEvent);

        var result = await Repository.OnBookingCancelledAsync(eventId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Available.Should().Be(6);
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task OnBookingCancelledAsync_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        var eventId = GetNonExistentEntityId();

        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(MockCollection, null);

        var act = async () => await Repository.OnBookingCancelledAsync(eventId, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Events with ID '{eventId}' was not found.");
    }
}
