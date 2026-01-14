namespace BookingService.Api.Tests.Models;

using BookingService.Api.Models;
using BookingService.Data.Models;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class BookingDtoTests
{
    [Fact]
    public void Constructor_WithBooking_ShouldMapAllProperties()
    {
        var booking = new Booking
        {
            Id = "507f1f77bcf86cd799439011",
            EventId = "507f1f77bcf86cd799439012",
            Status = BookingStatus.Registered,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var dto = new BookingDto(booking);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(booking.Id);
        dto.EventId.Should().Be(booking.EventId);
        dto.Status.Should().Be(booking.Status);
        dto.Name.Should().Be(booking.Name);
        dto.Email.Should().Be(booking.Email);
        dto.Phone.Should().Be(booking.Phone);
        dto.CreatedAt.Should().Be(booking.CreatedAt);
        dto.UpdatedAt.Should().Be(booking.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithBookingWithNullUpdatedAt_ShouldMapNull()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);

        var dto = new BookingDto(booking);

        dto.Should().NotBeNull();
        dto.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(BookingStatus.Registered)]
    [InlineData(BookingStatus.Canceled)]
    [InlineData(BookingStatus.QueueEnrolled)]
    [InlineData(BookingStatus.QueuePending)]
    public void Constructor_WithDifferentStatuses_ShouldMapCorrectly(string status)
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", status);

        var dto = new BookingDto(booking);

        dto.Status.Should().Be(status);
    }
}
