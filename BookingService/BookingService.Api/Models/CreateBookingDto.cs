namespace BookingService.Api.Models;

using System.ComponentModel.DataAnnotations;
using BookingService.Data.Utils;

public class CreateBookingDto
{
    [Required(ErrorMessage = "EventId is required.")]
    public required string EventId { get; set; }

    [AllowedValues(
        BookingStatus.Registered,
        BookingStatus.Canceled,
        BookingStatus.QueueEnrolled,
        BookingStatus.QueuePending)]
    public required string Status { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    public required string Name { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public required string Email { get; set; }

    [Required]
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
    public required string Phone { get; set; }
}
