namespace EventService.Api.Models.Api.Event;
using System.ComponentModel.DataAnnotations;

public class UpdateEventDto : IValidatableObject
{
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string? Title { get; set; }

    [StringLength(100, MinimumLength = 1, ErrorMessage = "HostedBy must be between 1 and 100 characters.")]
    public string? HostedBy { get; set; }

    public bool? IsPublic { get; set; }

    [StringLength(2000, ErrorMessage = "Details cannot exceed 2000 characters.")]
    public string? Details { get; set; }

    public DateTime? TimeStart { get; set; }

    public DateTime? TimeEnd { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TimeStart.HasValue && TimeEnd.HasValue && TimeStart.Value >= TimeEnd.Value)
        {
            yield return new ValidationResult(
                "TimeStart must be before TimeEnd.",
                [nameof(TimeStart), nameof(TimeEnd)]
            );
        }
    }
}