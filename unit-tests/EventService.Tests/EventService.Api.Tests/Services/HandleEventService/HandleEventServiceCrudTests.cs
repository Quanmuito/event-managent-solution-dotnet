namespace EventService.Api.Tests.Services.HandleEventService;

using EventService.Api.Models;
using EventService.Data.Models;
using EventService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class HandleEventServiceCrudTests : IClassFixture<HandleEventServiceTestFixture>
{
    private readonly HandleEventServiceTestFixture _fixture;

    public HandleEventServiceCrudTests(HandleEventServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnEventDto()
    {
        var eventEntity = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        var result = await _fixture.Service.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<EventDto>();
        ServiceTestHelper.AssertDtoMatchesEntity(result, eventEntity, "Id", "Title", "HostedBy", "IsPublic", "Details", "Available", "TimeStart", "TimeEnd", "CreatedAt", "UpdatedAt");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Events with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _fixture.Service.GetById("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Events with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task GetById_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _fixture.MockRepository.Setup(x => x.GetByIdAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _fixture.Service.GetById("invalid-id", CancellationToken.None);

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
        createdEvent.Available = dto.Available;
        createdEvent.TimeStart = dto.TimeStart;
        createdEvent.TimeEnd = dto.TimeEnd;

        _fixture.MockRepository.Setup(x => x.CreateAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event e, CancellationToken ct) => e);

        var result = await _fixture.Service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.HostedBy.Should().Be(dto.HostedBy);
        result.IsPublic.Should().Be(dto.IsPublic);
        result.Details.Should().Be(dto.Details);
        result.Available.Should().Be(dto.Available);
        result.TimeStart.Should().Be(dto.TimeStart);
        result.TimeEnd.Should().Be(dto.TimeEnd);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _fixture.MockRepository.Verify(x => x.CreateAsync(It.Is<Event>(e =>
            e.Title == dto.Title &&
            e.HostedBy == dto.HostedBy &&
            e.IsPublic == dto.IsPublic &&
            e.Details == dto.Details &&
            e.Available == dto.Available &&
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
        updatedEvent.Available = updateDto.Available!.Value;
        updatedEvent.TimeStart = updateDto.TimeStart!.Value;
        updatedEvent.TimeEnd = updateDto.TimeEnd!.Value;
        updatedEvent.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEvent);

        var result = await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be(updateDto.Title);
        result.HostedBy.Should().Be(updateDto.HostedBy);
        result.IsPublic.Should().Be(updateDto.IsPublic!.Value);
        result.Details.Should().Be(updateDto.Details);
        result.Available.Should().Be(updateDto.Available!.Value);
        result.TimeStart.Should().Be(updateDto.TimeStart);
        result.TimeEnd.Should().Be(updateDto.TimeEnd);
        result.UpdatedAt.Should().NotBeNull();
        _fixture.MockRepository.Verify(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNoValidFields_ShouldThrowArgumentException()
    {
        var updateDto = new UpdateEventDto();

        var act = async () => await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("No valid fields to update.");
    }

    [Fact]
    public async Task Update_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439999", It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Events with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _fixture.Service.Update("507f1f77bcf86cd799439999", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Events with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Update_WithInvalidFormatId_ShouldThrowFormatException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        _fixture.MockRepository.Setup(x => x.UpdateAsync("invalid-id", It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _fixture.Service.Update("invalid-id", updateDto, CancellationToken.None);

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

        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEvent);

        var result = await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("New Title");
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_WithAvailableField_ShouldUpdateAvailable()
    {
        var updateDto = new UpdateEventDto { Available = 75 };
        var updatedEvent = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        updatedEvent.Available = 75;
        updatedEvent.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEvent);

        var result = await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Available.Should().Be(75);
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldDeleteAndReturnTrue()
    {
        _fixture.MockRepository.Setup(x => x.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _fixture.Service.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeTrue();
        _fixture.MockRepository.Verify(x => x.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ShouldReturnFalse()
    {
        _fixture.MockRepository.Setup(x => x.DeleteAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _fixture.Service.Delete("507f1f77bcf86cd799439999", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _fixture.MockRepository.Setup(x => x.DeleteAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _fixture.Service.Delete("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }
}
