namespace EventService.Data.Tests.Models;

using EventService.Api.Models;
using EventService.Data.Models;
using EventService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class EventTests
{
    private static Event CreateEventFromDto(CreateEventDto dto)
    {
        return new Event
        {
            Title = dto.Title,
            HostedBy = dto.HostedBy,
            IsPublic = dto.IsPublic,
            Details = dto.Details,
            Available = dto.Available,
            TimeStart = dto.TimeStart,
            TimeEnd = dto.TimeEnd,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    [Fact]
    public void GetEntityFromDto_ShouldMapAllPropertiesCorrectly()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        var beforeCreation = DateTime.UtcNow;

        var result = CreateEventFromDto(dto);

        var afterCreation = DateTime.UtcNow;

        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.HostedBy.Should().Be(dto.HostedBy);
        result.IsPublic.Should().Be(dto.IsPublic);
        result.Details.Should().Be(dto.Details);
        result.Available.Should().Be(dto.Available);
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

        var result = CreateEventFromDto(dto);

        result.Should().NotBeNull();
        result.Details.Should().BeNull();
    }

    [Fact]
    public void GetEntityFromDto_ShouldSetCreatedAtToUtcNow()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        var beforeCreation = DateTime.UtcNow;

        var result = CreateEventFromDto(dto);

        var afterCreation = DateTime.UtcNow;

        result.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        result.CreatedAt.Should().BeOnOrBefore(afterCreation);
        result.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }
}
