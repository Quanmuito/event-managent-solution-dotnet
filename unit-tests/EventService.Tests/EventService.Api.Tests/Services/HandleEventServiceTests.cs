namespace EventService.Api.Tests.Services;
using EventService.Api.Models.Api.Event;
using EventService.Api.Models.Domain;
using EventService.Api.Services;
using EventService.Data;
using EventService.Tests.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class HandleEventServiceTests
{
    private readonly Mock<MongoDbContext> _mockMongoDbContext;
    private readonly Mock<IMongoCollection<Event>> _mockCollection;
    private readonly HandleEventService _service;

    public HandleEventServiceTests()
    {
        _mockMongoDbContext = new Mock<MongoDbContext>(Mock.Of<Microsoft.Extensions.Options.IOptions<EventService.Data.Settings.MongoDbSettings>>());
        _mockCollection = new Mock<IMongoCollection<Event>>();
        _mockMongoDbContext.Setup(x => x.GetCollection<Event>("Events")).Returns(_mockCollection.Object);
        _service = new HandleEventService(_mockMongoDbContext.Object);
    }

    [Fact]
    public async Task Search_WithNullQuery_ShouldReturnAllEvents()
    {
        var events = TestDataBuilder.CreateEventList(3);
        var mockFindFluent = new Mock<IFindFluent<Event, Event>>();
        mockFindFluent.Setup(x => x.ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        _mockCollection.Setup(x => x.Find(
                It.IsAny<System.Linq.Expressions.Expression<Func<Event, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(mockFindFluent.Object);

        var result = await _service.Search(null, CancellationToken.None);

        result.Should().HaveCount(3);
        _mockCollection.Verify(x => x.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Event, bool>>>(), It.IsAny<FindOptions>()), Times.Once);
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ShouldReturnAllEvents()
    {
        var events = TestDataBuilder.CreateEventList(2);
        var mockFindFluent = new Mock<IFindFluent<Event, Event>>();
        mockFindFluent.Setup(x => x.ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        _mockCollection.Setup(x => x.Find(
                It.IsAny<System.Linq.Expressions.Expression<Func<Event, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(mockFindFluent.Object);

        var result = await _service.Search("   ", CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Search_WithSingleKeyword_ShouldFilterEvents()
    {
        var events = TestDataBuilder.CreateEventList(1);
        var mockFindFluent = new Mock<IFindFluent<Event, Event>>();
        mockFindFluent.Setup(x => x.ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        _mockCollection.Setup(x => x.Find(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<FindOptions>()))
            .Returns(mockFindFluent.Object);

        var result = await _service.Search("test", CancellationToken.None);

        result.Should().HaveCount(1);
        _mockCollection.Verify(x => x.Find(It.IsAny<FilterDefinition<Event>>(), It.IsAny<FindOptions>()), Times.Once);
    }

    [Fact]
    public async Task Search_WithMultipleKeywords_ShouldFilterEvents()
    {
        var events = TestDataBuilder.CreateEventList(2);
        var mockFindFluent = new Mock<IFindFluent<Event, Event>>();
        mockFindFluent.Setup(x => x.ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        _mockCollection.Setup(x => x.Find(
                It.IsAny<FilterDefinition<Event>>(),
                It.IsAny<FindOptions>()))
            .Returns(mockFindFluent.Object);

        var result = await _service.Search("test, event", CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnEventDto()
    {
        var eventEntity = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        var mockFindFluent = new Mock<IFindFluent<Event, Event>>();
        mockFindFluent.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _mockCollection.Setup(x => x.Find(
                It.IsAny<System.Linq.Expressions.Expression<Func<Event, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(mockFindFluent.Object);

        var result = await _service.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(eventEntity.Id);
        result.Title.Should().Be(eventEntity.Title);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldThrowNullReferenceException()
    {
        var mockFindFluent = new Mock<IFindFluent<Event, Event>>();
        mockFindFluent.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null!);

        _mockCollection.Setup(x => x.Find(
                It.IsAny<System.Linq.Expressions.Expression<Func<Event, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(mockFindFluent.Object);

        var act = async () => await _service.GetById("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldCreateAndReturnEvent()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        _mockCollection.Setup(x => x.InsertOneAsync(
                It.IsAny<Event>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.HostedBy.Should().Be(dto.HostedBy);
        result.IsPublic.Should().Be(dto.IsPublic);
        result.Details.Should().Be(dto.Details);
        result.TimeStart.Should().Be(dto.TimeStart);
        result.TimeEnd.Should().Be(dto.TimeEnd);
        _mockCollection.Verify(x => x.InsertOneAsync(
            It.IsAny<Event>(),
            It.IsAny<InsertOneOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldUpdateAndReturnEvent()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        var existingEvent = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        var updatedEvent = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        updatedEvent.Title = updateDto.Title!;
        updatedEvent.UpdatedAt = DateTime.UtcNow;

        _mockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Event, bool>>>(),
                It.IsAny<UpdateDefinition<Event>>(),
                It.IsAny<FindOneAndUpdateOptions<Event>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEvent);

        var result = await _service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be(updateDto.Title);
        _mockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Event, bool>>>(),
            It.IsAny<UpdateDefinition<Event>>(),
            It.IsAny<FindOneAndUpdateOptions<Event>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNoValidFields_ShouldThrowArgumentException()
    {
        var updateDto = new UpdateEventDto();

        var act = async () => await _service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("No valid fields to update.");
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldDeleteAndReturnDeleteResult()
    {
        var deleteResult = new DeleteResult.Acknowledged(1);
        _mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Event, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        var result = await _service.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.DeletedCount.Should().Be(1);
        _mockCollection.Verify(x => x.DeleteOneAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Event, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ShouldReturnDeleteResultWithZeroCount()
    {
        var deleteResult = new DeleteResult.Acknowledged(0);
        _mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Event, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        var result = await _service.Delete("507f1f77bcf86cd799439999", CancellationToken.None);

        result.Should().NotBeNull();
        result.DeletedCount.Should().Be(0);
    }
}
