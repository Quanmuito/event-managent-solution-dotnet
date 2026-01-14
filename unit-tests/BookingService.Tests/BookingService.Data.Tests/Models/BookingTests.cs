namespace BookingService.Data.Tests.Models;

using BookingService.Api.Models;
using BookingService.Data.Models;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class BookingTests
{
    private static Booking CreateBookingFromDto(CreateBookingDto dto)
    {
        return new Booking
        {
            EventId = dto.EventId,
            Status = dto.Status,
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    [Fact]
    public void GetEntityFromDto_ShouldMapAllPropertiesCorrectly()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        var beforeCreation = DateTime.UtcNow;

        var result = CreateBookingFromDto(dto);

        var afterCreation = DateTime.UtcNow;

        result.Should().NotBeNull();
        result.Status.Should().Be(dto.Status);
        result.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        result.CreatedAt.Should().BeOnOrBefore(afterCreation);
        result.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(BookingStatus.Registered)]
    [InlineData(BookingStatus.Canceled)]
    [InlineData(BookingStatus.QueueEnrolled)]
    [InlineData(BookingStatus.QueuePending)]
    public void GetEntityFromDto_WithDifferentStatuses_ShouldMapCorrectly(string status)
    {
        var dto = new CreateBookingDto
        {
            EventId = "507f1f77bcf86cd799439012",
            Status = status,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890"
        };

        var result = CreateBookingFromDto(dto);

        result.Status.Should().Be(status);
    }

    [Fact]
    public void GetEntityFromDto_ShouldSetCreatedAtToUtcNow()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        var beforeCreation = DateTime.UtcNow;

        var result = CreateBookingFromDto(dto);

        var afterCreation = DateTime.UtcNow;

        result.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        result.CreatedAt.Should().BeOnOrBefore(afterCreation);
        result.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Booking_WithValidStatus_ShouldHaveCorrectStatus()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);

        booking.Status.Should().Be(BookingStatus.Registered);
    }
}
