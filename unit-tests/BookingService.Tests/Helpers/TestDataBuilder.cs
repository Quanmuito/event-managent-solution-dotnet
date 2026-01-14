namespace BookingService.Tests.Helpers;

using BookingService.Api.Models;
using BookingService.Data.Models;
using BookingService.Data.Utils;

public static class TestDataBuilder
{
    public static CreateBookingDto CreateValidCreateBookingDto()
    {
        return new CreateBookingDto
        {
            EventId = "507f1f77bcf86cd799439012",
            Status = BookingStatus.Registered,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890"
        };
    }

    public static CreateBookingDto CreateInvalidCreateBookingDto()
    {
        return new CreateBookingDto
        {
            EventId = "507f1f77bcf86cd799439012",
            Status = "invalid_status",
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890"
        };
    }

    public static UpdateBookingDto CreateValidUpdateBookingDto()
    {
        return new UpdateBookingDto
        {
            Status = BookingStatus.Canceled
        };
    }

    public static UpdateBookingDto CreateUpdateBookingDtoWithNullStatus()
    {
        return new UpdateBookingDto
        {
            Status = null
        };
    }

    public static Booking CreateBooking(string? id = null, string? status = null)
    {
        return new Booking
        {
            Id = id ?? "507f1f77bcf86cd799439011",
            EventId = "507f1f77bcf86cd799439012",
            Status = status ?? BookingStatus.Registered,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    public static List<Booking> CreateBookingList(int count = 3)
    {
        var bookings = new List<Booking>();
        var statuses = new[]
        {
            BookingStatus.Registered,
            BookingStatus.Canceled,
            BookingStatus.QueueEnrolled,
            BookingStatus.QueuePending
        };
        for (int i = 0; i < count; i++)
        {
            bookings.Add(CreateBooking($"507f1f77bcf86cd79943901{i}", statuses[i % statuses.Length]));
        }
        return bookings;
    }
}
