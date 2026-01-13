namespace BookingService.Api.Models;

using BookingService.Data.Models;

public class BookingDto(Booking @booking)
{
    public string Id { get; set; } = @booking.Id!;
    public DateTime CreatedAt { get; set; } = @booking.CreatedAt;
    public DateTime? UpdatedAt { get; set; } = @booking.UpdatedAt;
}