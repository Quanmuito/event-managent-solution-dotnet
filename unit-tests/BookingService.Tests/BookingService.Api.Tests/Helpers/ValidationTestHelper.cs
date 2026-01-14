namespace BookingService.Api.Tests.Helpers;

using System.ComponentModel.DataAnnotations;

public static class ValidationTestHelper
{
    public static ValidationContext CreateValidationContext<T>(T instance)
    {
        return new ValidationContext(instance!);
    }

    public static (bool IsValid, List<ValidationResult> Results) ValidateObject<T>(T instance)
    {
        var validationContext = new ValidationContext(instance!);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(instance!, validationContext, results, true);
        return (isValid, results);
    }
}
