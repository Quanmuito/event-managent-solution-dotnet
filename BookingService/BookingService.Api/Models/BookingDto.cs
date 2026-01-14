namespace BookingService.Api.Models;

using BookingService.Data.Models;

public class BookingDto(Booking @booking)
{
    public string Id { get; set; } = @booking.Id!;
    public string EventId { get; set; } = @booking.EventId;
    public string Status { get; set; } = @booking.Status;
    public string Name { get; set; } = @booking.Name;
    public string Email { get; set; } = @booking.Email;
    public string Phone { get; set; } = @booking.Phone;
    public DateTime CreatedAt { get; set; } = @booking.CreatedAt;
    public DateTime? UpdatedAt { get; set; } = @booking.UpdatedAt;
}
