namespace EventService.Api.Tests.Services;

using System.Collections.Generic;
using EventService.Api.Models;
using EventService.Api.Services;
using EventService.Data.Models;
using EventService.Data.Repositories;
using EventService.Tests.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class HandleEventServiceTests
{
    private readonly Mock<IEventRepository> _mockRepository;
    private readonly HandleEventService _service;

    public HandleEventServiceTests()
    {
        _mockRepository = new Mock<IEventRepository>();
        _service = new HandleEventService(_mockRepository.Object);
    }

    [Fact]
    public async Task Search_WithNullQuery_ShouldReturnAllEvents()
    {
        var events = TestDataBuilder.CreateEventList(3);
        _mockRepository.Setup(x => x.SearchAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _service.Search(null, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().AllBeOfType<EventDto>();
        _mockRepository.Verify(x => x.SearchAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ShouldReturnAllEvents()
    {
        var events = TestDataBuilder.CreateEventList(2);
        _mockRepository.Setup(x => x.SearchAsync("   ", It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _service.Search("   ", CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllBeOfType<EventDto>();
    }

    [Fact]
    public async Task Search_WithSingleKeyword_ShouldFilterEvents()
    {
        var events = TestDataBuilder.CreateEventList(1);
        _mockRepository.Setup(x => x.SearchAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _service.Search("test", CancellationToken.None);

        result.Should().HaveCount(1);
        result.Should().AllBeOfType<EventDto>();
        _mockRepository.Verify(x => x.SearchAsync("test", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_WithMultipleKeywords_ShouldFilterEvents()
    {
        var events = TestDataBuilder.CreateEventList(2);
        _mockRepository.Setup(x => x.SearchAsync("test, event", It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _service.Search("test, event", CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllBeOfType<EventDto>();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnEventDto()
    {
        var eventEntity = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        var result = await _service.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<EventDto>();
        AssertEventDtoMatchesEvent(result, eventEntity);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Events with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _service.GetById("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Events with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task GetById_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _service.GetById("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldCreateAndReturnEvent()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        var createdEvent = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        createdEvent.Title = dto.Title;
        createdEvent.HostedBy = dto.HostedBy;
        createdEvent.IsPublic = dto.IsPublic;
        createdEvent.Details = dto.Details;
        createdEvent.TimeStart = dto.TimeStart;
        createdEvent.TimeEnd = dto.TimeEnd;

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event e, CancellationToken ct) => e);

        var result = await _service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.HostedBy.Should().Be(dto.HostedBy);
        result.IsPublic.Should().Be(dto.IsPublic);
        result.Details.Should().Be(dto.Details);
        result.TimeStart.Should().Be(dto.TimeStart);
        result.TimeEnd.Should().Be(dto.TimeEnd);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _mockRepository.Verify(x => x.CreateAsync(It.Is<Event>(e =>
            e.Title == dto.Title &&
            e.HostedBy == dto.HostedBy &&
            e.IsPublic == dto.IsPublic &&
            e.Details == dto.Details &&
            e.TimeStart == dto.TimeStart &&
            e.TimeEnd == dto.TimeEnd), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldUpdateAndReturnEvent()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        var updatedEvent = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        updatedEvent.Title = updateDto.Title!;
        updatedEvent.HostedBy = updateDto.HostedBy!;
        updatedEvent.IsPublic = updateDto.IsPublic!.Value;
        updatedEvent.Details = updateDto.Details;
        updatedEvent.TimeStart = updateDto.TimeStart!.Value;
        updatedEvent.TimeEnd = updateDto.TimeEnd!.Value;
        updatedEvent.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEvent);

        var result = await _service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be(updateDto.Title);
        result.HostedBy.Should().Be(updateDto.HostedBy);
        result.IsPublic.Should().Be(updateDto.IsPublic!.Value);
        result.Details.Should().Be(updateDto.Details);
        result.TimeStart.Should().Be(updateDto.TimeStart);
        result.TimeEnd.Should().Be(updateDto.TimeEnd);
        result.UpdatedAt.Should().NotBeNull();
        _mockRepository.Verify(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()), Times.Once);
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
    public async Task Update_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439999", It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Events with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _service.Update("507f1f77bcf86cd799439999", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Events with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Update_WithInvalidFormatId_ShouldThrowFormatException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        _mockRepository.Setup(x => x.UpdateAsync("invalid-id", It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _service.Update("invalid-id", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Update_WithPartialFields_ShouldUpdateOnlySpecifiedFields()
    {
        var updateDto = new UpdateEventDto { Title = "New Title" };
        var updatedEvent = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        updatedEvent.Title = "New Title";
        updatedEvent.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEvent);

        var result = await _service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("New Title");
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldDeleteAndReturnTrue()
    {
        _mockRepository.Setup(x => x.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ShouldReturnFalse()
    {
        _mockRepository.Setup(x => x.DeleteAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.Delete("507f1f77bcf86cd799439999", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _mockRepository.Setup(x => x.DeleteAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _service.Delete("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    private static void AssertEventDtoMatchesEvent(EventDto dto, Event eventEntity)
    {
        dto.Should().NotBeNull();
        dto.Id.Should().Be(eventEntity.Id);
        dto.Title.Should().Be(eventEntity.Title);
        dto.HostedBy.Should().Be(eventEntity.HostedBy);
        dto.IsPublic.Should().Be(eventEntity.IsPublic);
        dto.Details.Should().Be(eventEntity.Details);
        dto.TimeStart.Should().Be(eventEntity.TimeStart);
        dto.TimeEnd.Should().Be(eventEntity.TimeEnd);
        dto.CreatedAt.Should().Be(eventEntity.CreatedAt);
        dto.UpdatedAt.Should().Be(eventEntity.UpdatedAt);
    }
}
