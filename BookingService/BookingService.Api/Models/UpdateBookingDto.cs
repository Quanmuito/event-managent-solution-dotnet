namespace BookingService.Api.Models;

using System.ComponentModel.DataAnnotations;
using BookingService.Data.Utils;

public class UpdateBookingDto
{
    [AllowedValues(
        BookingStatus.Registered,
        BookingStatus.Canceled,
        BookingStatus.QueueEnrolled,
        BookingStatus.QueuePending)]
    public string? Status { get; set; }

    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    public string? Name { get; set; }

    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
    public string? Phone { get; set; }
}
