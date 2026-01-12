namespace EventService.Data.Tests.Repositories;

using EventService.Data;
using EventService.Data.Exceptions;
using EventService.Data.Models;
using EventService.Data.Repositories;
using EventService.Data.Settings;
using EventService.Data.Tests.Helpers;
using EventService.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;

public class RepositoryTests
{
    private readonly Mock<MongoDbContext> _mockMongoDbContext;
    private readonly Mock<IMongoCollection<Event>> _mockCollection;
    private readonly Repository<Event> _repository;

    public RepositoryTests()
    {
        var mockOptions = new Mock<IOptions<MongoDbSettings>>();
        mockOptions.Setup(x => x.Value).Returns(new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "test-db"
        });
        _mockMongoDbContext = new Mock<MongoDbContext>(mockOptions.Object) { CallBase = true };
        _mockCollection = new Mock<IMongoCollection<Event>>(MockBehavior.Loose);
        _mockMongoDbContext.Setup(x => x.GetCollection<Event>("TestCollection")).Returns(_mockCollection.Object);
        _repository = new Repository<Event>(_mockMongoDbContext.Object, "TestCollection");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEvent()
    {
        var eventId = "507f1f77bcf86cd799439011";
        var expectedEvent = TestDataBuilder.CreateEvent(eventId);
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(_mockCollection, expectedEvent);

        var result = await _repository.GetByIdAsync(eventId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(eventId);
        result.Title.Should().Be(expectedEvent.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var eventId = "507f1f77bcf86cd799439999";
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(_mockCollection, null);

        var act = async () => await _repository.GetByIdAsync(eventId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"TestCollection with ID '{eventId}' was not found.");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidObjectId_ShouldThrowFormatException()
    {
        var invalidId = "invalid-id";

        var act = async () => await _repository.GetByIdAsync(invalidId, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage($"Invalid ObjectId format: {invalidId}");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEvents()
    {
        var events = new List<Event>
        {
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011"),
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439012"),
            TestDataBuilder.CreateEvent("507f1f77bcf86cd799439013")
        };
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, events);

        var result = await _repository.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(events);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyCollection_ShouldReturnEmptyList()
    {
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, []);

        var result = await _repository.GetAllAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertAndReturnEvent()
    {
        var newEvent = TestDataBuilder.CreateEvent();
        _mockCollection.Setup(x => x.InsertOneAsync(
                It.IsAny<Event>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _repository.CreateAsync(newEvent, CancellationToken.None);

        result.Should().Be(newEvent);
        _mockCollection.Verify(x => x.InsertOneAsync(
            It.Is<Event>(e => e.Id == newEvent.Id && e.Title == newEvent.Title),
            It.IsAny<InsertOneOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_ShouldUpdateAndReturnEvent()
    {
        var eventId = "507f1f77bcf86cd799439011";
        var updatedEvent = TestDataBuilder.CreateEvent(eventId);
        updatedEvent.Title = "Updated Title";
        var updateDefinition = Builders<Event>.Update.Set(e => e.Title, "Updated Title");

        _mockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<UpdateDefinition<Event>>(),
                It.IsAny<FindOneAndUpdateOptions<Event>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEvent);

        var result = await _repository.UpdateAsync(eventId, updateDefinition, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        _mockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<Event>>(),
            It.IsAny<UpdateDefinition<Event>>(),
            It.IsAny<FindOneAndUpdateOptions<Event>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var eventId = "507f1f77bcf86cd799439999";
        var updateDefinition = Builders<Event>.Update.Set(e => e.Title, "Updated Title");

        _mockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<UpdateDefinition<Event>>(),
                It.IsAny<FindOneAndUpdateOptions<Event>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null!);

        var act = async () => await _repository.UpdateAsync(eventId, updateDefinition, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"TestCollection with ID '{eventId}' was not found.");
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidObjectId_ShouldThrowFormatException()
    {
        var invalidId = "invalid-id";
        var updateDefinition = Builders<Event>.Update.Set(e => e.Title, "Updated Title");

        var act = async () => await _repository.UpdateAsync(invalidId, updateDefinition, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage($"Invalid ObjectId format: {invalidId}");
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldReturnTrue()
    {
        var eventId = "507f1f77bcf86cd799439011";
        var deleteResult = new DeleteResult.Acknowledged(1);

        _mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        var result = await _repository.DeleteAsync(eventId, CancellationToken.None);

        result.Should().BeTrue();
        _mockCollection.Verify(x => x.DeleteOneAsync(
            It.IsAny<FilterDefinition<Event>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        var eventId = "507f1f77bcf86cd799439999";
        var deleteResult = new DeleteResult.Acknowledged(0);

        _mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        var result = await _repository.DeleteAsync(eventId, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidObjectId_ShouldThrowFormatException()
    {
        var invalidId = "invalid-id";

        var act = async () => await _repository.DeleteAsync(invalidId, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage($"Invalid ObjectId format: {invalidId}");
    }
}
