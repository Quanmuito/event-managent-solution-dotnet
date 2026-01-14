namespace BookingService.Api.Models;

using System.ComponentModel.DataAnnotations;
using BookingService.Data.Utils;

public class CreateBookingDto
{
    [AllowedValues(
        BookingStatus.Registered,
        BookingStatus.Canceled,
        BookingStatus.QueueEnrolled,
        BookingStatus.QueuePending,
        BookingStatus.QueueConfirmed)]
    public required string Status { get; set; }
}
