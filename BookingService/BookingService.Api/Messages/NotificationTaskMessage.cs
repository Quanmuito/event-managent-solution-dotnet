namespace BookingService.Api.Messages;

using BookingService.Api.Models;

public record NotificationTaskMessage(
    BookingDto Booking,
    string Operation
);