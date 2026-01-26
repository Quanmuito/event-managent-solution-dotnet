namespace QueueService.Data.Tests.Repositories;

using QueueService.Data.Models;
using QueueService.Data.Repositories;
using QueueService.Tests.Helpers;
using DatabaseService;
using TestUtilities.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Xunit;

public class QueueRepositoryTests : RepositoryTestBase<Queue, QueueRepository>
{
    protected override string GetCollectionName()
    {
        return "Queues";
    }

    protected override QueueRepository CreateRepository(MongoDbContext mongoDbContext)
    {
        return new QueueRepository(mongoDbContext);
    }

    protected override Queue CreateEntity(string? id = null)
    {
        return TestDataBuilder.CreateQueue(id);
    }

    protected override string GetValidEntityId()
    {
        return "507f1f77bcf86cd799439011";
    }

    protected override string GetNonExistentEntityId()
    {
        return "507f1f77bcf86cd799439999";
    }

    protected override UpdateDefinition<Queue> CreateUpdateDefinition(Queue entity)
    {
        return Builders<Queue>.Update.Set(q => q.Available, entity.Available);
    }

    protected override void AssertEntityMatches(Queue actual, Queue expected)
    {
        base.AssertEntityMatches(actual, expected);
        actual.Id.Should().Be(expected.Id);
        actual.EventId.Should().Be(expected.EventId);
        actual.Available.Should().Be(expected.Available);
        actual.Top.Should().Be(expected.Top);
    }

    protected override bool AssertEntityEquals(Queue actual, Queue expected)
    {
        return actual.Id == expected.Id && actual.EventId == expected.EventId && actual.Available == expected.Available && actual.Top == expected.Top;
    }

    [Fact]
    public async Task GetByEventIdAsync_WithExistingEventId_ShouldReturnQueue()
    {
        var eventId = "507f1f77bcf86cd799439012";
        var expectedQueue = TestDataBuilder.CreateQueue("507f1f77bcf86cd799439011", eventId, 5, 10);
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(MockCollection, expectedQueue);

        var result = await Repository.GetByEventIdAsync(eventId, CancellationToken.None);

        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.Available.Should().Be(expectedQueue.Available);
        result.Top.Should().Be(expectedQueue.Top);
    }

    [Fact]
    public async Task GetByEventIdAsync_WithNonExistentEventId_ShouldThrowKeyNotFoundException()
    {
        var eventId = "507f1f77bcf86cd799439999";
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(MockCollection, null);

        var act = async () => await Repository.GetByEventIdAsync(eventId, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Queue with EventId '{eventId}' was not found.");
    }
}
