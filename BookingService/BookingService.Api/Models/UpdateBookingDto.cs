namespace BookingService.Api.Models;

using System.ComponentModel.DataAnnotations;
using BookingService.Data.Utils;

public class UpdateBookingDto : IValidatableObject
{
    public string? Status { get; set; }

    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    public string? Name { get; set; }

    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
    public string? Phone { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status is not null and not BookingStatus.Registered and not BookingStatus.Canceled and not BookingStatus.QueueEnrolled and not BookingStatus.QueuePending)
        {
            yield return new ValidationResult(
                $"Status must be one of: '{BookingStatus.Registered}', '{BookingStatus.Canceled}', '{BookingStatus.QueueEnrolled}', or '{BookingStatus.QueuePending}'. Provided status: '{Status}'.",
                [nameof(Status)]);
        }
    }
}
