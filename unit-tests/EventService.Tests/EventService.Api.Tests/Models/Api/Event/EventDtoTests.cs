namespace EventService.Api.Tests.Models.Api.Event;
using EventService.Api.Models.Api.Event;
using EventService.Api.Models.Domain;
using EventService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class EventDtoTests
{
    [Fact]
    public void Constructor_WithValidEvent_ShouldMapAllProperties()
    {
        var eventEntity = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");

        var result = new EventDto(eventEntity);

        result.Should().NotBeNull();
        result.Id.Should().Be(eventEntity.Id);
        result.Title.Should().Be(eventEntity.Title);
        result.HostedBy.Should().Be(eventEntity.HostedBy);
        result.IsPublic.Should().Be(eventEntity.IsPublic);
        result.Details.Should().Be(eventEntity.Details);
        result.TimeStart.Should().Be(eventEntity.TimeStart);
        result.TimeEnd.Should().Be(eventEntity.TimeEnd);
        result.CreatedAt.Should().Be(eventEntity.CreatedAt);
        result.UpdatedAt.Should().Be(eventEntity.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithNullUpdatedAt_ShouldSetUpdatedAtToNull()
    {
        var eventEntity = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        eventEntity.UpdatedAt = null;

        var result = new EventDto(eventEntity);

        result.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithUpdatedAt_ShouldMapUpdatedAt()
    {
        var eventEntity = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        var updatedAt = DateTime.UtcNow;
        eventEntity.UpdatedAt = updatedAt;

        var result = new EventDto(eventEntity);

        result.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void Constructor_WithNullDetails_ShouldSetDetailsToNull()
    {
        var eventEntity = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        eventEntity.Details = null;

        var result = new EventDto(eventEntity);

        result.Details.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullEvent_ShouldThrowNullReferenceException()
    {
        var act = () => new EventDto(null!);

        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void Constructor_WithEventHavingAllProperties_ShouldMapCorrectly()
    {
        var eventEntity = new Event
        {
            Id = "507f1f77bcf86cd799439011",
            Title = "Test Event",
            HostedBy = "Test Host",
            IsPublic = false,
            Details = "Test Details",
            TimeStart = DateTime.UtcNow.AddDays(1),
            TimeEnd = DateTime.UtcNow.AddDays(2),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = new EventDto(eventEntity);

        result.Id.Should().Be("507f1f77bcf86cd799439011");
        result.Title.Should().Be("Test Event");
        result.HostedBy.Should().Be("Test Host");
        result.IsPublic.Should().BeFalse();
        result.Details.Should().Be("Test Details");
        result.TimeStart.Should().Be(eventEntity.TimeStart);
        result.TimeEnd.Should().Be(eventEntity.TimeEnd);
        result.CreatedAt.Should().Be(eventEntity.CreatedAt);
        result.UpdatedAt.Should().Be(eventEntity.UpdatedAt);
    }
}
