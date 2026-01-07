namespace EventService.Api.Tests.Models.Domain;
using EventService.Api.Models.Api.Event;
using EventService.Api.Models.Domain;
using EventService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class EventTests
{
    [Fact]
    public void GetEntityFromDto_ShouldMapAllPropertiesCorrectly()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        var beforeCreation = DateTime.UtcNow;

        var result = Event.GetEntityFromDto(dto);

        var afterCreation = DateTime.UtcNow;

        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.HostedBy.Should().Be(dto.HostedBy);
        result.IsPublic.Should().Be(dto.IsPublic);
        result.Details.Should().Be(dto.Details);
        result.TimeStart.Should().Be(dto.TimeStart);
        result.TimeEnd.Should().Be(dto.TimeEnd);
        result.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        result.CreatedAt.Should().BeOnOrBefore(afterCreation);
        result.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void GetEntityFromDto_ShouldHandleNullDetails()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        dto.Details = null;

        var result = Event.GetEntityFromDto(dto);

        result.Should().NotBeNull();
        result.Details.Should().BeNull();
    }

    [Fact]
    public void GetEntityFromDto_ShouldSetCreatedAtToUtcNow()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        var beforeCreation = DateTime.UtcNow;

        var result = Event.GetEntityFromDto(dto);

        var afterCreation = DateTime.UtcNow;

        result.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        result.CreatedAt.Should().BeOnOrBefore(afterCreation);
        result.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }
}
